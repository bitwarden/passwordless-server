using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Passwordless.AdminConsole.Components.Shared;

public sealed class Page : ComponentBase
{
    [Parameter]
    public required string Title { get; set; }
    
    [Parameter]
    public required RenderFragment ChildContent { get; set; }
    
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<Microsoft.AspNetCore.Components.Web.PageTitle>(0);
        builder.AddAttribute(1, nameof(PageTitle.ChildContent), (RenderFragment)(b => {
                    b.AddContent(2, Title);
                }
            ));
        builder.CloseComponent();
        
        builder.OpenElement(3, "div");
        builder.AddAttribute(4, "class", "page-header-container");
        builder.OpenElement(5, "h1");
        builder.AddContent(6, Title);
        builder.CloseElement();
        builder.CloseElement();
        
        builder.OpenElement(7, "div");
        builder.AddAttribute(8, "class", "page-content-container");
        builder.AddContent(9, ChildContent);
        builder.CloseElement();
    }
}