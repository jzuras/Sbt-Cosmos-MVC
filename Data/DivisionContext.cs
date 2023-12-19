using Microsoft.EntityFrameworkCore;
using Sbt.Models;

namespace Sbt.Data;

public class DivisionContext : DbContext
{
    private readonly string _containerName = "organizations";

    public DbSet<Sbt.Models.Division> Divisions { get; set; } = default!;

    public DivisionContext(DbContextOptions<DivisionContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(this._containerName);

        modelBuilder.Entity<Division>().HasPartitionKey(d => d.Organization);
    }
}
