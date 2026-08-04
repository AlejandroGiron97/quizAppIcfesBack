using System.Text;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Infrastructure.Identity;
using IcfesApp.Infrastructure.Persistence;
using IcfesApp.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace IcfesApp.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        var jwtSettings = configuration.GetSection("Jwt").Get<JwtSettings>()
            ?? throw new InvalidOperationException("No se encontró la sección de configuración 'Jwt'.");

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();

        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
