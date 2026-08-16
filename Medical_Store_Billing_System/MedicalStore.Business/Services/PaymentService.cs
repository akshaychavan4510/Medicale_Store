using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;
using MedicalStore.MedicalStore.Business.Interfaces;
using MedicalStore.MedicalStore.Repository.Interface;
using MedicalStore.Repository.Interfaces;

namespace MedicalStore.MedicalStore.Business.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<PaymentService> logger)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IEnumerable<PaymentVM>> GetAllPaymentsAsync()
        {
            var payments = await _unitOfWork.Payments.GetAllAsync();
            return _mapper.Map<IEnumerable<PaymentVM>>(payments);
        }

        public async Task<PaymentVM?> GetPaymentByIdAsync(int paymentId)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(paymentId);
            return payment == null ? null : _mapper.Map<PaymentVM>(payment);
        }

        public async Task<bool> CreatePaymentAsync(PaymentVM paymentVM)
        {
            if (paymentVM == null) throw new ArgumentNullException(nameof(paymentVM));
            if (paymentVM.Amount <= 0) throw new InvalidOperationException("Payment amount must be > 0.");

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(paymentVM.SuppId)
                    ?? throw new InvalidOperationException($"Supplier {paymentVM.SuppId} not found.");

                var entity = _mapper.Map<Payment>(paymentVM);
                entity.PaymentDate = paymentVM.PaymentDate == default ? DateTime.Now : paymentVM.PaymentDate;
                entity.CreatedDate = DateTime.Now;

                await _unitOfWork.Payments.AddAsync(entity);

                // Reduce supplier balance (amount payable decreases)
                supplier.SuppBal -= paymentVM.Amount;
                _unitOfWork.Suppliers.Update(supplier);

                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Payment {PaymentId} of {Amount} to supplier {SuppId}.",
                    entity.PaymentId, paymentVM.Amount, paymentVM.SuppId);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to create payment for supplier {SuppId}.", paymentVM.SuppId);
                throw;
            }
        }

        public async Task<bool> UpdatePaymentAsync(PaymentVM paymentVM)
        {
            if (paymentVM == null) throw new ArgumentNullException(nameof(paymentVM));
            if (paymentVM.Amount <= 0) throw new InvalidOperationException("Payment amount must be > 0.");

            var existing = await _unitOfWork.Payments.GetByIdAsync(paymentVM.PaymentId);
            if (existing == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                decimal oldAmount = existing.Amount;
                int oldSuppId = existing.SuppId;

                // Restore old supplier balance (reverse the old payment)
                var oldSupplier = await _unitOfWork.Suppliers.GetByIdAsync(oldSuppId);
                if (oldSupplier != null)
                {
                    oldSupplier.SuppBal += oldAmount;
                    _unitOfWork.Suppliers.Update(oldSupplier);
                }

                // Apply new supplier balance
                var newSupplier = await _unitOfWork.Suppliers.GetByIdAsync(paymentVM.SuppId);
                if (newSupplier == null)
                    throw new InvalidOperationException($"Supplier {paymentVM.SuppId} not found.");

                newSupplier.SuppBal -= paymentVM.Amount;
                _unitOfWork.Suppliers.Update(newSupplier);

                // Update the payment entity
                existing.PaymentDate = paymentVM.PaymentDate == default ? existing.PaymentDate : paymentVM.PaymentDate;
                existing.SuppId = paymentVM.SuppId;
                existing.Amount = paymentVM.Amount;
                existing.PayMode = paymentVM.PayMode;
                existing.RefNo = paymentVM.RefNo;
                existing.Note = paymentVM.Note;

                _unitOfWork.Payments.Update(existing);
                await _unitOfWork.SaveChangesAsync();
                await _unitOfWork.CommitAsync();

                _logger.LogInformation("Payment {PaymentId} updated to {Amount} for supplier {SuppId}.",
                    existing.PaymentId, paymentVM.Amount, paymentVM.SuppId);

                return true;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackAsync();
                _logger.LogError(ex, "Failed to update payment {PaymentId}.", paymentVM.PaymentId);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var payment = await _unitOfWork.Payments.GetByIdAsync(id);
            if (payment == null) return false;

            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var supplier = await _unitOfWork.Suppliers.GetByIdAsync(payment.SuppId);
                if (supplier != null)
                {
                    supplier.SuppBal += payment.Amount;
                    _unitOfWork.Suppliers.Update(supplier);
                }

                _unitOfWork.Payments.Delete(payment);
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

        public async Task<IEnumerable<PaymentVM>> GetPaymentsBySupplierAsync(int supplierId)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsBySupplierAsync(supplierId);
            return _mapper.Map<IEnumerable<PaymentVM>>(payments);
        }

        public async Task<IEnumerable<PaymentVM>> GetPaymentsByDateRangeAsync(DateTime from, DateTime to)
        {
            var payments = await _unitOfWork.Payments.GetPaymentsByDateRangeAsync(from, to);
            return _mapper.Map<IEnumerable<PaymentVM>>(payments);
        }

        public async Task<decimal> GetTotalPaymentsAsync(DateTime from, DateTime to)
            => await _unitOfWork.Payments.GetTotalPaymentsAsync(from, to);
    }
}