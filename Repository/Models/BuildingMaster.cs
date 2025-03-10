using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace Repository.Models
{
    public class BuildingMaster : ICommonParams
    {
        [Key]
        public int BuildingId { get; set; }
        public int PropertyId { get; set; }
        public string BuildingName { get; set; } = String.Empty;
        public string BuildingDescription { get; set; } = String.Empty;
        public int NoOfFloors { get; set; }
        public int NoOfRooms { get; set; }
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
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
}
