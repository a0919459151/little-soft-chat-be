namespace LittleSoftChat.Shared.Domain.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / Size);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;

    public PagedResult() { }

    public PagedResult(List<T> items, int page, int size, int totalCount)
    {
        Items = items;
        Page = page;
        Size = size;
        TotalCount = totalCount;
    }
}
