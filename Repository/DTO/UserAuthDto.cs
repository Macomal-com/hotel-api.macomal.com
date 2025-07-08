using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class UserAuthDto
    {
        public int PageId { get; set; }
        public string PageName { get; set; } = string.Empty;
        public bool IsAuth { get; set; } 
        public bool IsParent { get; set; }
    }
}
