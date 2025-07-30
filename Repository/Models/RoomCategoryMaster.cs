using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomCategoryMaster : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int? BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int DefaultPax { get; set; }
        public int ExtraBed { get; set; }

        public string ColorCode { get; set; } = string.Empty;
        [NotMapped]
        public bool ChangeDetails { get; set; }

    }
    public class RoomCategoryMasterDTO
    {
        [Key]
        public string Type { get; set; } = String.Empty;
        public string Description { get; set; } = String.Empty;
        public int MinPax { get; set; }
        public int MaxPax { get; set; }
        public int BedTypeId { get; set; }
        public int NoOfRooms { get; set; }
        public string PlanDetails { get; set; } = String.Empty;
        public int DefaultPax { get; set; }
        public int ExtraBed { get; set; }

    }

    public class RoomCategoryValidator : AbstractValidator<RoomCategoryMaster>
    {
        private readonly DbContextSql _context;
        public RoomCategoryValidator(DbContextSql context)
        {
            _context = context;


            RuleFor(x => x.Type)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Room Type is required")
                .NotNull().WithMessage("Room Type is required");

            RuleFor(x => x.MinPax).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Min Pax is required")
               .NotNull().WithMessage("Min Pax is required")
               .GreaterThan(0).WithMessage("Min Pax should be greater than 0")
               .LessThan(x=>x.MaxPax).WithMessage("Min Pax should be less than max pax");

            RuleFor(x => x.MaxPax).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("Max Pax is required")
               .NotNull().WithMessage("Max Pax is required")
               .GreaterThan(0).WithMessage("Max Pax should be greater than 0")
               ;

            RuleFor(x => x.NoOfRooms).Cascade(CascadeMode.Stop)
               .NotEmpty().WithMessage("No of Rooms is required")
               .NotNull().WithMessage("No of Rooms is required")
               .GreaterThan(0).WithMessage("No of Rooms should be greater than 0")
               ;

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(isUniqueCategory)
                .When(x => x.Id == 0)
                .WithMessage("Room Type already exists with same name");
            RuleFor(x => x).Cascade(CascadeMode.Stop)
               .MustAsync(isUniqueUpdateCategory)
               .When(x => x.Id > 0)
               .WithMessage("Room Type already exists with same name");

        }

        private async Task<bool> isUniqueCategory(RoomCategoryMaster master, CancellationToken cancellationToken)
        {
           
                return !await _context.RoomCategoryMaster.AnyAsync(x => x.Type == master.Type && x.CompanyId == master.CompanyId  &&  x.IsActive == true, cancellationToken);
           

        }

        private async Task<bool> isUniqueUpdateCategory(RoomCategoryMaster master, CancellationToken cancellationToken)
        {

            return !await _context.RoomCategoryMaster.AnyAsync(x => x.Type == master.Type && x.CompanyId == master.CompanyId && x.IsActive == true && x.Id != master.Id, cancellationToken);


        }
    }

    public class RoomCategoryDeleteValidator : AbstractValidator<RoomCategoryMaster>
    {
        private readonly DbContextSql _context;
        public RoomCategoryDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(DoRoomCategoryExists)
                .WithMessage("You can't delete this Room Type, Room already exists!");
        }
        private async Task<bool> DoRoomCategoryExists(RoomCategoryMaster categotyMaster, CancellationToken cancellationToken)
        {
            return !await _context.RoomMaster.Where(x => x.IsActive == true && x.CompanyId == categotyMaster.CompanyId && x.RoomTypeId == categotyMaster.Id).AnyAsync();

        }
    }
}
