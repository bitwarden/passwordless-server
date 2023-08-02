using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Passwordless.AdminConsole.Helpers;

public static class ValidationExtensions
{
    public static void AddToModelState(this ValidationResult result, ModelStateDictionary modelState)
    {
        foreach (var error in result.Errors)
        {
            modelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }
    }
}