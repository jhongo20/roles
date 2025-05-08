using System;
using AuthSystem.Application.Commands.Email;
using AuthSystem.Application.Common;
using AuthSystem.Application.Queries;
using Xunit;

namespace AuthSystem.IntegrationTests.EmailConfirmation
{
    public class EmailConfirmationCommandTests
    {
        [Fact]
        public void GenerateEmailConfirmationToken_IsCorrectlyDefined()
        {
            // Arrange
            var command = new GenerateEmailConfirmationTokenCommand { Email = "test@example.com" };
            
            // Assert
            Assert.NotNull(command);
            Assert.Equal("test@example.com", command.Email);
        }

        [Fact]
        public void VerifyEmailConfirmationToken_IsCorrectlyDefined()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var command = new VerifyEmailConfirmationTokenCommand 
            { 
                UserId = userId, 
                Token = "valid-token" 
            };
            
            // Assert
            Assert.NotNull(command);
            Assert.Equal(userId, command.UserId);
            Assert.Equal("valid-token", command.Token);
        }

        [Fact]
        public void ResendEmailConfirmation_IsCorrectlyDefined()
        {
            // Arrange
            var command = new ResendEmailConfirmationCommand { Email = "test@example.com" };
            
            // Assert
            Assert.NotNull(command);
            Assert.Equal("test@example.com", command.Email);
        }

        [Fact]
        public void GetEmailConfirmationStatus_IsCorrectlyDefined()
        {
            // Arrange
            var query = new GetEmailConfirmationStatusQuery { Email = "test@example.com" };
            
            // Assert
            Assert.NotNull(query);
            Assert.Equal("test@example.com", query.Email);
        }
        
        [Fact]
        public void EmailCommandResult_Success_ReturnsCorrectResult()
        {
            // Arrange & Act
            var userId = Guid.NewGuid();
            var result = EmailCommandResult.Success("Test message", userId, true);
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test message", result.Message);
            Assert.Equal(userId, result.UserId);
        }
        
        [Fact]
        public void EmailCommandResult_Failure_ReturnsCorrectResult()
        {
            // Arrange & Act
            var result = EmailCommandResult.Failure("Error message");
            
            // Assert
            Assert.NotNull(result);
            Assert.Equal("Error message", result.Message);
        }
    }
}
