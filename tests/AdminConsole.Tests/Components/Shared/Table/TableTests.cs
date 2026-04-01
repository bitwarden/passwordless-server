using Bunit;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Components.Shared.Table;

public class TableTests : BunitContext
{
    [Fact]
    public void Table_Renders_ExpectedColumnHeaders()
    {
        // Arrange

        // Act
        var cut = Render<AdminConsole.Components.Shared.Tables.Table>(parameters =>
            parameters.Add(p => p.ColumnHeaders, new List<string> { "Column 1", "Column 2" }));

        // Assert
        var thead = cut.Find("thead");
        thead.MarkupMatches("<thead diff:ignoreAttributes><tr diff:ignoreAttributes><th diff:ignoreAttributes>Column 1</th><th diff:ignoreAttributes>Column 2</th></tr></thead>");
    }

    [Fact]
    public void Table_Renders_EmptyColumnHeaders()
    {
        // Arrange

        // Act
        var cut = Render<AdminConsole.Components.Shared.Tables.Table>(parameters =>
            parameters.Add(p => p.ColumnHeaders, new List<string> { "Column 1", "Column 2", string.Empty }));

        // Assert
        var thead = cut.Find("thead");
        thead.MarkupMatches("<thead diff:ignoreAttributes><tr diff:ignoreAttributes><th diff:ignoreAttributes>Column 1</th><th diff:ignoreAttributes>Column 2</th><th diff:ignoreAttributes></th></tr></thead>");
    }
}