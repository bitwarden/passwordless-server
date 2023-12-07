using Passwordless.Common.Services.Licensing.Models;

namespace Passwordless.Common.Services.Licensing.V1;

public record Plan(
    uint Seats,
    bool SupportsAuditLogging)
    : BasePlan(Seats);