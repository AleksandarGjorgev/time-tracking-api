using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TimeTrackingAPI.Models;

namespace TimeTrackingAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // DbSet za delovni čas in vrste odsotnosti
        public DbSet<WorkLog> WorkLogs { get; set; }
        public DbSet<AbsenceRecord> AbsenceRecords { get; set; }
    }
}
