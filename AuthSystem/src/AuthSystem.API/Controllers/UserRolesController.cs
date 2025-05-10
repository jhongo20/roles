using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AuthSystem.API.Filters;
using AuthSystem.Application.Commands.UserRoles;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.UserRoles;
using AuthSystem.Core.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/roles")]
    [Authorize]
    public class UserRolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserRolesController> _logger;

        public UserRolesController(
            IMediator mediator,
            ILogger<UserRolesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los roles asignados a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Lista de roles del usuario</returns>
        [HttpGet]
        [RequirePermission(PermissionConstants.ManageUserRoles)]
        public async Task<ActionResult<List<RoleDto>>> GetUserRoles(Guid userId)
        {
            try
            {
                var query = new GetUserRolesQuery { UserId = userId };
                var roles = await _mediator.Send(query);
                return Ok(roles);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al obtener roles del usuario {UserId}: {Message}", userId, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener roles del usuario {UserId}", userId);
                return StatusCode(500, "Error interno del servidor al obtener los roles del usuario");
            }
        }

        /// <summary>
        /// Asigna un rol a un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a asignar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{roleId}")]
        [RequirePermission(PermissionConstants.ManageUserRoles)]
        public async Task<ActionResult<UserRoleResponseDto>> AssignRoleToUser(Guid userId, Guid roleId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
                Guid? assignedBy = null;
                
                if (!string.IsNullOrEmpty(currentUserId) && Guid.TryParse(currentUserId, out var parsedUserId))
                {
                    assignedBy = parsedUserId;
                }

                var command = new AssignRoleToUserCommand
                {
                    UserId = userId,
                    RoleId = roleId,
                    AssignedBy = assignedBy
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al asignar rol {RoleId} al usuario {UserId}: {Message}", roleId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar rol {RoleId} al usuario {UserId}", roleId, userId);
                return StatusCode(500, "Error interno del servidor al asignar el rol al usuario");
            }
        }

        /// <summary>
        /// Quita un rol de un usuario
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <param name="roleId">ID del rol a quitar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{roleId}")]
        [RequirePermission(PermissionConstants.ManageUserRoles)]
        public async Task<ActionResult<UserRoleResponseDto>> RemoveRoleFromUser(Guid userId, Guid roleId)
        {
            try
            {
                var command = new RemoveRoleFromUserCommand
                {
                    UserId = userId,
                    RoleId = roleId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al quitar rol {RoleId} del usuario {UserId}: {Message}", roleId, userId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar rol {RoleId} del usuario {UserId}", roleId, userId);
                return StatusCode(500, "Error interno del servidor al quitar el rol del usuario");
            }
        }
    }
}
