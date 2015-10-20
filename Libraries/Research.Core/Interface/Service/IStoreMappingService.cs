using System.Collections.Generic;
using Research.Core;
using Research.Core.Domain.Stores;

namespace Research.Core.Interface.Service
{
    // lưu ý đến 1 số list data được cache theo storeId ( áp dụng cho các bảng có mapping với Store ). Trong trường hợp này,
    // cần chú ý để clear cache cho cả những data này mỗi khi có thay đổi trên bảng StoreMapping
    // VD như với bảng Language, ta có cache per request : LANGUAGES_ALL_HASHIDDEN_KEY
    // =====> Nhìn chung, với những bảng có ánh xạ StoreMapping, nếu cache static thì cache static tất cả, còn khi mapping với store
    // ( có yếu tố StoreId tham gia thì chỉ cache per request dựa trên static cache all đang có ). Bởi vì bảng dữ liệu hiện hành đang cache
    // static all + bảng các ánh xạ store mapping đc cache static, nên dữ liệu được lấy ra trực tiếp bằng cách tham khảo 2 cái static cache
    // mà ko phải đọc database, sau đó lại đc cache per request nên đảm bảo là sẽ nhanh thỏa mãn yêu cầu chủa chúng ta !!!!




    // Hệ thống định nghĩa cơ chế đa Store, trong đó mỗi store có thể chạy ở 1 domain riêng, thậm chí có 1 chính sách https/non https riêng
    // Một số đối tương trong hệ thống có thể ánh xạ đến Store để qui định xem store có đc phép đối tượng đó hay ko. Các đối
    // tượng tiêu biểu như sản phẩm, tin tức, tiền tệ
    // Các đối tượng này cài đặt IStoreMappingSupported, với 1 cờ LimitedToStores. Nếu cờ này bằng true thì đối tượng là thoải mái truy cập ở 
    // tất cả các store ( ko quan tâm đến dữ liệu trong bảng StoreMapping ). Nếu là false thì sẽ coi là đối tượng hạn chế truy cập và quyền
    // truy cập được qui định bởi các trường tương ứng trong bảng StoreMapping


    // Bảng language cũng được giới hạn với Store theo cách này. Theo đó, mỗi language có thể là toàn cục, hoặc bị giới hạn đến 1 số store
    // . Trong trường hợp language bị giới hạn, Store nào ko đc map với language sẽ ko có quyền hiển thị ở language đó



    /// <summary>
    /// Service cho phép ánh xạ Store và 1 số loại đối tượng như Product, Tin tức, cho phép giới hạn các đối tượng này lại chỉ
    /// sử dụng trong phạm vi 1 số Store nào đó
    /// </summary>
    public partial interface IStoreMappingService
    {
        void Delete(StoreMapping entity);

        /// <summary>
        /// Get by id, ko cache
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        StoreMapping GetById(int id);

        /// <summary>
        /// Lấy về danh sách các storeMapping của 1 đối tượng entity, với entity là 1 đối tượng hỗ trợ ánh xạ với store IStoreMappingSupported.
        /// Ko cache
        /// </summary>
        IList<StoreMapping> GetStoreMappings<T>(T entity) where T : BaseEntity, IStoreMappingSupported;

        void Insert(StoreMapping entity);

        /// <summary>
        /// Thêm 1 dòng ánh xạ giữa entity và storeId vào bảng StoreMapping
        /// Hàm này ko hề kiểm tra xem mapping đã tồn tại hay chưa, cần phải cẩn thận kiểm tra trước đó
        /// </summary>
        void InsertStoreMapping<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported;

        void Update(StoreMapping entity);

        /// <summary>
        /// Trả về danh sách các storeId có quyền truy cập và sử dụng đối tượng entity. Được cache static.
        /// Điều kiện clear là khi có thay đổi trong bảng StoreMapping, hoặc khi Store được thêm/xóa/(sửa ?)
        /// </summary>
        int[] GetStoresIdsWithAccess<T>(T entity) where T : BaseEntity, IStoreMappingSupported;

        /// <summary>
        /// Kiểm tra xem store hiện hành có được quyền truy cập entity hay ko.
        /// Thao tác kiểm tra thông qua cache
        /// </summary>
        bool Authorize<T>(T entity) where T : BaseEntity, IStoreMappingSupported;

        /// <summary>
        /// Kiểm tra xem store storeId có được phép truy cập entity hay ko. Dùng cache của GetStoresIdsWithAccess()
        /// </summary>
        bool Authorize<T>(T entity, int storeId) where T : BaseEntity, IStoreMappingSupported;
    }
}
