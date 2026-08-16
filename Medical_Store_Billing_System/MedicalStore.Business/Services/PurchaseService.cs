using AutoMapper;
using AutoMapper.QueryableExtensions;
using Medical_Store_Billing_System.Models;

using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace MedicalStore.Business.Services
{
    public class PurchaseService : IPurchaseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PurchaseService> _logger;

        public PurchaseService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PurchaseService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PurchaseMasterVM>> GetAllAsync()
        {
            return await _unitOfWork.Purchases.GetQueryable()
                .OrderByDescending(p => p.PurchaseDate)
                .ProjectTo<PurchaseMasterVM>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        // FIX: Use GetByIdWithDetailsAsync (includes Supplier + PurchaseDetails + Medicine)
        // instead of GetByIdAsync (bare FindAsync, no includes → InvalidCastException on mapping)
        public async Task<PurchaseMasterVM?> GetByIdAsync(int purchaseId)
        {
            var purchase = await _unitOfWork.Purchases.GetByIdWithDetailsAsync(purchaseId);
            if (purchase == null) return null;

            var vm = _mapper.Map<PurchaseMasterVM>(purchase);
            // PurchaseDetails are already loaded via Include in GetByIdWithDetailsAsync
            vm.PurchaseDetails = _mapper.Map<List<PurchaseDetailVM>>(purchase.PurchaseDetails);
            return vm;
        }

        public async Task<bool> CreatePurchaseAsync(PurchaseMasterVM purchaseVM)
        {
            if (purchaseVM == null) throw new ArgumentNullException(nameof(purchaseVM));
            if (purchaseVM.PurchaseDetails == null || !purchaseVM.PurchaseDetails.Any())
                throw new InvalidOperationException("At least one line item is required.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                decimal grandTotal = 0;

                foreach (var line in purchaseVM.PurchaseDetails)
                {
                    if (line.Qty <= 0)
                        throw new InvalidOperationException("Quantity must be greater than zero.");
                    if (line.Rate <= 0)
                        throw new InvalidOperationException("Rate must be greater than zero.");

                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(line.MedId);
                    if (medicine == null)
                        throw new InvalidOperationException($"Medicine id {line.MedId} was not found.");

                    line.Amt = Math.Round(line.Rate * line.Qty, 2);
                    line.GstAmt = Math.Round(line.Amt * line.GstPct / 100m, 2);
                    line.Total = line.Amt + line.GstAmt;

                    grandTotal += line.Total;

                    // Increase stock and record purchase rate
                    medicine.Stock += line.Qty;
                    medicine.PurchaseRate = line.Rate;
                    _unitOfWork.Medicines.Update(medicine);
                }

                purchaseVM.GrandTotal = grandTotal;
                purchaseVM.NetTotal = grandTotal - purchaseVM.Discount;

                var purchaseEntity = _mapper.Map<PurchaseMaster>(purchaseVM);
                purchaseEntity.PurchaseDate = purchaseVM.PurchaseDate == default ? DateTime.Now : purchaseVM.PurchaseDate;

                await _unitOfWork.Purchases.AddAsync(purchaseEntity);
                await _unitOfWork.SaveChangesAsync();

                foreach (var line in purchaseVM.PurchaseDetails)
                {
                    var detail = _mapper.Map<PurchaseDetails>(line);
                    detail.PurchaseId = purchaseEntity.PurchaseId;
                    await _unitOfWork.PurchaseDetails.AddAsync(detail);
                }

                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(purchaseVM.SuppId);
                if (supplier != null)
                {
                    supplier.SuppBal += grandTotal;
                    _unitOfWork.Suppliers.Update(supplier);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Purchase {PurchaseId} created for supplier {SuppId} with grand total {GrandTotal}.",
                    purchaseEntity.PurchaseId, purchaseVM.SuppId, grandTotal);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create purchase.");
                throw;
            }
        }
        public async Task<(int NextPurId, string NextInvoiceNo)> GetNextPurchaseNumberAsync()
        {
            // Get max existing PurchaseId from DB  (returns 0 if table is empty)
            var maxId = await _unitOfWork.Purchases
                            .GetQueryable()
                            .Select(p => (int?)p.PurchaseId)
                            .MaxAsync() ?? 0;

            var nextId = maxId + 1;

            // Check if that Invoice No already exists — keep incrementing until unique
            string invoiceNo;
            int invoiceNum = nextId;
            do
            {
                invoiceNo = $"INV-{invoiceNum:D4}";
                bool exists = await _unitOfWork.Purchases
                                    .GetQueryable()
                                    .AnyAsync(p => p.InvoiceNo == invoiceNo);
                if (!exists) break;
                invoiceNum++;
            } while (true);

            return (nextId, invoiceNo);
        }
        // ══════════════════════════════════════════════════════════════════════
        // ADD this method to PurchaseService.cs  (inside the PurchaseService class)
        // ══════════════════════════════════════════════════════════════════════

        public async Task<bool> UpdatePurchaseAsync(PurchaseMasterVM purchaseVM)
        {
            if (purchaseVM == null) throw new ArgumentNullException(nameof(purchaseVM));
            if (purchaseVM.PurchaseDetails == null || !purchaseVM.PurchaseDetails.Any())
                throw new InvalidOperationException("At least one line item is required.");

            // Load the existing purchase with all details + medicine + supplier
            var existing = await _unitOfWork.Purchases.GetByIdWithDetailsAsync(purchaseVM.PurId);
            if (existing == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // ── Step 1: Reverse old stock & supplier balance ──────────────
                foreach (var oldDetail in existing.PurchaseDetails)
                {
                    var med = await _unitOfWork.Medicines.GetByIdAsync(oldDetail.MedId);
                    if (med != null)
                    {
                        med.Stock -= oldDetail.Qty;          // undo old stock
                        _unitOfWork.Medicines.Update(med);
                    }
                }

                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(existing.SuppId);
                if (supplier != null)
                {
                    supplier.SuppBal -= existing.GrandTotal; // undo old payable
                    _unitOfWork.Suppliers.Update(supplier);
                }

                // ── Step 2: Delete old detail rows ────────────────────────────
                foreach (var oldDetail in existing.PurchaseDetails.ToList())
                    _unitOfWork.PurchaseDetails.Delete(oldDetail);

                // ── Step 3: Recalculate new lines & apply new stock ───────────
                decimal grandTotal = 0;
                foreach (var line in purchaseVM.PurchaseDetails)
                {
                    if (line.Qty <= 0) throw new InvalidOperationException("Quantity must be > 0.");
                    if (line.Rate <= 0) throw new InvalidOperationException("Rate must be > 0.");

                    var med = await _unitOfWork.Medicines.GetByIdAsync(line.MedId);
                    if (med == null)
                        throw new InvalidOperationException($"Medicine id {line.MedId} not found.");

                    line.Amt = Math.Round(line.Rate * line.Qty, 2);
                    line.GstAmt = Math.Round(line.Amt * line.GstPct / 100m, 2);
                    line.Total = line.Amt + line.GstAmt;
                    grandTotal += line.Total;

                    med.Stock += line.Qty;   // apply new stock
                    med.PurchaseRate = line.Rate;
                    _unitOfWork.Medicines.Update(med);
                }

                purchaseVM.GrandTotal = grandTotal;
                purchaseVM.NetTotal = grandTotal - purchaseVM.Discount;

                // ── Step 4: Update master header fields ───────────────────────
                existing.SuppId = purchaseVM.SuppId;
                existing.InvoiceNo = purchaseVM.InvoiceNo;
                existing.PurchaseDate = purchaseVM.PurchaseDate == default ? DateTime.Now : purchaseVM.PurchaseDate;
                existing.Discount = purchaseVM.Discount;
                existing.GrandTotal = grandTotal;
                existing.NetTotal = purchaseVM.NetTotal;
                existing.ModifiedDate = DateTime.UtcNow;
                _unitOfWork.Purchases.Update(existing);

                await _unitOfWork.SaveChangesAsync();

                // ── Step 5: Insert new detail rows ────────────────────────────
                foreach (var line in purchaseVM.PurchaseDetails)
                {
                    var detail = _mapper.Map<PurchaseDetails>(line);
                    detail.PurchaseId = existing.PurchaseId;
                    await _unitOfWork.PurchaseDetails.AddAsync(detail);
                }

                // ── Step 6: Update supplier payable with new total ────────────
                supplier = await _unitOfWork.Suppliers.GetByIdAsync(purchaseVM.SuppId);
                if (supplier != null)
                {
                    supplier.SuppBal += grandTotal;
                    _unitOfWork.Suppliers.Update(supplier);
                }

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Purchase {PurchaseId} updated. New grand total: {GrandTotal}.",
                    existing.PurchaseId, grandTotal);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update purchase {PurchaseId}.", purchaseVM.PurId);
                throw;
            }
        }

        // FIX: Use GetByIdWithDetailsAsync so PurchaseDetails are loaded before iterating
        // Old code used GetByIdAsync (no includes) then separately fetched details —
        // that caused EF tracking conflicts and the InvalidCastException on Delete
        public async Task<bool> DeleteAsync(int id)
        {
            var purchase = await _unitOfWork.Purchases.GetByIdWithDetailsAsync(id);
            if (purchase == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                // PurchaseDetails already loaded via Include — no separate query needed
                foreach (var detail in purchase.PurchaseDetails)
                {
                    var medicine = await _unitOfWork.Medicines.GetByIdAsync(detail.MedId);
                    if (medicine != null)
                    {
                        medicine.Stock -= detail.Qty;
                        _unitOfWork.Medicines.Update(medicine);
                    }
                }

                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(purchase.SuppId);
                if (supplier != null)
                {
                    supplier.SuppBal -= purchase.GrandTotal;
                    _unitOfWork.Suppliers.Update(supplier);
                }

                _unitOfWork.Purchases.Delete(purchase);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to delete purchase {PurchaseId}.", id);
                throw;
            }
        }

        public async Task<IEnumerable<PurchaseMasterVM>> GetPurchasesBySupplierAsync(int supplierId)
        {
            var purchases = await _unitOfWork.Purchases.GetBySupplierIdAsync(supplierId);
            return _mapper.Map<IEnumerable<PurchaseMasterVM>>(purchases);
        }

        public async Task<IEnumerable<PurchaseMasterVM>> GetPurchasesByDateRangeAsync(DateTime from, DateTime to)
        {
            var purchases = await _unitOfWork.Purchases.GetByDateRangeAsync(from, to);
            return _mapper.Map<IEnumerable<PurchaseMasterVM>>(purchases);
        }

        public async Task<decimal> GetTotalPurchaseAmountAsync(DateTime from, DateTime to)
            => await _unitOfWork.Purchases.GetTotalAmountAsync(from, to);
    }
}