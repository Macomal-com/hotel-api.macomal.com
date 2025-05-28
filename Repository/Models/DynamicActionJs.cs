using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.Models
{
    public class DynamicActionJs
    {
        public int Id { get; set; }
        public string ActionName { get; set; } = string.Empty;
        public string ActionJs { get; set; } = string.Empty;

        public string ReportName { get; set; } = string.Empty;
    }
}
