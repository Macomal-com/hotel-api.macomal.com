using FluentValidation;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomRateMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int RoomTypeId { get; set; }
        public decimal RoomRate { get; set; }
        public int Gst { get; set; }
        public decimal Discount { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string GstTaxType { get; set; } = string.Empty;
        public int HourId { get; set; }
        public decimal GstAmount { get; set; }

        public int RatePriority { get; set; }
    }

    public class RoomRateDateWise : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int RoomTypeId { get; set; }
        public decimal RoomRate { get; set; }
        public int Gst { get; set; }
        public decimal Discount { get; set; }
        public DateOnly FromDate { get; set; }
        public DateOnly ToDate { get; set; }
        public string RateType { get; set; } = string.Empty;
        public int WeekendDay { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public string GstTaxType { get; set; } = string.Empty;
        public decimal GstAmount { get; set; }
        public int RatePriority { get; set; }
    }

    public class RoomRateMasterDTO
    {
        public int RoomTypeId { get; set; }
        public decimal RoomRate { get; set; }
        public int Gst { get; set; }
        public decimal Discount { get; set; }

        public DateOnly? FromDate { get; set; }
        public DateOnly? ToDate { get; set; }
        public string RateType { get; set; } = string.Empty;
        public int WeekendDay { get; set; } 
        public string GstTaxType { get; set; } = string.Empty;
        public int HourId { get; set; }
    }

    public class RoomRateValidator : AbstractValidator<RoomRateMasterDTO>
    {
        private readonly DbContextSql _context;
        public RoomRateValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.RoomTypeId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Room Type is required")
                .NotEmpty().WithMessage("Room Tyoe is required");

            RuleFor(x => x.RoomRate)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Room rate is required")
                .NotEmpty().WithMessage("Room rate is required")
                .GreaterThanOrEqualTo(0).WithMessage("Room rate should be greater than or equal to 0");

            RuleFor(x => x.RateType)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Rate Type is required")
                .NotEmpty().WithMessage("Rate Type  is required");

            //RuleFor(x=>x.GstTaxType)
            //   .NotNull().WithMessage("Gst Tax Type is required")
            //    .NotEmpty().WithMessage("Gst Tax Type  is required")
            //    .When(x=>x.Gst > 0);


            RuleFor(x => x.FromDate)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("From date is required")
                .NotEmpty().WithMessage("From date is required")
                .When(x => x.RateType == "Weekend" || x.RateType == "Custom");

            RuleFor(x => x.ToDate).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("To date is required")
                .NotEmpty().WithMessage("To date is required")
                .When(x => x.RateType == "Weekend" || x.RateType == "Custom");

            RuleFor(x => x.WeekendDay).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Weekend Day is required")
                .NotEmpty().WithMessage("Weekend Day is required")
                .When(x => x.RateType == "Weekend");

            RuleFor(x => x.HourId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Hour is required")
                .NotEmpty().WithMessage("Hour is required")
                .When(x => x.RateType == "Hour");
        }
    }
}
