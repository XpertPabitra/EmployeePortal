using EmployeePortal.Data;
using EmployeePortal.Repositories.Interfaces;
using EmployeePortal.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 1. Add MVC services
builder.Services.AddControllersWithViews();

// 2. Add the Cache service (THIS IS THE MISSING PIECE)
builder.Services.AddDistributedMemoryCache();

// 3. Add Session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Register your custom services
builder.Services.AddScoped<DbHelper>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

var app = builder.Build();

// ... existing middleware (StaticFiles, Routing, etc.) ...

// 4. Use Session (Must be after UseRouting and before MapControllerRoute)
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();