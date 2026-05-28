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
        ?? builder.Configuration.GetConnectionString("MyDbConnection");

    if (string.IsNullOrEmpty(conn))
    {
        throw new Exception("Database connection string is missing.");
    }

    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// =====================
// CACHE CONFIG
// =====================
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];

        if (string.IsNullOrEmpty(options.Configuration))
        {
            throw new Exception("Redis connection string is missing.");
        }

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
// AUTO MIGRATION
// =====================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDatabaseContext>();
    db.Database.Migrate();
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