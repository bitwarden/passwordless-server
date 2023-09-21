using System.ComponentModel.DataAnnotations;
using AdminConsole.Models;
using Microsoft.AspNetCore.Identity;

namespace AdminConsole.Identity;

public class ConsoleAdmin : IdentityUser<string>
{
    public ConsoleAdmin() : base()
    {
        this.Id = Guid.NewGuid().ToString();
    }

    [MaxLength(50)]
    public string Name { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; }
}