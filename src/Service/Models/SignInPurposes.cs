namespace Passwordless.Service.Models;

public static class SignInPurposes
{
    private const string SignInName = "sign-in";
    public static SignInPurpose SignIn => new() { Value = SignInName };

    private const string StepUpName = "step-up";
    public static readonly SignInPurpose StepUp = new() { Value = StepUpName };
}