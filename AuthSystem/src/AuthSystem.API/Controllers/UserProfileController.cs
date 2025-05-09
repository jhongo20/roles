using System;
using System.Threading.Tasks;
using AuthSystem.Application.Commands.UserProfile;
using AuthSystem.Application.DTOs;
using AuthSystem.Application.Queries.UserProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserProfileController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UserProfileController> _logger;

        public UserProfileController(IMediator mediator, ILogger<UserProfileController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<UserDto>> GetUserProfile()
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Usuario no identificado" });
                }

                var query = new GetUserProfileQuery { UserId = Guid.Parse(userId) };
                var result = await _mediator.Send(query);

                if (result == null)
                {
                    return NotFound(new { error = "Perfil de usuario no encontrado" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener el perfil de usuario");
                return StatusCode(500, new { error = "Ha ocurrido un error al obtener el perfil de usuario" });
            }
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserProfile([FromBody] UpdateUserProfileCommand command)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Usuario no identificado" });
                }

                command.UserId = Guid.Parse(userId);

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Perfil actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el perfil de usuario");
                return StatusCode(500, new { error = "Ha ocurrido un error al actualizar el perfil de usuario" });
            }
        }

        [HttpPut("phone")]
        public async Task<ActionResult> UpdatePhoneNumber([FromBody] UpdatePhoneNumberCommand command)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Usuario no identificado" });
                }

                command.UserId = Guid.Parse(userId);

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Número de teléfono actualizado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar el número de teléfono");
                return StatusCode(500, new { error = "Ha ocurrido un error al actualizar el número de teléfono" });
            }
        }

        [HttpPut("password")]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            try
            {
                // Obtener el ID del usuario del token JWT
                var userId = User.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return BadRequest(new { error = "Usuario no identificado" });
                }

                command.UserId = Guid.Parse(userId);

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(new { error = result.Error });
                }

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar la contraseña");
                return StatusCode(500, new { error = "Ha ocurrido un error al cambiar la contraseña" });
            }
        }
    }
}
