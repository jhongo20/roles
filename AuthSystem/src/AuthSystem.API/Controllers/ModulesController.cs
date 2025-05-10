using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.API.Filters;
using AuthSystem.Application.Commands.Modules;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.Modules;
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
    public class ModulesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<ModulesController> _logger;

        public ModulesController(
            IMediator mediator,
            ILogger<ModulesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los módulos
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir módulos inactivos</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos de cada módulo</param>
        /// <returns>Lista de módulos</returns>
        [HttpGet]
        [RequirePermission(PermissionConstants.ViewModules)]
        public async Task<ActionResult<List<ModuleDto>>> GetAllModules(
            [FromQuery] bool includeInactive = false,
            [FromQuery] bool includePermissions = false)
        {
            try
            {
                var query = new GetAllModulesQuery
                {
                    IncludeInactive = includeInactive,
                    IncludePermissions = includePermissions
                };

                var modules = await _mediator.Send(query);
                return Ok(modules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los módulos");
                return StatusCode(500, "Error interno del servidor al obtener los módulos");
            }
        }

        /// <summary>
        /// Obtiene un módulo por su ID
        /// </summary>
        /// <param name="id">ID del módulo</param>
        /// <param name="includePermissions">Indica si se deben incluir los permisos del módulo</param>
        /// <returns>Detalles del módulo</returns>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.ViewModules)]
        public async Task<ActionResult<ModuleDto>> GetModuleById(
            Guid id,
            [FromQuery] bool includePermissions = false)
        {
            try
            {
                var query = new GetModuleByIdQuery
                {
                    ModuleId = id,
                    IncludePermissions = includePermissions
                };

                var module = await _mediator.Send(query);

                if (module == null)
                {
                    return NotFound($"No se encontró el módulo con ID '{id}'");
                }

                return Ok(module);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el módulo con ID {ModuleId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el módulo");
            }
        }

        /// <summary>
        /// Obtiene los permisos de un módulo
        /// </summary>
        /// <param name="id">ID del módulo</param>
        /// <returns>Lista de permisos del módulo</returns>
        [HttpGet("{id}/permissions")]
        [RequirePermission(PermissionConstants.ViewModules)]
        public async Task<ActionResult<List<PermissionDto>>> GetModulePermissions(Guid id)
        {
            try
            {
                var query = new GetModulePermissionsQuery { ModuleId = id };
                var permissions = await _mediator.Send(query);
                return Ok(permissions);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al obtener permisos del módulo {ModuleId}: {Message}", id, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener los permisos del módulo con ID {ModuleId}", id);
                return StatusCode(500, "Error interno del servidor al obtener los permisos del módulo");
            }
        }

        /// <summary>
        /// Crea un nuevo módulo
        /// </summary>
        /// <param name="command">Datos del módulo a crear</param>
        /// <returns>Módulo creado</returns>
        [HttpPost]
        [RequirePermission(PermissionConstants.CreateModules)]
        public async Task<ActionResult<ModuleResponseDto>> CreateModule([FromBody] CreateModuleCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetModuleById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear un módulo: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un módulo");
                return StatusCode(500, "Error interno del servidor al crear el módulo");
            }
        }

        /// <summary>
        /// Actualiza un módulo existente
        /// </summary>
        /// <param name="id">ID del módulo</param>
        /// <param name="command">Datos actualizados del módulo</param>
        /// <returns>Módulo actualizado</returns>
        [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.UpdateModules)]
        public async Task<ActionResult<ModuleResponseDto>> UpdateModule(Guid id, [FromBody] UpdateModuleCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest("El ID del módulo en la URL no coincide con el ID en el cuerpo de la solicitud");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar el módulo {ModuleId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el módulo con ID {ModuleId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar el módulo");
            }
        }

        /// <summary>
        /// Elimina un módulo
        /// </summary>
        /// <param name="id">ID del módulo a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [RequirePermission(PermissionConstants.DeleteModules)]
        public async Task<ActionResult<ModuleResponseDto>> DeleteModule(Guid id)
        {
            try
            {
                var command = new DeleteModuleCommand { Id = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al eliminar el módulo {ModuleId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el módulo con ID {ModuleId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar el módulo");
            }
        }

        /// <summary>
        /// Asocia un permiso a un módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{moduleId}/permissions/{permissionId}")]
        [RequirePermission(PermissionConstants.UpdateModules)]
        public async Task<ActionResult<ModulePermissionResponseDto>> AddPermissionToModule(Guid moduleId, Guid permissionId)
        {
            try
            {
                var command = new AddPermissionToModuleCommand
                {
                    ModuleId = moduleId,
                    PermissionId = permissionId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al asociar el permiso {PermissionId} al módulo {ModuleId}: {Message}", permissionId, moduleId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asociar el permiso {PermissionId} al módulo {ModuleId}", permissionId, moduleId);
                return StatusCode(500, "Error interno del servidor al asociar el permiso al módulo");
            }
        }

        /// <summary>
        /// Quita un permiso de un módulo
        /// </summary>
        /// <param name="moduleId">ID del módulo</param>
        /// <param name="permissionId">ID del permiso</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{moduleId}/permissions/{permissionId}")]
        [RequirePermission(PermissionConstants.UpdateModules)]
        public async Task<ActionResult<ModulePermissionResponseDto>> RemovePermissionFromModule(Guid moduleId, Guid permissionId)
        {
            try
            {
                var command = new RemovePermissionFromModuleCommand
                {
                    ModuleId = moduleId,
                    PermissionId = permissionId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al quitar el permiso {PermissionId} del módulo {ModuleId}: {Message}", permissionId, moduleId, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar el permiso {PermissionId} del módulo {ModuleId}", permissionId, moduleId);
                return StatusCode(500, "Error interno del servidor al quitar el permiso del módulo");
            }
        }
    }
}
