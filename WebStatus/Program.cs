using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddHealthChecks().AddCheck("self", () => HealthCheckResult.Healthy());
builder.Services.AddHealthChecksUI().AddInMemoryStorage();
var app = builder.Build();

app.UseHealthChecksUI(setup => 
{    
    setup.UIPath = "/hc-ui"; 
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseEndpoints(configure => 
{
    configure.MapDefaultControllerRoute();
    configure.MapHealthChecks("/liveness", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions { Predicate = p => p.Name.Contains("self") });    
});

app.MapRazorPages();
app.Run();
