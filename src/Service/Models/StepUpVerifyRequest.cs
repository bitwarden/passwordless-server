namespace Passwordless.Service.Models;

public record StepUpVerifyRequest(string Token, string Context);