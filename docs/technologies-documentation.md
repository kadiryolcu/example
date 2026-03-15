# Kullanılan Teknolojiler ve Kod Dokümantasyonu

## 📋 İçindekiler

1. [Authentication & Authorization](#authentication--authorization)
2. [Dependency Injection](#dependency-injection)
3. [CQRS Pattern](#cqrs-pattern)
4. [Object Mapping](#object-mapping)
5. [Validation](#validation)
6. [Caching](#caching)
7. [Logging](#logging)
8. [Mailing](#mailing)
9. [Localization](#localization)
10. [ElasticSearch](#elasticsearch)
11. [Security](#security)
12. [Pipelines](#pipelines)

---

## 🔐 Authentication & Authorization

### **Kullanılan Teknolojiler:**
- **Keycloak** - OpenID Connect / OAuth2
- **JWT Bearer** - Token-based authentication
- **Microsoft.AspNetCore.Authentication.JwtBearer** - .NET JWT middleware

### **Kod Yapılandırması:**

#### **Program.cs - JWT Bearer Authentication:**
```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// JWT Bearer Authentication konfigürasyonu
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakSettings.Authority;
        options.Audience = keycloakSettings.ClientId;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero
        };
        
        // Keycloak connection failure handling
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                // Log authentication failures
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                // Log successful validation
                return Task.CompletedTask;
            }
        };
    });

// Authorization middleware
builder.Services.AddAuthorization();
```

#### **Controller'da Kullanım:**
```csharp
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpGet("userinfo")]
    [Authorize] // JWT token gerektir
    public IActionResult GetUserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return Ok(new
        {
            UserId = userId,
            Email = email,
            Roles = roles,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }
}
```

#### **Keycloak Ayarları (appsettings.json):**
```json
{
  "KeycloakSettings": {
    "Authority": "http://localhost:8080/realms/master",
    "ClientId": "myclient",
    "ClientSecret": "PqOgZTQm2YxcIGT4gpE1UDKhOR91Pkd0",
    "MetadataAddress": "http://localhost:8080/realms/master/.well-known/openid-configuration"
  }
}
```

---

## 🏗️ Dependency Injection

### **Kullanılan Teknolojiler:**
- **Microsoft.Extensions.DependencyInjection** - .NET DI container
- **Service Registration Pattern** - Clean Architecture DI

### **Kod Yapılandırması:**

#### **ApplicationServiceRegistration.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        MailSettings mailSettings,
        FileLogConfiguration fileLogConfiguration,
        ElasticSearchConfig elasticSearchConfig,
        TokenOptions tokenOptions
    )
    {
        // AutoMapper konfigürasyonu
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        
        // MediatR CQRS konfigürasyonu
        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            
            // Pipeline behaviors (AOP)
            configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));
            configuration.AddOpenBehavior(typeof(CachingBehavior<,>));
            configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));
            configuration.AddOpenBehavior(typeof(RequestValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));
        });

        // Business rules otomatik kayıt
        services.AddSubClassesOfType(Assembly.GetExecutingAssembly(), typeof(BaseBusinessRules));

        // Validators otomatik kayıt
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Core servisler (Singleton)
        services.AddSingleton<IMailService, MailKitMailService>(_ => 
            new MailKitMailService(mailSettings));
        services.AddSingleton<ILogger, SerilogFileLogger>(_ => 
            new SerilogFileLogger(fileLogConfiguration));
        services.AddSingleton<IElasticSearch, ElasticSearchManager>(_ => 
            new ElasticSearchManager(elasticSearchConfig));

        // Application servisler (Scoped)
        services.AddScoped<IAuthService, AuthManager>();
        services.AddScoped<IAuthenticatorService, AuthenticatorManager>();
        services.AddScoped<IUserService, UserManager>();

        // Localization
        services.AddYamlResourceLocalization();

        // Security services
        services.AddSecurityServices<Guid, int, Guid>(tokenOptions);

        return services;
    }

    // Alt sınıfları otomatik olarak DI'ye ekleme
    public static IServiceCollection AddSubClassesOfType(
        this IServiceCollection services,
        Assembly assembly,
        Type type,
        Func<IServiceCollection, Type, IServiceCollection>? addWithLifeCycle = null
    )
    {
        var types = assembly.GetTypes().Where(t => t.IsSubclassOf(type) && type != t).ToList();
        foreach (Type? item in types)
            if (addWithLifeCycle == null)
                services.AddScoped(item);
            else
                addWithLifeCycle(services, type);
        return services;
    }
}
```

---

## 🔄 CQRS Pattern

### **Kullanılan Teknolojiler:**
- **MediatR** - Mediator pattern implementation
- **IRequest/IResponse** - Command/Query interfaces
- **Pipeline Behaviors** - Cross-cutting concerns

### **Kod Yapılandırması:**

#### **Command Örneği:**
```csharp
using MediatR;
using NArchitecture.Core.Application.Requests;
using NArchitecture.Core.Application.Responses;

public class CreateUserCommand : IRequest<CreatedUserResponse>, ISecuredRequest
{
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    
    public string[] Roles => [UsersOperationClaims.Admin];
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreatedUserResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<CreatedUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        User user = _mapper.Map<User>(request);
        await _userRepository.AddAsync(user);
        
        CreatedUserResponse response = _mapper.Map<CreatedUserResponse>(user);
        return response;
    }
}
```

#### **Query Örneği:**
```csharp
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;

public class GetListUserQuery : IRequest<GetListResponse<GetListUserListItemDto>>, ISecuredRequest
{
    public PageRequest PageRequest { get; set; }

    public string[] Roles => [UsersOperationClaims.Read];
}

public class GetListUserQueryHandler : IRequestHandler<GetListUserQuery, GetListResponse<GetListUserListItemDto>>
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public GetListUserQueryHandler(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<GetListResponse<GetListUserListItemDto>> Handle(
        GetListUserQuery request, 
        CancellationToken cancellationToken)
    {
        IPaginate<User> users = await _userRepository.GetListAsync(
            index: request.PageRequest.PageIndex,
            size: request.PageRequest.PageSize,
            enableTracking: false,
            cancellationToken: cancellationToken
        );

        GetListResponse<GetListUserListItemDto> response = _mapper
            .Map<GetListResponse<GetListUserListItemDto>>(users);
        return response;
    }
}
```

---

## 🔄 Object Mapping

### **Kullanılan Teknolojiler:**
- **AutoMapper 12.0.1** - Object-to-object mapping
- **AutoMapper.Extensions.Microsoft.DependencyInjection** - DI integration

### **Kod Yapılandırması:**

#### **Mapping Profile:**
```csharp
using AutoMapper;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // Entity -> DTO mapping
        CreateMap<User, CreatedUserResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        // DTO -> Entity mapping
        CreateMap<CreateUserCommand, User>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(_ => DateTime.UtcNow));

        // Reverse mapping (iki yönlü)
        CreateMap<User, GetUserResponse>().ReverseMap();

        // Complex mapping
        CreateMap<IPaginate<User>, GetListResponse<GetUserListItemDto>>();
    }
}
```

#### **Service Registration:**
```csharp
// ApplicationServiceRegistration.cs
services.AddAutoMapper(Assembly.GetExecutingAssembly());
```

#### **Handler'da Kullanım:**
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, CreatedUserResponse>
{
    private readonly IMapper _mapper;

    public CreateUserCommandHandler(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task<CreatedUserResponse> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // DTO -> Entity
        User user = _mapper.Map<User>(request);
        
        // Entity -> Response DTO
        CreatedUserResponse response = _mapper.Map<CreatedUserResponse>(user);
        
        return response;
    }
}
```

