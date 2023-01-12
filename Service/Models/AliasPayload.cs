using System.Collections.Generic;

namespace Service.Models
{
    using Aliases = HashSet<string>;

    public class AliasPayload
    {
        public string UserId { get; set; }
        public Aliases Aliases { get; set; }
    }
}
