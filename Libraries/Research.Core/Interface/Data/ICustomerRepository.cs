using Research.Core.Domain.Customers;
using System;

namespace Research.Core.Interface.Data
{
    public partial interface ICustomerRepository : IRepository<Customer>
    {
        /// <summary>
        /// Xóa bỏ bớt các tài khoản guest customer, cho phép loại bỏ bớt những tài khoản Guest không còn cần đến, giúp cho bảng Customer
        /// nhẹ hơn, từ đó truy cập nhanh hơn. Trả về số lượng customer bị xóa
        /// 
        /// Do có gọi đến Store proc, nên khác với những hàm khác ko gọi SaveChange(), hàm này sẽ tự saveChange những thay đổi của nó
        /// Nếu muốn rollback thì hãy bao hàm lại trong 1 transaction khác để có thể rollback
        /// </summary>
        int DeleteGuestCustomers(DateTime? createdFromUtc, DateTime? createdToUtc, DateTime? lastActivityDateUtcFrom,
            DateTime? lastActivityDateUtcTo, bool onlyWithoutShoppingCart);
    }
}
