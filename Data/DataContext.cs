using EntraGraphAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EntraGraphAPI.Data
{
    public class DataContext : DbContext
    {

        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {

        }

        public DbSet<Users> users { get; set;}
        public DbSet<UsersAttributes> usersAttributes { get; set;}

        public DbSet<LogAttribute> logAttributes { get; set; }

        public DbSet<StandardAttributes> standard_attributes { get; set; }
        public DbSet<evaluateNGACResult> evaluateAccessResults{ get; set; }
        public DbSet<newEvaluateNGACResult> newEvaluateAccessResults{ get; set; }
        public DbSet<hybridNGAC> hybridNGACs{ get; set; }

        public DbSet<AccessDecision> accessDecisions{ get; set; }
        public DbSet<getObjectAttributes> getObjectAttributes{ get; set; }
        public DbSet<getSubjectAttributes> getsubjectAttributes{ get; set; }
        public DbSet<Scenarios> scenarios{ get; set; }
        public DbSet<Evaluation_results> evaluation_Results{ get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

                modelBuilder.Entity<Users>()
                .ToTable("users")
                .HasKey(u => u.user_id);  // Set `user_id` as primary key

            modelBuilder.Entity<Users>()
                .HasIndex(u => u.id)
                .IsUnique();

                modelBuilder.Entity<UsersAttributes>()
                .ToTable("user_attributes")
                .HasKey(u => u.user_attribute_id); 
                
            modelBuilder.Entity<LogAttribute>()
                .ToTable("log_attributes")
                .HasKey(u => u.LogAttrId);

            modelBuilder.Entity<LogAttribute>()
                .OwnsOne(l => l.DeviceDetail, dd =>
                {
                    dd.Property(d => d.DeviceId).HasColumnName("deviceId");
                    dd.Property(d => d.DisplayName).HasColumnName("deviceDisplayName");
                    dd.Property(d => d.OperatingSystem).HasColumnName("operatingSystem");
                    dd.Property(d => d.Browser).HasColumnName("browser");
                    dd.Property(d => d.IsCompliant).HasColumnName("isCompliant");
                    dd.Property(d => d.IsManaged).HasColumnName("isManaged");
                    dd.Property(d => d.TrustType).HasColumnName("trustType");
                });

            modelBuilder.Entity<LogAttribute>()
                .OwnsOne(l => l.Location, loc =>
                {
                    loc.Property(l => l.City).HasColumnName("city");
                    loc.Property(l => l.State).HasColumnName("state");
                    loc.Property(l => l.CountryOrRegion).HasColumnName("countryOrRegion");
                    loc.Property(l => l.Latitude).HasColumnName("latitude");
                    loc.Property(l => l.Longitude).HasColumnName("longitude");
                    loc.Property(l => l.Altitude).HasColumnName("altitude");
                });

            modelBuilder.Entity<StandardAttributes>()
                .ToTable("standard_attributes")
                .HasKey(a => a.attribute_id);

            modelBuilder.Entity<evaluateNGACResult>()
                .ToFunction("evaluateAccess").HasNoKey();

            modelBuilder.Entity<AccessDecision>()
                .ToTable("AccessDecisions")
                .HasKey(a => a.Id);

            modelBuilder.Entity<getObjectAttributes>()
            .ToFunction("getObjectAttributes")
            .HasNoKey();

            modelBuilder.Entity<getSubjectAttributes>()
            .ToFunction("getSubjectAttributes")
            .HasNoKey();

            modelBuilder.Entity<newEvaluateNGACResult>()
            .ToFunction("newEvaluateNGACaccess").HasNoKey();

            modelBuilder.Entity<hybridNGAC>()
            .ToFunction("hybridNGAC").HasNoKey();

            modelBuilder.Entity<Scenarios>().ToTable("scenarios").HasKey(a => a.scenario_id);

            modelBuilder.Entity<Evaluation_results>().ToTable("evaluation_results").HasKey(a => a.result_id);
        }
    }
}