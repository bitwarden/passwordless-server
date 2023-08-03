namespace Passwordless.Api.Models;

public sealed class DeleteAppDto
{
    public string AppId { get; set; }

    /**
     * The administrator's username that initiated the deletion process.
     */
    public string DeletedBy { get; set; }
}