using System;
using AuthSystem.Application.Common;
using MediatR;

namespace AuthSystem.Application.Commands.Email
{
    public class GenerateEmailConfirmationTokenCommand : IRequest<EmailCommandResult>
    {
        public string Email { get; set; }
    }
}