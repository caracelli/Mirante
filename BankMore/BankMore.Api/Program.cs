using System.Reflection;
using System.Text;
using BankMore.Data;
using BankMore.Services;
using BankMore.Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

// Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<ContaService>();


// DB DI
var connection = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=BankMore.sqlite";
builder.Services.AddSingleton<IConnectionFactory>(_ => new SqliteConnectionFactory(connection));
builder.Services.AddScoped<IContaRepository, ContaRepository>();

// MediatR
builder.Services.AddMediatR(typeof(CriarContaHandler).Assembly);

// JWT
var jwt = configuration.GetSection("Jwt");
var secret = jwt.GetValue<string>("Secret") ?? "change_this_secret";
var key = Encoding.UTF8.GetBytes(secret);
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidIssuer = jwt.GetValue<string>("Issuer"),
        ValidAudience = jwt.GetValue<string>("Audience"),
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

var app = builder.Build();

// Create DB if not exist
DatabaseInitializer.Initialize(connection);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
