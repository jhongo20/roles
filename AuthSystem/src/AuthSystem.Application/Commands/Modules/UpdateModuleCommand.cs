using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Modules
{
    public class UpdateModuleCommand : IRequest<ModuleResponseDto>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string Route { get; set; }
        public int DisplayOrder { get; set; }
        public Guid? ParentId { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateModuleCommandHandler : IRequestHandler<UpdateModuleCommand, ModuleResponseDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<UpdateModuleCommandHandler> _logger;

        public UpdateModuleCommandHandler(
            IModuleRepository moduleRepository,
            IAuditService auditService,
            ILogger<UpdateModuleCommandHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ModuleResponseDto> Handle(UpdateModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el módulo
                var module = await _moduleRepository.GetByIdAsync(request.Id);
                if (module == null)
                {
                    throw new InvalidOperationException($"No se encontró el módulo con ID '{request.Id}'");
                }

                // Verificar si existe el módulo padre si se especifica
                if (request.ParentId.HasValue)
                {
                    // No permitir que un módulo sea su propio padre
                    if (request.ParentId.Value == request.Id)
                    {
                        throw new InvalidOperationException("Un módulo no puede ser su propio padre");
                    }

                    var parentModule = await _moduleRepository.GetByIdAsync(request.ParentId.Value);
                    if (parentModule == null)
                    {
                        throw new InvalidOperationException($"No se encontró el módulo padre con ID '{request.ParentId}'");
                    }
                }

                // Guardar valores originales para auditoría
                var originalName = module.Name;
                var originalDescription = module.Description;
                var originalIcon = module.Icon;
                var originalRoute = module.Route;
                var originalDisplayOrder = module.DisplayOrder;
                var originalParentId = module.ParentId;
                var originalIsActive = module.IsActive;

                // Actualizar propiedades
                module.Update(request.Name, request.Description, request.Icon, request.Route, 
                             request.IsActive, request.DisplayOrder, request.ParentId);
                // La fecha de actualización se actualiza en el método Update

                // Guardar cambios
                await _moduleRepository.UpdateAsync(module);

                // Registrar auditoría usando el método correcto
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Update",
                    entityName: "Module",
                    entityId: module.Id.ToString(),
                    oldValues: new { 
                        Name = originalName, 
                        Description = originalDescription,
                        Icon = originalIcon,
                        Route = originalRoute,
                        IsActive = originalIsActive,
                        DisplayOrder = originalDisplayOrder,
                        ParentId = originalParentId
                    },
                    newValues: new { 
                        Name = module.Name, 
                        Description = module.Description,
                        Icon = module.Icon,
                        Route = module.Route,
                        IsActive = module.IsActive,
                        DisplayOrder = module.DisplayOrder,
                        ParentId = module.ParentId
                    },
                    ipAddress: null,
                    userAgent: null);

                // Mapear a DTO de respuesta
                return new ModuleResponseDto
                {
                    Id = module.Id,
                    Name = module.Name,
                    Code = module.Name, // Usamos Name como Code ya que la entidad Module no tiene la propiedad Code
                    Description = module.Description,
                    Icon = module.Icon,
                    Route = module.Route,
                    DisplayOrder = module.DisplayOrder,
                    ParentId = module.ParentId,
                    IsActive = module.IsActive,
                    CreatedAt = module.CreatedAt,
                    UpdatedAt = module.UpdatedAt,
                    Success = true,
                    Message = $"Módulo '{module.Name}' actualizado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo con ID {ModuleId}", request.Id);
                throw;
            }
        }
    }
}
