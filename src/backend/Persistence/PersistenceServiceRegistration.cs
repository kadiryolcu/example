// Gerekli kütüphaneleri import etme
using Application.Services.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NArchitecture.Core.Persistence.DependencyInjection;
using Persistence.Contexts;
using Persistence.Repositories;

namespace Persistence;

// Persistence (veri erişim) katmanı servislerinin Dependency Injection yapılandırması
public static class PersistenceServiceRegistration
{
    // Persistence servislerini ekle
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Entity Framework Core DbContext'ini yapılandır
        // SQL Server kullanarak konfigürasyondan connection string ile veritabanı bağlantısı
        services.AddDbContext<BaseDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("BaseDb")));
        
        // Veritabanı migration'larını otomatik uygulama servisini ekle
        services.AddDbMigrationApplier(buildServices => buildServices.GetRequiredService<BaseDbContext>());

        // Repository'leri scoped olarak kaydet
        // Her repository ilgili entity için veritabanı işlemlerini yönetir
        services.AddScoped<IEmailAuthenticatorRepository, EmailAuthenticatorRepository>();
        services.AddScoped<IOperationClaimRepository, OperationClaimRepository>();
        services.AddScoped<IOtpAuthenticatorRepository, OtpAuthenticatorRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserOperationClaimRepository, UserOperationClaimRepository>();

        return services;
    }
}
