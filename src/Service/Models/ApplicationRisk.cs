namespace Passwordless.Service.Models;

public class ApplicationRisk : PerTenant
{
    public int Risk { get; set; }
    
    public ApplicationRiskAssessmentType AssessmentType { get; set; }
    
    public DateTimeOffset AssessedOn { get; set; }
    
    public AccountMetaInformation Application { get; set; }
}

public enum ApplicationRiskAssessmentType
{
    Unknown = 0,
    Automated = 1,
    Manual = 2
}