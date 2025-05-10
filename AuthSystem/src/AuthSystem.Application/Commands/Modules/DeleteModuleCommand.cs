using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.Modules
{
    public class DeleteModuleCommand : IRequest<ModuleResponseDto>
    {
        public Guid Id { get; set; }
    }

    public class DeleteModuleCommandHandler : IRequestHandler<DeleteModuleCommand, ModuleResponseDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly IAuditService _auditService;
        private readonly ILogger<DeleteModuleCommandHandler> _logger;

        public DeleteModuleCommandHandler(
            IModuleRepository moduleRepository,
            IAuditService auditService,
            ILogger<DeleteModuleCommandHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<ModuleResponseDto> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el módulo
                var module = await _moduleRepository.GetByIdAsync(request.Id);
                if (module == null)
                {
                    throw new InvalidOperationException($"No se encontró el módulo con ID '{request.Id}'");
                }

                // Verificar si tiene submódulos
                var hasChildModules = await _moduleRepository.HasChildModulesAsync(request.Id);
                if (hasChildModules)
                {
                    throw new InvalidOperationException($"No se puede eliminar el módulo '{module.Name}' porque tiene submódulos asociados");
                }

                // Verificar si tiene permisos asociados
                var hasPermissions = await _moduleRepository.HasPermissionsAsync(request.Id);
                if (hasPermissions)
                {
                    throw new InvalidOperationException($"No se puede eliminar el módulo '{module.Name}' porque tiene permisos asociados");
                }

                // Eliminar módulo (marcarlo como inactivo)
                module.Deactivate(); // Usar el método Deactivate en lugar de asignar directamente a las propiedades
                await _moduleRepository.UpdateAsync(module);

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    userId: null, // ID del usuario que realiza la acción (null si es sistema)
                    action: "Delete",
                    entityName: "Module",
                    entityId: module.Id.ToString(),
                    oldValues: new { IsActive = true },
                    newValues: new { IsActive = false },
                    ipAddress: null,
                    userAgent: null);

                // Mapear a DTO de respuesta
                return new ModuleResponseDto
                {
                    Id = module.Id,
                    Name = module.Name,
                    Code = module.Name, // Usamos Name como Code ya que la entidad Module no tiene la propiedad Code
                    IsActive = module.IsActive,
                    Success = true,
                    Message = $"Módulo '{module.Name}' eliminado exitosamente"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo con ID {ModuleId}", request.Id);
                throw;
            }
        }
    }
}
