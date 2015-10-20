using System.Collections.Generic;

namespace Research.Core
{
    /// <summary>
    /// dùng cho chức năng phân trăng với repository viết dựa trên EF
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IPagedList<T>: IList<T>
    {
        int PageIndex { get; }
        int PageSize { get; }
        int TotalCount { get; }
        int TotalPages { get; }
        bool HasPreviousPage { get; }
        bool HasNextPage { get; }
    }
}
