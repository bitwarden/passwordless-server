using Passwordless.AdminConsole.Components.Shared.Cards;
using Passwordless.AdminConsole.Components.Shared.Icons.DeveloperIcons;
using Passwordless.Common.Configuration;

namespace Passwordless.AdminConsole.Components.Pages.App.Onboarding;

public partial class GetStarted : BaseApplicationPage
{
    private IEnumerable<LinkCard.LinkCardModel>? _backendCards;
    private readonly IEnumerable<LinkCard.LinkCardModel> _frontendCards;

    public GetStarted()
    {
        _frontendCards = new LinkCard.LinkCardModel[]
        {
            new("Android", "Docs + Examples", typeof(AndroidIcon), new Uri("https://docs.passwordless.dev/guide/frontend/android.html")),
            new("ASP.NET Identity", "Docs + Examples", typeof(DotNetIcon), new Uri("https://docs.passwordless.dev/guide/frontend/aspnet.html")),
            new("Javascript", "Docs + Examples", typeof(JavascriptIcon), new Uri("https://docs.passwordless.dev/guide/frontend/javascript.html")),
            new("React", "Docs + Examples", typeof(ReactIcon), new Uri("https://docs.passwordless.dev/guide/frontend/react.html"))
        };
    }

    private bool _isInitialized;

    public string? PublicApiKey { get; private set; }

    public string? PrivateApiKey { get; private set; }

    protected override async Task OnInitializedAsync()
    {
        _backendCards = new LinkCard.LinkCardModel[]
        {
            new("REST API", "Use our simple REST API", typeof(OpenApiIcon), new Uri(Configuration.IsSelfHosted() ? $"{PasswordlessOptions.Value.ApiUrl}/swagger/index.html" : "https://docs.passwordless.dev/guide/api-documentation.html")),
            new(".NET", "Docs + Examples", typeof(DotNetIcon), new Uri("https://docs.passwordless.dev/guide/backend/dotnet.html")),
            new("Java", "Docs + Examples", typeof(JavaIcon), new Uri("https://docs.passwordless.dev/guide/backend/java.html")),
            new("Node.js", "Docs + Examples", typeof(NodeJsIcon), new Uri("https://docs.passwordless.dev/guide/backend/nodejs.html")),
            new("Python", "Docs + Examples", typeof(PythonIcon), new Uri("https://docs.passwordless.dev/guide/backend/python3.html")),
            new("Rust", "Unofficial SDK", typeof(PythonIcon), new Uri("https://github.com/davidzr/passwordless-rust")),
            new("Go", "Unofficial SDK", typeof(PythonIcon), new Uri("https://github.com/AJAYK-01/passwordless-go")),
            new("Spring Boot", "Unofficial SDK", typeof(JavaIcon), new Uri("https://github.com/sanjanarjn/spring-boot-starter-passwordless"))
        };

        var onboarding = await ApplicationService.GetOnboardingAsync(AppId);
        if (onboarding == null) throw new Exception("Onboarding not found");

        PublicApiKey = onboarding.ApiKey;
        PrivateApiKey = onboarding.ApiSecret;

        _isInitialized = true;
    }
}