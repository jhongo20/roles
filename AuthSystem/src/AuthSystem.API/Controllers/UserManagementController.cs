using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthSystem.API.Filters;
using AuthSystem.Application.Commands.UserManagement;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.UserManagement;
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
    public class UserManagementController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserManagementController> _logger;

        public UserManagementController(
            IMediator mediator,
            ILogger<UserManagementController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene todos los usuarios
        /// </summary>
        /// <param name="includeInactive">Indica si se deben incluir usuarios inactivos</param>
        /// <param name="searchTerm">Término de búsqueda (nombre, email, etc.)</param>
        /// <param name="pageNumber">Número de página para paginación</param>
        /// <param name="pageSize">Tamaño de página para paginación</param>
        /// <returns>Lista de usuarios</returns>
        [HttpGet]
        [RequirePermission(PermissionConstants.ViewUsers)]
        public async Task<ActionResult<List<UserDto>>> GetAllUsers(
            [FromQuery] bool includeInactive = false,
            [FromQuery] string searchTerm = null,
            [FromQuery] int? pageNumber = null,
            [FromQuery] int? pageSize = null)
        {
            try
            {
                var query = new GetAllUsersQuery
                {
                    IncludeInactive = includeInactive,
                    SearchTerm = searchTerm,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var users = await _mediator.Send(query);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todos los usuarios");
                return StatusCode(500, "Error interno del servidor al obtener los usuarios");
            }
        }

        /// <summary>
        /// Obtiene un usuario por su ID
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Detalles del usuario</returns>
        [HttpGet("{id}")]
        [RequirePermission(PermissionConstants.ViewUsers)]
        public async Task<ActionResult<UserDto>> GetUserById(Guid id)
        {
            try
            {
                var query = new GetUserByIdQuery { UserId = id };
                var user = await _mediator.Send(query);

                if (user == null)
                {
                    return NotFound($"No se encontró el usuario con ID '{id}'");
                }

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al obtener el usuario");
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        /// <param name="command">Datos del usuario a crear</param>
        /// <returns>Usuario creado</returns>
        [HttpPost]
        [RequirePermission(PermissionConstants.CreateUsers)]
        public async Task<ActionResult<UserResponseDto>> CreateUser([FromBody] CreateUserCommand command)
        {
            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al crear un usuario: {Message}", ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear un usuario");
                return StatusCode(500, "Error interno del servidor al crear el usuario");
            }
        }

        /// <summary>
        /// Actualiza un usuario existente
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="command">Datos actualizados del usuario</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}")]
        [RequirePermission(PermissionConstants.UpdateUsers)]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            try
            {
                if (id != command.Id)
                {
                    return BadRequest("El ID del usuario en la URL no coincide con el ID en el cuerpo de la solicitud");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al actualizar el usuario {UserId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al actualizar el usuario");
            }
        }

        /// <summary>
        /// Cambia el estado de un usuario (activar, desactivar, suspender, bloquear)
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="command">Datos del cambio de estado</param>
        /// <returns>Usuario actualizado</returns>
        [HttpPut("{id}/status")]
        [RequirePermission(PermissionConstants.UpdateUsers)]
        public async Task<ActionResult<UserResponseDto>> ChangeUserStatus(Guid id, [FromBody] ChangeUserStatusCommand command)
        {
            try
            {
                if (id != command.UserId)
                {
                    return BadRequest("El ID del usuario en la URL no coincide con el ID en el cuerpo de la solicitud");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al cambiar el estado del usuario {UserId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar el estado del usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al cambiar el estado del usuario");
            }
        }

        /// <summary>
        /// Restablece la contraseña de un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <param name="command">Datos para el restablecimiento de contraseña</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/reset-password")]
        [RequirePermission(PermissionConstants.UpdateUsers)]
        public async Task<ActionResult<UserResponseDto>> ResetUserPassword(Guid id, [FromBody] ResetUserPasswordCommand command)
        {
            try
            {
                if (id != command.UserId)
                {
                    return BadRequest("El ID del usuario en la URL no coincide con el ID en el cuerpo de la solicitud");
                }

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al restablecer la contraseña del usuario {UserId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al restablecer la contraseña del usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al restablecer la contraseña del usuario");
            }
        }

        /// <summary>
        /// Reenvía el correo de activación a un usuario
        /// </summary>
        /// <param name="id">ID del usuario</param>
        /// <returns>Resultado de la operación</returns>
        [HttpPost("{id}/resend-activation")]
        [RequirePermission(PermissionConstants.UpdateUsers)]
        public async Task<ActionResult<UserResponseDto>> ResendActivationEmail(Guid id)
        {
            try
            {
                var command = new ResendActivationEmailCommand { UserId = id };
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al reenviar el correo de activación al usuario {UserId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al reenviar el correo de activación al usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al reenviar el correo de activación");
            }
        }

        /// <summary>
        /// Elimina un usuario (marcándolo como inactivo)
        /// </summary>
        /// <param name="id">ID del usuario a eliminar</param>
        /// <returns>Resultado de la operación</returns>
        [HttpDelete("{id}")]
        [RequirePermission(PermissionConstants.DeleteUsers)]
        public async Task<ActionResult<UserResponseDto>> DeleteUser(Guid id)
        {
            try
            {
                // En lugar de eliminar físicamente, cambiamos el estado a inactivo
                var command = new ChangeUserStatusCommand
                {
                    UserId = id,
                    Status = AuthSystem.Core.Enums.UserStatus.Deleted, // Usamos Deleted en lugar de Inactive ya que no existe ese valor en el enum
                    Reason = "Usuario eliminado por administrador"
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Error de validación al eliminar el usuario {UserId}: {Message}", id, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar el usuario con ID {UserId}", id);
                return StatusCode(500, "Error interno del servidor al eliminar el usuario");
            }
        }
    }
}
