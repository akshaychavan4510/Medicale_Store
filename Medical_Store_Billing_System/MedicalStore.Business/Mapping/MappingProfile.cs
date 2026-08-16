using AutoMapper;
using Medical_Store_Billing_System.Models;
using MedicalStore.Business.ViewModels;

namespace MedicalStore.Business.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ── MedicineCategory ──────────────────────────────────────────
            CreateMap<MedicineCategory, MedicineCategoryVM>()
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CatId))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.CatName))
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            // ── Brand ─────────────────────────────────────────────────────
            CreateMap<Brand, BrandVM>()
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore());

            CreateMap<Brand, BrandDetailsVM>()
               .ForMember(dest => dest.CreatedDateFormatted,
                   opt => opt.Ignore())
               .ForMember(dest => dest.ModifiedDateFormatted,
                   opt => opt.Ignore())
               .ForMember(dest => dest.Status,
                   opt => opt.Ignore())
               .ForMember(dest => dest.MedicineCount,
                   opt => opt.Ignore())
               .ForMember(dest => dest.TotalStockValue,
                   opt => opt.Ignore());

            // ── MedicineMaster ────────────────────────────────────────────
            CreateMap<MedicineMaster, MedicineMasterVM>()
                .ForMember(dest => dest.MedicineId, opt => opt.MapFrom(src => src.MedId))
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? src.Category.CatName : string.Empty))
                .ForMember(dest => dest.BrandName,
                    opt => opt.MapFrom(src => src.Brand != null ? src.Brand.BrandName : string.Empty))
                .ReverseMap()
                .ForMember(dest => dest.MedId, opt => opt.MapFrom(src => src.MedicineId))
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.Category, opt => opt.Ignore())
                .ForMember(dest => dest.Brand, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseDetails, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Unit, opt => opt.Ignore())
                .ForMember(dest => dest.GstPct, opt => opt.Ignore());

            // ── Customer ──────────────────────────────────────────────────
            CreateMap<Customer, CustomerVM>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.CustName))
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.CustBal, opt => opt.Ignore())
                .ForMember(dest => dest.Sales, opt => opt.Ignore())
                .ForMember(dest => dest.Receipts, opt => opt.Ignore());

            // ── Supplier ──────────────────────────────────────────────────
            CreateMap<Supplier, SupplierVM>()
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.IsActive, opt => opt.Ignore())
                .ForMember(dest => dest.SuppBal, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseMasters, opt => opt.Ignore())
                .ForMember(dest => dest.Payments, opt => opt.Ignore());

            // ── Receipt ───────────────────────────────────────────────────
            CreateMap<Receipt, ReceiptVM>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : null))
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore());

            // ── Payment ───────────────────────────────────────────────────
            CreateMap<Payment, PaymentVM>()
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SuppName : null))
                .ReverseMap()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore());

            // ── PurchaseMaster: Entity → ViewModel ────────────────────────
            CreateMap<PurchaseMaster, PurchaseMasterVM>()
                .ForMember(dest => dest.PurId, opt => opt.MapFrom(src => src.PurchaseId))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate))
                .ForMember(dest => dest.SuppId, opt => opt.MapFrom(src => src.SuppId))
                .ForMember(dest => dest.SupplierName,
                    opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SuppName : ""))
                .ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.InvoiceNo))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                .ForMember(dest => dest.NetTotal, opt => opt.MapFrom(src => src.NetTotal))
                .ForMember(dest => dest.PurchaseDetails, opt => opt.Ignore());

            // ── PurchaseMaster: ViewModel → Entity ────────────────────────
            CreateMap<PurchaseMasterVM, PurchaseMaster>()
                .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.PurId))
                .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => src.PurchaseDate))
                .ForMember(dest => dest.SuppId, opt => opt.MapFrom(src => src.SuppId))
                .ForMember(dest => dest.InvoiceNo, opt => opt.MapFrom(src => src.InvoiceNo))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => src.Discount))
                .ForMember(dest => dest.NetTotal, opt => opt.MapFrom(src => src.NetTotal))
                .ForMember(dest => dest.PurchaseDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Supplier, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore());

            // ── PurchaseDetails: Entity → ViewModel ───────────────────────
            CreateMap<PurchaseDetails, PurchaseDetailVM>()
                .ForMember(dest => dest.MedId, opt => opt.MapFrom(src => src.MedId))
                .ForMember(dest => dest.Qty, opt => opt.MapFrom(src => src.Qty))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.Amt, opt => opt.MapFrom(src => src.Amt))
                .ForMember(dest => dest.GstPct, opt => opt.MapFrom(src => src.GstPct))
                .ForMember(dest => dest.GstAmt, opt => opt.MapFrom(src => src.GstAmt))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.MedicineName,
                    opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.MedName : ""));

            // ✅ FIX: PurchaseDetails: ViewModel → Entity
            // ✅ This was MISSING — now added in the fixed file:
            CreateMap<PurchaseDetailVM, PurchaseDetails>()
                .ForMember(dest => dest.MedId, opt => opt.MapFrom(src => src.MedId))
                .ForMember(dest => dest.Qty, opt => opt.MapFrom(src => src.Qty))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.Amt, opt => opt.MapFrom(src => src.Amt))
                .ForMember(dest => dest.GstPct, opt => opt.MapFrom(src => src.GstPct))
                .ForMember(dest => dest.GstAmt, opt => opt.MapFrom(src => src.GstAmt))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.PurchaseDetId, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseId, opt => opt.Ignore())
                .ForMember(dest => dest.ExpiryDate, opt => opt.Ignore())
                .ForMember(dest => dest.BatchNo, opt => opt.Ignore())
                .ForMember(dest => dest.PurchaseMaster, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore());

            // ── SaleMaster: Entity → ViewModel ─────────────────────────────
            CreateMap<SaleMaster, SaleMasterVM>()
                .ForMember(dest => dest.CustomerName,
                    opt => opt.MapFrom(src => src.Customer != null ? src.Customer.CustName : string.Empty))
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.CustId))
                .ForMember(dest => dest.CustId, opt => opt.MapFrom(src => src.CustId))
                .ForMember(dest => dest.SaleDetails, opt => opt.MapFrom(src => src.SaleDetails))
                .ForMember(dest => dest.GrandTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.SaleDate, opt => opt.MapFrom(src => src.SaleDate))
                .ForMember(dest => dest.SaleId, opt => opt.MapFrom(src => src.SaleId));

            // ── SaleMaster: ViewModel → Entity ─────────────────────────────
            CreateMap<SaleMasterVM, SaleMaster>()
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
                .ForMember(dest => dest.ModifiedDate, opt => opt.Ignore())
                .ForMember(dest => dest.Discount, opt => opt.MapFrom(src => 0m))
                .ForMember(dest => dest.NetTotal, opt => opt.MapFrom(src => src.GrandTotal))
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.SaleDetails, opt => opt.Ignore())
                .ForMember(dest => dest.Note, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedBy, opt => opt.Ignore());

            // ── SaleDetails: Entity → ViewModel ────────────────────────────
            CreateMap<SaleDetails, SaleDetailVM>()
                .ForMember(dest => dest.MedicineName,
                    opt => opt.MapFrom(src => src.Medicine != null ? src.Medicine.MedName : string.Empty))
                .ForMember(dest => dest.Gst, opt => opt.MapFrom(src => src.GstAmt))
                .ForMember(dest => dest.GstAmt, opt => opt.MapFrom(src => src.GstAmt))
                .ForMember(dest => dest.GstPct, opt => opt.MapFrom(src => src.GstPct))
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.Qty, opt => opt.MapFrom(src => src.Qty))
                .ForMember(dest => dest.Amt, opt => opt.MapFrom(src => src.Amt))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.MedId, opt => opt.MapFrom(src => src.MedId))
                .ForMember(dest => dest.SaleId, opt => opt.MapFrom(src => src.SaleId))
                .ForMember(dest => dest.SaleDetId, opt => opt.MapFrom(src => src.SaleDetId));

            // ── SaleDetails: ViewModel → Entity ────────────────────────────
            CreateMap<SaleDetailVM, SaleDetails>()
                .ForMember(dest => dest.GstAmt, opt => opt.MapFrom(src => src.GstAmt))
                .ForMember(dest => dest.GstPct, opt => opt.MapFrom(src => src.GstPct))
                .ForMember(dest => dest.SaleDetId, opt => opt.Ignore())
                .ForMember(dest => dest.SaleId, opt => opt.Ignore())
                .ForMember(dest => dest.SaleMaster, opt => opt.Ignore())
                .ForMember(dest => dest.Medicine, opt => opt.Ignore())
                .ForMember(dest => dest.Rate, opt => opt.MapFrom(src => src.Rate))
                .ForMember(dest => dest.Qty, opt => opt.MapFrom(src => src.Qty))
                .ForMember(dest => dest.Amt, opt => opt.MapFrom(src => src.Amt))
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Total))
                .ForMember(dest => dest.MedId, opt => opt.MapFrom(src => src.MedId));
        }
    }
}