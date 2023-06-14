namespace Passwordless.AspNetCore.Services.Implementations;

internal sealed class NoopCustomizeRegisterOptions : ICustomizeRegisterOptions
{
    public Task CustomizeAsync(CustomizeRegisterOptionsContext context)
    {
        return Task.CompletedTask;
    }
}
