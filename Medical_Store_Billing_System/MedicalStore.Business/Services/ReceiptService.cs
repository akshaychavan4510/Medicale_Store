// MedicalStore.Business/Services/ReceiptService.cs
using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class ReceiptService : IReceiptService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ReceiptService> _logger;

        public ReceiptService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ReceiptService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // ← FIXED: uses GetAllWithCustomerAsync() so Customer navigation is loaded
        //           and AutoMapper can populate CustomerName correctly
        public async Task<IEnumerable<ReceiptVM>> GetAllReceiptsAsync()
        {
            var receipts = await _unitOfWork.Receipts.GetAllWithCustomerAsync();
            return _mapper.Map<IEnumerable<ReceiptVM>>(receipts);
        }

        // ← FIXED: uses GetByIdWithCustomerAsync() so CustomerName is populated
        //           in Details, Edit and Delete views as well
        public async Task<ReceiptVM?> GetReceiptByIdAsync(int receiptId)
        {
            var receipt = await _unitOfWork.Receipts.GetByIdWithCustomerAsync(receiptId);
            return receipt == null ? null : _mapper.Map<ReceiptVM>(receipt);
        }

        public async Task<ReceiptVM> CreateReceiptAsync(ReceiptVM receiptVM)
        {
            if (receiptVM == null) throw new ArgumentNullException(nameof(receiptVM));
            if (receiptVM.Amount <= 0)
                throw new InvalidOperationException("Receipt amount must be > 0.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(receiptVM.CustId)
                    ?? throw new InvalidOperationException($"Customer {receiptVM.CustId} not found.");

                var entity = new Receipt
                {
                    ReceiptDate = receiptVM.ReceiptDate == default ? DateTime.Now : receiptVM.ReceiptDate,
                    CustId = receiptVM.CustId,
                    Amount = receiptVM.Amount,
                    PayMode = receiptVM.PayMode ?? "Cash",
                    RefNo = receiptVM.RefNo,
                    Note = receiptVM.Note,
                    CreatedBy = receiptVM.CreatedBy,
                    CreatedDate = DateTime.Now
                };

                await _unitOfWork.Receipts.AddAsync(entity);

                customer.CustBal -= receiptVM.Amount;
                _unitOfWork.Customers.Update(customer);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Receipt {ReceiptId} of {Amount} for customer {CustId}.",
                    entity.ReceiptId, receiptVM.Amount, receiptVM.CustId);

                // Re-fetch with Customer included so the returned VM has CustomerName
                return _mapper.Map<ReceiptVM>(
                    await _unitOfWork.Receipts.GetByIdWithCustomerAsync(entity.ReceiptId) ?? entity);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<ReceiptVM> UpdateReceiptAsync(ReceiptVM receiptVM)
        {
            if (receiptVM == null) throw new ArgumentNullException(nameof(receiptVM));
            if (receiptVM.Amount <= 0)
                throw new InvalidOperationException("Receipt amount must be > 0.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var entity = await _unitOfWork.Receipts.GetByIdAsync(receiptVM.ReceiptId)
                    ?? throw new KeyNotFoundException($"Receipt {receiptVM.ReceiptId} not found.");

                var customer = await _unitOfWork.Customers.GetByIdAsync(entity.CustId)
                    ?? throw new InvalidOperationException($"Customer {entity.CustId} not found.");

                decimal oldAmount = entity.Amount;
                int oldCustId = entity.CustId;

                if (oldCustId != receiptVM.CustId)
                {
                    // Restore old customer's balance
                    customer.CustBal += oldAmount;
                    _unitOfWork.Customers.Update(customer);

                    // Deduct from new customer
                    var newCustomer = await _unitOfWork.Customers.GetByIdAsync(receiptVM.CustId)
                        ?? throw new InvalidOperationException($"Customer {receiptVM.CustId} not found.");
                    newCustomer.CustBal -= receiptVM.Amount;
                    _unitOfWork.Customers.Update(newCustomer);
                }
                else
                {
                    // Same customer: adjust by difference
                    customer.CustBal += oldAmount;          // restore old
                    customer.CustBal -= receiptVM.Amount;   // apply new
                    _unitOfWork.Customers.Update(customer);
                }

                entity.ReceiptDate = receiptVM.ReceiptDate == default ? DateTime.Now : receiptVM.ReceiptDate;
                entity.CustId = receiptVM.CustId;
                entity.Amount = receiptVM.Amount;
                entity.PayMode = receiptVM.PayMode ?? "Cash";
                entity.RefNo = receiptVM.RefNo;
                entity.Note = receiptVM.Note;

                _unitOfWork.Receipts.Update(entity);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Receipt {ReceiptId} updated: {Amount} for customer {CustId}.",
                    entity.ReceiptId, receiptVM.Amount, receiptVM.CustId);

                return _mapper.Map<ReceiptVM>(
                    await _unitOfWork.Receipts.GetByIdWithCustomerAsync(entity.ReceiptId) ?? entity);
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var receipt = await _unitOfWork.Receipts.GetByIdAsync(id);
            if (receipt == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var customer = await _unitOfWork.Customers.GetByIdAsync(receipt.CustId);
                if (customer != null)
                {
                    customer.CustBal += receipt.Amount;   // reverse the balance reduction
                    _unitOfWork.Customers.Update(customer);
                }

                _unitOfWork.Receipts.Delete(receipt);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();
                return true;
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<ReceiptVM>> GetReceiptsByCustomerAsync(int customerId)
        {
            var receipts = await _unitOfWork.Receipts.GetReceiptsByCustomerAsync(customerId);
            return _mapper.Map<IEnumerable<ReceiptVM>>(receipts);
        }

        public async Task<IEnumerable<ReceiptVM>> GetReceiptsByDateRangeAsync(DateTime from, DateTime to)
        {
            var receipts = await _unitOfWork.Receipts.GetReceiptsByDateRangeAsync(from, to);
            return _mapper.Map<IEnumerable<ReceiptVM>>(receipts);
        }

        public async Task<decimal> GetTotalReceiptsAsync(DateTime from, DateTime to)
            => await _unitOfWork.Receipts.GetTotalReceiptsAsync(from, to);
    }
}
