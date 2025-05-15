using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using FluentValidation;
using RepositoryModels.Repository;
using Microsoft.EntityFrameworkCore;

namespace Repository.Models
{
    public class CompanyDetails : ICommonProperties
    {
        [Key]
        public int PropertyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        public string HotelTagline { get; set; } = string.Empty;
        public string PropertyLogo { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? PropertyLogoPath { get; set; }
        public string Website { get; set; } = string.Empty;
        public string PropertyType { get; set; } = string.Empty;
        public string ContactNo1 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PanNo { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public string CheckInTime { get; set; } = string.Empty;
        public string CheckOutTime { get; set; } = string.Empty;
        public int ClusterId { get; set; }
        public int OwnerId { get; set; }
        public string Taxtype { get; set; } = string.Empty;
        public string CheckedFormat { get; set; } = string.Empty;
        public string RoomRateEditTable { get; set; } = string.Empty;
        public string RateTaxtype { get; set; } = string.Empty;
        public string ElectricityDueDate { get; set; } = string.Empty;
        public string WaterDueDate { get; set; } = string.Empty;
        public string WifiDueDate { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }

        public int CompanyId { get; set; }

        public string FaxNo { get; set; } = string.Empty;
        public string BussinessType { get; set; } = string.Empty;
        public string ContactNo2 { get; set; } = string.Empty;
        public string RegistrationNo { get; set; } = string.Empty;
        public string EconomicActCode { get; set; } = string.Empty;
        public string AuditorsDetails { get; set; } = string.Empty;
        public string AuditSignatoryName { get; set; } = string.Empty;
        public string AuditorName { get; set; } = string.Empty;
        public string ManagerName { get; set; } = string.Empty;
        public string ManagerPhone { get; set; } = string.Empty;
        public string ManagerEmail { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public string OwnerPhone { get; set; } = string.Empty;
        public string OwnerEmail { get; set; } = string.Empty;        
        public string AuditorPlace { get; set; } = string.Empty;
        public string AuditorCity { get; set; } = string.Empty;
        public string TypeofClient { get; set; } = string.Empty;
        public string DatabaseName { get; set; } = string.Empty;
        public string TrialStatus { get; set; } = string.Empty;
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public string BranchStatus { get; set; } = string.Empty;
        public int RType { get; set; }
        public int Inventory { get; set; }
        public string HotelId { get; set; } = string.Empty;
        public int TotalAmount { get; set; }
        public int GstRate { get; set; }
        public string IsAccount { get; set; } = string.Empty;
        public string regno { get; set; } = string.Empty;        
        public string Image_Content { get; set; } = string.Empty;
        public string statecode { get; set; } = string.Empty;
        public string closingtime { get; set; } = string.Empty;
        public string ChannelmanagerID { get; set; } = string.Empty;
        public string EditInvoiceStatus { get; set; } = string.Empty;
        public string AvailablityStatus { get; set; } = string.Empty;
        public string Declarerate { get; set; } = string.Empty;
        public int Highbalance { get; set; }
        public int Sms_Limit { get; set; }
        public int Sms_Send { get; set; }
        public string RateType { get; set; } = string.Empty;
        public string checkinhr { get; set; } = string.Empty;
        public double KOTDiscount { get; set; }
        public string SaleInclude { get; set; } = string.Empty;
        public string latitude { get; set; } = string.Empty;
        public string longitude { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
        public string BankName { get; set; } = string.Empty;
        public string AccountNo { get; set; } = string.Empty;
        public string IFSC { get; set; } = string.Empty;
        public string Scode { get; set; } = string.Empty;
        public int ExtraHours { get; set; }
        public string TextType { get; set; } = string.Empty;
        public string SameDayhrs { get; set; } = string.Empty;
        public string WhatsAppSt { get; set; } = string.Empty;
        public string ShowInOutTime { get; set; } = string.Empty;
        public string Whatsapp_api { get; set; } = string.Empty;
        public string Whatsappapi_checkin { get; set; } = string.Empty;
        public string Channel_Manager { get; set; } = string.Empty;
        public string Allow_chkout_withamt { get; set; } = string.Empty;
        public string Banquet_db { get; set; } = string.Empty;
        public int Other1 { get; set; }
        public int Other2 { get; set; }
        public string IsCheckOutApplicable { get; set; } = string.Empty;
        public string CheckOutFormat { get; set; } = string.Empty;
        public string IsRoomRateEditable { get; set; } = string.Empty;
        public string GstType { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? watermarkfile { get; set; }
        public bool ApproveReservation { get; set; }

        public string CancelMethod { get; set; } = string.Empty;
        public string CancelCalculatedBy { get; set; } = string.Empty;
        public string CheckOutInvoice { get; set; } = string.Empty;
        public bool IsWhatsappNotification { get; set; }
        public bool IsEmailNotification { get; set; }

        public bool IsEarlyCheckInPolicyEnable { get; set; }
        public bool IsLateCheckOutPolicyEnable { get; set; }

        public bool IsDefaultCheckInTimeApplicable { get; set; }

        public bool IsDefaultCheckOutTimeApplicable { get; set; }
        public bool ReservationNotification { get; set; }
        public bool CheckinNotification { get; set; }
        public bool RoomShiftNotification { get; set; }
        public bool CheckOutNotification { get; set; }
        public bool CancelBookingNotification { get; set; }
        public string CalculateRoomRates { get; set; } = String.Empty;
        public bool CheckoutWithBalance { get; set; }
    }
    public class CompanyDetailsDTO
    {
        [Key]
        public int PropertyId { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string Pincode { get; set; } = string.Empty;
        
        public string HotelTagline { get; set; } = string.Empty;
        public string ContactNo1 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PanNo { get; set; } = string.Empty;
        public string Gstin { get; set; } = string.Empty;
        public int ClusterId { get; set; }
        public int OwnerId { get; set; }
        public string IsCheckOutApplicable { get; set; } = string.Empty;
        public string CheckOutFormat { get; set; } = string.Empty;
        public string IsRoomRateEditable { get; set; } = string.Empty;
        public string GstType { get; set; } = string.Empty;
        public bool ApproveReservation { get; set; }
        public string CheckOutInvoice { get; set; } = string.Empty;
        public bool IsWhatsappNotification { get; set; }
        public bool IsEmailNotification { get; set; }
        public bool ReservationNotification { get; set; }
        public bool CheckinNotification { get; set; }
        public bool RoomShiftNotification { get; set; }
        public bool CheckOutNotification { get; set; }
        public bool CancelBookingNotification { get; set; }
        public string CalculateRoomRates { get; set; } = String.Empty;
        public bool CheckoutWithBalance { get; set; }
    }
    public class PropertyDetailsValidator : AbstractValidator<CompanyDetails>
    {
        private readonly DbContextSql _context;
        public PropertyDetailsValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.CompanyName)
                .NotEmpty().WithMessage("Property Name is required")
                .NotNull().WithMessage("Property Name is required");

            RuleFor(x => x.ClusterId)
               .NotEmpty().WithMessage("Cluster is required")
               .NotNull().WithMessage("Cluster is required");

            //RuleFor(x => x.OwnerId)
            //   .NotEmpty().WithMessage("Landlord is required")
            //   .NotNull().WithMessage("Landlord is required");

            RuleFor(x => x.Gstin)
               .NotEmpty().WithMessage("GSTIN is required")
               .NotNull().WithMessage("GSTIN is required")               
               .Length(15)
               .WithMessage("GST No. length should be 15 numbers")
               .When(x => x.Gstin != "");

            RuleFor(x => x.State)
               .NotEmpty().WithMessage("State is required")
               .NotNull().WithMessage("State is required");

            RuleFor(x => x.City)
               .NotEmpty().WithMessage("City is required")
               .NotNull().WithMessage("City is required");

            RuleFor(x => x.ContactNo1)
               .NotEmpty().WithMessage("Contanct No is required")
               .NotNull().WithMessage("Contanct No is required")
               .Length(10)
                   .WithMessage("Contact No length should be 10 digits");
            RuleFor(x => x)
                .MustAsync(IsUniqueProperty)
                .When(x => x.PropertyId == 0)
                .WithMessage("Property already exists");

            RuleFor(x => x)
                .MustAsync(IsUniqueUpdateProperty)
                .When(x => x.PropertyId > 0)
                .WithMessage("Property already exists");

        }
        private async Task<bool> IsUniqueProperty(CompanyDetails cm, CancellationToken cancellationToken)
        {
            if(cm.Gstin == "")
            {
                var data = await _context.CompanyDetails.AnyAsync(x => x.CompanyName == cm.CompanyName && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);

                return !await _context.CompanyDetails.AnyAsync(x => x.CompanyName == cm.CompanyName && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
            }
            else { 

                var data = await _context.CompanyDetails.AnyAsync(x => x.Gstin == cm.Gstin && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
            return !await _context.CompanyDetails.AnyAsync(x => x.Gstin == cm.Gstin && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
            }
        }


        private async Task<bool> IsUniqueUpdateProperty(CompanyDetails cm, CancellationToken cancellationToken)
        {
            if (cm.Gstin != "")
            {
                return !await _context.CompanyDetails.AnyAsync(x => x.CompanyName == cm.CompanyName && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
            }
            else
            {
                return !await _context.CompanyDetails.AnyAsync(x => x.Gstin == cm.Gstin && x.IsActive == true && x.CompanyId == cm.CompanyId, cancellationToken);
            }
        }
    }
}
