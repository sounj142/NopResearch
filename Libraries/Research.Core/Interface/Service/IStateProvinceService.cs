using Research.Core.Domain.Directory;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// service StateProvince, thực hiện static cache all ( với gải định rằng chỉ có 1 số ít quốc gia có dữ liệu tỉnh thành ), dữ liệu
    /// static cache sắp xếp theo countryId trước sau đó mới đến DisplayOrder, và Name, để hỗ trợ cho cơ chế tìm các state thuộc về cùng 
    /// 1 tỉnh thành
    /// 
    /// Các thao tác khác cho phép lấy dữ liệu dựa trên static cache/trực tiếp database và cache vào perRequest cache
    /// 
    /// 
    /// /// Chú ý là thứ tự sắp xếp các StateProvince ko thực sự chính xác nếu có đa ngôn ngữ, vì thứ tự sắp xếp nguyên bản là theo StateProvince.Name ,
    /// mà nếu như dùng đến chuỗi dịch của Name trong LocalizedProperty thì thứ tự sắp xếp ban đầu sẽ ko còn đúng nữa. Nếu muốn đảm bảo
    /// thứ tự sắp xếp luôn đúng, cần phải sắp xếp lại 1 lần nữa với DisplayOrder và Name mới được dịch
    /// </summary>
    public partial interface IStateProvinceService
    {
        void Delete(StateProvince entity);

        /// <summary>
        /// Get by id, ko phân biệt published hay ko
        ///  Cache perrequest, cho phép chọn dữ liệu từ static cache hoặc database
        /// </summary>
        StateProvince GetStateProvinceById(int id, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy StateProvince theo tên viết tắt [ khác: có thêm countryId để bảo đảm tính chính xác ]. 
        /// Do ko dùng nhiều nên ko cache per request
        /// </summary>
        StateProvince GetStateProvinceByAbbreviation(int countryId, string abbreviation, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy về tất cả các StateProvince của countryId cho trước, perrequest cache.
        /// Nếu muốn có thứ tự sắp xếp đúng khi dùng bản dịch của StateProvince.Name, cần thực hiện sắp xếp lại kết quả của hàm này
        /// sau khi lấy về bản dịch theo languageId cụ thể.
        /// 
        /// Truyền autoOrder = false nếu ko muốn tự động sắp sẵn ( trường hợp dự định sẽ dùng ngôn ngữ cục bộ )
        /// </summary>
        IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true);

        /// <summary>
        /// Lấy về tất cả các tỉnh thành với cờ showHidden, thường dùng trong admin, ko cache.
        /// Truyền autoOrder = false nếu ko muốn tự động sắp sẵn ( trường hợp dự định sẽ dùng ngôn ngữ cục bộ )
        /// </summary>
        IList<StateProvince> GetStateProvinces(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true);

        void Insert(StateProvince entity);

        void Update(StateProvince entity);
    }
}
