namespace Passwordless.AdminConsole.RoutingHelpers;


public class LinkGeneratorDecorator : LinkGenerator
{
    private readonly LinkGenerator _innerLinkGenerator;
    public LinkGeneratorDecorator(IServiceProvider serviceProvider, Type linkGeneratorType)
    {
        _innerLinkGenerator = (LinkGenerator)ActivatorUtilities.CreateInstance(serviceProvider, linkGeneratorType);
    }

    public override string GetPathByAddress<TAddress>(
      HttpContext httpContext,
      TAddress address,
      RouteValueDictionary values,
      RouteValueDictionary ambientValues = null,
      PathString? pathBase = null,
      FragmentString fragment = default,
      LinkOptions options = null)
    {
        // clone the explicit route values
        var newValues = new RouteValueDictionary(values);

        // get culture from ambient route values
        if (ambientValues?.TryGetValue("app", out var value) == true)
        {
            // add to the cloned route values to make it explicit
            // respects existing explicit value if specified
            newValues.TryAdd("app", value);
        }

        return _innerLinkGenerator.GetPathByAddress(httpContext, address, newValues,
          ambientValues, pathBase, fragment, options);
    }

    public override string? GetPathByAddress<TAddress>(TAddress address, RouteValueDictionary values, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    {
        // clone the explicit route values
        var newValues = new RouteValueDictionary(values);

        // get culture from ambient route values
        //if (ambientValues?.TryGetValue("culture", out var value) == true)
        //{
        //    // add to the cloned route values to make it explicit
        //    // respects existing explicit value if specified
        //    newValues.TryAdd("culture", value);
        //}

        return _innerLinkGenerator.GetPathByAddress(address, newValues,
           pathBase, fragment, options);
    }

    public override string? GetUriByAddress<TAddress>(HttpContext httpContext, TAddress address, RouteValueDictionary values, RouteValueDictionary? ambientValues = null, string? scheme = null, HostString? host = null, PathString? pathBase = null, FragmentString fragment = default, LinkOptions? options = null)
    {
        // clone the explicit route values
        var newValues = new RouteValueDictionary(values);

        // get culture from ambient route values
        if (ambientValues?.TryGetValue("tenant", out var value) == true)
        {
            // add to the cloned route values to make it explicit
            // respects existing explicit value if specified
            newValues.TryAdd("tenant", value);
        }

        return _innerLinkGenerator.GetUriByAddress(httpContext, address, newValues,
          ambientValues, scheme, host, pathBase, fragment, options);
    }

    public override string? GetUriByAddress<TAddress>(TAddress address, RouteValueDictionary values, string? scheme, HostString host, PathString pathBase = default, FragmentString fragment = default, LinkOptions? options = null)
    {

        return _innerLinkGenerator.GetUriByAddress(address, values,
           scheme, host, pathBase, fragment, options);
    }

    // do the same with GetUriByAddress<TAddress> overload with ambient values,
    // straight pass-through for overloads without ambient values to use
}