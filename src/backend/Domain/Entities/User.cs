namespace Domain.Entities;

// Kullanıcı entity'si - NArchitecture Core'deki User sınıfından türetilir
// Guid tipinde ID kullanır
public class User : NArchitecture.Core.Security.Entities.User<Guid>
{
    // Kullanıcının yetki ilişkileri - bir kullanıcı birden fazla yetkiye sahip olabilir
    public virtual ICollection<UserOperationClaim> UserOperationClaims { get; set; } = default!;
    
    // Kullanıcının refresh token'ları - bir kullanıcının birden fazla refresh token'ı olabilir
    public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = default!;
    
    // Kullanıcının OTP doğrulama bilgileri - 2FA için
    public virtual ICollection<OtpAuthenticator> OtpAuthenticators { get; set; } = default!;
    
    // Kullanıcının e-posta doğrulama bilgileri - 2FA için
    public virtual ICollection<EmailAuthenticator> EmailAuthenticators { get; set; } = default!;
}
