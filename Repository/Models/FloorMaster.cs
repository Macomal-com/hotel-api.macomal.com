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
    public class FloorMaster : ICommonProperties
    {
        [Key]
        public int FloorId { get; set; }
        public string FloorNumber { get; set; } = string.Empty;
        public int? BuildingId { get; set; }
        public int PropertyId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
        public int NoOfRooms { get; set; }
    }
    public class FloorMasterDTO
    {
        [Key]
        public string FloorNumber { get; set; }
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public int NoOfRooms { get; set; }
    }

    public class FloorValidator : AbstractValidator<FloorMaster>
    {
        public readonly DbContextSql _context;
        public FloorValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.FloorNumber)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Floor Number is required")
                .NotEmpty().WithMessage("Floor Number is required");

            //RuleFor(x => x.NoOfRooms)
            //    .NotNull().WithMessage("No of Rooms is required")
            //    .NotEmpty().WithMessage("No of Rooms is required");

            RuleFor(x=>x.PropertyId)
                .Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Property is required")
                .NotEmpty().WithMessage("Property is required")
                .GreaterThan(0).WithMessage("Property is required");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsFloorNumberExists)   
                .When(x=>x.FloorId == 0)
                .WithMessage("Floor Number already exists");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
              .MustAsync(IsFloorUpdateNumberExists)
              .When(x => x.FloorId > 0)
              .WithMessage("Floor Number already exists");

            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(IsCountExceed)
                .When(x=>x.FloorId == 0)
                .WithMessage("You have already created total floors in this building.");

        }

        private async Task<bool> IsFloorNumberExists(FloorMaster floorMaster, CancellationToken cancellationToken)
        {
            if(floorMaster.BuildingId == 0)
            {
                return !await _context.FloorMaster.AnyAsync(x => x.FloorNumber == floorMaster.FloorNumber && x.PropertyId == floorMaster.PropertyId && x.IsActive == true, cancellationToken);
            }
            else
            {
                return !await _context.FloorMaster.AnyAsync(x => x.FloorNumber == floorMaster.FloorNumber && x.PropertyId == floorMaster.PropertyId && x.BuildingId == floorMaster.BuildingId && x.IsActive == true, cancellationToken);
            }
            
        }

        private async Task<bool> IsFloorUpdateNumberExists(FloorMaster floorMaster, CancellationToken cancellationToken)
        {
            if (floorMaster.BuildingId == 0)
            {
                return !await _context.FloorMaster.AnyAsync(x => x.FloorNumber == floorMaster.FloorNumber && x.PropertyId == floorMaster.PropertyId && x.FloorId!= floorMaster.FloorId && x.IsActive == true, cancellationToken);
            }
            else
            {
                return !await _context.FloorMaster.AnyAsync(x => x.FloorNumber == floorMaster.FloorNumber && x.PropertyId == floorMaster.PropertyId && x.BuildingId == floorMaster.BuildingId && x.FloorId != floorMaster.FloorId && x.IsActive == true, cancellationToken);
            }

        }

        private async Task<bool> IsCountExceed(FloorMaster floorMaster, CancellationToken cancellationToken)
        {
            if (floorMaster.BuildingId > 0)
            {
                var buildingMaster = await _context.BuildingMaster.FirstOrDefaultAsync(x => x.BuildingId == floorMaster.BuildingId && x.IsActive == true);

                if (buildingMaster == null)
                {
                    return false;
                }
                else
                {
                    if (buildingMaster.NoOfFloors == 0)
                    {
                        return true;
                    }
                    else
                    {
                        var alreadyCreatedCount = await _context.FloorMaster.CountAsync(x => x.BuildingId == floorMaster.BuildingId && x.IsActive == true);
                        if (buildingMaster.NoOfFloors == alreadyCreatedCount)
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
            else
            {
                return true;
            }

        }
    }
    public class FloorDeleteValidator : AbstractValidator<FloorMaster>
    {
        private readonly DbContextSql _context;
        public FloorDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoFloorExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this floor, room already exists!");

        }
        private async Task<bool> DoFloorExists(FloorMaster floor, CancellationToken cancellationToken)
        {
            return !await _context.RoomMaster.Where(x => x.IsActive == true && x.CompanyId == floor.CompanyId && x.FloorId == floor.FloorId).AnyAsync();

        }

    }
}
