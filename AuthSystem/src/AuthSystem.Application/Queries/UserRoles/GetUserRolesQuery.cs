using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.UserRoles
{
    public class GetUserRolesQuery : IRequest<List<RoleDto>>
    {
        public Guid UserId { get; set; }
    }

    public class GetUserRolesQueryHandler : IRequestHandler<GetUserRolesQuery, List<RoleDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetUserRolesQueryHandler> _logger;

        public GetUserRolesQueryHandler(
            IUserRepository userRepository,
            ILogger<GetUserRolesQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<RoleDto>> Handle(GetUserRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el usuario
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                {
                    throw new InvalidOperationException($"No se encontró el usuario con ID '{request.UserId}'");
                }

                // Obtener los roles del usuario
                var userRoles = await _userRepository.GetUserRolesAsync(request.UserId);

                // Mapear a DTOs
                return userRoles.Select(r => new RoleDto
                {
                    Id = r.Id,
                    Name = r.Name,
                    Description = r.Description,
                    IsActive = r.IsActive,
                    IsDefault = r.IsDefault,
                    Priority = r.Priority,
                    CreatedAt = r.CreatedAt
                    // UpdatedAt no existe en RoleDto
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los roles del usuario {UserId}", request.UserId);
                throw;
            }
        }
    }
}
