using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Enums;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace AuthSystem.Application.Commands.TwoFactor
{
    public class TwoFactorLoginCommand : IRequest<AuthResponseDto>
    {
        public Guid UserId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string UserAgent { get; set; } = string.Empty;
        public bool RememberMe { get; set; }
    }

    public class TwoFactorLoginCommandHandler : IRequestHandler<TwoFactorLoginCommand, AuthResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly IJwtService _jwtService;
        private readonly IAuditService _auditService;
        private readonly IMapper _mapper;
        private readonly ILogger<TwoFactorLoginCommandHandler> _logger;

        public TwoFactorLoginCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            IJwtService jwtService,
            IAuditService auditService,
            IMapper mapper,
            ILogger<TwoFactorLoginCommandHandler> logger)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _jwtService = jwtService;
            _auditService = auditService;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<AuthResponseDto> Handle(TwoFactorLoginCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener el usuario por ID
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    await LogFailedLoginAttempt(request, "Usuario no encontrado", null);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado. Por favor, inicie sesión nuevamente."
                    };
                }

                // 2. Verificar el estado del usuario
                if (user.Status != UserStatus.Active)
                {
                    string errorMessage;
                    switch (user.Status)
                    {
                        case UserStatus.Registered:
                            errorMessage = "La cuenta no ha sido activada. Por favor, revisa tu correo electrónico para activar la cuenta.";
                            break;
                        case UserStatus.Blocked:
                            errorMessage = "Esta cuenta ha sido bloqueada. Por favor, contacta con soporte para más información.";
                            break;
                        case UserStatus.Suspended:
                            errorMessage = "Esta cuenta ha sido suspendida temporalmente.";
                            break;
                        case UserStatus.Deleted:
                            errorMessage = "Esta cuenta ha sido eliminada.";
                            break;
                        default:
                            errorMessage = "La cuenta no está activa.";
                            break;
                    }

                    await LogFailedLoginAttempt(request, $"Usuario con estado {user.Status}", user.Id);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = errorMessage
                    };
                }

                // 3. Verificar si la cuenta tiene 2FA habilitado
                if (!user.TwoFactorEnabled)
                {
                    await LogFailedLoginAttempt(request, "2FA no habilitado", user.Id);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "La autenticación de dos factores no está habilitada para esta cuenta."
                    };
                }

                // 4. Obtener la configuración de 2FA
                var twoFactorSettings = await _userRepository.GetTwoFactorSettingsAsync(user.Id);
                if (twoFactorSettings == null)
                {
                    await LogFailedLoginAttempt(request, "Configuración 2FA no encontrada", user.Id);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Error en la configuración de autenticación de dos factores. Por favor, contacte con soporte."
                    };
                }

                // 5. Validar el código según el método configurado
                bool isValidCode = false;
                
                // Para este ejemplo, simplemente validamos con TOTP
                isValidCode = _totpService.ValidateCode(twoFactorSettings.SecretKey, request.Code);

                // 6. Si el código es inválido, registrar el fallo y retornar error
                if (!isValidCode)
                {
                    await LogFailedLoginAttempt(request, "Código 2FA inválido", user.Id);
                    return new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Código de verificación inválido. Por favor, inténtelo de nuevo."
                    };
                }

                // 7. Actualizar el contador de intentos fallidos y fecha de último login
                user.ResetAccessFailedCount();
                user.UpdateLastLoginDate();
                await _userRepository.UpdateAsync(user);

                // 8. Generar tokens JWT
                var (token, refreshToken) = await _jwtService.GenerateTokensAsync(user, request.RememberMe);

                // 9. Registrar nueva sesión
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

                // 10. Registrar login exitoso en auditoría
                await LogSuccessfulLoginAttempt(request, user.Id);

                // 11. Mapear usuario a DTO
                var userDto = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName ?? string.Empty,
                    LastName = user.LastName ?? string.Empty,
                    Status = user.Status.ToString(),
                    LastLoginDate = user.LastLoginDate,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt
                };

                // 13. Devolver respuesta exitosa
                return new AuthResponseDto
                {
                    Succeeded = true,
                    Token = token,
                    RefreshToken = refreshToken,
                    RequirePasswordChange = user.RequirePasswordChange,
                    User = userDto,
                    Message = "Inicio de sesión exitoso"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante la autenticación de dos factores para el usuario {UserId}", request.UserId);
                return new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error durante la autenticación de dos factores. Por favor, inténtelo de nuevo más tarde."
                };
            }
        }

        private string GetCachedVerificationCode(Guid userId, string method)
        {
            // En una implementación real, este código se obtendría de un almacenamiento en caché como Redis
            // Para este ejemplo, simplemente devolvemos un código fijo para simular la funcionalidad
            return "123456"; // Código de ejemplo para pruebas
        }

        private async Task LogFailedLoginAttempt(TwoFactorLoginCommand request, string reason, Guid? userId)
        {
            if (_auditService != null)
            {
                var username = "Unknown";
                if (userId.HasValue)
                {
                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    username = user?.Username ?? "Unknown";
                }

                await _auditService.LogLoginAttemptAsync(
                    username,
                    request.IpAddress,
                    request.UserAgent,
                    false,
                    $"2FA: {reason}",
                    userId);
            }
        }

        private async Task LogSuccessfulLoginAttempt(TwoFactorLoginCommand request, Guid userId)
        {
            if (_auditService != null)
            {
                var user = await _userRepository.GetByIdAsync(userId);
                var username = user?.Username ?? "Unknown";
                
                await _auditService.LogLoginAttemptAsync(
                    username,
                    request.IpAddress,
                    request.UserAgent,
                    true,
                    "2FA completado exitosamente",
                    userId);
            }
        }

        private string ExtractDeviceInfo(string userAgent)
        {
            // Implementación simple para extraer información del dispositivo
            if (string.IsNullOrEmpty(userAgent))
                return "Unknown";

            if (userAgent.Contains("Mobile") || userAgent.Contains("Android") || userAgent.Contains("iPhone"))
                return "Móvil";

            if (userAgent.Contains("Tablet") || userAgent.Contains("iPad"))
                return "Tablet";

            return "Escritorio";
        }
    }
}
