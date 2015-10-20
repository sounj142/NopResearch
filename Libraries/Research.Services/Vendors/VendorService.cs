using System;
using System.Linq;
using Research.Core;
using Research.Core.Domain.Vendors;
using Research.Services.Events;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;

namespace Research.Services.Vendors
{
    /// <summary>
    /// tạm thời ko có nhu cầu cache
    /// </summary>
    public partial class VendorService : BaseService<Vendor>, IVendorService
    {
        #region field, ctor, and property

        public VendorService(IRepository<Vendor> repository,
            IEventPublisher eventPublisher):
            base(repository, eventPublisher)
        {
        }

        #endregion

        #region method

        public IPagedList<Vendor> GetAllVendors(string name = null, int pageIndex = 0, 
            int pageSize = int.MaxValue, bool showHidden = false)
        {
            var query = _repository.Table.Where(p => !p.Deleted);
            if (!string.IsNullOrWhiteSpace(name)) query = query.Where(p => p.Name.Contains(name));
            if (!showHidden) query = query.Where(p => p.Active);
            query = query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name);

            return new PagedList<Vendor>(query, pageIndex, pageSize);
        }

        #endregion
    }
}
