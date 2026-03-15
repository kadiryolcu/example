// Gerekli kütüphaneleri import etme
using Application.Features.Auth.Rules;
using Application.Services.AuthenticatorService;
using Application.Services.AuthService;
using Application.Services.UsersService;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Dtos;
using NArchitecture.Core.Security.Enums;
using NArchitecture.Core.Security.JWT;

namespace Application.Features.Auth.Commands.Login;

// Kullanıcı giriş komutu - CQRS pattern'de Command sınıfı
// IRequest<LoggedResponse> interface'ini implement eder, dönüş tipi LoggedResponse'dur
public class LoginCommand : IRequest<LoggedResponse>
{
    // Kullanıcı giriş bilgilerini içeren DTO
    public UserForLoginDto UserForLoginDto { get; set; }
    // Kullanıcının IP adresi - güvenlik için kullanılır
    public string IpAddress { get; set; }

    // Parametresiz constructor
    public LoginCommand()
    {
        UserForLoginDto = null!; // Null-forgiving operator - bu property'nin null olmayacağını belirtir
        IpAddress = string.Empty;
    }

    // Parametreli constructor
    public LoginCommand(UserForLoginDto userForLoginDto, string ipAddress)
    {
        UserForLoginDto = userForLoginDto;
        IpAddress = ipAddress;
    }

    // Login komutunu işleyen Handler sınıfı
    // IRequestHandler<LoginCommand, LoggedResponse> interface'ini implement eder
    public class LoginCommandHandler : IRequestHandler<LoginCommand, LoggedResponse>
    {
        // Dependency Injection ile servisler
        private readonly AuthBusinessRules _authBusinessRules;              // Authentication iş kuralları
        private readonly IAuthenticatorService _authenticatorService;      // 2FA doğrulama servisi
        private readonly IAuthService _authService;                      // Authentication servisi
        private readonly IUserService _userService;                        // Kullanıcı servisi

        // Constructor - dependency injection ile servisleri alır
        public LoginCommandHandler(
            IUserService userService,
            IAuthService authService,
            AuthBusinessRules authBusinessRules,
            IAuthenticatorService authenticatorService
        )
        {
            _userService = userService;
            _authService = authService;
            _authBusinessRules = authBusinessRules;
            _authenticatorService = authenticatorService;
        }

        // Komutu işleyen ana metot
        public async Task<LoggedResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            // E-posta ile kullanıcıyı veritabanından ara
            User? user = await _userService.GetAsync(
                predicate: u => u.Email == request.UserForLoginDto.Email,
                cancellationToken: cancellationToken
            );
            // İş kurallarını kontrol et
            await _authBusinessRules.UserShouldBeExistsWhenSelected(user);           // Kullanıcı var mı?
            await _authBusinessRules.UserPasswordShouldBeMatch(user!, request.UserForLoginDto.Password); // Şifre doğru mu?

            // Giriş response'u oluştur
            LoggedResponse loggedResponse = new();

            // Eğer kullanıcı 2FA kullanıyorsa
            if (user!.AuthenticatorType is not AuthenticatorType.None)
            {
                // Eğer doğrulama kodu girilmemişse
                if (request.UserForLoginDto.AuthenticatorCode is null)
                {
                    // Doğrulama kodu gönder
                    await _authenticatorService.SendAuthenticatorCode(user);
                    // Gerekli doğrulama tipini response'a ekle
                    loggedResponse.RequiredAuthenticatorType = user.AuthenticatorType;
                    return loggedResponse;
                }

                // Doğrulama kodunu kontrol et
                await _authenticatorService.VerifyAuthenticatorCode(user, request.UserForLoginDto.AuthenticatorCode);
            }

            // Access token oluştur
            AccessToken createdAccessToken = await _authService.CreateAccessToken(user);

            // Refresh token oluştur
            Domain.Entities.RefreshToken createdRefreshToken = await _authService.CreateRefreshToken(user, request.IpAddress);
            // Refresh token'ı veritabanına ekle
            Domain.Entities.RefreshToken addedRefreshToken = await _authService.AddRefreshToken(createdRefreshToken);
            // Eski refresh token'ları temizle
            await _authService.DeleteOldRefreshTokens(user.Id);

            // Response'u doldur ve geri dön
            loggedResponse.AccessToken = createdAccessToken;
            loggedResponse.RefreshToken = addedRefreshToken;
            return loggedResponse;
        }
    }
}
