using Microsoft.Extensions.Logging;
using Moq;
using Passwordless.Common.Services.Mail;

namespace Passwordless.Common.Tests.Services.Mail;

public class PostmarkMailProviderTests
{
    [Fact]
    public async Task SendAsync_GivenMessageWithMessageType_WhenCorrespondingClientDoesNotExist_ThenWarningShouldBeLogged()
    {
        // Arrange
        var configuration = new PostmarkMailProviderConfiguration
        {
            DefaultConfiguration = new PostmarkClientConfiguration
            {
                Name = "Default",
                ApiKey = "ApiKey",
                From = "do-not-reply@passwordless.dev"
            }
        };
        var logger = new Mock<ILogger<PostmarkMailProvider>>();
        var sut = new PostmarkMailProvider(configuration, logger.Object);
        var message = new MailMessage
        {
            To = new List<string> { "mail@example.com"},
            TextBody = "Test message",
            MessageType = "Misconfigured Message Stream"
        };

        // Act
        try
        {
            await sut.SendAsync(message);
        }
        catch
        {
            // ignored
        }

        // Assert
        logger.Verify(x => x.Log(
                LogLevel.Warning, 
                It.IsAny<EventId>(), 
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains($"MessageType {message.MessageType} was set but default Postmark Mail client was used.")), 
                It.IsAny<Exception>(), 
                ((Func<It.IsAnyType, Exception, string>) It.IsAny<object>())!), 
            Times.Once);
    }
}