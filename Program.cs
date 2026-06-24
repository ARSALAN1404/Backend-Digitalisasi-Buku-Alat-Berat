using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using astratech_apps_backend.Repositories.Interfaces;
using astratech_apps_backend.Repositories.Implementations;
using astratech_apps_backend.Services;
using astratech_apps_backend.Helpers;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Komatsu Diagnostic API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// JWT Configuration - BACA DARI appsettings.json
var jwtKey = builder.Configuration["Jwt:Key"] ?? "komatsu_diagnostic_secret_key_2024";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "komatsu-backend";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "komatsu-mobile-app";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Register Dependencies
builder.Services.AddScoped<IFailureCodeRepository, FailureCodeRepository>();
builder.Services.AddScoped<IFailureDiagnosisService, FailureDiagnosisService>();
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();

// CORS ===
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// --- PENTING: PENGATURAN MIDDLEWARE ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// 1. TAMBAHKAN INI AGAR GAMBAR BISA DIAKSES VIA URL
app.UseStaticFiles(); 

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();