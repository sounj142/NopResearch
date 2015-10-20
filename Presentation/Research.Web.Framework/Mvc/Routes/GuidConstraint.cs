using System;
using System.Web;
using System.Web.Routing;

namespace Research.Web.Framework.Mvc.Routes
{
    /// <summary>
    /// Lớp đóng vai trò 1 bộ kiểm tra ràng buộc dùng trong các route của ứng dụng. Chịu trách nhiệm kiểm tra để đảm bảo là trong
    /// danh sách route values sẽ có 1 thành phần với tên parameterName phải mang giá trị của 1 chuỗi Guid hợp lệ
    /// 
    /// Kiểm tra là ở cả 2 chiều, phân giải url đến và sinh url trong view
    /// 
    /// Cách dùng: constraints: new { orderItemId = new GuidConstraint(false) }
    /// </summary>
    public class GuidConstraint: IRouteConstraint
    {
        private readonly bool _allowEmpty;

        /// <summary>
        /// allowEmpty: cho phép dùng 1 giá trị Guid rỗng ( toàn số 0 )
        /// </summary>
        public GuidConstraint(bool allowEmpty)
        {
            _allowEmpty = allowEmpty;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, 
            RouteValueDictionary values, RouteDirection routeDirection)
        {
            object value = values[parameterName];
            if (value != null)
            {
                string stringValue = value.ToString();
                if(!string.IsNullOrEmpty(stringValue))
                {
                    Guid guid;
                    return Guid.TryParse(stringValue, out guid) && (_allowEmpty || guid != Guid.Empty);
                }
            }
            return false; // trả về false nếu giá trị của parameterName ko phải là 1 chuỗi guid hợp lệ
        }
    }
}
