using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

// Gerekli kütüphaneleri import etme
using Application;
using Infrastructure;
using Microsoft.OpenApi.Models;
using NArchitecture.Core.CrossCuttingConcerns.Exception.WebApi.Extensions;
using NArchitecture.Core.CrossCuttingConcerns.Logging.Configurations;
using NArchitecture.Core.ElasticSearch.Models;
using NArchitecture.Core.Localization.WebApi;
using NArchitecture.Core.Mailing;
using NArchitecture.Core.Persistence.WebApi;
using NArchitecture.Core.Security.Encryption;
using NArchitecture.Core.Security.JWT;
using NArchitecture.Core.Security.WebApi.Swagger.Extensions;
using Persistence;
using Swashbuckle.AspNetCore.SwaggerUI;
using WebAPI;

// Web uygulaması builder'ını oluştur
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Controller servislerini ekle
builder.Services.AddControllers();
// Application katmanı servislerini yapılandır
// Mail ayarları, loglama, ElasticSearch ve JWT token ayarlarını konfigürasyondan al
builder.Services.AddApplicationServices(
    mailSettings: builder.Configuration.GetSection("MailSettings").Get<MailSettings>()
        ?? throw new InvalidOperationException("MailSettings section cannot found in configuration."),
    fileLogConfiguration: builder
        .Configuration.GetSection("SeriLogConfigurations:FileLogConfiguration")
        .Get<FileLogConfiguration>()
        ?? throw new InvalidOperationException("FileLogConfiguration section cannot found in configuration."),
    elasticSearchConfig: builder.Configuration.GetSection("ElasticSearchConfig").Get<ElasticSearchConfig>()
        ?? throw new InvalidOperationException("ElasticSearchConfig section cannot found in configuration."),
    tokenOptions: builder.Configuration.GetSection("TokenOptions").Get<TokenOptions>()
        ?? throw new InvalidOperationException("TokenOptions section cannot found in configuration.")
);

// Persistence (veri erişim) katmanı servislerini ekle
builder.Services.AddPersistenceServices(builder.Configuration);

// Infrastructure katmanı servislerini ekle
builder.Services.AddInfrastructureServices();

// HTTP context accessor servisini ekle
builder.Services.AddHttpContextAccessor();

// Keycloak OpenID Connect authentication'ı yapılandır
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var keycloakSettings = builder.Configuration.GetSection("KeycloakSettings");
    
    options.Authority = keycloakSettings["Authority"];
    options.RequireHttpsMetadata = false;
    options.Audience = "account";
    
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateAudience = true,
        ValidAudience = "account",
        NameClaimType = "preferred_username",
        RoleClaimType = "roles"
    };
    
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => 
        {
            // Authentication başarısız olursa
            return Task.CompletedTask;
        },
        OnTokenValidated = context => 
        {
            // Token validated
            return Task.CompletedTask;
        }
    };
});

// Dağıtık bellek önbelleği servisini ekle
builder.Services.AddDistributedMemoryCache();

// API endpoint'lerini keşfetmek için servisleri ekle
builder.Services.AddEndpointsApiExplorer();

// CORS (Cross-Origin Resource Sharing) politikasını yapılandır
// Tüm origin'lere, metotlara ve header'lara izin ver
builder.Services.AddCors(opt =>
    opt.AddDefaultPolicy(p =>
    {
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    })
);
// Swagger/OpenAPI belgelendirmesini yapılandır
builder.Services.AddSwaggerGen(opt =>
{
    // Bearer token güvenlik tanımını ekle
    opt.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"{builder.Configuration["KeycloakSettings:Authority"]}/protocol/openid-connect/auth"),
                TokenUrl = new Uri($"{builder.Configuration["KeycloakSettings:Authority"]}/protocol/openid-connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenID" },
                    { "profile", "Profile" },
                    { "email", "Email" },
                    { "roles", "Roles" }
                }
            }
        }
    });
    
    // OAuth2 güvenlik gereksinimini operation'lara ekle
    opt.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Web uygulamasını oluştur
WebApplication app = builder.Build();

// Eğer development ortamı ise Swagger'ı etkinleştir
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(); // Swagger UI'ı etkinleştir
    app.UseSwaggerUI(opt =>
    {
        opt.DocExpansion(DocExpansion.None); // Dokümanları varsayılan olarak kapalı tut
        // OAuth2 yapılandırması
        opt.OAuthClientId(builder.Configuration["KeycloakSettings:ClientId"]);
        opt.OAuthAppName("Backend API");
        opt.OAuthUsePkce();
    });
}

// Eğer production ortamı ise özel exception middleware'ini yapılandır
if (app.Environment.IsProduction())
    app.ConfigureCustomExceptionMiddleware();

// Veritabanı migration'larını otomatik uygula
app.UseDbMigrationApplier();

// Authentication (kimlik doğrulama) middleware'ini kullan
app.UseAuthentication();
// Authorization (yetkilendirme) middleware'ini kullan
app.UseAuthorization();

// Controller'ları endpoint olarak map'le
app.MapControllers();

// WebAPI konfigürasyon bölümünü belirle
const string webApiConfigurationSection = "WebAPIConfiguration";
// Konfigürasyondan WebAPI ayarlarını al
WebApiConfiguration webApiConfiguration =
    app.Configuration.GetSection(webApiConfigurationSection).Get<WebApiConfiguration>()
    ?? throw new InvalidOperationException($"\"{webApiConfigurationSection}\" section cannot found in configuration.");
// CORS politikasını izin verilen origin'lerle yapılandır
app.UseCors(opt => opt.WithOrigins(webApiConfiguration.AllowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials());

// Response yerelleştirmesini kullan
app.UseResponseLocalization();

// Uygulamayı çalıştır
app.Run();
