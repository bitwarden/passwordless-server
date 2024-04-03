using Passwordless.Common.Models.Apps;

namespace Passwordless.Service.Models;

public static class SignInPurposes
{
    public const string SignInName = "sign-in";
    public static SignInPurpose SignIn => new(SignInName);

    public const string StepUpName = "step-up";
    public static readonly SignInPurpose StepUp = new(StepUpName);
}