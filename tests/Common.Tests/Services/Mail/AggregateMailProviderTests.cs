using AutoFixture;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Passwordless.Common.Services.Mail;
using Passwordless.Common.Services.Mail.Aws;
using Passwordless.Common.Services.Mail.SendGrid;

namespace Passwordless.Common.Tests.Services.Mail;

public class AggregateMailProviderTests
{
    private readonly Fixture _fixture = new();

    private readonly Mock<IOptionsSnapshot<MailConfiguration>> _optionsMock;
    private readonly Mock<IMailProviderFactory> _factoryMock;

    private readonly AggregateMailProvider _sut;

    public AggregateMailProviderTests()
    {
        _optionsMock = new Mock<IOptionsSnapshot<MailConfiguration>>();
        _factoryMock = new Mock<IMailProviderFactory>();
        Mock<ILogger<AggregateMailProvider>> loggerMock = new();

        _sut = new AggregateMailProvider(_optionsMock.Object, _factoryMock.Object, loggerMock.Object);
    }

    [Fact]
    public async Task SendAsync_NoOp_WhenNoProvidersAreRegistered()
    {
        // Arrange
        var mailMessage = _fixture.Create<MailMessage>();
        var mailOptions = new MailConfiguration { From = "johndoe@example.com", Providers = [] };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        // Act & assert
        await _sut.SendAsync(mailMessage);
    }

    [Fact]
    public async Task SendAsync_Overrides_MailMessageSenderFromConfiguration_WhenMailMessageHasNoSender()
    {
        // Arrange
        var mailMessage = _fixture.Build<MailMessage>().Without(x => x.From).Create();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers =
            [
                _fixture.Build<AwsMailProviderOptions>()
                    .With(x => x.Name, AwsMailProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridMailProviderOptions>()
                    .With(x => x.Name, SendGridMailProviderOptions.Provider)
                    .Create()
            ]
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider),
                It.IsAny<BaseMailProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(
            x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider), It.IsAny<AwsMailProviderOptions>()),
            Times.Once);
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
            Providers =
            [
                _fixture.Build<AwsMailProviderOptions>()
                    .With(x => x.Name, AwsMailProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridMailProviderOptions>()
                    .With(x => x.Name, SendGridMailProviderOptions.Provider)
                    .Create()
            ]
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider),
                It.IsAny<BaseMailProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(
            x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider), It.IsAny<AwsMailProviderOptions>()),
            Times.Once);
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
            Providers =
            [
                _fixture.Build<AwsMailProviderOptions>()
                    .With(x => x.Name, AwsMailProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridMailProviderOptions>()
                    .With(x => x.Name, SendGridMailProviderOptions.Provider)
                    .Create()
            ]
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider),
                It.IsAny<BaseMailProviderOptions>()))
            .Returns(awsProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(
            x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider), It.IsAny<AwsMailProviderOptions>()),
            Times.Once);
    }

    [Fact]
    public async Task SendAsync_Attempts_ToUseSecondProvider_WhenFirstProviderFails()
    {
        // Arrange
        var mailMessage = _fixture.Create<MailMessage>();
        var mailOptions = new MailConfiguration
        {
            From = "johndoe@example.com",
            Providers =
            [
                _fixture.Build<AwsMailProviderOptions>()
                    .With(x => x.Name, AwsMailProviderOptions.Provider)
                    .Create(),
                _fixture.Build<SendGridMailProviderOptions>()
                    .With(x => x.Name, SendGridMailProviderOptions.Provider)
                    .Create()
            ]
        };
        _optionsMock.SetupGet(x => x.Value).Returns(mailOptions);

        var awsProviderMock = new Mock<IMailProvider>();
        awsProviderMock.Setup(x => x.SendAsync(It.IsAny<MailMessage>()))
            .Throws<Exception>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider),
                It.IsAny<BaseMailProviderOptions>()))
            .Returns(awsProviderMock.Object);

        var sendGridProviderMock = new Mock<IMailProvider>();
        _factoryMock.Setup(x => x.Create(It.Is<string>(y => y == SendGridMailProviderOptions.Provider),
                It.IsAny<BaseMailProviderOptions>()))
            .Returns(sendGridProviderMock.Object);

        // Act
        await _sut.SendAsync(mailMessage);

        // Assert
        _factoryMock.Verify(
            x => x.Create(It.Is<string>(y => y == AwsMailProviderOptions.Provider), It.IsAny<AwsMailProviderOptions>()),
            Times.Once);
        _factoryMock.Verify(
            x => x.Create(It.Is<string>(y => y == SendGridMailProviderOptions.Provider),
                It.IsAny<SendGridMailProviderOptions>()), Times.Once);

    }
}