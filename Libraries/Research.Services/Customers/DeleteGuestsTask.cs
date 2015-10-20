using Research.Core.Domain.Customers;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using System;

namespace Research.Services.Customers
{
    /// <summary>
    /// Tác vụ chịu trách nhiệm xóa định kỳ các tài khoản Guest rác ko còn dùng đến nữa để làm sạch bảng Customer
    /// </summary>
    public class DeleteGuestsTask : ITask
    {
        private readonly ICustomerService _customerService;
        private readonly CustomerSettings _customerSettings;

        public DeleteGuestsTask(ICustomerService customerService, CustomerSettings customerSettings)
        {
            _customerService = customerService;
            _customerSettings = customerSettings;
        }

        public void Execute()
        {
            var now = DateTime.UtcNow;
            DateTime? createdToUtc = _customerSettings.MinLiveMinuteToDeleteGuestCustomer.HasValue
                ? (DateTime?)now.AddMinutes(-_customerSettings.MinLiveMinuteToDeleteGuestCustomer.Value)
                : null;
            DateTime? lastActivityDateUtcTo = _customerSettings.MinLastActivityMinuteToDeleteGuestCustomer.HasValue
                ? (DateTime?)now.AddMinutes(-_customerSettings.MinLastActivityMinuteToDeleteGuestCustomer.Value)
                : null;

            _customerService.DeleteGuestCustomers(null, createdToUtc, null, lastActivityDateUtcTo, true);
        }
    }
}
