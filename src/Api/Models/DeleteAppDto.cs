namespace Passwordless.Api.Models;

public sealed class DeleteAppDto
{
    /**
     * The administrator's username that initiated the deletion process.
     */
    public string DeletedBy { get; set; }
}