---

## ✅ Validation

### **Kullanılan Teknolojiler:**
- **FluentValidation** - Validation framework
- **IValidator** - Validation interface

### **Kod Yapılandırması:**

#### **Validator Örneği:**
```csharp
using FluentValidation;
using Application.Features.Users.Commands.Create;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi boş olamaz")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad boş olamaz")
            .MinimumLength(2).WithMessage("Ad en az 2 karakter olmalı")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad boş olamaz")
            .MinimumLength(2).WithMessage("Soyad en az 2 karakter olmalı")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre boş olamaz")
            .MinimumLength(8).WithMessage("Şifre en az 8 karakter olmalı")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]")
            .WithMessage("Şifre en az bir büyük harf, bir küçük harf, bir rakam ve bir özel karakter içermelidir");
    }
}
```

#### **Service Registration:**
```csharp
// ApplicationServiceRegistration.cs
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
```

---

## 🚀 Caching

### **Kullanılan Teknolojiler:**
- **Microsoft.Extensions.Caching.Memory** - In-memory caching
- **Pipeline Behavior** - AOP caching

### **Kod Yapılandırması:**

#### **Caching Behavior:**
```csharp
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using NArchitecture.Core.Application.Pipelines.Caching;

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachableRequest
{
    private readonly IMemoryCache _memoryCache;

    public CachingBehavior(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        string cacheKey = request.CacheKey;

        if (_memoryCache.TryGetValue(cacheKey, out TResponse? cachedResponse))
        {
            return cachedResponse!;
        }

        TResponse response = await next(request, cancellationToken);

        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(request.CacheDuration),
            SlidingExpiration = TimeSpan.FromMinutes(request.SlidingExpiration)
        };

        _memoryCache.Set(cacheKey, response, cacheOptions);

        return response;
    }
}
```

