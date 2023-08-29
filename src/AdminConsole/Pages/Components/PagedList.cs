namespace AdminConsole.Pages.Components;

public class PagedList
{
    public int CurrentIndex { get; set; }
    public int TotalPages { get; set; }
    
    public IEnumerable<int> Pages { get; }

    public PagedList(int count, int pageIndex, int numberOfRecords)
    {
        TotalPages = (int)Math.Ceiling(count/(double)numberOfRecords);
        CurrentIndex = pageIndex;

        Pages = Enumerable.Range(1, TotalPages);
    }

    public bool HasPreviousPage => CurrentIndex > 1;
    public bool HasNextPage => CurrentIndex < TotalPages;
}