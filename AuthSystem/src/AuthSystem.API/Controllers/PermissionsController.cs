using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.API.Filters;
using AuthSystem.Application.Commands.Permissions;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.Permissions;
using AuthSystem.Core.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(
            IMediator mediator,
            ILogger<PermissionsController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los permisos
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir permisos inactivos</param>
        /// <param name="category">Categoría para filtrar (opcional)</param>
        /// <returns>Lista de permisos</returns>
        [HttpGet]
        [RequirePermission(PermissionConstants.ViewRoles)]
        public async Task<ActionResult<List<PermissionDto>>> GetAllPermissions(
            [FromQuery] bool includeInactive = false,
            [FromQuery] string category = null)
        {
            try
            {
                var query = new GetAllPermissionsQuery
                {
                    IncludeInactive = includeInactive,
                    Category = category
                };

                var permissions = await _mediator.Send(query);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los permisos");
                return StatusCode(500, "Error interno del servidor al obtener los permisos");
            }
        }

        /// <summary>
        /// Obtiene un permiso por su ID
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <returns>Detalles del permiso</returns>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.ViewRoles)]
        public async Task<ActionResult<PermissionDto>> GetPermissionById(Guid id)
        {
            try
            {
                var query = new GetPermissionByIdQuery { Id = id };
                var permission = await _mediator.Send(query);

                if (permission == null)
                {
                    return NotFound($"No se encontró el permiso con ID '{id}'");
                }

                return Ok(permission);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el permiso");
            }
        }

        /// <summary>
        /// Obtiene todas las categorías de permisos
        /// </summary>
        /// <returns>Lista de categorías</returns>
        [HttpGet("categories")]
        [RequirePermission(PermissionConstants.ViewRoles)]
        public async Task<ActionResult<List<string>>> GetPermissionCategories()
        {
            try
            {
                var query = new GetPermissionCategoriesQuery();
                var categories = await _mediator.Send(query);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener las categorías de permisos");
                return StatusCode(500, "Error interno del servidor al obtener las categorías de permisos");
            }
        }

        /// <summary>
        /// Crea un nuevo permiso
        /// </summary>
        /// <param name="command">Datos del permiso a crear</param>
        /// <returns>Permiso creado</returns>
        [HttpPost]
        [RequirePermission(PermissionConstants.CreateRoles)]
        public async Task<ActionResult<PermissionResponseDto>> CreatePermission([FromBody] CreatePermissionCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetPermissionById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear un permiso: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un permiso");
                return StatusCode(500, "Error interno del servidor al crear el permiso");
            }
        }

        /// <summary>
        /// Actualiza un permiso existente
        /// </summary>
        /// <param name="id">ID del permiso</param>
        /// <param name="command">Datos actualizados del permiso</param>
        /// <returns>Permiso actualizado</returns>
        [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.UpdateRoles)]
        public async Task<ActionResult<PermissionResponseDto>> UpdatePermission(Guid id, [FromBody] UpdatePermissionCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest("El ID del permiso en la URL no coincide con el ID en el cuerpo de la solicitud");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar el permiso {PermissionId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar el permiso");
            }
        }

        /// <summary>
        /// Elimina un permiso
        /// </summary>
        /// <param name="id">ID del permiso a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [RequirePermission(PermissionConstants.DeleteRoles)]
        public async Task<ActionResult<DeletePermissionResponseDto>> DeletePermission(Guid id)
        {
            try
            {
                var command = new DeletePermissionCommand { Id = id };
                var result = await _mediator.Send(command);

                if (!result.Success)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el permiso con ID {PermissionId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar el permiso");
            }
        }
    }
}