#### **Cacheable Request:**
```csharp
public class GetListUserQuery : IRequest<GetListResponse<GetListUserListItemDto>>, 
                                   ISecuredRequest, 
                                   ICachableRequest
{
    public PageRequest PageRequest { get; set; }
    
    public string CacheKey => $"GetListUser_{PageRequest.PageIndex}_{PageRequest.PageSize}";
    public int CacheDuration => 30; // 30 dakika
    public int SlidingExpiration => 10; // 10 dakika sliding
    
    public string[] Roles => [UsersOperationClaims.Read];
}
```

#### **appsettings.json:**
```json
{
  "CacheSettings": {
    "SlidingExpiration": 2
  }
}
```

---

## 📝 Logging

### **Kullanılan Teknolojiler:**
- **Serilog** - Structured logging
- **Serilog.Sinks.File** - File logging
- **ElasticSearch** - Centralized logging

### **Kod Yapılandırması:**

#### **Serilog Configuration:**
```csharp
using Serilog;
using Serilog.Core;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Serilog.File;

public class SerilogFileLogger : ILogger
{
    private readonly Logger _logger;

    public SerilogFileLogger(FileLogConfiguration fileLogConfiguration)
    {
        _logger = new LoggerConfiguration()
            .WriteTo.File(
                path: fileLogConfiguration.FolderPath + "\\logs-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
                restrictedToMinimumLevel: GetSerilogLogLevel(fileLogConfiguration.LogLevel)
            )
            .CreateLogger();
    }

    public void Log(LogDetail logDetail)
    {
        if (logDetail.LogLevel == LogLevel.Information)
            _logger.Information(logDetail.Message, logDetail.Exception);
        else if (logDetail.LogLevel == LogLevel.Warning)
            _logger.Warning(logDetail.Message, logDetail.Exception);
        else if (logDetail.LogLevel == LogLevel.Error)
            _logger.Error(logDetail.Message, logDetail.Exception);
    }

    private Serilog.Events.LogEventLevel GetSerilogLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Information => Serilog.Events.LogEventLevel.Information,
            LogLevel.Warning => Serilog.Events.LogEventLevel.Warning,
            LogLevel.Error => Serilog.Events.LogEventLevel.Error,
            _ => Serilog.Events.LogEventLevel.Information
        };
    }
}
```

#### **Logging Pipeline Behavior:**
```csharp
using MediatR;
using NArchitecture.Core.Application.Pipelines.Logging;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger _logger;

    public LoggingBehavior(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.Info($"Handling {typeof(TRequest).Name}");

        try
        {
            TResponse response = await next(request, cancellationToken);
            
            _logger.Info($"Handled {typeof(TRequest).Name} successfully");
            
            return response;
        }
        catch (Exception ex)
        {
            _logger.Error($"Error handling {typeof(TRequest).Name}: {ex.Message}", ex);
            throw;
        }
    }
}
```

---

## 📧 Mailing

### **Kullanılan Teknolojiler:**
- **MailKit** - Cross-platform email library
- **MimeKit** - MIME message creation

### **Kod Yapılandırması:**

