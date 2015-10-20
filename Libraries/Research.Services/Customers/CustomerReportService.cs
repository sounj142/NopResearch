using Research.Core;
using Research.Core.Domain.Customers;
using Research.Core.Domain.Orders;
using Research.Core.Domain.Payments;
using Research.Core.Domain.Shipping;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using System;
using System.Linq;

namespace Research.Services.Customers
{
    public partial class CustomerReportService : ICustomerReportService
    {
        #region Fields, ctors, properties

        private readonly ICustomerRepository _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly ICustomerService _customerService;

        public CustomerReportService(ICustomerRepository customerRepository,
            IRepository<Order> orderRepository,
            ICustomerService customerService)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _customerService = customerService;
        }

        #endregion

        #region Methods

        public IPagedList<BestCustomerReportLine> GetBestCustomersReport(DateTime? createdFromUtc, DateTime? createdToUtc, 
            OrderStatus? os, PaymentStatus? ps, ShippingStatus? ss, int orderBy, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            int? orderStatusId = null;
            if (os.HasValue) orderStatusId = (int)os.Value;

            int? paymentStatusId = null;
            if (ps.HasValue) paymentStatusId = (int)ps.Value;

            int? shippingStatusId = null;
            if (ss.HasValue) shippingStatusId = (int)ss.Value;

            var query1 = from c in _customerRepository.Table
                         join o in _orderRepository.Table on c.Id equals o.CustomerId
                         where !c.Deleted && !o.Deleted && 
                            (!createdFromUtc.HasValue || o.CreatedOnUtc >= createdFromUtc.Value) &&
                            (!createdToUtc.HasValue || o.CreatedOnUtc <= createdToUtc.Value) &&
                            (!orderStatusId.HasValue || o.OrderStatusId == orderStatusId.Value) &&
                            (!paymentStatusId.HasValue || o.PaymentStatusId == paymentStatusId.Value) &&
                            (!shippingStatusId.HasValue || o.ShippingStatusId == shippingStatusId.Value)
                         select new { c, o };
            var query = from co in query1
                        group co by co.c.Id into g
                        select new BestCustomerReportLine // khác chỗ này
                        {
                            CustomerId = g.Key,
                            OrderTotal = g.Sum(x => x.o.OrderTotal),
                            OrderCount = g.Count()
                        };
            switch(orderBy)
            {
                case 1:
                    query = query.OrderByDescending(p => p.OrderTotal);
                    break;
                case 2:
                    query = query.OrderByDescending(p => p.OrderCount);
                    break;
                default:
                    throw new ArgumentException("Wrong orderBy parameter", "orderBy");
            }

            var result = new PagedList<BestCustomerReportLine>(query, pageIndex, pageSize);
            return result;
        }

        public int GetRegisteredCustomersReport(int days)
        {
            // khác ở đây, ko dùng _dateTimeHelper để chuyển đổi ngày giờ sang ngày giờ của current customer vì thấy vô lý quá
            DateTime date = DateTime.UtcNow.AddDays(-days);

            var registeredCustomerRole = _customerService.GetCustomerRoleBySystemName(SystemCustomerRoleNames.Registered);
            if (registeredCustomerRole == null) return 0;

            return _customerRepository.Table
                .Where(c => !c.Deleted && c.CreatedOnUtc >= date &&
                        c.CustomerRoles.Any(r => r.Id == registeredCustomerRole.Id))
                .Count();
        }

        #endregion
    }
}
