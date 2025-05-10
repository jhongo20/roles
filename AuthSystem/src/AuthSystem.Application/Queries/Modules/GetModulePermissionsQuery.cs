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
    public class GetModulePermissionsQuery : IRequest<List<PermissionDto>>
    {
        public Guid ModuleId { get; set; }
    }

    public class GetModulePermissionsQueryHandler : IRequestHandler<GetModulePermissionsQuery, List<PermissionDto>>
    {
        private readonly IModuleRepository _moduleRepository;
        private readonly ILogger<GetModulePermissionsQueryHandler> _logger;

        public GetModulePermissionsQueryHandler(
            IModuleRepository moduleRepository,
            ILogger<GetModulePermissionsQueryHandler> logger)
        {
            _moduleRepository = moduleRepository;
            _logger = logger;
        }

        public async Task<List<PermissionDto>> Handle(GetModulePermissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si existe el módulo
                var module = await _moduleRepository.GetByIdAsync(request.ModuleId);
                if (module == null)
                {
                    throw new InvalidOperationException($"No se encontró el módulo con ID '{request.ModuleId}'");
                }

                // Obtener permisos del módulo
                var permissions = await _moduleRepository.GetModulePermissionsAsync(request.ModuleId);

                // Mapear a DTOs
                return permissions.Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Code = p.Code,
                    Description = p.Description,
                    Category = p.Category,
                    IsActive = true, // La entidad Permission no tiene la propiedad IsActive, asumimos que todos los permisos están activos
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los permisos del módulo con ID {ModuleId}", request.ModuleId);
                throw;
            }
        }
    }
}
