using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using NeverNeverLand.Services;
using NeverNeverLand.Services.SendGridEmailService;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity with Roles
builder.Services
    .AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()                               // enable roles
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// Configure Stripe settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddScoped<IEmailService, SendGridEmailService>();
builder.Services.AddScoped<IPriceService, PriceService>();

builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


// ---------- SEED DATA (DB, seasons/prices, admin user/role) ----------
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Ensure DB exists/migrated
    var db = services.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    // --- Admin role & user seed ---
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var config = services.GetRequiredService<IConfiguration>();

    const string AdminRole = "Admin";
    // Role
    if (!await roleManager.RoleExistsAsync(AdminRole))
        await roleManager.CreateAsync(new IdentityRole(AdminRole));

    // Credentials from config (see appsettings section below)
    var adminEmail = config["Admin:Email"] ?? "support@neverneverlandpark.com";
    var adminPassword = config["Admin:Password"] ?? "HumptyDumpty$1432";

    // User
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true // because RequireConfirmedAccount = true
        };
        var create = await userManager.CreateAsync(adminUser, adminPassword);
        if (!create.Succeeded)
            throw new Exception("Failed to create admin user: " +
                string.Join("; ", create.Errors.Select(e => e.Description)));
    }

    // Add to role
    if (!await userManager.IsInRoleAsync(adminUser, AdminRole))
        await userManager.AddToRoleAsync(adminUser, AdminRole);


    // --- SEASON SETUP ---
    var seasons = new[]
    {
        new { Name = "Early Bird", Start = "01/01", End = "03/31", Discount = 0.15m },
        new { Name = "Regular",    Start = "04/01", End = "08/31", Discount = 0.00m },
        new { Name = "Late",       Start = "09/01", End = "10/31", Discount = 0.25m },
        new { Name = "Off Season", Start = "11/01", End = "12/31", Discount = 1.00m } // 100% discount = off sale
    };
    int year = DateTime.UtcNow.Year;
    var seasonEntities = new List<Season>();
    foreach (var s in seasons)
    {
        var start = DateOnly.ParseExact($"{year}-{s.Start}", "yyyy-MM/dd", CultureInfo.InvariantCulture);
        var end = DateOnly.ParseExact($"{year}-{s.End}", "yyyy-MM/dd", CultureInfo.InvariantCulture);
        var existing = await db.Seasons.FirstOrDefaultAsync(se => se.Name == s.Name && se.StartDate == start && se.EndDate == end);
        if (existing == null)
        {
            existing = new Season { Name = s.Name, StartDate = start, EndDate = end, IsActive = true };
            db.Seasons.Add(existing);
            await db.SaveChangesAsync();
        }
        seasonEntities.Add(existing);
    }

    // --- PRICING SETUP ---
    var basePrices = new[]
    {
        new { Item = "Personal", Amount = 99.00m },
        new { Item = "Family",   Amount = 149.00m },
        new { Item = "Family+",  Amount = 199.00m }
    };
    foreach (var season in seasonEntities)
    {
        var discount = seasons.First(s => s.Name == season.Name).Discount;
        foreach (var bp in basePrices)
        {
            if (discount == 1.00m) continue; // Off Season: off sale
            var price = bp.Amount * (1 - discount);
            var existing = await db.Prices.FirstOrDefaultAsync(p => p.SeasonId == season.Id && p.Item == bp.Item && p.IsActive);
            if (existing != null) existing.IsActive = false;
            db.Prices.Add(new Price
            {
                SeasonId = season.Id,
                Kind = "Pass",
                Item = bp.Item,
                Amount = price,
                Currency = "USD",
                Channel = "Online",
                EffectiveStartUtc = DateTime.UtcNow,
                IsActive = true
            });
        }
    }
    await db.SaveChangesAsync();
}
// --------------------------------------------------------------------

app.Run();
