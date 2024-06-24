namespace Passwordless.Common.Models.Apps;

public record SignInPurpose(string Value)
{
    public const string SignInName = "sign-in";
    public static SignInPurpose SignIn => new(SignInName);

    public const string StepUpName = "step-up";
    public static readonly SignInPurpose StepUp = new(StepUpName);
};