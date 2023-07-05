using FakeRelay.Core;
using Microsoft.AspNetCore.HttpOverrides;
using Prometheus;

var webApplicationOptions = new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory,
    Args = args,
};    
var builder = WebApplication.CreateBuilder(webApplicationOptions);
Config.Init(builder.Configuration.GetValue<string>("CONFIG_PATH"));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // I don't want to know what the nginx ip is. Also: the container that runs this doesn't expose a port to the world
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
    
    options.ForwardLimit = 1;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpLogging();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

var useMetrics = builder.Configuration.GetValue<bool?>("EXPOSE_METRICS");
if (useMetrics == true)
{
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapMetrics();
    });
}

app.Run();
