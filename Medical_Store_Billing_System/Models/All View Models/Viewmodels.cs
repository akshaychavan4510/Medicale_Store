using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MedicalStore.Business.ViewModels
{
    // ── Medicine Category ─────────────────────────────────────────────
    public class MedicineCategoryVM
    {
        public int CatId { get; set; }

        // Alias for compatibility
        public int CategoryId { get => CatId; set => CatId = value; }

        [Required(ErrorMessage = "Category Name is required.")]
        [StringLength(150)]
        [Display(Name = "Category Name")]
        public string CatName { get; set; } = string.Empty;

        // Alias for compatibility
        public string CategoryName { get => CatName; set => CatName = value; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

    // ── Brand ─────────────────────────────────────────────────────────
    public class BrandVM
    {
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Brand Name is required.")]
        [StringLength(150)]
        [Display(Name = "Brand Name")]
        public string BrandName { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

    public class BrandDetailsVM
    {
        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }

        // Additional details
        public int MedicineCount { get; set; }
        public decimal TotalStockValue { get; set; }

        // Computed properties
        public string Status => IsActive ? "Active" : "Inactive";
        public string CreatedDateFormatted => CreatedDate.ToString("dd-MM-yyyy hh:mm tt");
        public string ModifiedDateFormatted => ModifiedDate?.ToString("dd-MM-yyyy hh:mm tt") ?? "Never";
    }

    // ── Medicine Master ───────────────────────────────────────────────
    public class MedicineMasterVM
    {
        public int MedicineId { get; set; }

        // Alias used by controller (id != vm.MedId check) and views
        public int MedId { get => MedicineId; set => MedicineId = value; }

        [Required(ErrorMessage = "Medicine name is required.")]
        [StringLength(200)]
        [Display(Name = "Medicine Name")]
        public string MedName { get; set; } = string.Empty;

        // Alias used in Index view (item.MedNm)
        public string MedNm { get => MedName; set => MedName = value; }

        [Required(ErrorMessage = "Category is required.")]
        [Display(Name = "Category")]
        public int CatId { get; set; }

        public string? CategoryName { get; set; }

        [Required(ErrorMessage = "Brand is required.")]
        [Display(Name = "Brand")]
        public int BrandId { get; set; }

        public string? BrandName { get; set; }

        [StringLength(500)]
        [Display(Name = "Ingredients")]
        public string? Ingredients { get; set; }

        [Required(ErrorMessage = "Rate is required.")]
        [Range(0.01, 999999.99, ErrorMessage = "Rate must be greater than 0.")]
        [Display(Name = "Sale Rate (₹)")]
        public decimal Rate { get; set; }

        // Alias used in ReportService (m.SaleRate)
        public decimal SaleRate { get => Rate; set => Rate = value; }

        [Display(Name = "Purchase Rate (₹)")]
        public decimal PurchaseRate { get; set; }

        [Display(Name = "Stock")]
        public decimal Stock { get; set; }           // decimal to match entity
    }

    // ── Customer ──────────────────────────────────────────────────────
    public class CustomerVM
    {
        public int CustId { get; set; }

        // Alias for compatibility
        public int CustomerId { get => CustId; set => CustId = value; }

        [Required(ErrorMessage = "Customer Name is required.")]
        [StringLength(150)]
        [Display(Name = "Customer Name")]
        public string CustName { get; set; } = string.Empty;

        // Alias for compatibility
        public string CustomerName { get => CustName; set => CustName = value; }

        [StringLength(15)]
        [Phone]
        [Display(Name = "Phone")]
        public string? CustPhone { get; set; }

        [StringLength(100)]
        [EmailAddress]
        [Display(Name = "Email")]
        public string? CustEmail { get; set; }

        [StringLength(250)]
        [Display(Name = "Address")]
        public string? CustAddress { get; set; }

        [Display(Name = "Balance (₹)")]
        public decimal CustBal { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }
    }

    // ── Supplier ──────────────────────────────────────────────────────
    public class SupplierVM
    {
        public int SuppId { get; set; }

        [Required, StringLength(150)]
        public string SuppName { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SuppPhone { get; set; }

        [EmailAddress, StringLength(100)]
        public string? SuppEmail { get; set; }

        [StringLength(250)]
        public string? SuppAddress { get; set; }

        [StringLength(50)]
        public string? GstNo { get; set; }

        public decimal SuppBal { get; set; }
    }

    // ── Sale Detail ───────────────────────────────────────────────────
    public class SaleDetailVM
    {
        public int SaleDetId { get; set; }
        public int SaleId { get; set; }

        // FK – posted from <select name="SaleDetails[n].MedId">
        public int MedId { get; set; }

        // Display only (not posted; removed from ModelState in controller)
        public string? MedicineName { get; set; }

        // Posted from rate-input (readonly, filled by AJAX)
        public decimal Rate { get; set; }

        // Posted from qty-input
        public decimal Qty { get; set; }

        // Computed server-side; also posted as readonly hidden value
        public decimal Amt { get; set; }

        // ── GST fields ───────────────────────────────────────────────────────
        // GstPct  : posted from <input type="hidden" name="SaleDetails[n].GstPct" value="5">
        //           SaleService reads this:  line.GstAmt = line.Amt * line.GstPct / 100
        public decimal GstPct { get; set; } = 5m;

        // GstAmt  : computed; posted from gst-input (name="SaleDetails[n].GstAmt")
        public decimal GstAmt { get; set; }

        // Gst     : alias kept for backward compatibility (Invoice view uses d.Gst)
        //           Both getter and setter delegate to GstAmt so the value is never duplicated.
        public decimal Gst
        {
            get => GstAmt;
            set => GstAmt = value;
        }

        // Computed line total; posted from total-input
        public decimal Total { get; set; }
    }

    // ── Sale Master ───────────────────────────────────────────────────
    public class SaleMasterVM
    {
        public int SaleId { get; set; }
        public DateTime SaleDate { get; set; } = DateTime.Now;

        // FK – posted from <select name="CustId">
        public int CustId { get; set; }

        // Display only
        public string? CustomerName { get; set; }

        // Computed grand total – posted from #grandTotalHidden; removed from ModelState
        // so server-side re-calculation in SaleService is authoritative.
        public decimal GrandTotal { get; set; }

        public List<SaleDetailVM> SaleDetails { get; set; } = new();

        // Alias for backward compatibility
        public int CustomerId
        {
            get => CustId;
            set => CustId = value;
        }
    }

    // ── Purchase Detail Line ──────────────────────────────────────────
    public class PurchaseMasterVM
    {
        public int PurId { get; set; }

        [Display(Name = "Purchase Date")]
        [DataType(DataType.Date)]
        public DateTime PurchaseDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Please select a supplier.")]
        [Display(Name = "Supplier")]
        public int SuppId { get; set; }

        public string? SupplierName { get; set; }

        [Display(Name = "Invoice No")]
        [MaxLength(100)]
        public string? InvoiceNo { get; set; }

        [Display(Name = "Grand Total")]
        public decimal GrandTotal { get; set; }

        [Display(Name = "Discount")]
        public decimal Discount { get; set; }

        [Display(Name = "Net Total")]
        public decimal NetTotal { get; set; }

        public List<PurchaseDetailVM> PurchaseDetails { get; set; } = new();
    }

    public class PurchaseDetailVM
    {
        public int MedId { get; set; }

        public decimal Qty { get; set; }

        public decimal Rate { get; set; }

        public decimal Amt { get; set; }

        public decimal GstPct { get; set; }

        public decimal GstAmt { get; set; }

        public decimal Total { get; set; }

        public string? MedicineName { get; set; }
    }
    // ── Receipt ───────────────────────────────────────────────────────
    public class ReceiptVM
    {
        // ============================================================
        // RECEIPT ID
        // ============================================================
        public int ReceiptId { get; set; }

        // ============================================================
        // RECEIPT DATE
        // ============================================================
        [Required(ErrorMessage = "Receipt date is required.")]
        [Display(Name = "Receipt Date")]
        [DataType(DataType.Date)]
        public DateTime ReceiptDate { get; set; } = DateTime.Now;

        // ============================================================
        // CUSTOMER
        // ============================================================
        [Required(ErrorMessage = "Please select a customer.")]
        [Display(Name = "Customer")]
        public int CustId { get; set; }

        // ============================================================
        // CUSTOMER NAME
        // Used for displaying customer name in Index/Details
        // ============================================================
        [Display(Name = "Customer Name")]
        public string? CustomerName { get; set; }

        // ============================================================
        // AMOUNT
        // ============================================================
        [Required(ErrorMessage = "Amount is required.")]
        [Range(
            0.01,
            double.MaxValue,
            ErrorMessage = "Amount must be greater than 0."
        )]
        [Display(Name = "Amount (₹)")]
        public decimal Amount { get; set; }

        // ============================================================
        // PAYMENT MODE
        // ============================================================
        [Display(Name = "Payment Mode")]
        public string? PayMode { get; set; } = "Cash";

        // ============================================================
        // REFERENCE NUMBER
        // ============================================================
        [Display(Name = "Reference No.")]
        [MaxLength(100, ErrorMessage = "Reference number cannot exceed 100 characters.")]
        public string? RefNo { get; set; }

        // ============================================================
        // NOTE
        // ============================================================
        [Display(Name = "Note")]
        public string? Note { get; set; }

        // ============================================================
        // CREATED BY
        // ============================================================
        [Display(Name = "Created By")]
        public string? CreatedBy { get; set; }

        // ============================================================
        // CREATED DATE
        // ============================================================
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
    // ── Payment ───────────────────────────────────────────────────────
    public class PaymentVM
    {
        public int PaymentId { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Supplier is required.")]
        [Display(Name = "Supplier")]
        public int SuppId { get; set; }

        [Display(Name = "Supplier Name")]
        public string? SupplierName { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount (₹)")]
        public decimal Amount { get; set; }

        [Display(Name = "Payment Mode")]
        public string? PayMode { get; set; } = "Cash";

        [Display(Name = "Reference No.")]
        public string? RefNo { get; set; }

        public string? Note { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; }
    }

    // ── Dashboard ─────────────────────────────────────────────────────
    public class DashboardVM
    {
        public decimal TodaySales { get; set; }
        public decimal MonthSales { get; set; }
        public decimal TodayPurchases { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalSuppliers { get; set; }
        public int TotalMedicines { get; set; }
        public int LowStockCount { get; set; }
        public decimal TotalReceivable { get; set; }
        public decimal TotalPayable { get; set; }
        public List<DashboardChartPointVM> SalesTrend { get; set; } = new();
        public List<LowStockItemVM> LowStockItems { get; set; } = new();
    }

    public class DashboardChartPointVM
    {
        public string Label { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    public class LowStockItemVM
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public decimal Stock { get; set; }
    }

    // ── Login ─────────────────────────────────────────────────────────
    public class LoginVM
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }
    }

    // ── Report ViewModels ─────────────────────────────────────────────
    public class LedgerEntryVM
    {
        public DateTime Date { get; set; }
        public string Particulars { get; set; } = string.Empty;
        public string Type => Particulars.StartsWith("Sale") ? "Sale" : "Receipt";
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
    }

    public class DailySaleReportVM
    {
        public DateTime Date { get; set; }
        public DateTime SaleDate { get => Date; set => Date = value; }
        public int SaleId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal GstAmt { get; set; }
        public decimal Total { get; set; }
    }

    public class PurchaseReportVM
    {
        public DateTime Date { get; set; }
        public DateTime PurchaseDate { get => Date; set => Date = value; }
        public int PurId { get; set; }
        public string InvoiceNo { get; set; } = string.Empty;
        public string SupplierName { get; set; } = string.Empty;
        public string MedicineName { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal Qty { get; set; }
        public decimal Rate { get; set; }
        public decimal GstAmt { get; set; }
        public decimal Total { get; set; }
    }

    public class CustomerLedgerVM
    {
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public List<LedgerEntryVM> Entries { get; set; } = new();
        public decimal ClosingBalance { get; set; }
    }

    public class SupplierLedgerVM
    {
        public int SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public List<LedgerEntryVM> Entries { get; set; } = new();
        public decimal ClosingBalance { get; set; }
    }

    public class StockReportVM
    {
        public int MedicineId { get; set; }
        public string MedicineName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string BrandName { get; set; } = string.Empty;
        public decimal Stock { get; set; }
        public decimal Rate { get; set; }
        public decimal StockValue { get; set; }
        public bool IsLowStock { get; set; }
    }
}