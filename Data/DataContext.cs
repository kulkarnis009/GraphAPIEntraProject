using EntraGreaphAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EntraGreaphAPI.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<CustomAttributes> customAttributes { get; set; }
        public DbSet<Users> users { get; set;}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
                modelBuilder.Entity<CustomAttributes>()
                    .ToTable("custom_attributes")
                    .HasKey(us => us.custom_attribute_id);

                modelBuilder.Entity<Users>()
                    .ToTable("users")
                    .HasKey(u => new { u.user_id, u.user_UUID});
  
        }
    }
}