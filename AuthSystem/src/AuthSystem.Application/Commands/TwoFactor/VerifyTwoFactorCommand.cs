using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.TwoFactor
{
    public class VerifyTwoFactorCommand : IRequest<AuthResponseDto>
    {
        public Guid UserId { get; set; }
        public string Code { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool RememberMe { get; set; }
    }

    public class VerifyTwoFactorCommandHandler : IRequestHandler<VerifyTwoFactorCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly IJwtService _jwtService;
        private readonly IAuditService _auditService;
        private readonly ILogger<VerifyTwoFactorCommandHandler> _logger;

        public VerifyTwoFactorCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            IJwtService jwtService,
            IAuditService auditService,
            ILogger<VerifyTwoFactorCommandHandler> logger)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _jwtService = jwtService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(VerifyTwoFactorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de verificación 2FA para usuario no existente: {UserId}", request.UserId);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // 2. Verificar que el usuario tenga 2FA habilitado
                if (!user.TwoFactorEnabled)
                {
                    _logger.LogWarning("Intento de verificación 2FA para usuario sin 2FA habilitado: {UserId}", request.UserId);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "La autenticación de dos factores no está habilitada para este usuario"
                    };
                }

                // 3. Obtener la configuración de 2FA del usuario
                var twoFactorSettings = await _userRepository.GetTwoFactorSettingsAsync(request.UserId);
                if (twoFactorSettings == null)
                {
                    _logger.LogWarning("Usuario con 2FA habilitado pero sin configuración: {UserId}", request.UserId);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "No hay configuración de autenticación de dos factores para este usuario"
                    };
                }

                // 4. Verificar el código TOTP
                bool isValidCode = _totpService.ValidateCode(twoFactorSettings.SecretKey, request.Code);
                if (!isValidCode)
                {
                    await LogFailedLoginAttempt(request, user.Id, "Código 2FA inválido");
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Código de verificación inválido"
                    };
                }

                // 5. Generar tokens JWT
                var (token, refreshToken) = await _jwtService.GenerateTokensAsync(user, request.RememberMe);

                // 6. Registrar sesión
                var sessionId = Guid.NewGuid();
                await _userRepository.AddSessionAsync(new UserSession
                {
                    Id = sessionId,
                    UserId = user.Id,
                    Token = token,
                    RefreshToken = refreshToken,
                    IPAddress = request.IpAddress,
                    UserAgent = request.UserAgent,
                    DeviceInfo = ExtractDeviceInfo(request.UserAgent),
                    IssuedAt = DateTime.UtcNow,
                    ExpiresAt = request.RememberMe ? DateTime.UtcNow.AddDays(7) : DateTime.UtcNow.AddHours(2)
                });

                // 7. Registrar login exitoso
                await LogSuccessfulLoginAttempt(request, user.Id);

                // 8. Mapear usuario a DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Status = user.Status.ToString(),
                    LastLoginDate = user.LastLoginDate,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt
                };

                // 9. Retornar resultado exitoso
                return new AuthResponseDto
                {
                    Succeeded = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    RequirePasswordChange = user.RequirePasswordChange,
                    User = userDto,
                    Message = "Verificación de dos factores exitosa"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la verificación 2FA para usuario {UserId}", request.UserId);
                return new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error durante la verificación. Por favor, inténtelo de nuevo más tarde."
                };
            }
        }

        private string ExtractDeviceInfo(string userAgent)
        {
            // Implementación simple para extraer información del dispositivo del User-Agent
            if (string.IsNullOrEmpty(userAgent))
                return "Desconocido";

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return "Móvil";

            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                return "Tablet";

            return "Escritorio";
        }

        private async Task LogFailedLoginAttempt(VerifyTwoFactorCommand request, Guid userId, string reason)
        {
            if (_auditService != null)
            {
                await _auditService.LogLoginAttemptAsync(
                    userId.ToString(),
                    request.IpAddress,
                    request.UserAgent,
                    false,
                    reason,
                    userId);
            }
        }

        private async Task LogSuccessfulLoginAttempt(VerifyTwoFactorCommand request, Guid userId, string? additionalInfo = null)
        {
            if (_auditService != null)
            {
                await _auditService.LogLoginAttemptAsync(
                    userId.ToString(),
                    request.IpAddress,
                    request.UserAgent,
                    true,
                    additionalInfo,
                    userId);
            }
        }
    }
}
