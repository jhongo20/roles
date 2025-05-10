using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Permissions
{
    public class GetPermissionByIdQuery : IRequest<PermissionDto>
    {
        public Guid Id { get; set; }
    }

    public class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, PermissionDto>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<GetPermissionByIdQueryHandler> _logger;

        public GetPermissionByIdQueryHandler(
            IPermissionRepository permissionRepository,
            ILogger<GetPermissionByIdQueryHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        public async Task<PermissionDto> Handle(GetPermissionByIdQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permission = await _permissionRepository.GetByIdAsync(request.Id);
                if (permission == null)
                {
                    return null;
                }

                // Mapear a DTO
                return new PermissionDto
                {
                    Id = permission.Id,
                    Name = permission.Name,
                    Code = permission.Code,
                    Description = permission.Description,
                    Category = permission.Category,
                    // La propiedad IsActive no existe en Permission, asumimos que todos los permisos están activos
                    IsActive = true,
                    CreatedAt = permission.CreatedAt,
                    UpdatedAt = permission.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID {PermissionId}", request.Id);
                throw;
            }
        }
    }
}
