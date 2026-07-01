using Azure.Storage.Blobs;
using ExamBuilder.Data;
using ExamBuilder.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// DbContext registrieren
builder.Services.AddDbContext<ExamBuilderContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ExamBuilderContext")));

// Identity registrieren (ohne Default-UI)
builder.Services.AddIdentity<ExamBuilderUser, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
    })
    .AddEntityFrameworkStores<ExamBuilderContext>()
    .AddDefaultTokenProviders();

// Login/AccessDenied Pfade konfigurieren
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Azure Blob Storage registrieren
builder.Services.AddSingleton(new BlobServiceClient(
    builder.Configuration.GetConnectionString("AzureBlobStorage")));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Datenbank migrieren und Seed-Daten einfügen
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ExamBuilderContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ExamBuilderUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    db.Database.Migrate();
    await DbSeeder.Seed(db, userManager, roleManager);
}

app.Run();
