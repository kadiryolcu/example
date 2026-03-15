// Gerekli kütüphaneleri import etme
using System.Reflection;
using Application.Services.AuthenticatorService;
using Application.Services.AuthService;
using Application.Services.UsersService;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NArchitecture.Core.Application.Pipelines.Authorization;
using NArchitecture.Core.Application.Pipelines.Caching;
using NArchitecture.Core.Application.Pipelines.Logging;
using NArchitecture.Core.Application.Pipelines.Transaction;
using NArchitecture.Core.Application.Pipelines.Validation;
using NArchitecture.Core.Application.Rules;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Abstraction;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Configurations;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Serilog.File;
using NArchitecture.Core.ElasticSearch;
using NArchitecture.Core.ElasticSearch.Models;
using NArchitecture.Core.Localization.Resource.Yaml.DependencyInjection;
using NArchitecture.Core.Mailing;
using NArchitecture.Core.Mailing.MailKit;
using NArchitecture.Core.Security.DependencyInjection;
using NArchitecture.Core.Security.JWT;

namespace Application;

// Application katmanı servislerinin Dependency Injection yapılandırması
public static class ApplicationServiceRegistration
{
    // Application servislerini ekle
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        MailSettings mailSettings,
        FileLogConfiguration fileLogConfiguration,
        ElasticSearchConfig elasticSearchConfig,
        TokenOptions tokenOptions
    )
    {
        // AutoMapper'ı yapılandır - mevcut assembly'deki profilleri otomatik tespit eder
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // MediatR'ı yapılandır - CQRS pattern için
        services.AddMediatR(configuration =>
        {
            // Mevcut assembly'deki servisleri kaydet
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Pipeline behavior'ları ekle (AOP - Aspect Oriented Programming)
            configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));      // Yetkilendirme
            configuration.AddOpenBehavior(typeof(CachingBehavior<,>));           // Önbelleğe alma
            configuration.AddOpenBehavior(typeof(CacheRemovingBehavior<,>));      // Önbellek temizleme
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));             // Loglama
            configuration.AddOpenBehavior(typeof(RequestValidationBehavior<,>));   // Validasyon
            configuration.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));    // Transaction yönetimi
        });

        // BaseBusinessRules sınıfından türeyen tüm sınıfları otomatik olarak servis olarak ekle
        services.AddSubClassesOfType(Assembly.GetExecutingAssembly(), typeof(BaseBusinessRules));

        // Mevcut assembly'deki tüm validatörleri otomatik olarak ekle
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Core servisleri singleton olarak kaydet
        // Mail servisi - MailKit kullanarak e-posta gönderimi
        services.AddSingleton<IMailService, MailKitMailService>(_ => new MailKitMailService(mailSettings));
        // Log servisi - Serilog kullanarak dosyaya loglama
        services.AddSingleton<ILogger, SerilogFileLogger>(_ => new SerilogFileLogger(fileLogConfiguration));
        // ElasticSearch servisi - loglama ve arama için
        services.AddSingleton<IElasticSearch, ElasticSearchManager>(_ => new ElasticSearchManager(elasticSearchConfig));

        // Application servislerini scoped olarak kaydet
        // Authentication servisi - kullanıcı girişi ve token yönetimi
        services.AddScoped<IAuthService, AuthManager>();
        // Authenticator servisi - 2FA doğrulama işlemleri
        services.AddScoped<IAuthenticatorService, AuthenticatorManager>();
        // User servisi - kullanıcı yönetimi işlemleri
        services.AddScoped<IUserService, UserManager>();

        // Yerelleştirme servisini ekle - YAML dosyalarından dil desteği
        services.AddYamlResourceLocalization();

        // Güvenlik servislerini ekle - JWT token yönetimi
        services.AddSecurityServices<Guid, int, Guid>(tokenOptions);

        return services;
    }

    // Belirtilen tipten türeyen tüm sınıfları servis olarak ekle
    public static IServiceCollection AddSubClassesOfType(
        this IServiceCollection services,
        Assembly assembly,
        Type type,
        Func<IServiceCollection, Type, IServiceCollection>? addWithLifeCycle = null
    )
    {
        // Assembly'de belirtilen tipten türeyen tüm sınıfları bul
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && type != t).ToList();
        // Her bir sınıfı servis olarak ekle
        foreach (Type? item in types)
            if (addWithLifeCycle == null)
                services.AddScoped(item);  // Varsayılan olarak scoped
            else
                addWithLifeCycle(services, type);  // Özel yaşam döngüsü
        return services;
    }
}
