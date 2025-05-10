using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Roles
{
    public class CreateRoleCommand : IRequest<RoleResponseDto>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsDefault { get; set; }
        public int Priority { get; set; }
    }

    public class RoleResponseDto
    {
        public bool Succeeded { get; set; }
        public string Error { get; set; }
        public RoleDto Role { get; set; }
    }

    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, RoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<CreateRoleCommandHandler> _logger;

        public CreateRoleCommandHandler(
            IRoleRepository roleRepository,
            IAuditService auditService,
            ILogger<CreateRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<RoleResponseDto> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe un rol con el mismo nombre
                var existingRole = await _roleRepository.FindByNameAsync(request.Name);
                if (existingRole != null)
                {
                    return new RoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"Ya existe un rol con el nombre '{request.Name}'"
                    };
                }

                // Crear el nuevo rol
                var role = new Role(
                    name: request.Name,
                    description: request.Description,
                    isDefault: request.IsDefault,
                    priority: request.Priority
                );

                // Guardar en la base de datos
                await _roleRepository.AddAsync(role);
                await _roleRepository.SaveChangesAsync();

                // Registrar la acción en la auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Create",
                    entityName: "Role",
                    entityId: role.Id.ToString(),
                    oldValues: null,
                    newValues: new { Name = role.Name, Description = role.Description, IsDefault = role.IsDefault },
                    ipAddress: null,
                    userAgent: null
                );

                // Mapear a DTO y devolver respuesta
                return new RoleResponseDto
                {
                    Succeeded = true,
                    Role = new RoleDto
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Description = role.Description,
                        IsActive = role.IsActive,
                        IsDefault = role.IsDefault,
                        Priority = role.Priority,
                        CreatedAt = role.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el rol: {Message}", ex.Message);
                return new RoleResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al crear el rol. Por favor, inténtelo de nuevo."
                };
            }
        }
    }
}
