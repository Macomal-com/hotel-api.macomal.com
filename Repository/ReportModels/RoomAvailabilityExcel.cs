using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.ReportModels
{
    public class RoomAvailabilityExcel
    {
        public string StartDate { get; set; } = string.Empty;
        public string EndDate { get; set; } = string.Empty;


        public Dictionary<int, bool> ClusterIds { get; set; } = new Dictionary<int, bool>();
    }

    
}
