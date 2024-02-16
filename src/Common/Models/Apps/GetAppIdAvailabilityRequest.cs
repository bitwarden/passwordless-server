using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public record GetAppIdAvailabilityRequest([MinLength(3)] string AppId);