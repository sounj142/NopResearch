
namespace Research.Core.Interface.Service
{
    /// <summary>
    /// Service thực hiện công việc từ địa chỉ Ip, tra trong 1 database đặc biệt để tìm ra location ( thường sẽ đảm bảo chính xác ở
    /// mức country ). Tìm ra mã quốc gia và tên quốc gia tương ứng với ip đó, có thể dùng ở chức năng tự động chọn country khi
    /// xổ ra 1 combobox nhiều country, hoặc chức năng tự động chọn ngôn ngữ dựa trên ip ( ??? ), hoặc chức năng thống kê
    /// cho biết những ip nào truy cập web, đến từ quốc gia nào, hoặc chức năng tính thuế
    /// 
    /// Thực ra cơ chế của MaxMind.GeoIP2 cho phép xác định tới tận quận huyện tỉnh thành và tọa độ kinh vĩ độ ( Xem hàm 
    /// CountryResponse GetInformation(string ipAddress) ), tuy nhiên độ chính xác còn phải xem xét
    /// </summary>
    public partial interface IGeoLookupService
    {
        /// <summary>
        /// Tìm mã chuẩn Iso của country tương ứng với ipAddress
        /// </summary>
        string LookupCountryIsoCode(string ipAddress);

        /// <summary>
        /// Tìm ra tên quốc gia tương ứng với ip
        /// </summary>
        string LookupCountryName(string ipAddress);
    }
}
