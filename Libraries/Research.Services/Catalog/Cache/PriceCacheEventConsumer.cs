using Research.Core.Domain.Catalog;
using Research.Core.Events;
using Research.Services.Events;
using System.Collections.Generic;

namespace Research.Services.Catalog.Cache
{
    public partial class PriceCacheEventConsumer:
        //product categories
        ICacheConsumer<EntityInserted<ProductCategory>>,
        ICacheConsumer<EntityUpdated<ProductCategory>>,
        ICacheConsumer<EntityDeleted<ProductCategory>>,
        ICacheConsumer<EntityAllChange<ProductCategory>>,
        //products
        ICacheConsumer<EntityInserted<Product>>,
        ICacheConsumer<EntityUpdated<Product>>,
        ICacheConsumer<EntityDeleted<Product>>,
        ICacheConsumer<EntityAllChange<Product>>
    {
        private void AddCacheToClear(IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        { 
        }

        public void HandleEvent(EntityInserted<ProductCategory> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<ProductCategory> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<ProductCategory> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityUpdated<Product> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityDeleted<Product> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityInserted<Product> eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public int Order
        {
            get { return 20; }
        }

        ////int IConsumer<EntityInserted<Product>>.Order
        ////{
        ////    get { return -50; }
        ////}

        public void HandleEvent(EntityAllChange<ProductCategory> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }

        public void HandleEvent(EntityAllChange<Product> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            AddCacheToClear(staticCachePrefixes, perRequestCachePrefixes);
        }
    }
}
