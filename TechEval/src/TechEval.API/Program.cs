using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;
using TechEval.API.Hubs;
using TechEval.API.Middleware;
using TechEval.API.Seeds;
using TechEval.API.Services;
using TechEval.Application;
using TechEval.Infrastructure;
using TechEval.Infrastructure.Identity;
using TechEval.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, lc) =>
    lc.ReadFrom.Configuration(ctx.Configuration)
      .Enrich.FromLogContext()
      .WriteTo.Console());

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
        opts.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(opts =>
    opts.SerializerOptions.Converters.Add(
        new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((doc, _, _) =>
    {
        doc.Info.Title = "TechEval API";
        doc.Info.Version = "v1";
        doc.Info.Description = "API para el sistema de evaluación técnica de candidatos TechEval. " +
                               "Endpoints para autenticación, gestión de evaluaciones, sesiones y resultados.";
        return System.Threading.Tasks.Task.CompletedTask;
    });
});

// Agregar SignalR
builder.Services.AddSignalR();

// Agregar servicio de background para temporizadores
builder.Services.AddHostedService<ServicioTemporizadorSesiones>();

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(policy =>
        policy.WithOrigins(
            builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? ["http://localhost:3000"])
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opts => opts.WithTitle("TechEval API"));
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseStaticFiles();
app.MapControllers();

// Mapear SignalR Hub
app.MapHub<HubEvaluacion>("/hubs/sesiones");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<TechEvalDbContext>();
    db.Database.Migrate();
    await SeedRoles(scope.ServiceProvider);
    await SeedUsuariosDesarrollo(scope.ServiceProvider);
    await SeedEvaluaciones.EjecutarAsync(scope.ServiceProvider);
}

app.Run();

static async Task SeedRoles(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<
        Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole>>();
    string[] roles = ["Administrador", "Evaluador", "Candidato"];
    foreach (var rol in roles)
    {
        if (!await roleManager.RoleExistsAsync(rol))
            await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole(rol));
    }
}

static async Task SeedUsuariosDesarrollo(IServiceProvider services)
{
    var userManager = services.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<UsuarioApp>>();

    var usuariosSeed = new[]
    {
        new { Email = "admin@techeval.com", Nombre = "Admin TechEval", Password = "Password123!", Rol = "Administrador" },
        new { Email = "evaluador@techeval.com", Nombre = "Evaluador Demo", Password = "Password123!", Rol = "Evaluador" },
        new { Email = "candidato@techeval.com", Nombre = "Candidato Demo", Password = "Password123!", Rol = "Candidato" }
    };

    foreach (var item in usuariosSeed)
    {
        var usuario = await userManager.FindByEmailAsync(item.Email);
        if (usuario is null)
        {
            usuario = new UsuarioApp
            {
                UserName = item.Email,
                Email = item.Email,
                Nombre = item.Nombre,
                EmailConfirmed = true
            };

            var resultadoCreate = await userManager.CreateAsync(usuario, item.Password);
            if (!resultadoCreate.Succeeded)
                throw new InvalidOperationException($"No se pudo crear usuario seed {item.Email}: {string.Join(", ", resultadoCreate.Errors.Select(e => e.Description))}");
        }

        if (!await userManager.IsInRoleAsync(usuario, item.Rol))
        {
            var resultadoRol = await userManager.AddToRoleAsync(usuario, item.Rol);
            if (!resultadoRol.Succeeded)
                throw new InvalidOperationException($"No se pudo asignar rol {item.Rol} a {item.Email}: {string.Join(", ", resultadoRol.Errors.Select(e => e.Description))}");
        }
    }
}

public partial class Program { }
