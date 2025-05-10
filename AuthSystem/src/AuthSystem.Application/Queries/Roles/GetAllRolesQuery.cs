using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Roles
{
    public class GetAllRolesQuery : IRequest<List<RoleDto>>
    {
        public bool IncludeInactive { get; set; } = false;
    }

    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleDto>>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly ILogger<GetAllRolesQueryHandler> _logger;

        public GetAllRolesQueryHandler(
            IRoleRepository roleRepository,
            ILogger<GetAllRolesQueryHandler> logger)
        {
            _roleRepository = roleRepository;
            _logger = logger;
        }

        public async Task<List<RoleDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                
                // Filtrar roles inactivos si es necesario
                if (!request.IncludeInactive)
                {
                    roles = roles.Where(r => r.IsActive).ToList();
                }

                // Ordenar por prioridad y nombre
                roles = roles.OrderByDescending(r => r.Priority)
                             .ThenBy(r => r.Name)
                             .ToList();

                // Mapear a DTOs
                var roleDtos = new List<RoleDto>();
                foreach (var role in roles)
                {
                    var permissions = await _roleRepository.GetRolePermissionsAsync(role.Id);
                    
                    roleDtos.Add(new RoleDto
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
                    });
                }

                return roleDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles: {Message}", ex.Message);
                return new List<RoleDto>();
            }
        }
    }
}
