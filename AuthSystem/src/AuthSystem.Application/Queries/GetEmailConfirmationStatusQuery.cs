using System.Threading;
using System.Threading.Tasks;
using AuthSystem.Application.Common;
using AuthSystem.Core.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AuthSystem.Application.Queries
{
    public class GetEmailConfirmationStatusQuery : IRequest<EmailCommandResult>
    {
        public string Email { get; set; }
    }

    public class GetEmailConfirmationStatusQueryHandler : IRequestHandler<GetEmailConfirmationStatusQuery, EmailCommandResult>
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<GetEmailConfirmationStatusQueryHandler> _logger;

        public GetEmailConfirmationStatusQueryHandler(
            IUserRepository userRepository,
            ILogger<GetEmailConfirmationStatusQueryHandler> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<EmailCommandResult> Handle(GetEmailConfirmationStatusQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Verificando estado de confirmación para {Email}", request.Email);

            var user = await _userRepository.GetByEmailAsync(request.Email);
            
            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado para email {Email}", request.Email);
                return EmailCommandResult.Failure("Usuario no encontrado");
            }

            _logger.LogInformation("Estado de confirmación para {Email}: {IsConfirmed}", 
                request.Email, user.EmailConfirmed);
            
            return EmailCommandResult.Success(
                user.EmailConfirmed ? "Email confirmado" : "Email no confirmado",
                user.Id,
                user.EmailConfirmed);
        }
    }
}
