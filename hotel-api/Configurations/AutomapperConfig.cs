using AutoMapper;
using Repository.Models;
namespace hotel_api.Configurations
{
    public class AutomapperConfig: Profile
    {
        public AutomapperConfig()
        {
            CreateMap<ClusterDTO, ClusterMaster>().ReverseMap();
        }
    }
}
