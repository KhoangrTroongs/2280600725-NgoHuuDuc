using Microsoft.EntityFrameworkCore;

namespace NgoHuuDuc_2280600725.Helpers
{
    public class PaginatedList<T> : List<T>
    {
        public int PageIndex { get; private set; }
        public int TotalPages { get; private set; }
        public int TotalItems { get; private set; }
        public int PageSize { get; private set; }

        public PaginatedList(List<T> items, int count, int pageIndex, int pageSize)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            TotalItems = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            this.AddRange(items);
        }

        public bool HasPreviousPage => PageIndex > 1;
        public bool HasNextPage => PageIndex < TotalPages;

        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageIndex, int pageSize)
        {
            var count = await source.CountAsync();
            
            // Ensure pageIndex is valid
            if (pageIndex < 1)
                pageIndex = 1;
            else if (count > 0 && pageSize > 0 && pageIndex > Math.Ceiling(count / (double)pageSize))
                pageIndex = (int)Math.Ceiling(count / (double)pageSize);
            
            var items = await source.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }

        public static PaginatedList<T> Create(IEnumerable<T> source, int pageIndex, int pageSize)
        {
            var items = source.ToList();
            var count = items.Count;
            
            // Ensure pageIndex is valid
            if (pageIndex < 1)
                pageIndex = 1;
            else if (count > 0 && pageSize > 0 && pageIndex > Math.Ceiling(count / (double)pageSize))
                pageIndex = (int)Math.Ceiling(count / (double)pageSize);
            
            var paginatedItems = items.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
            return new PaginatedList<T>(paginatedItems, count, pageIndex, pageSize);
        }
    }
}
