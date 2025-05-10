using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.API.Filters;
using AuthSystem.Application.Commands.Roles;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.Roles;
using AuthSystem.Core.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // Requiere autenticación, los permisos se verificarán en cada método
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IMediator mediator, ILogger<RolesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los roles
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir roles inactivos</param>
        /// <returns>Lista de roles</returns>
        [HttpGet]
        [RequirePermission(PermissionConstants.ViewRoles)]
        public async Task<ActionResult<List<RoleDto>>> GetAllRoles([FromQuery] bool includeInactive = false)
        {
            try
            {
                var query = new GetAllRolesQuery { IncludeInactive = includeInactive };
                var roles = await _mediator.Send(query);
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los roles");
                return StatusCode(500, new { error = "Ha ocurrido un error al obtener los roles" });
            }
        }

        /// <summary>
        /// Obtiene un rol por su ID
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <returns>Detalles del rol</returns>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.ViewRoles)]
        public async Task<ActionResult<RoleDto>> GetRoleById(Guid id)
        {
            try
            {
                var query = new GetRoleByIdQuery { Id = id };
                var role = await _mediator.Send(query);

                if (role == null)
                {
                    return NotFound(new { error = $"No se encontró un rol con el ID '{id}'" });
                }

                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el rol con ID {RoleId}", id);
                return StatusCode(500, new { error = "Ha ocurrido un error al obtener el rol" });
            }
        }

        /// <summary>
        /// Crea un nuevo rol
        /// </summary>
        /// <param name="command">Datos del rol a crear</param>
        /// <returns>Rol creado</returns>
        [HttpPost]
        [RequirePermission(PermissionConstants.CreateRoles)]
        public async Task<ActionResult<RoleResponseDto>> CreateRole([FromBody] CreateRoleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return CreatedAtAction(nameof(GetRoleById), new { id = result.Role.Id }, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el rol");
                return StatusCode(500, new { error = "Ha ocurrido un error al crear el rol" });
            }
        }

        /// <summary>
        /// Actualiza un rol existente
        /// </summary>
        /// <param name="id">ID del rol</param>
        /// <param name="command">Datos actualizados del rol</param>
        /// <returns>Rol actualizado</returns>
        [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.UpdateRoles)]
        public async Task<ActionResult<RoleResponseDto>> UpdateRole(Guid id, [FromBody] UpdateRoleCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest(new { error = "El ID del rol en la URL no coincide con el ID en el cuerpo de la solicitud" });
                }

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el rol con ID {RoleId}", id);
                return StatusCode(500, new { error = "Ha ocurrido un error al actualizar el rol" });
            }
        }

        /// <summary>
        /// Elimina un rol
        /// </summary>
        /// <param name="id">ID del rol a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [RequirePermission(PermissionConstants.DeleteRoles)]
        public async Task<ActionResult<DeleteRoleResponseDto>> DeleteRole(Guid id)
        {
            try
            {
                var command = new DeleteRoleCommand { Id = id };
                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Rol eliminado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el rol con ID {RoleId}", id);
                return StatusCode(500, new { error = "Ha ocurrido un error al eliminar el rol" });
            }
        }

        /// <summary>
        /// Agrega un permiso a un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{roleId}/permissions/{permissionId}")]
        [RequirePermission(PermissionConstants.ManageRolePermissions)]
        public async Task<ActionResult<PermissionRoleResponseDto>> AddPermissionToRole(Guid roleId, Guid permissionId)
        {
            try
            {
                var userId = User.FindFirst("sub")?.Value;
                Guid? assignedBy = null;
                
                if (!string.IsNullOrEmpty(userId))
                {
                    assignedBy = Guid.Parse(userId);
                }

                var command = new AddPermissionToRoleCommand
                {
                    RoleId = roleId,
                    PermissionId = permissionId,
                    AssignedBy = assignedBy
                };

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Permiso agregado al rol exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al agregar permiso {PermissionId} al rol {RoleId}", permissionId, roleId);
                return StatusCode(500, new { error = "Ha ocurrido un error al agregar el permiso al rol" });
            }
        }

        /// <summary>
        /// Elimina un permiso de un rol
        /// </summary>
        /// <param name="roleId">ID del rol</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        [RequirePermission(PermissionConstants.ManageRolePermissions)]
        public async Task<ActionResult<PermissionRoleResponseDto>> RemovePermissionFromRole(Guid roleId, Guid permissionId)
        {
            try
            {
                var command = new RemovePermissionFromRoleCommand
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                };

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Permiso eliminado del rol exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar permiso {PermissionId} del rol {RoleId}", permissionId, roleId);
                return StatusCode(500, new { error = "Ha ocurrido un error al eliminar el permiso del rol" });
            }
        }
    }
}
