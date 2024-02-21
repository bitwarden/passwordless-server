using System.ComponentModel.DataAnnotations;

namespace Passwordless.Common.Models.Apps;

public record GetAppIdAvailabilityRequest([MinLength(3), RegularExpression("^[a-z]{1}[a-z0-9]{2,49}$")] string AppId);