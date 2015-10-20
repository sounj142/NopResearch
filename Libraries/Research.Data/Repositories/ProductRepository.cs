using Research.Core.Domain.Catalog;
using Research.Core.Interface.Data;

namespace Research.Data.Repositories
{
    public partial class ProductRepository : EfRepository<Product>, IProductRepository
    {
        public ProductRepository(IUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }
    }
}
