using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Core
{
    [Serializable]
    public class PagedList<T> : List<T>, IPagedList<T>
    {
        public int PageIndex { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }
        public int TotalPages { get; private set; }

        public bool HasPreviousPage
        {
            get { return (PageIndex > 0); }
        }

        public bool HasNextPage
        {
            get { return (PageIndex < TotalPages - 1); }
        }
        /// <summary>
        /// Có khác biệt 1 chút khi source chứa nguồn dữ liệu đã đc phân trang, ko có thao tác skip hay take
        /// </summary>
        public PagedList(IEnumerable<T> source, int pageIndex, int pageSize, int totalCount)
        {
            this.PageIndex = pageIndex;
            this.PageSize = pageSize;
            this.TotalCount = totalCount;
            TotalPages = totalCount / pageSize;
            if (TotalPages * pageSize < totalCount) ++TotalPages;
            this.AddRange(source);
        }
        /// <summary>
        /// source là nguồn dữ liệu chưa qua phân trang, thường là 1 IQueryable đại diện cho truy vấn SQL
        /// </summary>
        public PagedList(IQueryable<T> source, int pageIndex, int pageSize):
            this(source.Skip(pageSize * pageIndex).Take(pageSize), pageIndex, pageSize, source.Count())
        {
        }

        public PagedList(IList<T> source, int pageIndex, int pageSize):
            this(source.Skip(pageSize * pageIndex).Take(pageSize), pageIndex, pageSize, source.Count())
        {
        }
    }
}
