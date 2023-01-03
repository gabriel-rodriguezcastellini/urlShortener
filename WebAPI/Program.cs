using Data;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
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
builder.Services.AddSwaggerGen();
//builder.Services.AddStackExchangeRedisCache(options =>
//{
//    options.Configuration = "redis:6379"; // redis is the container name of the redis service. 6379 is the default port
//    options.InstanceName = "SampleInstance";
//});

//Configure other services up here
var multiplexer = ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("redis"));
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddTransient<Random>();
builder.Services.AddTransient<RandomUrlGenerator>();
builder.Services.AddTransient<ShortUrlRepository>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

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
