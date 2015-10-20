using System;
using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Catalog;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Events;

namespace Research.Services.Catalog
{
    public partial class ProductService : BaseService<Product>, IProductService
    {
        public ProductService(IProductRepository repository, 
            IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        {

        }
    }
}
