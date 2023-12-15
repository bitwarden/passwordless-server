namespace Passwordless.Common.Models.Apps;

public record MarkDeleteApplicationResponse(bool IsDeleted, DateTime DeleteAt, ICollection<string> AdminEmails);