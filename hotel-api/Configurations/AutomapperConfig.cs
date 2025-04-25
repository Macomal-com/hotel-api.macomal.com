using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Repository.Models;
namespace hotel_api.Configurations
{
    public class AutomapperConfig : Profile
    {
        public AutomapperConfig()
        {
            CreateMap<ClusterDTO, ClusterMaster>().ReverseMap();
            CreateMap<CompanyDetailsDTO, CompanyDetails>().ReverseMap();
            CreateMap<BedTypeMasterDTO, BedTypeMaster>().ReverseMap();
            CreateMap<BuildingMasterDTO, BuildingMaster>().ReverseMap();
            CreateMap<FloorMasterDTO, FloorMaster>().ReverseMap();
            CreateMap<LandlordDetailsDTO, LandlordDetails>().ReverseMap();
            CreateMap<OwnerMasterDTO, OwnerMaster>().ReverseMap();
            CreateMap<RoomCategoryMasterDTO, RoomCategoryMaster>().ReverseMap();
            CreateMap<RoomMasterDTO, RoomMaster>().ReverseMap();
            CreateMap<RoomRateMasterDTO, RoomRateMaster>().ReverseMap();
            CreateMap<ServicableMasterDTO, ServicableMaster>().ReverseMap();
            CreateMap<VendorMasterDTO, VendorMaster>().ReverseMap();
            CreateMap<UserDetailsDTO, UserDetails>().ReverseMap();
            CreateMap<StaffManagementMasterDTO, StaffManagementMaster>().ReverseMap();
            CreateMap<PaymentModeDTO, PaymentMode>().ReverseMap();
            CreateMap<GroupMasterDTO, GroupMaster>().ReverseMap();
            CreateMap<SubGroupMasterDTO, SubGroupMaster>().ReverseMap();
            CreateMap<VendorServiceMasterDTO, VendorServiceMaster>().ReverseMap();
            CreateMap<GstMasterDTO, GstMaster>().ReverseMap();
            CreateMap<CommissionMasterDTO, CommissionMaster>().ReverseMap();
            CreateMap<HourMasterDTO, HourMaster>().ReverseMap();
            CreateMap<ExtraPoliciesDTO, ExtraPolicies>().ReverseMap();

            CreateMap<DocumentMasterDTO, DocumentMaster>()
                .ForMember(dest => dest.Prefix1, opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.Prefix1) ? "0" : src.Prefix1))
                .ForMember(dest => dest.Prefix2, opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.Prefix2) ? "0" : src.Prefix2))
                .ForMember(dest => dest.Suffix, opt => opt.MapFrom(src =>
                            string.IsNullOrWhiteSpace(src.Suffix) ? "0" : src.Suffix))
                .ReverseMap();
            CreateMap<AgentDetailsDTO, AgentDetails>().ReverseMap();

            CreateMap<GuestDetailsDTO, GuestDetails>().ReverseMap();

            CreateMap<BookingDetailDTO, BookingDetail>()
                .ReverseMap();

            CreateMap<PaymentDetailsDTO, PaymentDetails>().ReverseMap();

            CreateMap<ReservationDetailsDTO, ReservationDetails>().ReverseMap();
            CreateMap<CancelPolicyMasterDTO, CancelPolicyMaster>().ReverseMap();
            CreateMap<ReminderMasterDTO, ReminderMaster>().ReverseMap();
            CreateMap<ReminderHistoryMasterDTO, ReminderHistoryMaster>().ReverseMap();


            CreateMap<BookingDetail, BookingDetailCheckInDTO>()
            .ForMember(dest => dest.BookedRoomRates, opt => opt.Ignore()) // Ignore here, populate manually
            .ForMember(dest => dest.GuestDetails, opt => opt.Ignore())
            ;    // Same


            CreateMap<BookingDetailCheckInDTO, BookingDetail>()
                .ForMember(dest => dest.CheckInDate, opt => opt.MapFrom(src => DateTime.Parse(src.CheckInDate)))
                .ForMember(dest => dest.CheckOutDate, opt => opt.MapFrom(src => DateTime.Parse(src.CheckOutDate)))
                .ForMember(dest => dest.ReservationDate, opt => opt.MapFrom(src => DateTime.Parse(src.ReservationDate))); 

        }
    }
}
