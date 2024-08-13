using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.SendGrid;

namespace Passwordless.Common.Tests.Services.Mail;

public class OrderedMailProviderTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IOptionsSnapshot<MailConfiguration>> _optionsMock;
    private readonly Mock<IMailProviderFactory> _factoryMock;

    private readonly OrderedMailProvider _sut;

    public OrderedMailProviderTests()
    {
        _optionsMock = new Mock<IOptionsSnapshot<MailConfiguration>>();
        _factoryMock = new Mock<IMailProviderFactory>();
        Mock<ILogger<OrderedMailProvider>> loggerMock = new();

        _sut = new OrderedMailProvider(_optionsMock.Object, _factoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_Throws_InvalidOperationException_WhenNoProvidersAreRegistered()
    {
        // Arrange
        var mailMessage = _fixture.Create<MailMessage>();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers = new List<BaseProviderOptions>()
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        // Act
        var actual = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _sut.SendAsync(mailMessage));

        // Assert
        Assert.Equal(OrderedMailProvider.FallBackFailedMessage, actual.Message);
    }

    [Fact]
    public async Task SendAsync_Overrides_MailMessageSenderFromConfiguration_WhenMailMessageHasNoSender()
    {
        // Arrange
        var mailMessage = _fixture.Build<MailMessage>().Without(x => x.From).Create();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers = new List<BaseProviderOptions>
            {
                _fixture.Build<AwsProviderOptions>()
                    .With(x => x.Name, AwsProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridProviderOptions>()
                    .With(x => x.Name, SendGridProviderOptions.Provider)
                    .Create()
            }
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<BaseProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<AwsProviderOptions>()), Times.Once);
        awsProviderMock.Verify(x => x.SendAsync(It.Is<MailMessage>(p => p.From == mailOptions.From)), Times.Once);
    }

    [Fact]
    public async Task SendAsync_DoesNotOverride_MailMessageSenderFromConfiguration_WhenMailMessageHasSender()
    {
        // Arrange
        var mailMessage = _fixture.Build<MailMessage>().Create();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers = new List<BaseProviderOptions>
            {
                _fixture.Build<AwsProviderOptions>()
                    .With(x => x.Name, AwsProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridProviderOptions>()
                    .With(x => x.Name, SendGridProviderOptions.Provider)
                    .Create()
            }
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<BaseProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<AwsProviderOptions>()), Times.Once);
        awsProviderMock.Verify(x => x.SendAsync(It.Is<MailMessage>(p => p.From == mailOptions.From)), Times.Never);
        awsProviderMock.Verify(x => x.SendAsync(It.Is<MailMessage>(p => p.From == mailMessage.From)), Times.Once);
    }


    [Fact]
    public async Task SendAsync_Executes_FirstRegisteredProviderFirst_WhenMultipleAreFound()
    {
        // Arrange
        var mailMessage = _fixture.Create<MailMessage>();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers = new List<BaseProviderOptions>
            {
                _fixture.Build<AwsProviderOptions>()
                    .With(x => x.Name, AwsProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridProviderOptions>()
                    .With(x => x.Name, SendGridProviderOptions.Provider)
                    .Create()
            }
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<BaseProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<AwsProviderOptions>()), Times.Once);
    }

    [Fact]
    public async Task SendAsync_Attempts_ToUseSecondProvider_WhenFirstProviderFails()
    {
        // Arrange
        var mailMessage = _fixture.Create<MailMessage>();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers = new List<BaseProviderOptions>
            {
                _fixture.Build<AwsProviderOptions>()
                    .With(x => x.Name, AwsProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridProviderOptions>()
                    .With(x => x.Name, SendGridProviderOptions.Provider)
                    .Create()
            }
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        awsProviderMock.Setup(x => x.SendAsync(It.IsAny<MailMessage>()))
            .Throws<Exception>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<BaseProviderOptions>()))
            .Returns(awsProviderMock.Object);

        var sendGridProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == SendGridProviderOptions.Provider), It.IsAny<BaseProviderOptions>()))
            .Returns(sendGridProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(x => x.Create(It.Is<string>(y => y == AwsProviderOptions.Provider), It.IsAny<AwsProviderOptions>()), Times.Once);
        _factoryMock.Verify(x => x.Create(It.Is<string>(y => y == SendGridProviderOptions.Provider), It.IsAny<SendGridProviderOptions>()), Times.Once);

    }
}