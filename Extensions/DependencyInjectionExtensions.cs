using AutoMapper;
using sbm.Server.Interfaces;
using sbm.Server.Mapping;
using sbm.Server.Services;
using sbm.Server.Services.BackgroundServices;

namespace sbm.Server.Extensions
{
    public static class DependencyInjectionExtensions
    {

        public static void InjectAutomapperService(this IServiceCollection services)
        {
            var config = new MapperConfiguration(cfg => { cfg.AddProfile(new AutoMapperProfile()); });
            var mapper = config.CreateMapper();
            services.AddSingleton<IMapper>(mapper);
        }
        public static void ConfigureHttpClient(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient(
                "SapService",
                client =>
                {
                    client.BaseAddress = new Uri(configuration.GetValue<string>("SapService:Url"));
                    client.Timeout = TimeSpan.FromSeconds(configuration.GetValue<int>("SapService:TimeoutInSeconds"));
                    //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Configuration.GetValue<string>("SapService:AuthenticationToken"));
                })
                .ConfigurePrimaryHttpMessageHandler(
                    () => new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
                    });
        }

        public static void InjectApiServicesDependencies(this IServiceCollection services)
        {
            //services.AddScoped<ISapByLayerService, SapByLayerService>();
            services.AddScoped<ISapService, SapService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<IConsumptionService, ConsumptionService>();

        }
    }
}
