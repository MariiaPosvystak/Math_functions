using Math_Functions.Data;
using Math_Functions.Models;
using Math_Functions.Teenused;
using Microsoft.EntityFrameworkCore;

namespace Math_Functions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Ilma selle reata nõuab MVC kõiki string-välju kohustuslikuks
            builder.Services.AddControllersWithViews(valikud =>
                valikud.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true);

            // DbContext registreerimine
            builder.Services.AddDbContext<AndmeKontekst>(valikud =>
                valikud.UseSqlite(builder.Configuration.GetConnectionString("Andmebaas")));

            builder.Services.AddSingleton<FunktsiooniArvutaja>();

            var app = builder.Build();

            // Käivitamisel teeme migratsiooni ja lisame näidisandmed
            using (var skoop = app.Services.CreateScope())
            {
                var kontekst = skoop.ServiceProvider.GetRequiredService<AndmeKontekst>();
                var arvutaja = skoop.ServiceProvider.GetRequiredService<FunktsiooniArvutaja>();

                kontekst.Database.Migrate();

                if (!kontekst.FunktsiooniUurimised.Any())
                {
                    var naited = new List<FunktsiooniUurimine>
                    {
                        new FunktsiooniUurimine { Valem = "x^3 - 3*x", Tuletis = "3*x^2 - 3" },
                        new FunktsiooniUurimine { Valem = "x^2 - 4*x + 3", Tuletis = "2*x - 4" },
                        new FunktsiooniUurimine { Valem = "x^3 - 12*x", Tuletis = "3*x^2 - 12" }
                    };

                    foreach (var naide in naited)
                    {
                        arvutaja.Analuusi(naide);
                    }

                    kontekst.FunktsiooniUurimised.AddRange(naited);
                    kontekst.SaveChanges();
                }
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=FunktsiooniUurimine}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
