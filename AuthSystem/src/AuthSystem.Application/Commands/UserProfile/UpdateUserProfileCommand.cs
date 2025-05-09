using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserProfile
{
    public class UpdateUserProfileCommand : IRequest<UpdateUserProfileResponse>
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
    }

    public class UpdateUserProfileResponse
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
    }

    public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<UpdateUserProfileCommandHandler> _logger;

        public UpdateUserProfileCommandHandler(
            IUserRepository userRepository,
            ILogger<UpdateUserProfileCommandHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                    return new UpdateUserProfileResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // Actualizar los campos del usuario
                user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
                
                // Guardar los cambios
                await _userRepository.UpdateAsync(user);

                return new UpdateUserProfileResponse
                {
                    Succeeded = true
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario {UserId}", request.UserId);
                return new UpdateUserProfileResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al actualizar el perfil del usuario"
                };
            }
        }
    }
}
