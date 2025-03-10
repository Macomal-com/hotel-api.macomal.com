using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class ServicableMaster : ICommonParams
    {
        [Key]
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
        public bool IsActive { get; set; }
        public string CreatedDate { get; set; } = String.Empty;
        public string UpdatedDate { get; set; } = String.Empty;
        public int UserId { get; set; }
        public int CompanyId { get; set; }
    }

    public class ServicableMasterDTO
    {
        public string ServiceName { get; set; } = String.Empty;
        public string ServiceDescription { get; set; } = String.Empty;
        }
}
