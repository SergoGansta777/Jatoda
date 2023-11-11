using System.Text;
using JatodaBackendApi.Model;
using JatodaBackendApi.Providers;
using JatodaBackendApi.Providers.Interfaces;
using JatodaBackendApi.Repositories;
using JatodaBackendApi.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using JatodaBackendApi.Services;
using JatodaBackendApi.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
// TODO: Вынести все файлы настройки сервисов в отдельную директорию Services
// public void ConfigureServices(IServiceCollection services)


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Configuration.AddJsonFile("appsettings.json");

builder.Services.AddDbContext<JatodaContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("JatodaConnection");
    options.UseNpgsql(connectionString);
});

builder.Services.AddScoped<IRepository<Todonote>, ToDoRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<Tag>, TagRepository>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ITodoProvider<Todonote>, TodoProvider>();
builder.Services.AddScoped<IUserProvider<User>, UserProvider>();
builder.Services.AddSingleton<ICacheRepository, CacheRepository>();

var cacheConnectionString = builder.Configuration.GetConnectionString("CacheConnection");
builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(cacheConnectionString));

builder.Services.AddAuthentication(
    options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }
).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();