using Repository.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository.DTO
{
    public class RoomServiceFormResponse
    {
        public List<GroupMaster> Groups { get; set; } = new List<GroupMaster>();

        public List<SubGroupMaster> SubGroups { get; set; } = new List<SubGroupMaster>();

        public List<ServicesDTO> Services { get; set; } = new List<ServicesDTO>();

        public string KotNumber { get; set; } = string.Empty;
    }
}
