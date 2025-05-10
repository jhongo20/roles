using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Roles
{
    public class UpdateRoleCommand : IRequest<RoleResponseDto>
    {
        public Guid Id { get; set; }
        
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; } = string.Empty;
        
        public string? Description { get; set; } = string.Empty;
        
        public bool IsActive { get; set; }
        
        public bool IsDefault { get; set; }
        
        public int Priority { get; set; }
    }

    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResponseDto>
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<UpdateRoleCommandHandler> _logger;

        public UpdateRoleCommandHandler(
            IRoleRepository roleRepository,
            IAuditService auditService,
            ILogger<UpdateRoleCommandHandler> logger)
        {
            _roleRepository = roleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<RoleResponseDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Buscar el rol por ID
                var role = await _roleRepository.GetByIdAsync(request.Id);
                if (role == null)
                {
                    return new RoleResponseDto
                    {
                        Succeeded = false,
                        Error = $"No se encontró un rol con el ID '{request.Id}'"
                    };
                }

                // Verificar si el nuevo nombre ya está en uso por otro rol
                if (role.Name != request.Name)
                {
                    var existingRole = await _roleRepository.FindByNameAsync(request.Name);
                    if (existingRole != null && existingRole.Id != request.Id)
                    {
                        return new RoleResponseDto
                        {
                            Succeeded = false,
                            Error = $"Ya existe otro rol con el nombre '{request.Name}'"
                        };
                    }
                }

                // Guardar valores originales para auditoría
                var originalName = role.Name;
                var originalDescription = role.Description;

                // Actualizar el rol
                role.Update(
                    name: request.Name,
                    description: request.Description,
                    isActive: request.IsActive,
                    isDefault: request.IsDefault,
                    priority: request.Priority
                );

                // Guardar cambios
                await _roleRepository.UpdateAsync(role);
                await _roleRepository.SaveChangesAsync();

                // Registrar la acción en la auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Update",
                    entityName: "Role",
                    entityId: role.Id.ToString(),
                    oldValues: new { Name = originalName, Description = originalDescription },
                    newValues: new { Name = role.Name, Description = role.Description },
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
                _logger.LogError(ex, "Error al actualizar el rol: {Message}", ex.Message);
                return new RoleResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al actualizar el rol. Por favor, inténtelo de nuevo."
                };
            }
        }
    }
}
