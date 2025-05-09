using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.TwoFactor
{
    public class DisableTwoFactorCommand : IRequest<DisableTwoFactorResponse>
    {
        public Guid UserId { get; set; }
    }

    public class DisableTwoFactorResponse
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public string Message { get; set; }
    }

    public class DisableTwoFactorCommandHandler : IRequestHandler<DisableTwoFactorCommand, DisableTwoFactorResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<DisableTwoFactorCommandHandler> _logger;

        public DisableTwoFactorCommandHandler(
            IUserRepository userRepository,
            ILogger<DisableTwoFactorCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<DisableTwoFactorResponse> Handle(DisableTwoFactorCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // 1. Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Intento de deshabilitar 2FA para usuario no existente: {UserId}", request.UserId);
                    return new DisableTwoFactorResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // 2. Verificar que el usuario tenga 2FA habilitado
                if (!user.TwoFactorEnabled)
                {
                    _logger.LogWarning("Intento de deshabilitar 2FA para usuario que ya lo tiene deshabilitado: {UserId}", request.UserId);
                    return new DisableTwoFactorResponse
                    {
                        Succeeded = false,
                        Error = "La autenticación de dos factores ya está deshabilitada para este usuario"
                    };
                }

                // 3. Deshabilitar 2FA para el usuario
                user.DisableTwoFactor();
                await _userRepository.UpdateAsync(user);

                // 4. Eliminar la configuración de 2FA
                await _userRepository.RemoveTwoFactorSettingsAsync(request.UserId);

                _logger.LogInformation("2FA deshabilitado exitosamente para usuario {UserId}", request.UserId);

                return new DisableTwoFactorResponse
                {
                    Succeeded = true,
                    Message = "Autenticación de dos factores deshabilitada exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar 2FA para usuario {UserId}", request.UserId);
                return new DisableTwoFactorResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al deshabilitar la autenticación de dos factores"
                };
            }
        }
    }
}
