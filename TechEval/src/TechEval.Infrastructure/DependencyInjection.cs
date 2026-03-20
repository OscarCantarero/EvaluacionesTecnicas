using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using TechEval.Application.Common.Interfaces;
using TechEval.Domain.Evaluaciones.Repositorios;
using TechEval.Domain.Resultados.Repositorios;
using TechEval.Domain.Sesiones.Repositorios;
using TechEval.Infrastructure.Caching;
using TechEval.Infrastructure.Identity;
using TechEval.Infrastructure.Persistence;
using TechEval.Infrastructure.Persistence.Repositories;
using TechEval.Infrastructure.Services;
using TechEval.Infrastructure.Storage;

namespace TechEval.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        services.AddDbContext<TechEvalDbContext>(opts =>
        {
            opts.UseNpgsql(configuration.GetConnectionString("Default"));
            opts.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        });

        // Identity
        services.AddIdentity<UsuarioApp, IdentityRole>(opts =>
        {
            opts.Password.RequireDigit = true;
            opts.Password.RequiredLength = 8;
            opts.Password.RequireUppercase = true;
            opts.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<TechEvalDbContext>()
        .AddDefaultTokenProviders();

        // JWT Auth
        var jwtSettings = configuration.GetSection("Jwt");
        services.AddAuthentication(opts =>
        {
            opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
            };
        });

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("Redis")!));
        services.AddSingleton<RedisCache>();

        // Application services
        services.AddScoped<IServicioAuth, ServicioAuth>();
        services.AddScoped<IContextoUsuario, ContextoUsuario>();
        services.AddScoped<IServicioArchivos, LocalServicioArchivos>();
        services.AddScoped<IServicioGeneradorPDF, ServicioGeneradorPDF>();
        services.AddScoped<IServicioEmail, ServicioEmailPlaceholder>();

        // IA Evaluation – Power Automate
        services.Configure<EvaluacionIAOptions>(
            configuration.GetSection(EvaluacionIAOptions.SectionName));
        services.AddHttpClient<IServicioEvaluacionIA, ServicioEvaluacionIA>();

        // Repositories
        services.AddScoped<IRepositorioEvaluacion, RepositorioEvaluacion>();
        services.AddScoped<IRepositorioSesionEvaluacion, RepositorioSesionEvaluacion>();
        services.AddScoped<IRepositorioResultadoEvaluacion, RepositorioResultadoEvaluacion>();
        services.AddScoped<IRepositorioCategoria, RepositorioCategoria>();

        // HttpContextAccessor (needed for IContextoUsuario)
        services.AddHttpContextAccessor();

        return services;
    }
}
