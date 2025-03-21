using FluentValidation;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomMaster : ICommonProperties
    {
        [Key]
        public int RoomId { get; set; }
        public int? FloorId { get; set; }
        public int? BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int RoomNo { get; set; }
        public int RoomTypeId { get; set; }
        public string Description { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }
    public class RoomMasterDTO
    {
        [Key]
        public int FloorId { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int RoomNo { get; set; }
        public int RoomTypeId { get; set; }
        public string Description { get; set; } = String.Empty;
    }

    public class RoomMasterValidator : AbstractValidator<RoomMaster>
    {
        private readonly DbContextSql _context;
        public RoomMasterValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.RoomNo)
                .NotNull().WithMessage("Room No is required")
                .NotEmpty().WithMessage("Room No is required");

            RuleFor(x => x.RoomTypeId)
                .NotEmpty().WithMessage("Room Type Id is required")
                .NotNull().WithMessage("Room Type Id is required")
                .GreaterThan(0).WithMessage("Room Type id is required");

            RuleFor(x => x.PropertyId)
                .NotEmpty().WithMessage("Property Id is required")
                .NotNull().WithMessage("Property Id is required")
                .GreaterThan(0).WithMessage("Property id is required");

            RuleFor(x => x)
                .MustAsync(IsUniqueRoomNo)
                .WithMessage("Room No already exists");

            RuleFor(x => x)
                .MustAsync(IsRoomCategoryCountExceed)
                .WithMessage("You have already created total floors in this room type.");

            RuleFor(x => x)
                .MustAsync(IsFloorCountExceed)
                .When(x=>x.FloorId > 0)
                .WithMessage("You have already created total floors in this room type.");

        }

        private async Task<bool> IsUniqueRoomNo(RoomMaster master, CancellationToken cancellationToken)
        {
            if(master.BuildingId > 0)
            {
                return !await _context.RoomMaster.AnyAsync(x => x.RoomNo == master.RoomNo && x.PropertyId == master.PropertyId && x.IsActive == true && x.BuildingId == master.BuildingId, cancellationToken);
            }
            else
            {
                return !await _context.RoomMaster.AnyAsync(x => x.RoomNo == master.RoomNo && x.PropertyId == master.PropertyId && x.IsActive == true , cancellationToken);
            }
                


        }

        private async Task<bool> IsRoomCategoryCountExceed(RoomMaster master, CancellationToken cancellationToken)
        {
            var alreadyCreatedCount = await _context.RoomMaster.CountAsync(x => x.RoomTypeId == master.RoomTypeId && x.PropertyId == master.PropertyId && x.IsActive == true);

            var roomcategory = await _context.RoomCategoryMaster.FirstOrDefaultAsync(x => x.Id == master.RoomId && x.IsActive == true);

           if(roomcategory == null)
            {
                return false;
            }

           if(roomcategory.NoOfRooms == alreadyCreatedCount)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

        private async Task<bool> IsFloorCountExceed(RoomMaster master, CancellationToken cancellationToken)
        {
            var alreadyCreatedCount = await _context.RoomMaster.CountAsync(x => x.RoomTypeId == master.RoomTypeId && x.PropertyId == master.PropertyId && x.IsActive == true);

            var floorMaster = await _context.FloorMaster.FirstOrDefaultAsync(x => x.FloorId == master.FloorId && x.IsActive == true);

            if (floorMaster == null)
            {
                return false;
            }

            if (floorMaster.NoOfRooms == alreadyCreatedCount)
            {
                return false;
            }
            else
            {
                return true;
            }

        }

    }
}
