using IcfesApp.Infrastructure;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// 1. Services
// ─────────────────────────────────────────────────────────────

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();

// OpenAPI + Authorize button (Bearer JWT) applied at operation level
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Paste the JWT from /api/auth/login. Do not include the word 'Bearer'."
        };
        return Task.CompletedTask;
    });

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
// 2. HTTP pipeline (THE ORDER HERE MATTERS)
// ─────────────────────────────────────────────────────────────

// 1. CORS FIRST (so images and the API get the headers)
if (app.Environment.IsDevelopment())
{
    app.UseCors(LocalDevCorsPolicy);
}

// 2. Static files afterwards
app.UseStaticFiles();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IcfesApp API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────────────────────────
// 3. Migrations and initial data
//    - Roles and default admin: in every environment (the app does not work without them).
//    - Demo data (users with a known password): Development ONLY.
// ─────────────────────────────────────────────────────────────

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();

    await RoleSeeder.SeedAsync(scope.ServiceProvider);

    if (app.Environment.IsDevelopment())
    {
        await DataSeeder.SeedAsync(scope.ServiceProvider);
    }
}

app.Run();