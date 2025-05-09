using System;
using System.Threading.Tasks;
using AuthSystem.Application.Commands.Authentication;
using AuthSystem.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IMediator mediator, ILogger<AuthController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] AuthenticateCommand command)
        {
            try
            {
                // Agregar información del cliente a la solicitud
                command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                command.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                if (result.RequiresTwoFactor)
                {
                    // Si se requiere 2FA, devolver un resultado específico
                    return Ok(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de login");
                return StatusCode(500, new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error durante la autenticación. Por favor, inténtelo de nuevo más tarde."
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            try
            {
                command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                command.UserAgent = HttpContext.Request.Headers["User-Agent"].ToString();

                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de refresh token");
                return StatusCode(500, new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error al renovar el token. Por favor, inicie sesión nuevamente."
                });
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] LogoutCommand command)
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
                
                if (!result)
                {
                    return BadRequest(new { error = "No se pudo cerrar la sesión" });
                }

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de logout");
                return StatusCode(500, new { error = "Ha ocurrido un error al cerrar la sesión" });
            }
        }
    }
}
