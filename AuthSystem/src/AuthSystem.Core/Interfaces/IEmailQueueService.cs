using System;
using System.Threading.Tasks;

namespace AuthSystem.Core.Interfaces
{
    public interface IEmailQueueService
    {
        void QueueConfirmationEmail(string email, Guid userId, string token);
        void QueuePasswordResetEmail(string email, Guid userId, string token);
        void QueueTwoFactorCodeEmail(string email, string code);
        void QueueGenericEmail(string to, string subject, string body, bool isHtml = true);
    }
}
