using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;

var builder = WebApplication.CreateBuilder(args);

// =====================
// DATABASE CONFIG (MySQL)
// =====================
builder.Services.AddDbContext<MyDatabaseContext>(options =>
{
    var conn =
        builder.Configuration["AZURE_SQL_CONNECTIONSTRING"]
        ?? builder.Configuration.GetConnectionString("AZURE_SQL_CONNECTIONSTRING");

    if (string.IsNullOrWhiteSpace(conn))
        throw new Exception("Database connection string is missing.");

    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// =====================
// CACHE
// =====================
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        var redis = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];

        if (string.IsNullOrWhiteSpace(redis))
            throw new Exception("Redis connection string is missing.");

        options.Configuration = redis;
        options.InstanceName = "SampleInstance";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// =====================
// MVC
// =====================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =====================
// SAFE MIGRATION (IMPORTANT FIX)
// =====================
using (var scope = app.Services.CreateScope())
{
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<MyDatabaseContext>();
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        // Prevent crash in Azure startup
        Console.WriteLine("Migration failed: " + ex.Message);
    }
}

// =====================
// PIPELINE
// =====================
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
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();