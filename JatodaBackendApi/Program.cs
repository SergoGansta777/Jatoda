using JatodaBackendApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container from the Startup class
builder.Host.ConfigureServices(
    (hostContext, services) => { Startup.ConfigureServices(hostContext, services); }
);

// Add configuration sources
builder.Configuration.AddJsonFile("appsettings.json");

var app = builder.Build();

// Configure the HTTP request pipeline
Startup.Configure(app, app.Environment);

app.Run();