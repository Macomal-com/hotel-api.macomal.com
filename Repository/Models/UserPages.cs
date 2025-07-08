using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class UserPages
    {
        [Key]
        public int PageId { get; set; }

        public string PageName { get; set; } = string.Empty;

        public string PageRoute { get; set; } = string.Empty;


        public string PageAliasName { get; set; } = string.Empty;


        public string PageIcon { get; set; } = string.Empty;


        public bool IsParent { get; set; }

        public string PageParent { get; set; } = string.Empty;


        public string ProcedureName { get; set; } = string.Empty;


        public bool Other1 { get; set; }

        public int PageOrder { get; set; }

        public bool ContainsChild { get; set; }
    }
}
