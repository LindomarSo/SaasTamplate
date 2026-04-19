using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SaasStarter.Application.Services.Auth;
using SaasStarter.Application.Services.Payments;
using SaasStarter.Application.Services.Plans;
using SaasStarter.Application.Services.Promotions;
using System.Reflection;

namespace SaasStarter.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IPlanService, PlanService>();
        services.AddScoped<IPromotionService, PromotionService>();

        // TODO: registre aqui os seus services de negócio específicos
        // Exemplo:
        // services.AddScoped<IMyFeatureService, MyFeatureService>();

        return services;
    }
}
