using Microsoft.EntityFrameworkCore;
using DotNetCoreSqlDb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add database context and cache
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseMySql(
            builder.Configuration.GetConnectionString("MyDbConnection"),
            ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MyDbConnection"))
        ));

    builder.Services.AddDistributedMemoryCache();
}
else
{
    builder.Services.AddDbContext<MyDatabaseContext>(options =>
        options.UseMySql(
            builder.Configuration["AZURE_MYSQL_CONNECTIONSTRING"],
            ServerVersion.AutoDetect(builder.Configuration["AZURE_MYSQL_CONNECTIONSTRING"])
        ));

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration["AZURE_REDIS_CONNECTIONSTRING"];
        options.InstanceName = "SampleInstance";
    });
}

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add App Service logging
builder.Logging.AddAzureWebAppDiagnostics();

var app = builder.Build();

// Configure the HTTP request pipeline.
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