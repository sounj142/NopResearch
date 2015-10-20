
namespace Research.Core.Plugins
{
    /// <summary>
    /// Toàn bộ mớ thuộc họ *OfficialFeed* chỉ là để tìm kiếm danh sách mấy cái plugin trên server nop, tên, giá cả, hình ảnh, tác giả,
    /// ..v.v..... để người dùng tìm đến link mua/down chúng nó ở trên máy chủ chứ chả làm được gì sất
    /// </summary>
    public class OfficialFeedVersion
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
