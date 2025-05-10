using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Modules
{
    public class GetAllModulesQuery : IRequest<List<ModuleDto>>
    {
        public bool IncludeInactive { get; set; } = false;
        public bool IncludePermissions { get; set; } = false;
    }

    public class GetAllModulesQueryHandler : IRequestHandler<GetAllModulesQuery, List<ModuleDto>>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogger<GetAllModulesQueryHandler> _logger;

        public GetAllModulesQueryHandler(
            IModuleRepository moduleRepository,
            ILogger<GetAllModulesQueryHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
        }

        public async Task<List<ModuleDto>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var modules = await _moduleRepository.GetAllAsync(request.IncludeInactive);
                var result = new List<ModuleDto>();

                foreach (var module in modules)
                {
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

                    result.Add(moduleDto);
                }

                // Organizar jerárquicamente los módulos
                return OrganizeModulesHierarchically(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                throw;
            }
        }

        private List<ModuleDto> OrganizeModulesHierarchically(List<ModuleDto> modules)
        {
            // Identificar módulos raíz (sin padre)
            var rootModules = modules.Where(m => !m.ParentId.HasValue).ToList();

            // Para cada módulo raíz, asignar sus hijos recursivamente
            foreach (var rootModule in rootModules)
            {
                AssignChildModules(rootModule, modules);
            }

            return rootModules;
        }

        private void AssignChildModules(ModuleDto parentModule, List<ModuleDto> allModules)
        {
            // Encontrar todos los módulos hijos del módulo actual
            var childModules = allModules
                .Where(m => m.ParentId.HasValue && m.ParentId.Value == parentModule.Id)
                .OrderBy(m => m.DisplayOrder) // Usando DisplayOrder en lugar de Order
                .ToList();

            parentModule.ChildModules = childModules;

            // Recursivamente asignar hijos a cada hijo
            foreach (var childModule in childModules)
            {
                AssignChildModules(childModule, allModules);
            }
        }
    }
}
