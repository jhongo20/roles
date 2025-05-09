using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserProfile
{
    public class UpdatePhoneNumberCommand : IRequest<UpdatePhoneNumberResponse>
    {
        public Guid UserId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class UpdatePhoneNumberResponse
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
    }

    public class UpdatePhoneNumberCommandHandler : IRequestHandler<UpdatePhoneNumberCommand, UpdatePhoneNumberResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdatePhoneNumberCommandHandler> _logger;

        public UpdatePhoneNumberCommandHandler(
            IUserRepository userRepository,
            ILogger<UpdatePhoneNumberCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UpdatePhoneNumberResponse> Handle(UpdatePhoneNumberCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                    return new UpdatePhoneNumberResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // Validar el número de teléfono
                if (string.IsNullOrWhiteSpace(request.PhoneNumber))
                {
                    return new UpdatePhoneNumberResponse
                    {
                        Succeeded = false,
                        Error = "El número de teléfono no puede estar vacío"
                    };
                }

                // Actualizar el número de teléfono usando el método UpdateProfile
                // Mantenemos los valores actuales de FirstName y LastName
                user.UpdateProfile(user.FirstName, user.LastName, request.PhoneNumber);
                
                // Guardar los cambios
                await _userRepository.UpdateAsync(user);

                return new UpdatePhoneNumberResponse
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el número de teléfono del usuario {UserId}", request.UserId);
                return new UpdatePhoneNumberResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al actualizar el número de teléfono"
                };
            }
        }
    }
}
