using Research.Core;
using Research.Core.Domain.Common;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service chịu trách nhiệm lưu các dữ liệu chung cho các entity nào có nhu cầu cử dụng, tức là các entity có thể định nghĩa thêm
    /// các property mới ( một cách động ) và lưu chúng ở đây, thường dùng để lưu dữ liệu tạm cho người dùng khi họ thực hiện các 
    /// thao tác cần lưu trữ mà ko dùng session như là check out, nhập tên, địa chỉ, loại thanh toán, v.v.v..
    /// 
    /// 
    /// Tất cả các BaseEntity đều có khả năng tự định nghĩa thêm các property và lưu vào trong GenericAttribute, điều này tạo nên sự
    /// linh hoạt cho Nop trong việc động hóa cấu trúc của các entity theo nhu cầu ( nhất là nhu cầu lưu trữ các dữ liệu tạm thay thế
    /// cho session )
    /// </summary>
    public partial interface IGenericAttributeService
    {
        void Delete(GenericAttribute entity);

        GenericAttribute GetById(int id);

        void Insert(GenericAttribute entity);

        void Update(GenericAttribute entity);

        /// <summary>
        /// Lấy về tất cả các attribute(property) tự định nghĩa của thực thể ( id=entityId, type name=keyGroup). 
        /// Dữ liệu được cache per request
        /// </summary>
        IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup);

        /// <summary>
        /// Lấy về tất cả các attribute(property) tự định nghĩa của thực thể entity. 
        /// Dữ liệu được cache per request
        /// </summary>
        IList<GenericAttribute> GetAttributesForEntity(BaseEntity entity);

        /// <summary>
        /// Lấy ra property có tên key của thực thể entity, ép kiểu sẵn sang TPropType. Dữ liệu được cache per request, và lấy ra dựa
        /// vào 1 dữ liệu per request khác là hàm GetAttributesForEntity().
        /// Trả về default value trong trường hợp ko tìm thấy property trong bảng GenericAttribute
        /// </summary>
        TPropType GetAttribute<TPropType>(BaseEntity entity, string key, int storeId = 0);

        /// <summary>
        /// Định nghĩa thêm 1 property có tên key, có giá trị value cho đối tượng entity, với Storeid. Lưu dữ liệu vào bảng GenericAttribute
        /// storeId qui định property thuộc về store nào ( 0 nếu là công cộng cho tất cả store )
        /// </summary>
        void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0, bool saveChange = true);

        /// <summary>
        /// Gọi saveChange và phát sinh sự kiện allChange nếu có thay đổi phát sinh
        /// </summary>
        void SaveChange(bool publishEvent = true);

        ///// <summary>
        ///// Hàm chịu trách nhiệm trả về tên KeyGroup đúng cho đối tượng entity ( tên GetUnproxiedEntityType )
        ///// </summary>
        ////string GetKeyGroupName(BaseEntity entity);
    }
}
