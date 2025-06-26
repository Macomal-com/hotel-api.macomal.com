using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class RoomAssetMapping
    {
        [Key]
        public int Id { get; set; }
        public int RoomId { get; set; }
        public int AssetId { get; set; }
        public int Quantity { get; set; }
        public string AssetOwner { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime UpdatedDate { get; set; }
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class MappingDTO
    {
        public int Id { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string AssetOwner { get; set; } = string.Empty;
        public string? CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
    public class RoomAssetMappingDTO
    {
        public int RoomId { get; set; }
        public string RoomNo { get; set; } = string.Empty;
        public List<MappingDTO> AssetData { get; set; } = new List<MappingDTO>();
    }
}
