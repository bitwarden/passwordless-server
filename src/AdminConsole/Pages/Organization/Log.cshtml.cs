using AdminConsole.Pages.Components;
using AdminConsole.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Passwordless.AdminConsole;
using Passwordless.AdminConsole.AuditLog.DTOs;
using Passwordless.AdminConsole.Services;

namespace AdminConsole.Pages.Organization;

public class Log : PageModel
{
    private readonly IAuditLogService _auditLogService;
    private readonly DataService _dataService;
    private readonly ICurrentContext _currentContext;

    public Models.Organization Organization { get; set; }
    public IEnumerable<AuditLogEvent> Events { get; set; }
    public PagedList PageList { get; set; }
    public int RetentionPeriod { get; private set; }

    public Log(IAuditLogService auditLogService, DataService dataService, ICurrentContext currentContext)
    {
        _auditLogService = auditLogService;
        _dataService = dataService;
        _currentContext = currentContext;
    }

    public async Task<ActionResult> OnGet(int pageNumber = 1, int numberOfResults = 100)
    {
        var features = _currentContext.OrganizationFeatures;

        if (!features.AuditLoggingIsEnabled) return RedirectToPage("Overview");

        RetentionPeriod = features.AuditLoggingRetentionPeriod;
        Organization = await _dataService.GetOrganization();

        var eventTask = _auditLogService.GetAuditLogs(Organization.Id, pageNumber, numberOfResults);
        var countTask = _auditLogService.GetAuditLogCount(Organization.Id);

        await Task.WhenAll(eventTask, countTask);
        
        Events = eventTask.Result.Events;

        var itemCount = countTask.Result;

        PageList = new PagedList(itemCount, pageNumber, numberOfResults);

        return Page();
    }
}