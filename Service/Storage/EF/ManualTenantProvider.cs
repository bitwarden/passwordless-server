using Service;

public class ManualTenantProvider : ITenantProvider
    {

        
        public ManualTenantProvider(string tenant)
        {
            Tenant = tenant;
        }

        public string Tenant { get; set; }
    }