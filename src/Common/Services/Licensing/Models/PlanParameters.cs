namespace Passwordless.Common.Services.Licensing.Models;

public record PlanParameters(
    uint Seats,
    bool SupportsAuditLogging);