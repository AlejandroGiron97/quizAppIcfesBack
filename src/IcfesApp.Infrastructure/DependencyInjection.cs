using System.Text;
using IcfesApp.Application.Common.Interfaces;
using IcfesApp.Domain.Constants;
using IcfesApp.Infrastructure.Content;
using IcfesApp.Infrastructure.Content.Proxies;
using IcfesApp.Infrastructure.Content.SubscriptionOwnership;
using IcfesApp.Infrastructure.Email;
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
        var provider = configuration["Database:Provider"] ?? "Sqlite";

        var connectionStringKey = provider == "SqlServer"
            ? "SqlServerConnection"
            : "DefaultConnection";

        var connectionString = configuration.GetConnectionString(connectionStringKey)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringKey}' was not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            switch (provider)
            {
                case "SqlServer":
                    options.UseSqlServer(connectionString);
                    break;

                case "Sqlite":
                default:
                    options.UseSqlite(connectionString);
                    break;
            }
        });

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
            ?? throw new InvalidOperationException("Configuration section 'Jwt' was not found.");

        services.Configure<JwtSettings>(configuration.GetSection("Jwt"));
        services.Configure<EmailSettings>(configuration.GetSection("Email"));
        services.Configure<GoogleSettings>(configuration.GetSection("Authentication:Google"));
        services.Configure<PracticeAccessSettings>(configuration.GetSection(PracticeAccessSettings.SectionName));

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

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.RequireAdmin, policy => policy.RequireRole(Roles.Admin));
            options.AddPolicy(Policies.RequireTeacher, policy => policy.RequireRole(Roles.Admin, Roles.Teacher));
            options.AddPolicy(Policies.RequireStudent, policy => policy.RequireRole(Roles.Admin, Roles.Teacher, Roles.Student));
        });

        AddAuthenticationServices(services);
        AddContentServices(services);

        return services;
    }

    /// <summary>Registration, session, password, email confirmation and 2FA.</summary>
    private static void AddAuthenticationServices(IServiceCollection services)
    {
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthTokenIssuer, AuthTokenIssuer>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<IAuthEmailSender, AuthEmailSender>();

        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IEmailConfirmationService, EmailConfirmationService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
    }

    /// <summary>Subjects, questions, institutions, invitation codes, plans, subscriptions, reports and practice.</summary>
    private static void AddContentServices(IServiceCollection services)
    {
        services.AddMemoryCache();

        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IInstitutionService, InstitutionService>();
        services.AddScoped<IInvitationCodeService, InvitationCodeService>();
        services.AddScoped<IReportService, ReportService>();

        // "Whose subscription is it" strategies: SubscriptionService receives all of them and picks the one that applies.
        services.AddSingleton<ISubscriptionOwnerStrategy, ParticularSubscriptionOwnerStrategy>();
        services.AddSingleton<ISubscriptionOwnerStrategy, InstitutionalSubscriptionOwnerStrategy>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();

        // Proxies: whoever asks for the interface gets the proxy, and the proxy wraps the real service.
        AddWithProxy<ISubjectService, SubjectService, CachedSubjectService>(services);
        AddWithProxy<IPlanService, PlanService, CachedPlanService>(services);
        AddWithProxy<IPracticeSessionService, PracticeSessionService, SubscriptionCheckingPracticeSessionService>(services);
    }

    /// <summary>
    /// Registers <typeparamref name="TProxy"/> as the implementation of <typeparamref name="TService"/>,
    /// passing it the real <typeparamref name="TRealService"/> as the service it wraps.
    /// The proxy's constructor must take a <typeparamref name="TService"/> parameter (the real service);
    /// its other dependencies are resolved from the container as usual.
    /// </summary>
    private static void AddWithProxy<TService, TRealService, TProxy>(IServiceCollection services)
        where TService : class
        where TRealService : class, TService
        where TProxy : class, TService
    {
        services.AddScoped<TRealService>();
        services.AddScoped<TService>(serviceProvider =>
            ActivatorUtilities.CreateInstance<TProxy>(serviceProvider, serviceProvider.GetRequiredService<TRealService>()));
    }
}