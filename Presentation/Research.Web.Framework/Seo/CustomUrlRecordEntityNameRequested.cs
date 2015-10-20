using Research.Core.Domain.Seo;
using System.Web.Routing;

namespace Research.Web.Framework.Seo
{
    /// <summary>
    /// Đối tượng chứa sự kiện dùng để thông báo rằng có 1 loại UrlRecord.EntityName mới mà code trong GenericPathRoute chưa thể xử lý
    /// Để bắt sự kiện này, cần cài đặt giao diện IConsumer[CustomUrlRecordEntityNameRequested]
    /// </summary>
    public class CustomUrlRecordEntityNameRequested
    {
        public CustomUrlRecordEntityNameRequested(RouteData routeData, UrlRecordForCaching urlRecord)
        {
            this.RouteData = routeData;
            this.UrlRecord = urlRecord;
        }

        public RouteData RouteData { get; private set; }

        public UrlRecordForCaching UrlRecord { get; private set; }
    }
}
