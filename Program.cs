// --- pakettide paigaldus (Package Manager Console / terminal) ---
// dotnet add package Microsoft.EntityFrameworkCore.Sqlite
// dotnet add package Microsoft.EntityFrameworkCore.Design
//
// --- Program.cs: lisa enne builder.Build() ---
// using Microsoft.EntityFrameworkCore;
// using Math_functions.Data;
//
// builder.Services.AddDbContext<AppDbContext>(o =>
//     o.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=funktsioonid.db"));
//
// --- migratsioon ---
// dotnet ef migrations add AlgneAndmebaas
// dotnet ef database update
//
// --- Program.cs: marsruut (kui vaikimisi on teine) ---
// app.MapControllerRoute(name: "default", pattern: "{controller=Funktsioonid}/{action=Index}/{id?}");