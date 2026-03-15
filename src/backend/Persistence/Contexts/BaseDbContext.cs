// Gerekli kütüphaneleri import etme
using System.Reflection;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Persistence.Contexts;

// Entity Framework Core DbContext sınıfı
// Veritabanı tablolarını ve entity'leri yönetir
public class BaseDbContext : DbContext
{
    // Konfigürasyon ayarlarına erişim için property
    protected IConfiguration Configuration { get; set; }
    
    // Veritabanı tablolarını temsil eden DbSet'ler
    // Her DbSet bir veritabanı tablosuna karşılık gelir
    public DbSet<EmailAuthenticator> EmailAuthenticators { get; set; }        // E-posta doğrulama tablosu
    public DbSet<OperationClaim> OperationClaims { get; set; }                // Yetki işlemleri tablosu
    public DbSet<OtpAuthenticator> OtpAuthenticators { get; set; }            // OTP doğrulama tablosu
    public DbSet<RefreshToken> RefreshTokens { get; set; }                    // Refresh token tablosu
    public DbSet<User> Users { get; set; }                                    // Kullanıcılar tablosu
    public DbSet<UserOperationClaim> UserOperationClaims { get; set; }        // Kullanıcı yetki ilişkileri tablosu

    // DbContext constructor'ı - DbContextOptions ve IConfiguration alır
    public BaseDbContext(DbContextOptions dbContextOptions, IConfiguration configuration)
        : base(dbContextOptions)
    {
        Configuration = configuration;
    }

    // Model yapılandırmasını override eden metot
    // Entity Framework'ün entity'leri veritabanı tablolarına nasıl map'leyeceğini belirler
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Mevcut assembly'deki tüm konfigürasyon sınıflarını otomatik uygula
        // Bu sayede her entity için ayrı ayrı konfigürasyon yapılabilir
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
