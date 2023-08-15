namespace Passwordless.AdminConsole.Models.DTOs;

public record MarkDeleteApplicationRequest(string AppId, string DeletedBy);
public record MarkDeleteApplicationResponse(string Message, bool IsDeleted, DateTime DeleteAt);