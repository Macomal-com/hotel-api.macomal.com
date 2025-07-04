
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserPropertyMapping : ICommonProperties
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PropertyId { get; set; }
        public int ClusterId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } 
        public DateTime UpdatedDate { get; set; }
        public bool IsActive { get; set; }

        public bool AllProperties { get; set; }
        [NotMapped]
        public int CompanyId { get ; set; }
    }
}
