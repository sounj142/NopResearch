using Research.Core.Domain.Customers;
using Research.Core.Domain.Orders;
using Research.Core.Domain.Payments;
using Research.Core.Domain.Shipping;
using System;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service kết xuất report thống kê cho bảng Customer
    /// </summary>
    public partial interface ICustomerReportService
    {
        /// <summary>
        /// Lấy về danh sách những khách hàng tốt nhất, tức là những khách hàng mua hàng nhiều nhất hoặc là tiêu nhiều tiền nhất ..v....v..
        /// </summary>
        /// <param name="createdFromUtc">Order created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Order created date to (UTC); null to load all records</param>
        /// <param name="os">Order status; null to load all records</param>
        /// <param name="ps">Order payment status; null to load all records</param>
        /// <param name="ss">Order shipment status; null to load all records</param>
        /// <param name="orderBy">1 - order by order total, 2 - order by number of orders</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Report</returns>
        IPagedList<BestCustomerReportLine> GetBestCustomersReport(DateTime? createdFromUtc, DateTime? createdToUtc,
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy, int pageIndex = 0, int pageSize = int.MaxValue);

        /// <summary>
        /// Trả về số lượng người dùng đăng ký trong thời gian days ngày gần đây
        /// </summary>
        int GetRegisteredCustomersReport(int days);
    }
}
