namespace Passwordless.AdminConsole.Components.Shared.Tables;

public class PagedTable
{
    public PagedTable(int items, int page, int pageSize)
    {
        TotalPages = items / pageSize + (items % pageSize == 0 ? 0 : 1);
        CurrentPage = page;

        Pages = Enumerable.Range(1, TotalPages);
    }

    public int CurrentPage { get; }
    public int TotalPages { get; }

    public IEnumerable<int> Pages { get; }

    public bool HasPreviousPage => CurrentPage > 1;
    public bool HasNextPage => CurrentPage < TotalPages;
}