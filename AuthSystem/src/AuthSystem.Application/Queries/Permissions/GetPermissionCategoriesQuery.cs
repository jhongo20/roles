using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.Permissions
{
    public class GetPermissionCategoriesQuery : IRequest<List<string>>
    {
    }

    public class GetPermissionCategoriesQueryHandler : IRequestHandler<GetPermissionCategoriesQuery, List<string>>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly ILogger<GetPermissionCategoriesQueryHandler> _logger;

        public GetPermissionCategoriesQueryHandler(
            IPermissionRepository permissionRepository,
            ILogger<GetPermissionCategoriesQueryHandler> logger)
        {
            _permissionRepository = permissionRepository;
            _logger = logger;
        }

        public async Task<List<string>> Handle(GetPermissionCategoriesQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var permissions = await _permissionRepository.GetAllAsync();
                
                // Obtener categorías únicas y ordenadas
                return permissions
                    .Select(p => p.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías de permisos");
                throw;
            }
        }
    }
}
