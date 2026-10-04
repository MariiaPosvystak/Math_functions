using Math_Functions.Models;
using Microsoft.EntityFrameworkCore;

namespace Math_Functions.Data
{
    public class AndmeKontekst : DbContext
    {
        public AndmeKontekst(DbContextOptions<AndmeKontekst> options) : base(options)
        {
        }

        public DbSet<FunktsiooniUurimine> FunktsiooniUurimised { get; set; }

    }
}
