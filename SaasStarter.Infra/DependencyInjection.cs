using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using SaasStarter.Application.Common.Interfaces;
using SaasStarter.Domain.Identity;
using SaasStarter.Infra.Persistence;
using SaasStarter.Infra.Repositories;
using SaasStarter.Infra.Services;

namespace SaasStarter.Infra;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core + PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // ASP.NET Core Identity
        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddRoles<IdentityRole<Guid>>()
        .AddEntityFrameworkStores<AppDbContext>()
        .AddDefaultTokenProviders();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());

        // Repositories
        services.AddScoped<IUserSubscriptionRepository, UserSubscriptionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IPromotionCodeRepository, PromotionCodeRepository>();

        // TODO: registre aqui os repositórios das suas entidades de negócio

        // Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IStripeService, StripeService>();
        services.AddScoped<IGoogleTokenValidator, GoogleTokenValidator>();

        // Resend (e-mail transacional)
        services.AddOptions<ResendClientOptions>().Configure(o =>
        {
            o.ApiToken = configuration["Resend:ApiKey"]
                ?? throw new InvalidOperationException("Resend:ApiKey não configurado.");
        });
        services.AddHttpClient<IResend, ResendClient>();
        services.AddScoped<IEmailSenderService, EmailSenderService>();

        return services;
    }
}
