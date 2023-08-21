using Microsoft.EntityFrameworkCore;
using PastebinhezHasonlo.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

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
app.UseAuthentication();;

app.UseAuthorization();

// Identity-nek razor page routing
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
