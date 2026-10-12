using JobFinder.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace JobFinder.Tests;

/// <summary>
/// Boots the real application pipeline with the database
/// swapped for EF InMemory. No live network: Postgres and
/// the Python API are never touched.
/// </summary>
public class TestWebApplicationFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType ==
                    typeof(DbContextOptions<ApplicationDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(
                options => options.UseInMemoryDatabase(
                    $"JobFinderTests-{Guid.NewGuid()}"));
        });
    }
}
