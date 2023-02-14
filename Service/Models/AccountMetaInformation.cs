using System;
using System.Text.Json;

namespace Service.Models
{
    public class AccountMetaInformation : PerTenant
    {
        public string SubscriptionTier { get; set; }
        public string[] AdminEmails { get; set; }

        public string AdminEmailsSerialized
        {
            get
            {
                return JsonSerializer.Serialize(AdminEmails);
            }
            set
            {
                AdminEmails = JsonSerializer.Deserialize<string[]>(value);
            }
        }
        public string AcountName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
