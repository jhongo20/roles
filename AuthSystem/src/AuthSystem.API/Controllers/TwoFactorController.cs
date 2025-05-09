using System;
using System.Threading.Tasks;
using AuthSystem.Application.Commands.TwoFactor;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TwoFactorController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<TwoFactorController> _logger;
        private readonly ISmsService _smsService;
        private readonly IUserRepository _userRepository;

        public TwoFactorController(
            IMediator mediator, 
            ILogger<TwoFactorController> logger,
            ISmsService smsService,
            IUserRepository userRepository)
        {
            _mediator = mediator;
            _logger = logger;
            _smsService = smsService;
            _userRepository = userRepository;
        }

        [HttpPost("send-code")]
        [AllowAnonymous]
        public async Task<ActionResult> SendVerificationCode([FromBody] SendTwoFactorCodeCommand command)
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar código de verificación 2FA");
                return StatusCode(500, new { error = "Ha ocurrido un error al enviar el código de verificación. Por favor, inténtelo de nuevo más tarde." });
            }
        }

        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> VerifyTwoFactor([FromBody] VerifyTwoFactorCommand command)
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

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en la verificación 2FA");
                return StatusCode(500, new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error durante la verificación. Por favor, inténtelo de nuevo más tarde."
                });
            }
        }

        [HttpPost("enable")]
        [Authorize]
        public async Task<ActionResult> EnableTwoFactor([FromBody] EnableTwoFactorCommand command)
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
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al habilitar 2FA");
                return StatusCode(500, new { error = "Ha ocurrido un error al habilitar la autenticación de dos factores. Por favor, inténtelo de nuevo más tarde." });
            }
        }

        [HttpPost("disable")]
        [Authorize]
        public async Task<ActionResult> DisableTwoFactor([FromBody] DisableTwoFactorCommand command)
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
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al deshabilitar 2FA");
                return StatusCode(500, new { error = "Ha ocurrido un error al deshabilitar la autenticación de dos factores. Por favor, inténtelo de nuevo más tarde." });
            }
        }
    }
}
