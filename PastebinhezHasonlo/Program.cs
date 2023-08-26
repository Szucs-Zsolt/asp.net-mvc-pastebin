using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using PastebinhezHasonlo.Data;                      // Role-ok neve ebben van

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // EntityFramework el�rje az adatb�zist, dependency injection
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        // Identity elérése (saját role-ok hozzáadásával)
        builder.Services
                .AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
        // Megerősítő emailt küldene ki. Ez nincs megoldva, csak a visszajelzés, hogy sikerült.
        builder.Services.AddScoped<IEmailSender, EmailSender>();

        // Identity Razor page-t használ
        builder.Services.AddRazorPages();

        // Háttérfolyamat: rendszeresen törli a lejárt határidejű üzeneteket az adatbázisból
        builder.Services.AddHostedService<RepeatedDatabaseCleanup>();
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseAuthentication(); ;

        app.UseAuthorization();

        // Identity-nek razor page routing
        app.MapRazorPages();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");


        // Új adatbázis már a default role-okkal és admin userrel indul
        using (var scope = app.Services.CreateScope())
        {
            // RoleManager<T> elérése, milyen típusú role-ok kezelése
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            // Role-ok neve statikus file-ból
            var roles = new[] { Role.Admin, Role.User };    

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var userManager = 
                scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            string email = "admin@admin";
            string password = "Jelszó1";
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var admin = new IdentityUser()
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                
                await userManager.CreateAsync(admin, password);
                await userManager.AddToRoleAsync(admin, Role.Admin);

            }
        }
        app.Run();
    }
}