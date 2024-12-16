using sbm.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace sbm.Server.Extensions
{
    public static class DbContextFactoryExtensions
    {
        public static void ConfigureDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContextFactory<SbmContext>(options => options
                .UseLazyLoadingProxies(false)
                .ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(CoreEventId.LazyLoadOnDisposedContextWarning);
                    warnings.Ignore(CoreEventId.NavigationBaseIncludeIgnored);
                })
                .UseSqlServer(configuration.GetConnectionString("SBMConnection"),
                    opts => opts.CommandTimeout((int)TimeSpan.FromMinutes(15).TotalSeconds)).EnableSensitiveDataLogging().LogTo(Console.WriteLine).EnableDetailedErrors());

        }
    }
}
