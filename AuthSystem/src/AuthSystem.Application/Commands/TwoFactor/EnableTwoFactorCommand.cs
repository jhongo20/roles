using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.TwoFactor
{
    public class EnableTwoFactorCommand : IRequest<EnableTwoFactorResponse>
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; }
    }

    public class EnableTwoFactorResponse
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public class EnableTwoFactorCommandHandler : IRequestHandler<EnableTwoFactorCommand, EnableTwoFactorResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly ISmsService _smsService;
        private readonly ILogger<EnableTwoFactorCommandHandler> _logger;

        public EnableTwoFactorCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            ISmsService smsService,
            ILogger<EnableTwoFactorCommandHandler> logger)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<EnableTwoFactorResponse> Handle(EnableTwoFactorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de habilitar 2FA para usuario no existente: {UserId}", request.UserId);
                    return new EnableTwoFactorResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // 2. Validar el número de teléfono
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return new EnableTwoFactorResponse
                    {
                        Succeeded = false,
                        Error = "Debe proporcionar un número de teléfono válido"
                    };
                }

                // 3. Generar una clave secreta para TOTP
                string secretKey = _totpService.GenerateSecretKey();

                // 4. Crear o actualizar la configuración de 2FA
                var twoFactorSettings = new UserTwoFactorSettings
                {
                    UserId = request.UserId,
                    SecretKey = secretKey,
                    Method = "SMS",
                    IsEnabled = true,
                    UpdatedAt = DateTime.UtcNow
                };

                await _userRepository.SaveTwoFactorSettingsAsync(twoFactorSettings);

                // 5. Habilitar 2FA para el usuario
                user.EnableTwoFactor();
                await _userRepository.UpdateAsync(user);

                // 6. Enviar un código de prueba al número de teléfono
                string code = _totpService.GenerateCode(secretKey);
                await _smsService.SendVerificationCodeAsync(request.PhoneNumber, code);

                _logger.LogInformation("2FA habilitado exitosamente para usuario {UserId}", request.UserId);

                return new EnableTwoFactorResponse
                {
                    Succeeded = true,
                    Message = "Autenticación de dos factores habilitada exitosamente. Se ha enviado un código de prueba a su teléfono."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar 2FA para usuario {UserId}", request.UserId);
                return new EnableTwoFactorResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al habilitar la autenticación de dos factores"
                };
            }
        }
    }
}
