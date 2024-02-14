namespace Passwordless.Common.Background;

public static class ExecutionPlanUtility
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="executionTime">Time of day when the service should run.</param>
    /// <param name="period">Period of time between executions.</param>
    /// <param name="timeProvider">Represents the current time.</param>
    /// <returns></returns>
    public static ExecutionPlan GetExecutionPlan(TimeOnly executionTime, TimeSpan period, TimeProvider timeProvider)
    {
        var currentTime = TimeOnly.FromTimeSpan(timeProvider.GetUtcNow().TimeOfDay);

        TimeSpan initialDelay;
        if (executionTime >= currentTime)
        {
            initialDelay = executionTime - currentTime;
        }
        else
        {
            initialDelay = period - (currentTime - executionTime);
            var multiplier = Math.Round(Math.Abs(initialDelay.Divide(period)), MidpointRounding.ToZero);
            initialDelay = initialDelay.Add(period.Multiply(multiplier));
        }

        initialDelay = initialDelay.Subtract(period.Multiply(Math.Floor(initialDelay.Divide(period))));

        return new ExecutionPlan(initialDelay);
    }
}