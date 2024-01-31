using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.Mail;

namespace Passwordless.AdminConsole.Services;

public class InvitationService<TDbContext> : IInvitationService where TDbContext : ConsoleDbContext
{
    private readonly IDbContextFactory<TDbContext> _dbContextFactory;
    private readonly IMailService _mailService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;

    public InvitationService(IDbContextFactory<TDbContext> dbContextFactory, IMailService mailService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
    {
        _dbContextFactory = dbContextFactory;
        _mailService = mailService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
    }

    public async Task SendInviteAsync(string toEmail, int targetOrgId, string targetOrgName, string fromEmail, string fromName)
    {
        var inv = new Invite()
        {
            ToEmail = toEmail,
            TargetOrgId = targetOrgId,
            TargetOrgName = targetOrgName,
            FromEmail = fromEmail,
            FromName = fromName
        };

        // generate code
        var code = RandomNumberGenerator.GetBytes(32);

        // hash code
        var hashedCode = HashCode(code);

        inv.HashedCode = hashedCode;
        inv.CreatedAt = DateTime.UtcNow;
        inv.ExpireAt = inv.CreatedAt.AddDays(7);

        // store
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Invites.Add(inv);
        await db.SaveChangesAsync();

        var urlBuilder = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
        string link = urlBuilder.PageLink("/organization/join", values: new { code = Convert.ToBase64String(code) });
        // send email
        await _mailService.SendInviteAsync(inv, link);
    }

    private string HashCode(byte[] code)
    {
        return Convert.ToBase64String(SHA256.HashData(code));
    }

    private string HashCode(string code)
    {
        var bytes = Convert.FromBase64String(code);
        return HashCode(bytes);
    }

    public async Task<List<Invite>> GetInvitesAsync(int orgId)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Invites.Where(i => i.TargetOrgId == orgId).ToListAsync();
    }

    public async Task CancelInviteAsync(Invite inviteToCancel)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        await db.Invites.Where(i => i.HashedCode == inviteToCancel.HashedCode).ExecuteDeleteAsync();
    }

    public async Task<Invite> GetInviteFromRawCodeAsync(string code)
    {
        var hashed = HashCode(code);
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        return await db.Invites.Where(i => i.HashedCode == hashed).FirstOrDefaultAsync();
    }

    public async Task<bool> ConsumeInviteAsync(Invite inv)
    {
        if (inv == null)
        {
            return false;
        }

        // check if expired
        if (inv.ExpireAt < DateTime.UtcNow)
        {
            return false;
        }

        // delete it
        await using var db = await _dbContextFactory.CreateDbContextAsync();
        db.Invites.Remove(inv);
        await db.SaveChangesAsync();

        return true;
    }
}