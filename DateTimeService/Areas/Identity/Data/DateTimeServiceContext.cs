using DateTimeService.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DateTimeService.Data
{
    public class DateTimeServiceContext : IdentityDbContext<DateTimeServiceUser>
    {
        public DbSet<ElasticLogElement> Logs { get; set; }
        public DateTimeServiceContext(DbContextOptions<DateTimeServiceContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }
    }
}
