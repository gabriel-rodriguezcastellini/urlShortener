using Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using System.Reflection;
using WebAPI.CustomExceptionMiddleware;
using WebAPI.ExceptionFilters;
using WebAPI.Extensions;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)    
    .WriteTo.Console()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()    
    .WriteTo.Seq(builder.Configuration.GetConnectionString("seq")));
Serilog.Debugging.SelfLog.Enable(Console.Error);
// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<HttpResponseExceptionFilter>();
}).ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(context.ModelState)
        {
            ContentTypes =
            {
                    // using static System.Net.Mime.MediaTypeNames;
                    Application.Json,
                    Application.Xml
            }
        };
    options.ClientErrorMapping[StatusCodes.Status404NotFound].Link =
            "https://httpstatuses.com/404";
})
    .AddXmlSerializerFormatters();


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "URL Shortener API",
        Description = "An URL shortener service",        
        Contact = new OpenApiContact
        {
            Name = "Gabriel Rodríguez Castellini",
            Url = new Uri("https://www.linkedin.com/in/gabrielrodriguezcastellini/"),
            Email = "gabriel.rodriguezcastellini@outlook.com"
        }        
    });
    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

//Configure other services up here
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddTransient<Random>();
builder.Services.AddTransient<RandomUrlGenerator>();
builder.Services.AddTransient<ShortUrlRepository>();
builder.Services.AddHealthChecks().AddRedis(builder.Configuration.GetConnectionString("redis"), "redis", HealthStatus.Unhealthy);

builder.Services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = healthCheck => healthCheck.Tags.Contains("ready");
});

//builder.Services.AddHealthChecksUI().AddSqlServerStorage(builder.Configuration.GetConnectionString("health-checks"));

var app = builder.Build();
app.MapHealthChecks("health");

app.UseStaticFiles();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
    options.InjectStylesheet("/swagger-ui/custom.css");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{    
    app.UseDeveloperExceptionPage();
    app.UseExceptionHandler("/error-development");
}
else
{
    app.UseExceptionHandler("/error");
}

app.ConfigureExceptionHandler();
app.UseMiddleware<ExceptionMiddleware>();
app.ConfigureCustomExceptionMiddleware();
app.UseStatusCodePagesWithReExecute("/StatusCode", "?statusCode={0}");
app.UseAuthorization();

app.MapControllers();

app.Run();