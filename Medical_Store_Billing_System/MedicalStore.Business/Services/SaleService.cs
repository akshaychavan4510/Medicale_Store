using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.Extensions.Logging;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaleService> _logger;

        public SaleService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<SaleService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ── GET ALL (with SaleDetails + Medicine included) ─────────────────
        public async Task<IEnumerable<SaleMasterVM>> GetAllAsync()
        {
            var sales = await _unitOfWork.Sales.GetAllWithDetailsAsync();
            return _mapper.Map<IEnumerable<SaleMasterVM>>(sales);
        }

        // ── GET BY ID ──────────────────────────────────────────────────────
        public async Task<SaleMasterVM?> GetByIdAsync(int id)
        {
            var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            return sale == null ? null : _mapper.Map<SaleMasterVM>(sale);
        }

        // ── CREATE ─────────────────────────────────────────────────────────
        // FIX: Use CreateSaleWithDetailsAsync so SaleId is correctly set on
        //      every detail row before they are inserted.
        //      Old code built detailsList locally but never attached it to
        //      saleEntity.SaleDetails, so details were NEVER saved → "No items".
        public async Task<bool> CreateSaleAsync(SaleMasterVM saleVM)
        {
            if (saleVM == null) throw new ArgumentNullException(nameof(saleVM));
            if (saleVM.SaleDetails == null || !saleVM.SaleDetails.Any())
                throw new InvalidOperationException("At least one line item is required.");

            // 1. Validate medicines and compute line totals
            decimal grandTotal = 0;
            var detailEntities = new List<SaleDetails>();

            foreach (var line in saleVM.SaleDetails)
            {
                if (line.Qty <= 0) throw new InvalidOperationException($"Qty must be > 0 for medicine {line.MedId}.");
                if (line.Rate <= 0) throw new InvalidOperationException($"Rate must be > 0 for medicine {line.MedId}.");

                var medicine = await _unitOfWork.Medicines.GetByIdAsync(line.MedId);
                if (medicine == null)
                    throw new InvalidOperationException($"Medicine {line.MedId} not found.");
                if (medicine.Stock < line.Qty)
                    throw new InvalidOperationException(
                        $"Insufficient stock for '{medicine.MedName}'. Available: {medicine.Stock}, Requested: {line.Qty}.");

                // Compute amounts
                line.Amt = Math.Round(line.Rate * line.Qty, 2);
                line.GstAmt = Math.Round(line.Amt * line.GstPct / 100m, 2);
                line.Total = line.Amt + line.GstAmt;
                grandTotal += line.Total;

                // Map VM → Entity (SaleId will be set inside CreateSaleWithDetailsAsync)
                var detailEntity = _mapper.Map<SaleDetails>(line);
                detailEntity.GstAmt = line.GstAmt;
                detailEntities.Add(detailEntity);
            }

            // 2. Build the SaleMaster entity
            var saleEntity = _mapper.Map<SaleMaster>(saleVM);
            saleEntity.SaleDate = saleVM.SaleDate == default ? DateTime.Now : saleVM.SaleDate;
            saleEntity.CreatedDate = DateTime.UtcNow;
            saleEntity.ModifiedDate = null;
            saleEntity.GrandTotal = grandTotal;
            saleEntity.NetTotal = grandTotal;
            saleEntity.Discount = 0;

            // 3. ✅ Use repository method that:
            //      a) Inserts SaleMaster → gets SaleId
            //      b) Sets SaleId on each detail row
            //      c) Inserts all SaleDetails
            //      d) Updates customer balance & medicine stock inside a transaction
            await _unitOfWork.Sales.CreateSaleWithDetailsAsync(saleEntity, detailEntities);

            _logger.LogInformation("Sale {SaleId} created for customer {CustId} with total {GrandTotal}.",
                saleEntity.SaleId, saleVM.CustId, grandTotal);

            return true;
        }

        // ── DELETE ─────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(id);
            if (sale == null) return false;

            using var transaction = await _unitOfWork.BeginTransactionAsync();
            try
            {
                // Restore stock
                foreach (var detail in sale.SaleDetails)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedId);
                    if (medicine != null)
                    {
                        medicine.Stock += detail.Qty;
                        _unitOfWork.Medicines.Update(medicine);
                    }
                }

                // Reduce customer balance
                var customer = await _unitOfWork.Customers.GetByIdAsync(sale.CustId);
                if (customer != null)
                {
                    customer.CustBal -= sale.GrandTotal;
                    _unitOfWork.Customers.Update(customer);
                }

                _unitOfWork.Sales.Delete(sale);
                await _unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Sale {SaleId} deleted.", id);
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}