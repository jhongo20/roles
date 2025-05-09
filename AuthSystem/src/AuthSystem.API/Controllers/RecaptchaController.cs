using System;
using System.Threading.Tasks;
using AuthSystem.Application.Commands.Authentication;
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
    public class RecaptchaController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IRecaptchaService _recaptchaService;
        private readonly ILogger<RecaptchaController> _logger;

        public RecaptchaController(
            IMediator mediator,
            IRecaptchaService recaptchaService,
            ILogger<RecaptchaController> logger)
        {
            _mediator = mediator;
            _recaptchaService = recaptchaService;
            _logger = logger;
        }

        /// <summary>
        /// Verifica un token de reCAPTCHA
        /// </summary>
        /// <param name="request">Solicitud con el token de reCAPTCHA</param>
        /// <returns>Resultado de la verificación</returns>
        [HttpPost("verify")]
        [AllowAnonymous]
        public async Task<ActionResult<RecaptchaVerificationResponse>> VerifyToken([FromBody] RecaptchaVerificationRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Token))
                {
                    return BadRequest(new RecaptchaVerificationResponse
                    {
                        Success = false,
                        Message = "El token de reCAPTCHA es requerido"
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var isValid = await _recaptchaService.ValidateTokenAsync(request.Token, ipAddress);

                return Ok(new RecaptchaVerificationResponse
                {
                    Success = isValid,
                    Message = isValid 
                        ? "Verificación exitosa" 
                        : "Verificación fallida. Por favor, intente nuevamente."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar token de reCAPTCHA");
                return StatusCode(500, new RecaptchaVerificationResponse
                {
                    Success = false,
                    Message = "Ha ocurrido un error al verificar el token de reCAPTCHA"
                });
            }
        }

        /// <summary>
        /// Registra un nuevo usuario con validación de reCAPTCHA
        /// </summary>
        /// <param name="request">Datos de registro del usuario</param>
        /// <returns>Resultado del registro</returns>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            try
            {
                // Validar primero el token de reCAPTCHA
                if (string.IsNullOrEmpty(request.RecaptchaToken))
                {
                    return BadRequest(new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "El token de reCAPTCHA es requerido"
                    });
                }

                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var isValidRecaptcha = await _recaptchaService.ValidateTokenAsync(request.RecaptchaToken, ipAddress);

                if (!isValidRecaptcha)
                {
                    _logger.LogWarning("Intento de registro con token de reCAPTCHA inválido: {IP}", ipAddress);
                    return BadRequest(new AuthResponseDto
                    {
                        Succeeded = false,
                        Error = "Verificación de reCAPTCHA fallida. Por favor, intente nuevamente."
                    });
                }

                // Crear comando de registro
                var command = new RegisterCommand
                {
                    Username = request.Username,
                    Email = request.Email,
                    Password = request.Password,
                    ConfirmPassword = request.ConfirmPassword,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    RecaptchaToken = request.RecaptchaToken,
                    IpAddress = ipAddress,
                    UserAgent = HttpContext.Request.Headers["User-Agent"].ToString()
                };

                // Enviar comando al handler
                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(result);
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en el proceso de registro");
                return StatusCode(500, new AuthResponseDto
                {
                    Succeeded = false,
                    Error = "Ha ocurrido un error durante el registro. Por favor, inténtelo de nuevo más tarde."
                });
            }
        }

        /// <summary>
        /// Obtiene la configuración pública de reCAPTCHA (solo la clave del sitio)
        /// </summary>
        /// <returns>Configuración pública de reCAPTCHA</returns>
        [HttpGet("config")]
        [AllowAnonymous]
        public ActionResult<RecaptchaConfigResponse> GetConfig()
        {
            try
            {
                // Obtener la configuración del servicio
                var config = _recaptchaService.GetPublicConfig();
                
                return Ok(new RecaptchaConfigResponse
                {
                    SiteKey = config.SiteKey
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener configuración de reCAPTCHA");
                return StatusCode(500, new RecaptchaConfigResponse
                {
                    SiteKey = string.Empty,
                    Error = "Ha ocurrido un error al obtener la configuración de reCAPTCHA"
                });
            }
        }
    }

    public class RecaptchaVerificationRequest
    {
        public string Token { get; set; }
    }

    public class RecaptchaVerificationResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class RecaptchaConfigResponse
    {
        public string SiteKey { get; set; }
        public string Error { get; set; }
    }
}
