using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Modules
{
    public class GetModuleByIdQuery : IRequest<ModuleDto>
    {
        public Guid ModuleId { get; set; }
        public bool IncludePermissions { get; set; } = false;
    }

    public class GetModuleByIdQueryHandler : IRequestHandler<GetModuleByIdQuery, ModuleDto>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogger<GetModuleByIdQueryHandler> _logger;

        public GetModuleByIdQueryHandler(
            IModuleRepository moduleRepository,
            ILogger<GetModuleByIdQueryHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
        }

        public async Task<ModuleDto> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
                if (module == null)
                {
                    return null;
                }

                var moduleDto = new ModuleDto
                {
                    Id = module.Id,
                    Name = module.Name,
                    // Code no existe en la entidad Module, pero sí en ModuleDto
                    Code = module.Name, // Usamos Name como Code por ahora
                    Description = module.Description,
                    Icon = module.Icon,
                    Route = module.Route,
                    DisplayOrder = module.DisplayOrder, // Usar DisplayOrder en lugar de Order
                    ParentId = module.ParentId, // Usar ParentId en lugar de ParentModuleId
                    IsActive = module.IsActive,
                    CreatedAt = module.CreatedAt,
                    UpdatedAt = module.UpdatedAt
                };

                // Obtener permisos del módulo si se solicita
                if (request.IncludePermissions)
                {
                    var permissions = await _moduleRepository.GetModulePermissionsAsync(module.Id);
                    moduleDto.Permissions = permissions.Select(p => new PermissionDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Code = p.Code,
                        Description = p.Description,
                        Category = p.Category,
                        // La propiedad IsActive no existe en Permission, asumimos que todos los permisos están activos
                        IsActive = true,
                        CreatedAt = p.CreatedAt,
                        UpdatedAt = p.UpdatedAt
                    }).ToList();
                }

                // Obtener submódulos si existen
                var childModules = await _moduleRepository.GetChildModulesAsync(module.Id);
                moduleDto.ChildModules = childModules.Select(m => new ModuleDto
                {
                    Id = m.Id,
                    Name = m.Name,
                    // Code no existe en la entidad Module, pero sí en ModuleDto
                    Code = m.Name, // Usamos Name como Code por ahora
                    Description = m.Description,
                    Icon = m.Icon,
                    Route = m.Route,
                    DisplayOrder = m.DisplayOrder, // Usar DisplayOrder en lugar de Order
                    ParentId = m.ParentId, // Usar ParentId en lugar de ParentModuleId
                    IsActive = m.IsActive,
                    CreatedAt = m.CreatedAt,
                    UpdatedAt = m.UpdatedAt
                }).ToList();

                return moduleDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID {ModuleId}", request.ModuleId);
                throw;
            }
        }
    }
}
