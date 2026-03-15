// Gerekli kütüphaneleri import etme
using Application.Features.Auth.Rules;
using Application.Services.AuthService;
using Application.Services.Repositories;
using Domain.Entities;
using MediatR;
using NArchitecture.Core.Application.Dtos;
using NArchitecture.Core.Security.Hashing;
using NArchitecture.Core.Security.JWT;

namespace Application.Features.Auth.Commands.Register;

// Kullanıcı kayıt komutu - CQRS pattern'de Command sınıfı
// IRequest<RegisteredResponse> interface'ini implement eder, dönüş tipi RegisteredResponse'dur
public class RegisterCommand : IRequest<RegisteredResponse>
{
    // Kullanıcı kayıt bilgilerini içeren DTO
    public UserForRegisterDto UserForRegisterDto { get; set; }
    // Kullanıcının IP adresi - güvenlik için kullanılır
    public string IpAddress { get; set; }

    // Parametresiz constructor
    public RegisterCommand()
    {
        UserForRegisterDto = null!; // Null-forgiving operator - bu property'nin null olmayacağını belirtir
        IpAddress = string.Empty;
    }

    // Parametreli constructor
    public RegisterCommand(UserForRegisterDto userForRegisterDto, string ipAddress)
    {
        UserForRegisterDto = userForRegisterDto;
        IpAddress = ipAddress;
    }

    // Register komutunu işleyen Handler sınıfı
    // IRequestHandler<RegisterCommand, RegisteredResponse> interface'ini implement eder
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisteredResponse>
    {
        // Dependency Injection ile servisler
        private readonly IUserRepository _userRepository;    // Kullanıcı repository'si
        private readonly IAuthService _authService;          // Authentication servisi
        private readonly AuthBusinessRules _authBusinessRules; // Authentication iş kuralları

        // Constructor - dependency injection ile servisleri alır
        public RegisterCommandHandler(
            IUserRepository userRepository,
            IAuthService authService,
            AuthBusinessRules authBusinessRules
        )
        {
            _userRepository = userRepository;
            _authService = authService;
            _authBusinessRules = authBusinessRules;
        }

        // Komutu işleyen ana metot
        public async Task<RegisteredResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            // İş kuralını kontrol et - e-posta adresi daha önce kullanılmış mı?
            await _authBusinessRules.UserEmailShouldBeNotExists(request.UserForRegisterDto.Email);

            // Şifreyi hash'le - güvenlik için düz metin şifre saklanmaz
            HashingHelper.CreatePasswordHash(
                request.UserForRegisterDto.Password,
                passwordHash: out byte[] passwordHash,    // Hash'lenmiş şifre
                passwordSalt: out byte[] passwordSalt     // Şifre tuzu
            );
            // Yeni kullanıcı entity'si oluştur
            User newUser =
                new()
                {
                    Email = request.UserForRegisterDto.Email,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                };
            // Kullanıcıyı veritabanına ekle
            User createdUser = await _userRepository.AddAsync(newUser);

            // Access token oluştur
            AccessToken createdAccessToken = await _authService.CreateAccessToken(createdUser);

            // Refresh token oluştur
            Domain.Entities.RefreshToken createdRefreshToken = await _authService.CreateRefreshToken(
                createdUser,
                request.IpAddress
            );
            // Refresh token'ı veritabanına ekle
            Domain.Entities.RefreshToken addedRefreshToken = await _authService.AddRefreshToken(createdRefreshToken);

            // Kayıt response'u oluştur ve token'ları ekle
            RegisteredResponse registeredResponse = new() { AccessToken = createdAccessToken, RefreshToken = addedRefreshToken };
            return registeredResponse;
        }
    }
}
