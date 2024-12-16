using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using sbm.Server.Extensions;
using sbm.Server.Services.BackgroundServices;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(options =>
        {
            builder.Configuration.Bind("AzureAd", options);
            options.TokenValidationParameters.NameClaimType = "name";
        }, options => { builder.Configuration.Bind("AzureAd", options); });

builder.Services.AddAuthorization(config =>
{
    config.AddPolicy("AuthZPolicy", policyBuilder =>
        policyBuilder.Requirements.Add(new ScopeAuthorizationRequirement() { RequiredScopesConfigurationKey = $"AzureAd:Scopes" }));
});


builder.Services.AddControllers()
    .AddNewtonsoftJson(options => {
        options.UseMemberCasing();
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.ConfigureDatabaseServices(builder.Configuration);
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.InjectAutomapperService();
builder.Services.InjectApiServicesDependencies();
builder.Services.AddCronJob<MySchedulerJob>(options =>
{
    // Corre cada 10 minuto
    options.CronExpression = "*/10 * * * *";
    options.TimeZone = TimeZoneInfo.Local;
});
builder.Services.ConfigureHttpClient(builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseSwagger();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}
app.UseExceptionHandler(static a => a.Run(static async context => {
    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
    var exception = exceptionHandlerPathFeature?.Error;
    // Log the exception, generate a custom response, etc. context.Response.StatusCode = 500;
    await context.Response.WriteAsJsonAsync(new { Error = "An unexpected error occurred" });
}));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

