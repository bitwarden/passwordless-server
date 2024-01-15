namespace Passwordless.Common.Models.Reporting;

/// <summary>
/// 
/// </summary>
/// <param name="CreatedAt">When the report was created</param>
/// <param name="Users">Number of unique users with credentials</param>
/// <param name="Credentials">Number of credentials</param>
public record PeriodicCredentialReportResponse(DateOnly CreatedAt, int Users, int Credentials);