#### **Mail Service:**
```csharp
using MailKit.Net.Smtp;
using MimeKit;
using NArchitecture.Core.Mailing;

public class MailKitMailService : IMailService
{
    private readonly MailSettings _mailSettings;

    public MailKitMailService(MailSettings mailSettings)
    {
        _mailSettings = mailSettings;
    }

    public async Task SendEmailAsync(Mail mail)
    {
        using var email = new MimeMessage();
        email.From.Add(new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail));
        email.To.AddRange(mail.ToAddresses.Select(x => new MailboxAddress(x)));
        email.Subject = mail.Subject;
        
        var bodyBuilder = new BodyBuilder { HtmlBody = mail.HtmlBody };
        if (!string.IsNullOrEmpty(mail.TextBody))
            bodyBuilder.TextBody = mail.TextBody;
        
        email.Body = bodyBuilder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_mailSettings.SmtpServer, _mailSettings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync(_mailSettings.Username, _mailSettings.Password);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
```

#### **Mail Settings:**
```json
{
  "MailSettings": {
    "SenderName": "Example App",
    "SenderEmail": "noreply@example.com",
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "your-email@gmail.com",
    "Password": "your-app-password"
  }
}
```

---

## 🌍 Localization

### **Kullanılan Teknolojiler:**
- **Yaml** - Configuration files for localization
- **Resource-based localization** - Multi-language support

### **Kod Yapılandırması:**

#### **Localization Service Registration:**
```csharp
using NArchitecture.Core.Localization.Resource.Yaml.DependencyInjection;

// ApplicationServiceRegistration.cs
services.AddYamlResourceLocalization();
```

#### **Yaml Resource Files:**
```yaml
# Resources/Locales/tr-TR.yaml
welcome_message: "Hoş Geldiniz"
user_not_found: "Kullanıcı bulunamadı"
invalid_credentials: "Geçersiz kimlik bilgileri"
operation_successful: "İşlem başarılı"

# Resources/Locales/en-US.yaml
welcome_message: "Welcome"
user_not_found: "User not found"
invalid_credentials: "Invalid credentials"
operation_successful: "Operation successful"
```

---

## 🔍 ElasticSearch

### **Kullanılan Teknolojiler:**
- **NEST** - Elasticsearch .NET client
- **Elasticsearch.Net** - Low-level client

### **Kod Yapılandırması:**

#### **ElasticSearch Manager:**
```csharp
using NArchitecture.Core.ElasticSearch;
using Nest;

public class ElasticSearchManager : IElasticSearch
{
    private readonly ElasticClient _elasticClient;

    public ElasticSearchManager(ElasticSearchConfig elasticSearchConfig)
    {
        var settings = new ConnectionSettings(new Uri(elasticSearchConfig.ConnectionString))
            .DefaultIndex(elasticSearchConfig.IndexName)
            .BasicAuthentication(elasticSearchConfig.UserName, elasticSearchConfig.Password);

        _elasticClient = new ElasticClient(settings);
    }

    public async Task SaveDocumentAsync<T>(T document) where T : class
    {
        var response = await _elasticClient.IndexDocumentAsync(document);
        return response.IsValid;
    }

    public async Task<List<T>> SearchAsync<T>(SearchRequest searchRequest) where T : class
    {
        var searchResponse = await _elasticClient.SearchAsync<T>(s => s
            .Query(q => q
                .MultiMatch(mm => mm
                    .Fields(searchRequest.Fields.ToArray())
                    .Query(searchRequest.Query)
                )
            )
            .From(searchRequest.From)
            .Size(searchRequest.Size)
        );

        return searchResponse.Documents.ToList();
    }
}
```

#### **ElasticSearch Configuration:**
```json
{
  "ElasticSearchConfig": {
    "ConnectionString": "http://localhost:9200",
    "IndexName": "logs",
    "UserName": "",
    "Password": ""
  }
}
```

---

## 🔒 Security

### **Kullanılan Teknolojiler:**
- **Microsoft.IdentityModel.Tokens** - JWT token handling
- **System.Security.Cryptography** - Cryptographic operations

### **Kod Yapılandırması:**

