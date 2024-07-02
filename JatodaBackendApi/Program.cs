using AspNetCoreRateLimit;
using JatodaBackendApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");
builder.Host.ConfigureServices((context, services) =>
{
    services.RegisterServices(context.Configuration);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Just Another ToDo App API V1");
});

app.UseHttpsRedirection();
app.UseIpRateLimiting();
app.UseRouting();
app.UseAuthorization();
app.UseAuthentication();
app.UseCors(options => options
    .WithOrigins("http://localhost:3000", "http://localhost:8080", "http://localhost:4200")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

app.ConfigureExceptionHandler(app.Services.GetRequiredService<ILogger<Program>>());
app.MapControllers();

app.Run();