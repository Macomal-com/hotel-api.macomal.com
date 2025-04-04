using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace RepositoryModels.Repository
{
    public class DbContextSql:DbContext
    {
        public DbContextSql(DbContextOptions<DbContextSql> options) : base(options)
        {
        }

        public DbSet<CompanyDetails> CompanyDetails { get; set; }
        public DbSet<LandlordDetails> LandlordDetails { get; set; }
        public DbSet<ClusterMaster> ClusterMaster { get; set; }
        public DbSet<BedTypeMaster> BedTypeMaster { get; set; }
        public DbSet<BuildingMaster> BuildingMaster { get; set; }
        public DbSet<FloorMaster> FloorMaster { get; set; }
        public DbSet<OwnerMaster> OwnerMaster { get; set; }
        public DbSet<RoomCategoryMaster> RoomCategoryMaster { get; set; }
        public DbSet<RoomMaster> RoomMaster { get; set; }
        public DbSet<RoomRateMaster> RoomRateMaster { get; set; }
        public DbSet<ServicableMaster> ServicableMaster { get; set; }
        public DbSet<VendorMaster> VendorMaster { get; set; }
        public DbSet<UserCreation> UserCreation { get; set; }
        public DbSet<StaffManagementMaster> StaffManagementMaster { get; set; }
        public DbSet<PaymentMode> PaymentMode { get; set; }
        public DbSet<GroupMaster> GroupMaster { get; set; }
        public DbSet<SubGroupMaster> SubGroupMaster { get; set; }

        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<GstMaster> GstMaster { get; set; }
        public DbSet<VendorServiceMaster> VendorServiceMaster { get; set; }
        public DbSet<CommissionMaster> CommissionMaster { get; set; }
        public DbSet<PropertyImages> PropertyImages { get; set; }
        public DbSet<HourMaster> HourMaster { get; set; }

        public DbSet<UserPropertyMapping> UserPropertyMapping { get; set; }
        public DbSet<RoomRateDateWise> RoomRateDateWise { get; set; }
        public DbSet<PaxMaster> PaxMaster { get; set; }

        public DbSet<DocumentMaster> DocumentMaster { get; set; }
        public DbSet<ExtraPolicies> ExtraPolicies { get; set; }
        public DbSet<AgentDetails> AgentDetails { get; set; }
        public DbSet<RoomAvailability> RoomAvailability { get; set; }
        public DbSet<GstRangeMaster> GstRangeMaster { get; set; }

        public DbSet<GuestDetails> GuestDetails { get; set; }

        public DbSet<BookingDetail> BookingDetail { get; set; }
        public DbSet<PaymentDetails> PaymentDetails { get; set; }
        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    modelBuilder.Entity<ValidationData>().HasNoKey();
        //    modelBuilder.Entity<Dashboard>().HasNoKey();
        //    modelBuilder.Entity<UserPages>().HasNoKey();
        //    modelBuilder.Entity<UserPageAuthResult>().HasNoKey();
        //    // Other configurations...
        //}
    }

}
