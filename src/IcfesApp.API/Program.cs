using IcfesApp.Infrastructure;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1. Servicios
// ─────────────────────────────────────────────────────────────

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// Documento OpenAPI nativo (.NET 10) → expone /openapi/v1.json
builder.Services.AddOpenApi();

// CORS solo en Development: permite cualquier localhost/127.0.0.1 (cualquier puerto)
// para no bloquear a los equipos de front (Angular, React, etc.) mientras prueban.
const string LocalDevCorsPolicy = "LocalDevCorsPolicy";
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(LocalDevCorsPolicy, policy =>
            policy.SetIsOriginAllowed(origin =>
                    Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                    (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
                .AllowAnyMethod()
                .AllowAnyHeader());
    });
}

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// 2. Pipeline HTTP
// ─────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // sirve /openapi/v1.json

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IcfesApp API v1");
        options.RoutePrefix = "swagger"; // UI en /swagger
    });

    app.UseCors(LocalDevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────────────────────────
// 3. Seeding inicial (crea BD si no existe + aplica migraciones + roles)
// ─────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await RoleSeeder.SeedAsync(scope.ServiceProvider);
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();