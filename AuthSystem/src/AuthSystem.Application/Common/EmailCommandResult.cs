using System;

namespace AuthSystem.Application.Common
{
    public class EmailCommandResult
    {
        public bool Succeeded { get; private set; }
        public string Message { get; private set; }
        public Guid? UserId { get; private set; }
        public bool IsConfirmed { get; private set; }

        private EmailCommandResult(bool succeeded, string message, Guid? userId = null, bool isConfirmed = false)
        {
            Succeeded = succeeded;
            Message = message;
            UserId = userId;
            IsConfirmed = isConfirmed;
        }

        public static EmailCommandResult Success(string message = "Operación completada con éxito", Guid? userId = null, bool isConfirmed = false)
        {
            return new EmailCommandResult(true, message, userId, isConfirmed);
        }

        public static EmailCommandResult Failure(string message)
        {
            return new EmailCommandResult(false, message);
        }
    }
}
