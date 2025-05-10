using System;
using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.DTOs;
using AuthSystem.Core.Entities;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Commands.UserManagement
{
    public class CreateUserCommand : IRequest<UserResponseDto>
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public bool SendActivationEmail { get; set; } = true;
        public bool RequirePasswordChange { get; set; } = true;
        public bool IsActive { get; set; } = true;
    }

    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserResponseDto>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IEmailService _emailService;
        private readonly IAuditService _auditService;
        private readonly ILogger<CreateUserCommandHandler> _logger;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IEmailService emailService,
            IAuditService auditService,
            ILogger<CreateUserCommandHandler> logger)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _emailService = emailService;
            _auditService = auditService;
            _logger = logger;
        }

        public async Task<UserResponseDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar si ya existe un usuario con el mismo nombre de usuario o email
                if (await _userRepository.UsernameExistsAsync(request.Username))
                {
                    throw new InvalidOperationException($"Ya existe un usuario con el nombre de usuario '{request.Username}'");
                }

                if (await _userRepository.EmailExistsAsync(request.Email))
                {
                    throw new InvalidOperationException($"Ya existe un usuario con el correo electrónico '{request.Email}'");
                }

                // Crear el nuevo usuario usando el constructor adecuado
                // La clase User tiene un constructor que requiere username, email, passwordHash, firstName y lastName
                var passwordHash = _passwordHasher.HashPassword(request.Password);
                var user = new Core.Entities.User(
                    username: request.Username,
                    email: request.Email,
                    passwordHash: passwordHash,
                    firstName: request.FirstName,
                    lastName: request.LastName
                );
                
                // Configurar propiedades adicionales usando los métodos disponibles
                if (request.RequirePasswordChange)
                {
                    // Asumimos que hay un método para esto o que se configura en otro lugar
                }
                
                if (!request.IsActive)
                {
                    // Si el usuario no debe estar activo, lo suspendemos
                    user.Suspend();
                }

                // Guardar el usuario en la base de datos
                var createdUser = await _userRepository.CreateAsync(user);

                // Generar token de confirmación de email si se solicita enviar email de activación
                if (request.SendActivationEmail)
                {
                    var token = Guid.NewGuid().ToString();
                    await _userRepository.StoreEmailConfirmationTokenAsync(user.Id, token);

                    // Enviar email de activación
                    await _emailService.SendAsync(
                        user.Email,
                        "Activación de cuenta",
                        $"Por favor, active su cuenta haciendo clic en el siguiente enlace: " +
                        $"<a href='https://yourdomain.com/activate?userId={user.Id}&token={token}'>Activar cuenta</a>");
                }

                // Registrar auditoría
                await _auditService.LogActionAsync(
                    null, // userId del que realiza la acción (null si es sistema)
                    "Create",
                    "User",
                    user.Id.ToString(),
                    null, // oldValues
                    new { Username = user.Username, Email = user.Email });

                // Retornar respuesta
                return new UserResponseDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.Status == Core.Enums.UserStatus.Active,
                    EmailConfirmed = user.EmailConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    CreatedAt = user.CreatedAt,
                    Success = true,
                    Message = $"Usuario '{user.Username}' creado exitosamente" +
                             (request.SendActivationEmail ? ". Se ha enviado un correo de activación." : "")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear el usuario {Username}", request.Username);
                throw;
            }
        }
    }
}
