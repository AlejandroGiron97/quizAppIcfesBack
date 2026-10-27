using IcfesApp.Infrastructure;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1. Servicios
// ─────────────────────────────────────────────────────────────

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// OpenAPI + botón Authorize (Bearer JWT) aplicado a nivel de operación
builder.Services.AddOpenApi(options =>
{
    // 1) Definir el esquema de seguridad
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Pega el JWT de /api/auth/login. No incluyas la palabra 'Bearer'."
        };
        return Task.CompletedTask;
    });

    // 2) Aplicar el requisito de seguridad a TODAS las operaciones
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        operation.Security ??= new List<OpenApiSecurityRequirement>();
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer")] = new List<string>()
        });
        return Task.CompletedTask;
    });
});

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
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IcfesApp API v1");
        options.RoutePrefix = "swagger";
    });

    app.UseCors(LocalDevCorsPolicy);
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────────────────────────
// 3. Seeding inicial
// ─────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    await RoleSeeder.SeedAsync(scope.ServiceProvider);
    await DataSeeder.SeedAsync(scope.ServiceProvider);
}

app.Run();