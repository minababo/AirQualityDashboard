using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Sensor> Sensors { get; set; }
    public DbSet<AQIData> AQIData { get; set; }
    public DbSet<SimulationSettings> SimulationSettings { get; set; }
}