using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Passwordless.AdminConsole.Db;
using Passwordless.AdminConsole.Identity;
using Passwordless.AdminConsole.Services.Mail;
using Passwordless.Common.Extensions;

namespace Passwordless.AdminConsole.Services;

public class InvitationService : IInvitationService
{
    private readonly ConsoleDbContext _db;
    private readonly IMailService _mailService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public InvitationService(ConsoleDbContext db, IMailService mailService, IHttpContextAccessor httpContextAccessor)
    {
        _db = db;
        _mailService = mailService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task SendInviteAsync(string toEmail, int targetOrgId, string targetOrgName, string fromEmail, string fromName)
    {
        var inv = new Invite
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
        _db.Invites.Add(inv);
        await _db.SaveChangesAsync();

        var link = $"{_httpContextAccessor.HttpContext!.Request.GetBaseUrl()}/organization/join?code={Uri.EscapeDataString(Convert.ToBase64String(code))}";

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
        return await _db.Invites.Where(i => i.TargetOrgId == orgId).ToListAsync();
    }

    public async Task CancelInviteAsync(Invite inviteToCancel)
    {
        await _db.Invites.Where(i => i.HashedCode == inviteToCancel.HashedCode).ExecuteDeleteAsync();
    }

    public async Task<Invite> GetInviteFromRawCodeAsync(string code)
    {
        var hashed = HashCode(code);
        return await _db.Invites.Where(i => i.HashedCode == hashed).FirstOrDefaultAsync();
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
        _db.Invites.Remove(inv);
        await _db.SaveChangesAsync();

        return true;
    }
}