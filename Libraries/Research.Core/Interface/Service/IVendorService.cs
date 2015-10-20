using Research.Core;
using Research.Core.Domain.Vendors;

namespace Research.Core.Interface.Service
{
    public partial interface IVendorService
    {
        Vendor GetById(int id);

        void Delete(Vendor entity);

        IPagedList<Vendor> GetAllVendors(string name = null, int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false);

        void Insert(Vendor entity);

        void Update(Vendor entity);
    }
}
