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
