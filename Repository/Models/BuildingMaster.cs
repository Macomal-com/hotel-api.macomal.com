using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RepositoryModels.Repository;

namespace Repository.Models
{
    public class BuildingMaster : ICommonProperties
    {
        [Key]
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public string BuildingName { get; set; } = String.Empty;
        public string BuildingDescription { get; set; } = String.Empty;
        public int NoOfFloors { get; set; }
        public int NoOfRooms { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime UpdatedDate { get; set; } = DateTime.Now;
        public string CreatedBy { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }

        [NotMapped]
        public IFormFile? BuildingImages { get; set; }
        public string BuildingImagesPath { get; set; } = String.Empty;
        public string Facilities { get; set; } = String.Empty;

    }
    public class BuildingMasterDTO
    {
        [Key]
        public int PropertyId { get; set; }
        public string BuildingName { get; set; } = String.Empty;
        public string BuildingDescription { get; set; } = String.Empty;
        public int NoOfFloors { get; set; }
        public int NoOfRooms { get; set; }

        [NotMapped]
        public IFormFile? BuildingImages { get; set; }
        public string BuildingImagesPath { get; set; } = String.Empty;
        public string Facilities { get; set; } = String.Empty;

    }

    public class BuildingMasterValidator : AbstractValidator<BuildingMaster>
    {
        private DbContextSql _context;
        public BuildingMasterValidator(DbContextSql context)
        {
            _context = context;

            RuleFor(x => x.BuildingName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Building Name is required")
                .NotNull().WithMessage("Building Name is required");

            RuleFor(x => x.PropertyId).Cascade(CascadeMode.Stop)
                .NotNull().WithMessage("Property is required")
                .NotEmpty().WithMessage("Property is required")
                .GreaterThan(0).WithMessage("Property is required");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueBuildingName)
                .When(x => x.BuildingId == 0)
                .WithMessage("Building Name must be unique");

            RuleFor(x => x).Cascade(CascadeMode.Stop)
                .MustAsync(IsUniqueUpdateBuildingName)
                .When(x => x.BuildingId > 0)
                .WithMessage("Building Name must be unique");


        }

        private async Task<bool> IsUniqueBuildingName(BuildingMaster master, CancellationToken cancellationToken)
        {

            return !await _context.BuildingMaster.AnyAsync(x => x.BuildingName == master.BuildingName && x.PropertyId == master.PropertyId && x.IsActive == true, cancellationToken);


        }

        private async Task<bool> IsUniqueUpdateBuildingName(BuildingMaster master, CancellationToken cancellationToken)
        {

            return !await _context.BuildingMaster.AnyAsync(x => x.BuildingName == master.BuildingName && x.BuildingId != master.BuildingId && x.PropertyId == master.PropertyId && x.IsActive == true, cancellationToken);


        }

        
    }
    public class BuildingDeleteValidator : AbstractValidator<BuildingMaster>
    {
        private readonly DbContextSql _context;
        public BuildingDeleteValidator(DbContextSql context)
        {
            _context = context;
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoBuildingExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Building, Floor already exists!");
            RuleFor(x => x)
                .Cascade(CascadeMode.Stop)
                .MustAsync(DoBuildingInRoomExists)
                .When(x => x.IsActive == false)
                .WithMessage("You can't delete this Building, Room already exists!");
        }
        private async Task<bool> DoBuildingExists(BuildingMaster building, CancellationToken cancellationToken)
        {
            return !await _context.FloorMaster.Where(x => x.IsActive == true && x.CompanyId == building.CompanyId && x.BuildingId == building.BuildingId).AnyAsync();

        }

        private async Task<bool> DoBuildingInRoomExists(BuildingMaster building, CancellationToken cancellationToken)
        {
            return !await _context.RoomMaster.Where(x => x.IsActive == true && x.CompanyId == building.CompanyId && x.BuildingId == building.BuildingId).AnyAsync();

        }
    }
}
