using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AdminConsole.TagHelpers;

[HtmlTargetElement("script", Attributes = "")]
public class ScriptTagNonce : TagHelper
{
    private readonly IHttpContextAccessor _accessor;

    public ScriptTagNonce(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (_accessor.HttpContext?.Items["csp-nonce"] is string nonce)
        {
            output.Attributes.Add("nonce", nonce);
        }
    }
}