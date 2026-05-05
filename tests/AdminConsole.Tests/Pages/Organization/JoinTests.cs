using System.Security.Cryptography;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.EventLog.DTOs;
using Passwordless.AdminConsole.EventLog.Loggers;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Pages.Organization;
using Passwordless.AdminConsole.Services;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.AdminConsole.Tests.DataFactory;
using Passwordless.AdminConsole.Tests.Factory;
using Xunit;

namespace Passwordless.AdminConsole.Tests.Pages.Organization;

/// <summary>
/// Regression tests for VULN-548 — exercises the real <see cref="Join"/> page model with the real
/// <see cref="InvitationService"/> against an in-memory <see cref="ConsoleDbContext"/>. Each scenario
/// is named after a step in the HackerOne PoC.
/// </summary>
public class JoinTests : IDisposable, IAsyncDisposable
{
    private readonly ConsoleDbContext _dbContext;
    private readonly Mock<IMailService> _mailServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IEventLogger> _eventLoggerMock = new();
    private readonly FakeTimeProvider _timeProvider = new(new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero));
    private readonly FakeUserManager _userManager;
    private readonly FakeMagicLinkSignInManager _magicLinkSignInManager;
    private readonly InvitationService _invitationService;

    public JoinTests()
    {
        _dbContext = DbContextFactory.Create();
        _invitationService = new InvitationService(
            _dbContext,
            _mailServiceMock.Object,
            _httpContextAccessorMock.Object,
            _timeProvider);
        _userManager = new FakeUserManager();
        _magicLinkSignInManager = new FakeMagicLinkSignInManager(_userManager);
    }

    [Fact]
    public async Task OnGet_LiveInvite_PopulatesFormAndInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "user@example.com", expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(7));
        var sut = CreateJoin();

        var result = await sut.OnGet(rawCode);

        Assert.IsType<PageResult>(result);
        Assert.False(sut.ModelState.ContainsKey("bad-invite"));
        Assert.NotNull(sut.Invite);
        Assert.Equal("user@example.com", sut.Invite!.ToEmail);
        Assert.Equal(rawCode, sut.Form.Code);
        Assert.Equal("user@example.com", sut.Form.Email);
        Assert.NotEmpty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task OnGet_ExpiredInvite_RendersBadInvite_AndDeletesInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "user@example.com", expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(-1));
        var sut = CreateJoin();

        var result = await sut.OnGet(rawCode);

        Assert.IsType<PageResult>(result);
        Assert.True(sut.ModelState.ContainsKey("bad-invite"));
        Assert.Null(sut.Invite);
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
        _eventLoggerMock.Verify(x => x.LogEvent(It.IsAny<OrganizationEventDto>()), Times.Once);
    }

    [Fact]
    public async Task OnGet_UnknownCode_RendersBadInvite()
    {
        var (otherRawCode, _) = GenerateCode();
        var sut = CreateJoin();

        var result = await sut.OnGet(otherRawCode);

        Assert.IsType<PageResult>(result);
        Assert.True(sut.ModelState.ContainsKey("bad-invite"));
        Assert.Null(sut.Invite);
        _eventLoggerMock.Verify(x => x.LogEvent(It.IsAny<OrganizationEventDto>()), Times.Never);
    }

    [Fact]
    public async Task OnPost_LiveInvite_BadInviteEmail_DoesNotCreateAdmin_AndPreservesInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "otherguy@corp.example", expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(7));
        var sut = CreateJoin();

        var result = await sut.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "flexo@other.com",
            Name = "Flexo",
            AcceptsTermsAndPrivacy = true
        });

        Assert.IsType<PageResult>(result);
        Assert.True(sut.ModelState.ContainsKey("bad-invite"));
        Assert.Empty(_userManager.CreatedUsers);
        Assert.Empty(_magicLinkSignInManager.SentEmails);
        Assert.NotEmpty(await _dbContext.Invites.AsNoTracking().ToListAsync());
        _eventLoggerMock.Verify(x => x.LogEvent(It.IsAny<OrganizationEventDto>()), Times.Once);
    }

    [Fact]
    public async Task OnPost_ExpiredInvite_AnyEmail_DoesNotCreateAdmin_AndDeletesInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "otherguy@corp.example", expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30));
        var sut = CreateJoin();

        var result = await sut.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "flexo@other.com",
            Name = "Flexo",
            AcceptsTermsAndPrivacy = true
        });

        Assert.IsType<PageResult>(result);
        Assert.True(sut.ModelState.ContainsKey("bad-invite"));
        Assert.Empty(_userManager.CreatedUsers);
        Assert.Empty(_magicLinkSignInManager.SentEmails);
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task OnPost_ExpiredInvite_Replay_FindsNoInvite()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "otherguy@corp.example", expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(-30));
        var firstAttempt = CreateJoin();

        await firstAttempt.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "flexo@other.com",
            Name = "Flexo",
            AcceptsTermsAndPrivacy = true
        });
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());

        var secondAttempt = CreateJoin();
        var result = await secondAttempt.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "roberto@other.com",
            Name = "Roberto",
            AcceptsTermsAndPrivacy = true
        });

        Assert.IsType<PageResult>(result);
        Assert.True(secondAttempt.ModelState.ContainsKey("bad-invite"));
        Assert.Empty(_userManager.CreatedUsers);
        Assert.Empty(_magicLinkSignInManager.SentEmails);
    }

    [Fact]
    public async Task OnPost_LiveInvite_MatchingEmail_CreatesAdminAndSendsMagicLink()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "user@example.com", targetOrgId: 42, expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(7));
        var sut = CreateJoin();

        var result = await sut.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "user@example.com",
            Name = "User",
            AcceptsTermsAndPrivacy = true
        });

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/Organization/Verify", redirect.Url);
        var created = Assert.Single(_userManager.CreatedUsers);
        Assert.Equal("user@example.com", created.Email);
        Assert.Equal(42, created.OrganizationId);
        Assert.Equal("user@example.com", Assert.Single(_magicLinkSignInManager.SentEmails));
        Assert.Empty(await _dbContext.Invites.AsNoTracking().ToListAsync());
    }

    [Fact]
    public async Task OnPost_LiveInvite_MatchingEmail_CaseInsensitive_Succeeds()
    {
        var (rawCode, hashedCode) = GenerateCode();
        SeedInvite(hashedCode, "User@Example.com", targetOrgId: 7, expireAtUtc: _timeProvider.GetUtcNow().UtcDateTime.AddDays(7));
        var sut = CreateJoin();

        var result = await sut.OnPost(new Join.JoinForm
        {
            Code = rawCode,
            Email = "user@example.com",
            Name = "User",
            AcceptsTermsAndPrivacy = true
        });

        Assert.IsType<RedirectResult>(result);
        Assert.Single(_userManager.CreatedUsers);
        Assert.Single(_magicLinkSignInManager.SentEmails);
    }

    private Join CreateJoin()
    {
        var join = new Join(
            _invitationService,
            _userManager,
            _magicLinkSignInManager,
            _mailServiceMock.Object,
            _eventLoggerMock.Object,
            _timeProvider);

        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new PageActionDescriptor(),
            new ModelStateDictionary());
        join.PageContext = new PageContext(actionContext)
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        };
        join.Url = new StubUrlHelper(actionContext);

        return join;
    }

    private sealed class StubUrlHelper : IUrlHelper
    {
        public StubUrlHelper(ActionContext actionContext) => ActionContext = actionContext;

        public ActionContext ActionContext { get; }

        public string Action(UrlActionContext actionContext) => "/stub";
        public string Content(string? contentPath) => contentPath ?? "/stub";
        public bool IsLocalUrl(string? url) => true;
        public string Link(string? routeName, object? values) => "/stub";
        public string RouteUrl(UrlRouteContext routeContext) => "/stub";
    }

    private void SeedInvite(string hashedCode, string toEmail, DateTime expireAtUtc, int targetOrgId = 1)
    {
        _dbContext.Invites.Add(new Invite
        {
            HashedCode = hashedCode,
            ToEmail = toEmail,
            TargetOrgId = targetOrgId,
            TargetOrgName = "ExampleOrg",
            FromEmail = "admin@example.com",
            FromName = "Admin",
            CreatedAt = expireAtUtc.AddDays(-7),
            ExpireAt = expireAtUtc
        });
        _dbContext.SaveChanges();
    }

    private static (string rawCode, string hashedCode) GenerateCode()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return (Convert.ToBase64String(bytes), Convert.ToBase64String(SHA256.HashData(bytes)));
    }

    public void Dispose() => _dbContext.Dispose();

    public ValueTask DisposeAsync() => _dbContext.DisposeAsync();
}
