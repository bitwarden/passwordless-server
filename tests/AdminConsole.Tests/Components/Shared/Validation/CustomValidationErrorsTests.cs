using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using Passwordless.AdminConsole.Components.Shared.Validation;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Validation;

public class CustomValidationErrorsTests : BunitContext
{
    [Fact]
    public void CustomValidationErrors_RendersNothing_WhenNoValidationMessages()
    {
        // Arrange
        var editContext = new EditContext(new object());

        // Act
        var cut = Render<CustomValidationErrors>(parameters => parameters
            .Add(p => p.EditContext, editContext));

        // Assert
        Assert.Empty(cut.Markup);
    }

    [Fact]
    public void CustomValidationErrors_RendersValidationMessages_WhenValidationMessagesExist()
    {
        // Arrange
        var editContext = new EditContext(new object());
        var validationMessageStore = new ValidationMessageStore(editContext);
        validationMessageStore.Add(new FieldIdentifier(new object(), "Field"), "My error message.");

        // Act
        var cut = Render<CustomValidationErrors>(parameters => parameters
            .Add(p => p.EditContext, editContext));

        // Assert
        cut.MarkupMatches("<ul class=\"validation-errors\"><li class=\"validation-message\">My error message.</li></ul>");
    }
}