using Moq;
using Passwordless.Common.Background;

namespace Passwordless.Common.Tests.Background;

public class ExecutionPlanUtilityTests
{
    [Fact]
    public void GetExecutionPlan_WhenExecutionTimeIsSameAsCurrentTime_ReturnsCorrectInitialDelay()
    {
        // Arrange
        var executionTime = new TimeOnly(22, 00, 00);
        var period = new TimeSpan(1, 0, 0, 0);

        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2021, 1, 1, 22, 0, 0, DateTimeKind.Utc)));

        // Act
        var actual = ExecutionPlanUtility.GetExecutionPlan(executionTime, period, timeProvider.Object);

        // Assert
        Assert.Equal(new TimeSpan(0, 0, 0, 0), actual.InitialDelay);
    }

    [Fact]
    public void GetExecutionPlan_WhenExecutionTimeIsAfterCurrentTime_ReturnsCorrectInitialDelay()
    {
        // Arrange
        var executionTime = new TimeOnly(22, 00, 00);
        var period = new TimeSpan(1, 0, 0, 0);

        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2021, 1, 1, 21, 30, 0, DateTimeKind.Utc)));

        // Act
        var actual = ExecutionPlanUtility.GetExecutionPlan(executionTime, period, timeProvider.Object);

        // Assert
        Assert.Equal(new TimeSpan(0, 0, 30, 0), actual.InitialDelay);
    }

    [Fact]
    public void GetExecutionPlan_WhenExecutionTimeIsBeforeCurrentTime_ReturnsCorrectInitialDelay()
    {
        // Arrange
        var executionTime = new TimeOnly(22, 00, 00);
        var period = new TimeSpan(1, 0, 0, 0);

        var timeProvider = new Mock<TimeProvider>();
        timeProvider.Setup(x => x.GetUtcNow()).Returns(new DateTimeOffset(new DateTime(2021, 1, 1, 22, 30, 0, DateTimeKind.Utc)));

        // Act
        var actual = ExecutionPlanUtility.GetExecutionPlan(executionTime, period, timeProvider.Object);

        // Assert
        Assert.Equal(new TimeSpan(0, 23, 30, 0), actual.InitialDelay);
    }
}