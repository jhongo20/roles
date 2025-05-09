using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;

namespace AuthSystem.Application.Commands.User
{
    public class UpdateProfileCommand : IRequest<UpdateProfileResponse>
    {
        public Guid UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePictureUrl { get; set; }
    }

    public class UpdateProfileResponse
    {
        public bool Succeeded { get; set; }
        public string? Error { get; set; }
        public UserDto? User { get; set; }
    }

    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, UpdateProfileResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateProfileCommandHandler> _logger;

        public UpdateProfileCommandHandler(
            IUserRepository userRepository,
            IMapper mapper,
            ILogger<UpdateProfileCommandHandler> logger)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<UpdateProfileResponse> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning("Usuario no encontrado: {UserId}", request.UserId);
                    return new UpdateProfileResponse
                    {
                        Succeeded = false,
                        Error = "Usuario no encontrado"
                    };
                }

                // Actualizar el perfil del usuario
                user.UpdateProfile(
                    request.FirstName ?? user.FirstName,
                    request.LastName ?? user.LastName,
                    request.PhoneNumber ?? user.PhoneNumber
                );

                // Guardar los cambios
                await _userRepository.UpdateAsync(user);

                // Mapear el usuario a DTO
                var userDto = _mapper.Map<UserDto>(user);

                return new UpdateProfileResponse
                {
                    Succeeded = true,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil del usuario {UserId}", request.UserId);
                return new UpdateProfileResponse
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al actualizar el perfil del usuario"
                };
            }
        }
    }
}