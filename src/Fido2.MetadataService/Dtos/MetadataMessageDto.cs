namespace Passwordless.Fido2.MetadataService.Dtos;

public class MetadataMessageDto
{
    public ICollection<EntryDto> Entries { get; set; }
}