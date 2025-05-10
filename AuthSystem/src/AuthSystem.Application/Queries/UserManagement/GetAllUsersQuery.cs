using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Enums;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries.UserManagement
{
    public class GetAllUsersQuery : IRequest<List<UserDto>>
    {
        public bool IncludeInactive { get; set; } = false;
        public string SearchTerm { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, List<UserDto>>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetAllUsersQueryHandler> _logger;

        public GetAllUsersQueryHandler(
            IUserRepository userRepository,
            ILogger<GetAllUsersQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var users = await _userRepository.GetAllAsync();

                // Filtrar por usuarios activos si se solicita
                if (!request.IncludeInactive)
                {
                    users = users.Where(u => u.Status == UserStatus.Active).ToList();
                }

                // Filtrar por término de búsqueda si se proporciona
                if (!string.IsNullOrEmpty(request.SearchTerm))
                {
                    var searchTerm = request.SearchTerm.ToLowerInvariant();
                    users = users.Where(u => 
                        u.Username.ToLowerInvariant().Contains(searchTerm) ||
                        u.Email.ToLowerInvariant().Contains(searchTerm) ||
                        (u.FirstName != null && u.FirstName.ToLowerInvariant().Contains(searchTerm)) ||
                        (u.LastName != null && u.LastName.ToLowerInvariant().Contains(searchTerm))
                    ).ToList();
                }

                // Aplicar paginación si se solicita
                if (request.PageNumber.HasValue && request.PageSize.HasValue)
                {
                    users = users
                        .Skip((request.PageNumber.Value - 1) * request.PageSize.Value)
                        .Take(request.PageSize.Value)
                        .ToList();
                }

                // Mapear a DTOs
                return users.Select(u => new UserDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    PhoneNumber = u.PhoneNumber,
                    IsActive = u.Status == Core.Enums.UserStatus.Active,
                    EmailConfirmed = u.EmailConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd?.DateTime,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                throw;
            }
        }
    }
}
