using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole.Middleware;

namespace Passwordless.AdminConsole.RoutingHelpers;

public static class RazorOptionsExtentions
{
    public static RazorPagesOptions AddTenantRouting(this RazorPagesOptions options)
    {
        options.Conventions.AddFolderRouteModelConvention("/App/", model =>
        {
            var selectorCount = model.Selectors.Count;
            for (var i = 0; i < selectorCount; i++)
            {
                var selector = model.Selectors[i];
                var template = selector.AttributeRouteModel!.Template!;
                if (template.StartsWith("App/"))
                {
                    template = template.Substring(7);
                }
                //selector.AttributeRouteModel.Template = "t/{tenant}/" + template;
                model.Selectors.Add(new SelectorModel
                {
                    AttributeRouteModel = new AttributeRouteModel
                    {
                        Order = 0,
                        Template = $"app/{{{RouteParameters.AppId}}}/{template}"
                    }
                });

                selector.AttributeRouteModel.SuppressLinkGeneration = true;
            }
        });

        return options;
    }
}