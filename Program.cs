using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

// PERBAIKAN: Berdasarkan kode yang kamu kirim, ini adalah using yang benar:
using astratech_apps_backend.Repositories.Implementations;             // Untuk IFailureCodeRepository & FailureCodeRepository (no 3 & 4)
using astratech_apps_backend.Services;                 // Untuk IFailureDiagnosisService & FailureDiagnosisService (no 1 & 2)
using astratech_apps_backend.Repositories.Interfaces;  // Untuk IHistoryRepository (no 5)
using astratech_apps_backend.Helpers;

// Jika nanti kamu punya HistoryRepository (Implementasi), tambahkan using tempat filenya berada
// Contoh: using astratech_apps_backend.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// 1. Add services to container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 2. Swagger with JWT
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Komatsu Diagnostic API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Masukkan token JWT anda."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// 3. JWT Configuration
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

// 4. --- REGISTER DEPENDENCIES (SESUAI KODE NO 1-5) ---
builder.Services.AddScoped<IFailureCodeRepository, FailureCodeRepository>();
builder.Services.AddScoped<IFailureDiagnosisService, FailureDiagnosisService>();
builder.Services.AddSingleton<JwtHelper>();

// Catatan: Pastikan kamu sudah membuat class "HistoryRepository" 
// yang mengimplementasikan "IHistoryRepository"
// builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();

// 5. CORS Setup
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

// 6. Configure Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseStaticFiles(); 
app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();