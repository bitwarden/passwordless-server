namespace Passwordless.Common.Background;

/// <summary>
/// 
/// </summary>
/// <param name="InitialDelay">Time to wait before the first execution.</param>
public record ExecutionPlan(TimeSpan InitialDelay);