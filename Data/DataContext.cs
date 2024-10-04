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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomAttributes>()
                    .ToTable("custom_attributes")
                    .HasKey(us => us.custom_attribute_id);
  
        }
    }
}