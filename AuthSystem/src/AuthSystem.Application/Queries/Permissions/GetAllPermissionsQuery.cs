using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Permissions
{
    public class GetAllPermissionsQuery : IRequest<List<PermissionDto>>
    {
        public bool IncludeInactive { get; set; } = false;
        public string Category { get; set; } = null;
    }

    public class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, List<PermissionDto>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<GetAllPermissionsQueryHandler> _logger;

        public GetAllPermissionsQueryHandler(
            IPermissionRepository permissionRepository,
            ILogger<GetAllPermissionsQueryHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        public async Task<List<PermissionDto>> Handle(GetAllPermissionsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permissions = await _permissionRepository.GetAllAsync(request.IncludeInactive);

                // Filtrar por categoría si se especifica
                if (!string.IsNullOrEmpty(request.Category))
                {
                    permissions = permissions.Where(p => p.Category == request.Category).ToList();
                }

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
                _logger.LogError(ex, "Error al obtener todos los permisos");
                throw;
            }
        }
    }
}
