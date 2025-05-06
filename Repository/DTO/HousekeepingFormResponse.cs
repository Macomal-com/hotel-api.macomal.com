using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class HousekeepingFormResponse
    {
        //public List<ServicesStatus> AllServicesStatus { get; set; } = new List<ServicesStatus>();

        public List<string> AllServicesStatus { get; set; } = new List<string>();
        public List<object> ServicesStatus { get; set; } = new List<object>();
        public List<RoomMaster> RoomMaster { get; set; }
        = new List<RoomMaster>();

        public List<RoomCategoryMaster> RoomCategoryMaster { get; set; } = new List<RoomCategoryMaster>();

        public List<StaffManagementMaster> Staff { get; set; } = new List<StaffManagementMaster>();
    }
}
