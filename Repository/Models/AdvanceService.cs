﻿using FluentValidation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class AdvanceService : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int GroupId { get; set; }
        public int SubGroupId { get; set; }
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public decimal ServicePrice { get; set; } //serviceprice
        public int Quantity { get; set; } 
        public string TaxType { get; set; } = String.Empty;
        public decimal TotalAmount { get; set; } //totalserviceprice * quantity
        public decimal GSTPercentage { get; set; }
        public decimal GstAmount { get; set; } //gstamount * quantity
        public decimal IGSTPercentage { get; set; }
        public decimal IgstAmount { get; set; }//0
        public decimal CGSTPercentage { get; set; } 
        public decimal CgstAmount { get; set; } //cgstamount * quanitity
        public decimal SGSTPercentage { get; set; }
        public decimal SgstAmount { get; set; } //sgstamount * quantity
        public decimal TotalServicePrice { get; set; }//inclusive and exclusive amount
        public DateTime ServiceDate { get; set; } //validation - checkin and checkoutdate
        public string ServiceTime { get; set; } = String.Empty;
        public string KotNo { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public decimal DiscountAmount { get; set; }
        public bool IsActive { get; set; }

        public string ReservationNo { get; set; } = string.Empty;
       
    }

    public class AdvanceServicesValidator : AbstractValidator<AdvanceService>
    {
        public AdvanceServicesValidator()
        {
            RuleFor(x => x.BookingId)
                .NotNull().WithMessage("Booking Id is required")
                .NotEmpty().WithMessage("Booking Id is required")
                .GreaterThan(0).WithMessage("Booking Id is required");

            RuleFor(x => x.RoomId)
                .NotNull().WithMessage("Room Id is required")
                .NotEmpty().WithMessage("Room Id is required")
                .GreaterThan(0).WithMessage("Room Id is required");

            RuleFor(x => x.GroupId)
                .NotNull().WithMessage("Group Id is required")
                .NotEmpty().WithMessage("Group Id is required")
                .GreaterThan(0).WithMessage("Group Id is required");

            RuleFor(x => x.SubGroupId)
                .NotNull().WithMessage("Sub Group Id is required")
                .NotEmpty().WithMessage("Sub Group Id is required")
                .GreaterThan(0).WithMessage("Sub Group Id is required");

            RuleFor(x => x.ServiceId)
               .NotNull().WithMessage("Service Id is required")
               .NotEmpty().WithMessage("Service Id is required")
               .GreaterThan(0).WithMessage("Service Id is required");

            RuleFor(x => x.ServiceName)
               .NotNull().WithMessage("Service Name is required")
               .NotEmpty().WithMessage("Service Name is required");

            RuleFor(x => x.ServicePrice)
               .NotNull().WithMessage("Service Price is required")
               .NotEmpty().WithMessage("Service Price is required")
               .GreaterThanOrEqualTo(0).WithMessage("Service Price is required");

            RuleFor(x => x.Quantity)
               .NotNull().WithMessage("Quantity is required")
               .NotEmpty().WithMessage("Quantity is required")
               .GreaterThan(0).WithMessage("Quantity is required");

            RuleFor(x => x.ServiceDate)
               .NotNull().WithMessage("Service Date is required")
               .NotEmpty().WithMessage("Service Date is required")
               ;

            RuleFor(x => x.ServiceTime)
               .NotNull().WithMessage("Service Time is required")
               .NotEmpty().WithMessage("Service Time is required")
               ;

            RuleFor(x => x.TaxType)
               .NotNull().WithMessage("Tax Type is required")
               .NotEmpty().WithMessage("Tax Type is required")
               ;

            RuleFor(x => x.TotalServicePrice)
               .NotNull().WithMessage("TotalServicePrice is required")
               .NotEmpty().WithMessage("TotalServicePrice is required")
               .GreaterThanOrEqualTo(0).WithMessage("TotalServicePrice is required");

            RuleFor(x => x.TotalAmount)
               .NotNull().WithMessage("TotalAmount is required")
               .NotEmpty().WithMessage("TotalAmount is required")
               .GreaterThanOrEqualTo(0).WithMessage("TotalAmount is required");

            RuleFor(x => x.GSTPercentage)
              .NotNull().WithMessage("GST is required")
              .NotEmpty().WithMessage("GST is required")
              .GreaterThanOrEqualTo(0).WithMessage("GST is required");

            RuleFor(x => x.GstAmount)
              .NotNull().WithMessage("GstAmount is required")
              .NotEmpty().WithMessage("GstAmount is required")
              .GreaterThanOrEqualTo(0).WithMessage("GstAmount is required");

        }
    }
}
