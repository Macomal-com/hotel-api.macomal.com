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
        public int FloorNumber { get; set; }
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
        public int FloorNumber { get; set; }
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
                .NotNull().WithMessage("Floor Number is required")
                .NotEmpty().WithMessage("Floor Number is required");

            RuleFor(x => x.NoOfRooms)
                .NotNull().WithMessage("No of Rooms is required")
                .NotEmpty().WithMessage("No of Rooms is required");

            RuleFor(x=>x.PropertyId)
                .NotNull().WithMessage("Property is required")
                .NotEmpty().WithMessage("Property is required")
                .GreaterThan(0).WithMessage("Property is required");

            RuleFor(x => x)
                .MustAsync(IsFloorNumberExists)   
                .When(x=>x.FloorId == 0)
                .WithMessage("Floor Number already exists");

            RuleFor(x => x)
              .MustAsync(IsFloorUpdateNumberExists)
              .When(x => x.FloorId > 0)
              .WithMessage("Floor Number already exists");

            RuleFor(x => x)
                .MustAsync(IsCountExceed)
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
                var totalFloorCount = await _context.BuildingMaster.CountAsync(x=>x.BuildingId == floorMaster.BuildingId);

                if(totalFloorCount == 0)
                {
                    return true;
                }
                var alreadyCreatedCount = await _context.FloorMaster.CountAsync(x => x.BuildingId == floorMaster.BuildingId);

                if (totalFloorCount == alreadyCreatedCount)
                    return false;
                else
                    return true;

            }
            else
            {
                return true;
            }

        }
    }
}
