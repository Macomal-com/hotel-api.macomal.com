using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.RequestDTO
{
    public class HousekeepingRequestDTO
    {
        

        public string Remarks { get; set; } = string.Empty;
        public int ServiceBy { get; set; }

        public List<HouseKeepingRooms> HouseKeepingRooms = new List<HouseKeepingRooms>();
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string ServiceStatus { get; set; } = string.Empty;
    }

    public class HouseKeepingRooms
    {
        public int RoomAvailaibilityId { get; set; }
        public int RoomId { get; set; }
        public int RoomTypeId { get; set; }

        public string RoomNo { get; set; } = string.Empty;
        public string RoomStatus { get; set; } = string.Empty;
    }
}
