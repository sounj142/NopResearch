using Research.Core.Domain.Directory;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Cache static tất cả các country, sau đó, các thao tác truy vấn con sẽ sử dụng dữ liệu từ static cache, lưu tạm vào per request cache
    /// . Có hỗ trợ chọn lựa lấy từ static cache hoặc lấy trực tiếp từ database.
    /// 
    /// 
    /// 
    /// Chú ý là thứ tự sắp xếp các Country ko thực sự chính xác nếu có đa ngôn ngữ, vì thứ tự sắp xếp nguyên bản là theo Country.Name ,
    /// mà nếu như dùng đến chuỗi dịch của Name trong LocalizedProperty thì thứ tự sắp xếp ban đầu sẽ ko còn đúng nữa. Nếu muốn đảm bảo
    /// thứ tự sắp xếp luôn đúng, cần phải sắp xếp lại 1 lần nữa với DisplayOrder và Name mới được dịch.
    /// Đã kiểm tra, quả nhiên NOP sắp xếp sai thứ tự các country sau khi đc dịch. Đúng ra là phải sắp xếp theo thứ tự tên sau khi đã dịch,
    /// thậm chí phải dùng chuẩn so sánh chuỗi của quốc gia tương ứng
    /// </summary>
    public partial interface ICountryService
    {
        void Delete(Country entity);

        /// <summary>
        /// Get tất cả các country theo cờ published, cache perequest. Trường hợp thiết lập cấu hình yêu cầu giới hạn StoreMapping giữa
        /// Country - Store thì sẽ tiến hành lọc lại dữ liệu 1 lần nữa để đảm bảo yêu cầu này ( map với store hiện hành )
        /// </summary>
        IList<Country> GetAllCountries(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true);

        /// <summary>
        /// Gets all countries that allow billing, perrequest cache.
        /// </summary>
        IList<Country> GetAllCountriesForBilling(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true);

        /// <summary>
        /// Gets all countries that allow shipping, perrequest cache
        /// </summary>
        IList<Country> GetAllCountriesForShipping(bool autoOrder = true, bool showHidden = false, bool getFromStaticCache = true);

        Country GetCountryById(int countryId, bool getFromStaticCache = true);

        IList<Country> GetCountriesByIds(int[] countryIds, bool getFromStaticCache = true);

        /// <summary>
        /// Gets a country by two letter ISO code
        /// </summary>
        Country GetCountryByTwoLetterIsoCode(string twoLetterIsoCode, bool getFromStaticCache = true);

        /// <summary>
        /// Gets a country by three letter ISO code
        /// </summary>
        Country GetCountryByThreeLetterIsoCode(string threeLetterIsoCode, bool getFromStaticCache = true);

        void Insert(Country entity);

        void Update(Country entity);
    }
}
