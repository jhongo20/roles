using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Roles
{
    public class GetRoleByIdQuery : IRequest<RoleDto>
    {
        public Guid Id { get; set; }
    }

    public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, RoleDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<GetRoleByIdQueryHandler> _logger;

        public GetRoleByIdQueryHandler(
            IRoleRepository roleRepository,
            ILogger<GetRoleByIdQueryHandler> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<RoleDto> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(request.Id);
                if (role == null)
                {
                    return null;
                }

                // Obtener los permisos asociados al rol
                var permissions = await _roleRepository.GetRolePermissionsAsync(role.Id);

                // Mapear a DTO
                var roleDto = new RoleDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsActive = role.IsActive,
                    IsDefault = role.IsDefault,
                    Priority = role.Priority,
                    CreatedAt = role.CreatedAt,
                    Permissions = permissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description,
                        Category = p.Category
                    }).ToList()
                };

                return roleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol por ID: {Message}", ex.Message);
                return null;
            }
        }
    }
}
