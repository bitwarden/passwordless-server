namespace Passwordless.Common.Services.Licensing.Models;

public record Plan(
    uint Seats,
    bool SupportsAuditLogging)
    : BasePlan(Seats);