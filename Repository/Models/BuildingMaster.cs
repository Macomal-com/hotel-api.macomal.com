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
                .NotEmpty().WithMessage("Building Name is required")
                .NotNull().WithMessage("Building Name is required");

            RuleFor(x => x.PropertyId)
                .NotNull().WithMessage("Property is required")
                .NotEmpty().WithMessage("Property is required")
                .GreaterThan(0).WithMessage("Property is required");

            RuleFor(x => x)
                .MustAsync(IsUniqueBuildingName)
                .When(x => x.BuildingId == 0)
                .WithMessage("Building Name must be unique");

            RuleFor(x => x)
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
}
