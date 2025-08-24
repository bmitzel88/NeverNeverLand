using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NeverNeverLand.Data;
using NeverNeverLand.Models;
using NeverNeverLand.Services;
using NeverNeverLand.Services.SendGridEmailService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddControllersWithViews();

// Configure Stripe settings
builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

builder.Services.AddScoped<IEmailService, SendGridEmailService>();

builder.Services.AddSingleton<IPriceService, PriceService>();

builder.Services.AddSession();

var app = builder.Build();

app.UseSession();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    // Ensure a default season
    var defaultSeason = await db.Seasons
        .FirstOrDefaultAsync(s => s.IsActive && s.AlwaysOn);
    if (defaultSeason == null)
    {
        defaultSeason = new Season
        {
            Name = "Default",
            AlwaysOn = true,
            IsActive = true
        };
        db.Seasons.Add(defaultSeason);
        await db.SaveChangesAsync();
    }

    // Upsert prices for simple types
    async Task UpsertAsync(string admissionType, decimal amount, string currency = "USD")
    {
        var existing = await db.Prices
            .Where(p => p.SeasonId == defaultSeason.Id && p.AdmissionType == admissionType && p.IsActive)
            .ToListAsync();

        foreach (var p in existing) p.IsActive = false;

        db.Prices.Add(new Price
        {
            SeasonId = defaultSeason.Id,
            AdmissionType = admissionType,
            Amount = amount,
            Currency = currency,
            EffectiveStartUtc = DateTime.UtcNow,
            IsActive = true
        });
    }

    await UpsertAsync("Adult", 10.00m);
    await UpsertAsync("Child", 5.00m);
    await UpsertAsync("Infant", 0.00m);

    await db.SaveChangesAsync();
}

app.Run();


