using System.Security.Cryptography;
using AdminConsole.Db;
using AdminConsole.Identity;
using AdminConsole.Services.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;

namespace AdminConsole.Services;

public class InvitationService
{
    private readonly ConsoleDbContext _db;
    private readonly IMailService _mailService;
    private readonly IUrlHelperFactory _urlHelperFactory;
    private readonly IActionContextAccessor _actionContextAccessor;

    public InvitationService(ConsoleDbContext db, IMailService mailService, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor)
    {
        _db = db;
        _mailService = mailService;
        _urlHelperFactory = urlHelperFactory;
        _actionContextAccessor = actionContextAccessor;
    }

    public async Task SendInviteAsync(string toEmail, int TargetOrgId, string TargetOrgName, string fromEmail, string fromName)
    {
        var inv = new Invite()
        {
            ToEmail = toEmail,
            TargetOrgId = TargetOrgId,
            TargetOrgName = TargetOrgName,
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
        _db.Invites.Add(inv);
        await _db.SaveChangesAsync();

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

    public async Task<List<Invite>> GetInvites(int orgId)
    {

        return await _db.Invites.Where(i => i.TargetOrgId == orgId).ToListAsync();
    }

    public async Task CancelInviteAsync(string hashedCode)
    {
        await _db.Invites.Where(i => i.HashedCode == hashedCode).ExecuteDeleteAsync();
    }

    public async Task<Invite> GetInviteFromRawCodeAsync(string code)
    {
        var hashed = HashCode(code);
        return await _db.Invites.Where(i => i.HashedCode == hashed).FirstOrDefaultAsync();
    }

    public async Task<bool> ConsumeInvite(Invite inv)
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
        _db.Invites.Remove(inv);
        await _db.SaveChangesAsync();

        return true;
    }
}