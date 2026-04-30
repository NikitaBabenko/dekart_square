using DecisionHelper.Core.Limits;
using DecisionHelper.Infrastructure.Limits;
using DecisionHelper.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DecisionHelper.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddDecisionHelperInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddDbContext<AppDbContext>(o => o.UseNpgsql(connectionString));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<IUsageLimiter, PgUsageLimiter>();
        return services;
    }
}
