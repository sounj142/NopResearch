
namespace Research.Core.Domain.Seo
{
    /// <summary>
    /// Represents a page title SEO adjustment
    /// Miêu tả cho sự điều chỉnh page title SEO. Tạm hiểu là qui định về thứ tự sắp xếp các chuỗi keyword SEO ?
    /// </summary>
    public enum PageTitleSeoAdjustment
    {
        /// <summary>
        /// Pagename comes after storename
        /// </summary>
        PagenameAfterStorename = 0,
        /// <summary>
        /// Storename comes after pagename
        /// </summary>
        StorenameAfterPagename = 10
    }
}
