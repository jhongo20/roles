using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.TwoFactor
{
    public class SendTwoFactorCodeCommand : IRequest<SendTwoFactorCodeResponse>
    {
        public Guid UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
    }

    public class SendTwoFactorCodeResponse
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public class SendTwoFactorCodeCommandHandler : IRequestHandler<SendTwoFactorCodeCommand, SendTwoFactorCodeResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ITotpService _totpService;
        private readonly ISmsService _smsService;
        private readonly ILogger<SendTwoFactorCodeCommandHandler> _logger;

        public SendTwoFactorCodeCommandHandler(
            IUserRepository userRepository,
            ITotpService totpService,
            ISmsService smsService,
            ILogger<SendTwoFactorCodeCommandHandler> logger)
        {
            _userRepository = userRepository;
            _totpService = totpService;
            _smsService = smsService;
            _logger = logger;
        }

        public async Task<SendTwoFactorCodeResponse> Handle(SendTwoFactorCodeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de enviar código 2FA para usuario no existente: {UserId}", request.UserId);
                    return new SendTwoFactorCodeResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // 2. Verificar que el usuario tenga 2FA habilitado
                if (!user.TwoFactorEnabled)
                {
                    _logger.LogWarning("Intento de enviar código 2FA para usuario sin 2FA habilitado: {UserId}", request.UserId);
                    return new SendTwoFactorCodeResponse
                    {
                        Succeeded = false,
                        Error = "La autenticación de dos factores no está habilitada para este usuario"
                    };
                }

                // 3. Obtener la configuración de 2FA del usuario
                var twoFactorSettings = await _userRepository.GetTwoFactorSettingsAsync(request.UserId);
                if (twoFactorSettings == null)
                {
                    _logger.LogWarning("Usuario con 2FA habilitado pero sin número de teléfono configurado: {UserId}", request.UserId);
                    return new SendTwoFactorCodeResponse
                    {
                        Succeeded = false,
                        Error = "No hay un número de teléfono configurado para la autenticación de dos factores"
                    };
                }

                // 4. Generar código TOTP
                string code = _totpService.GenerateCode(twoFactorSettings.SecretKey);

                // 5. Enviar código por SMS
                // Verificamos que el usuario tenga un número de teléfono configurado
                if (user == null || string.IsNullOrEmpty(user.PhoneNumber))
                {
                    _logger.LogWarning("Usuario sin número de teléfono configurado: {UserId}", request.UserId);
                    return new SendTwoFactorCodeResponse
                    {
                        Succeeded = false,
                        Error = "No hay un número de teléfono configurado para la autenticación de dos factores"
                    };
                }
                
                await _smsService.SendVerificationCodeAsync(user.PhoneNumber, code);

                _logger.LogInformation("Código 2FA enviado exitosamente al usuario {UserId}", request.UserId);

                return new SendTwoFactorCodeResponse
                {
                    Succeeded = true,
                    Message = "Código de verificación enviado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código 2FA para usuario {UserId}", request.UserId);
                return new SendTwoFactorCodeResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al enviar el código de verificación"
                };
            }
        }
    }
}
