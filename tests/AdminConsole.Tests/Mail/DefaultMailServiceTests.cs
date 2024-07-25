using Microsoft.Extensions.Options;
using Moq;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.Common.Services.Mail;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Mail;

public class DefaultMailServiceTests
{
    private readonly Mock<IOptionsSnapshot<MailConfiguration>> _configurationMock = new();
    private readonly Mock<IMailProvider> _providerMock = new();

    private readonly DefaultMailService _sut;

    public DefaultMailServiceTests()
    {
        _sut = new DefaultMailService(_configurationMock.Object, _providerMock.Object);
    }

    [InlineData("a")]
    [Theory]
    public async Task SendMagicLinksDisabledAsync_Throws_ArgumentException_WhenEmailAddressIsInvalid(string email)
    {
        // Arrange
        _configurationMock.SetupGet(x => x.Value).Returns(new MailConfiguration());

        // Act
        var actual = await Assert.ThrowsAsync<ArgumentException>(async () =>
            await _sut.SendMagicLinksDisabledAsync("organizationName", email));

        // Assert
        Assert.Equal("email", actual.ParamName);
        Assert.Equal("The e-mail address provided is invalid. (Parameter 'email')", actual.Message);
    }

    [Fact]
    public async Task SendMagicLinksDisabledAsync_SendsEmail_WhenEmailAddressIsValid()
    {
        // Arrange
        var organizationName = "Contoso";
        const string email = "johndoe@example.com";
        _configurationMock.SetupGet(x => x.Value).Returns(new MailConfiguration());

        // Act
        await _sut.SendMagicLinksDisabledAsync(organizationName, email);

        // Assert
        _providerMock.Verify(x => x.SendAsync(It.Is<MailMessage>(m =>
            m.To.All(recipient => recipient == email) &&
            m.Subject == $"Magic links have been disabled for 'Contoso'" &&
            m.TextBody.Contains(organizationName) &&
            m.Tag == "magic-links-disabled"
        )), Times.Once);
    }
}