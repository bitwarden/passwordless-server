using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.ViewFeatures.Buffers;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers;



[HtmlTargetElement("feedback")]
public class FeedbackTagHelper : TagHelper
{


    public override int Order => -1001; // beats the default order of -1000

    [HtmlAttributeName("message")]
    public string Message { get; set; }

    [HtmlAttributeName("LinkUrl")]
    public string LinkUrl { get; set; }

    [HtmlAttributeName("LinkText")]
    public string LinkText { get; set; }

    [HtmlAttributeName("Name")]
    public string Name { get; set; }

    [HtmlAttributeName("Model")]
    public object Model { get; set; }


    private readonly PartialTagHelper _partialTagHelper;
    public FeedbackTagHelper(ICompositeViewEngine viewEngine, IViewBufferScope viewBufferScope)
    {
        _partialTagHelper = new(viewEngine, viewBufferScope);
    }

    /// <summary>
    /// A <see cref="ViewDataDictionary"/> to pass into the partial view.
    /// </summary>
    public ViewDataDictionary ViewData { get; set; }

    /// <summary>
    /// Gets the <see cref="Microsoft.AspNetCore.Mvc.Rendering.ViewContext"/> of the executing view.
    /// </summary>
    [HtmlAttributeNotBound]
    [ViewContext]
    public ViewContext ViewContext { get; set; }


    public override void Init(TagHelperContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (Name is not null)
        {
            _partialTagHelper.Name = Name;
        }

        if (Model is not null)
        {
            _partialTagHelper.Model = Model;
        }
        else
        {
            _partialTagHelper.Model = new FeedbackModel(Message, Name, LinkText, LinkUrl);
        }
        _partialTagHelper.ViewData = ViewData;
        _partialTagHelper.ViewContext = ViewContext;
        _partialTagHelper.Init(context);
    }

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {

        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (output == null)
        {
            throw new ArgumentNullException(nameof(output));
        }

        if (context.SuppressedByAspIf())
        {
            // The asp-if Tag Helper has run already and determined this element shouldn't be rendered so just return now.
            output.SuppressOutput();
            return Task.CompletedTask;
        }

        return _partialTagHelper.ProcessAsync(context, output);
    }
}

public record FeedbackModel(string Message, string Name, string LinkText, string LinkUrl);