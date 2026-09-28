using JobFinder.Data;
using JobFinder.Services;
using JobFinder.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Services
builder.Services.AddScoped<IJobService, JobService>();
builder.Services.AddScoped<IApplicationService, ApplicationService>();
builder.Services.AddScoped<IRecommendationService,RecommendationService>();
builder.Services.AddHttpClient<
    IRecommendationService,
    RecommendationService>(
    client =>
    {
        var baseUrl =
            builder.Configuration[
                "Services:PythonApiBaseUrl"
            ]
            ?? "http://127.0.0.1:8000/";

        client.BaseAddress =
            new Uri(baseUrl);

        client.Timeout =
            TimeSpan.FromMinutes(5);
    });
        builder.Services.AddHttpClient<
        IApplicationAgentService,
        ApplicationAgentService>(
        client =>
        {
            var baseUrl =
                builder.Configuration[
                    "Services:PythonApiBaseUrl"
                ]
                ?? "http://127.0.0.1:8000/";

            client.BaseAddress =
                new Uri(baseUrl);

            client.Timeout =
                TimeSpan.FromMinutes(5);
        });

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<ApplicationDocumentGenerator>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

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

app.MapControllers();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();