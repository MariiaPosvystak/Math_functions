using Math_Functions.Models;
using Microsoft.EntityFrameworkCore;

namespace Math_functions.Data;

public class AndmeKontekst : DbContext
{
    public AndmeKontekst(DbContextOptions<AndmeKontekst> options) : base(options) { }

    public DbSet<FunktsiooniUurimine> FunktsiooniUurimised => Set<FunktsiooniUurimine>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<FunktsiooniUurimine>(e =>
        {
            e.Property(x => x.Valem).IsRequired().HasMaxLength(200);
            e.Property(x => x.Maaramispiirkond).HasMaxLength(300);
            e.Property(x => x.Tuletis).HasMaxLength(300);
        });
    }
}