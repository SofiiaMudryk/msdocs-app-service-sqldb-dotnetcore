using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;

var builder = WebApplication.CreateBuilder(args);

// DATABASE
builder.Services.AddDbContext<MyDatabaseContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString(
        builder.Environment.IsDevelopment()
            ? "MyDbConnection"
            : "AZURE_SQL_CONNECTIONSTRING"
    );

    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
});

// CACHE
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
        options.InstanceName = "SampleInstance";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// AUTO MIGRATION ON STARTUP (KEY FIX)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyDatabaseContext>();
    db.Database.Migrate();
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
    pattern: "{controller=Todos}/{action=Index}/{id?}");

app.Run();