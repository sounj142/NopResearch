using Research.Core.Caching;
using Research.Core.Domain.Catalog;
using Research.Core.Domain.Localization;
using Research.Core.Events;
using Research.Core.Infrastructure;
using Research.Services.Events;
using System.Collections.Generic;

namespace Research.Web.Infrastructure.Cache
{
    /// <summary>
    /// Đối tượng cài đặt tất cả các interface IConsumer[T], là trung tâm thực hiện clear static cache của Research.Web ( clear các
    /// cache được thêm vào trong các controller )
    /// 
    /// Hiện tại thì mỗi khi cần phát ra sự kiện, IEventPublisher ( singleton ) sẽ gọi đến Autofact để lấy về các IConsumer. Điều này thường
    /// dẫn tới việc tạo mới đối tượng ModelCacheEventConsumer, trong khi cài đặt của ModelCacheEventConsumer cho phép đối tượng
    /// này có thể tồn tại singleton để đỡ tốn xử lý. Cần cải tiến ở đây
    /// 
    /// Ghi chú: IConsumer có thể do cả các plugin cài đặt, thượng vàng hạ cám, nói chung là ko thể qui chuẩn và có thể singleton
    /// như ModelCacheEventConsumer. Thôi thì cứ per request cũng được
    /// 
    /// </summary>
    public partial class ModelCacheEventConsumer :
        BaseCacheWriter,

        // languages
        ICacheConsumer<EntityInserted<Language>>,
        ICacheConsumer<EntityUpdated<Language>>,
        ICacheConsumer<EntityDeleted<Language>>,
        ICacheConsumer<EntityAllChange<Language>>,

        //manufacturers
        ICacheConsumer<EntityInserted<Manufacturer>>,
        ICacheConsumer<EntityUpdated<Manufacturer>>,
        ICacheConsumer<EntityDeleted<Manufacturer>>,
        ICacheConsumer<EntityAllChange<Manufacturer>>,

        //product manufacturers
        ICacheConsumer<EntityInserted<ProductManufacturer>>,
        ICacheConsumer<EntityUpdated<ProductManufacturer>>,
        ICacheConsumer<EntityDeleted<ProductManufacturer>>,
        ICacheConsumer<EntityAllChange<ProductManufacturer>>,

        //categories
        ICacheConsumer<EntityInserted<Category>>,
        ICacheConsumer<EntityUpdated<Category>>,
        ICacheConsumer<EntityDeleted<Category>>,
        ICacheConsumer<EntityAllChange<Category>>,

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
        // Không như các CacheWrite tồn tại đơn lẻ phân tán, ta sẽ cài đặt cái này để xóa các cache còn lại bằng duy nhất lớp này
        // Để làm đc điều đó, ta sẽ cài đặt ModelCacheEventConsumer kế thừa từ BaseWriter và sẽ cung cấp 1 hà sa số các
        // ICacheWrite khác, nhưng tất cả chúng sẽ đều đc cài đặt bởi ModelCacheEventConsumer. Đồng thời ModelCacheEventConsumer cũng
        // sẽ đóng vai trò xóa cache bằng cách cài đặt các IConsumer

        #region Manage Product Cache

        public void HandleEvent(EntityInserted<Product> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<Product> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<Product> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityAllChange<Product> eventMessage, 
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        #endregion



        public int Order
        {
            get { return 10; }
        }

        ////int IConsumer<EntityInserted<Product>>.Order
        ////{
        ////    get { return -10; }
        ////}




        public void HandleEvent(EntityInserted<Language> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<Language> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<Language> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityInserted<Manufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<Manufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<Manufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityInserted<ProductManufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<ProductManufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<ProductManufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityInserted<Category> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<Category> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<Category> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityInserted<ProductCategory> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityUpdated<ProductCategory> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityDeleted<ProductCategory> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        

        public void HandleEvent(EntityAllChange<Language> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityAllChange<Manufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityAllChange<ProductManufacturer> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityAllChange<Category> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }

        public void HandleEvent(EntityAllChange<ProductCategory> eventMessage, IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {

        }
    }
}