#### **JWT Token Service:**
```csharp
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

public class JwtHelper : ITokenHelper
{
    private readonly TokenOptions _tokenOptions;

    public JwtHelper(TokenOptions tokenOptions)
    {
        _tokenOptions = tokenOptions;
    }

    public AccessToken CreateToken(User user, IList<Claim> operationClaims)
    {
        var securityKey = SecurityKeyHelper.CreateSecurityKey(_tokenOptions.SecurityKey);
        var signingCredentials = SigningCredentialsHelper.CreateSigningCredentials(securityKey);
        
        var jwt = CreateJwtSecurityToken(user, signingCredentials, operationClaims);
        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        var token = jwtSecurityTokenHandler.WriteToken(jwt);

        return new AccessToken
        {
            Token = token,
            Expiration = jwt.ValidTo,
            RefreshToken = CreateRefreshToken(),
            RefreshTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOptions.RefreshTokenExpiration)
        };
    }

    private JwtSecurityToken CreateJwtSecurityToken(User user, SigningCredentials signingCredentials, IList<Claim> operationClaims)
    {
        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpiration),
            notBefore: DateTime.UtcNow,
            claims: SetClaims(user, operationClaims),
            signingCredentials: signingCredentials
        );
        return jwt;
    }

    private IEnumerable<Claim> SetClaims(User user, IList<Claim> operationClaims)
    {
        var claims = new List<Claim>();
        
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Email, user.Email));
        claims.Add(new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"));
        
        if (operationClaims != null)
            claims.AddRange(operationClaims);

        return claims;
    }
}
```

---

## 🔄 Pipelines

### **Kullanılan Teknolojiler:**
- **MediatR Pipeline Behaviors** - AOP implementation
- **Cross-cutting concerns** - Separation of concerns

### **Kod Yapılandırması:**

#### **Authorization Pipeline:**
```csharp
using MediatR;
using NArchitecture.Core.Application.Pipelines.Authorization;

public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ISecuredRequest
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthorizationBehavior(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var userRoles = _httpContextAccessor.HttpContext?.User?.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        var requiredRoles = request.Roles;

        if (!requiredRoles.Any(role => userRoles.Contains(role)))
        {
            throw new AuthorizationException("Bu işlem için yetkiniz bulunmamaktadır.");
        }

        return await next(request, cancellationToken);
    }
}
```

#### **Validation Pipeline:**
```csharp
using MediatR;
using FluentValidation;
using NArchitecture.Core.Application.Pipelines.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken))
            );

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return await next(request, cancellationToken);
    }
}
```

#### **Transaction Pipeline:**
```csharp
using MediatR;
using NArchitecture.Core.Application.Pipelines.Transaction;
using System.Transactions;

public class TransactionScopeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
        var response = await next(request, cancellationToken);
        transactionScope.Complete();
        return response;
    }
}
```

---

## 📊 Pipeline Configuration Order

### **MediatR Pipeline Sırası:**
```csharp
services.AddMediatR(configuration =>
{
    configuration.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    
    // Pipeline behaviors (çalışma sırası önemli)
    configuration.AddOpenBehavior(typeof(RequestValidationBehavior<,>));      // 1. Validation
    configuration.AddOpenBehavior(typeof(AuthorizationBehavior<,>));        // 2. Authorization
    configuration.AddOpenBehavior(typeof(CachingBehavior<,>));             // 3. Caching
    configuration.AddOpenBehavior(typeof(LoggingBehavior<,>));            // 4. Logging
    configuration.AddOpenBehavior(typeof(TransactionScopeBehavior<,>));     // 5. Transaction
});
```

### **Pipeline Akışı:**
1. **Request Validation** - Input validasyonu
2. **Authorization** - Yetki kontrolü
3. **Caching** - Cache kontrolü
4. **Logging** - Loglama
5. **Transaction** - Veritabanı transaction
6. **Handler Execution** - Asıl iş mantığı

---

## 🎯 Best Practices

### **📝 Kod Standartları:**
- Her pipeline behavior tek sorumluluk taşımalı
- Validation business logic'den ayrı olmalı
- Authorization merkezi yönetilmeli
- Logging structured olmalı

### **🔒 Güvenlik:**
- JWT token'ları güvenli bir şekilde sakla
- Sensitive verileri loglama
- Rate limiting implement et
- Input validation kullan

### **⚡ Performans:**
- Cache stratejisi optimize et
- Async/await pattern kullan
- Database connection pooling
- Lazy loading yerine eager loading

### **🧪 Test:**
- Unit test her pipeline için
- Integration test endpoint'ler için
- Mock external dependencies
- Test coverage > 80%

---

## 🚀 Özet

Bu doküman, projede kullanılan tüm teknolojilerin detaylı açıklamalarını ve kod örneklerini içermektedir. Her teknoloji:

- ✅ **Kullanım amacı**
- ✅ **Konfigürasyon adımları**
- ✅ **Kod örnekleri**
- ✅ **Best practices**
- ✅ **Integration noktaları**

ile tam olarak dokümante edilmiştir.
