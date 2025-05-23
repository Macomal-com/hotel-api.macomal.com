using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class ShiftExtentRequest
    {
        public int BookingId { get; set; }
        public int RoomId { get; set; }
        public string RoomNo { get; set; } = string.Empty;
        public string ReservationNo { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;

        public int ShiftRoomId { get; set; }
        public string ShiftRoomNo { get; set; } = string.Empty;

        public int ShiftRoomTypeId { get; set; } 
        public DateOnly ShiftDate { get; set; } = new DateOnly(1900, 01, 01);

        public DateOnly ExtendedDate { get; set; } = new DateOnly(1900, 01, 01);

        public int ExtendHour { get; set; }
    }

    public class ShiftExtentRequestValidator : AbstractValidator<ShiftExtentRequest>
    {
        public ShiftExtentRequestValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Type is required")
                .NotNull().WithMessage("Type is required");

            RuleFor(x => x.BookingId)
                .NotNull().WithMessage("Booking Id is required")
                .NotEmpty().WithMessage("Booking Id is required")
                .GreaterThan(0).WithMessage("Booking Id is required");

            RuleFor(x => x.RoomId)
                .NotNull().WithMessage("Room Id is required")
                .NotEmpty().WithMessage("Room Id is required")
                .GreaterThan(0).WithMessage("Room Id is required");

            RuleFor(x => x.RoomNo)
                .NotNull().WithMessage("Room No is required")
                .NotEmpty().WithMessage("Room No is required");

            RuleFor(x => x.ReservationNo)
               .NotNull().WithMessage("Reservation No is required")
               .NotEmpty().WithMessage("Reservation No is required");

            RuleFor(x => x.ShiftRoomId)
               .NotNull().WithMessage("Shift Room Id is required")
               .NotEmpty().WithMessage("Shift Room Id is required")
               .GreaterThan(0).WithMessage("Shift Room Id is required")
               .When(x=>x.Type == "Shift");

            RuleFor(x => x.ShiftRoomId)
               .NotNull().WithMessage("Shift Room Id is required")
               .NotEmpty().WithMessage("Shift Room Id is required")
               .GreaterThan(0).WithMessage("Shift Room Id is required")
               .When(x => x.Type == "Shift");

            RuleFor(x => x.ShiftRoomNo)
              .NotNull().WithMessage("Shift Room No is required")
              .NotEmpty().WithMessage("Shift Room No is required")
              .When(x => x.Type == "Shift");

            RuleFor(x => x.ShiftRoomTypeId)
              .NotNull().WithMessage("Shift Room Type is required")
              .NotEmpty().WithMessage("Shift Room Type is required")
              .When(x => x.Type == "Shift");

            RuleFor(x => x.ShiftDate)
              .NotNull().WithMessage("Shift Date is required")
              .NotEmpty().WithMessage("Shift Date is required")
              .When(x => x.Type == "Shift");

            RuleFor(x => x.ExtendedDate)
             .NotNull().WithMessage("Extended Date is required")
             .NotEmpty().WithMessage("Extended Date is required")
             .When(x => x.Type == "Extend");
        }
    }
}
