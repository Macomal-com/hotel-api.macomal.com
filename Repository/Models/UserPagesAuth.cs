using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserPagesAuth
    {
        [Key]
        public int Id { get; set; }

        public int PageId { get; set; }

        public int UserId { get; set; }

        public int CompanyId { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedDate { get; set; }

        public int CreatedBy { get; set; }
    }
}
