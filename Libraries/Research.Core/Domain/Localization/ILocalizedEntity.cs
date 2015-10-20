

namespace Research.Core.Domain.Localization
{
    /// <summary>
    /// Miêu tả 1 đối tượng có đa ngôn ngữ. 1 đối tượng muốn hỗ trợ đa ngôn ngữ thông qua LocalizedProperty (ILocalizedEntityService) 
    /// thì PHẢI cài đặt interface này
    /// 
    /// 
    /// Chẳng hạn Product có 2 property là Name và Description là yêu cầu đa ngôn ngữ. Khi đó bản thân đối tượng Product
    /// vẫn chứa dữ liệu cho 2 property này ( gọi là dữ liệu tiêu chuẩn ), còn 1 bảng khác là LocalizedProperty sẽ chứa dữ liệu bản dịch
    /// của các property này sang các ngôn ngữ khác. Product muốn lấy bản dịch của nó cho ngôn ngữ nào thì có thể qua bảng này lấy
    /// hoặc có thể dùng dữ liệu mặc định mà nó có nếu ko tìm thấy
    /// 
    /// Ko như Nop, lúc lấy ra property, ta sẽ có 1 trường bool qui định có lấy dữ liệu tiêu chuẩn khi ko có bản dịch
    /// hay ko, như thế thì ở 1 số trường hợp ta có thể ràng buộc chỉ lấy về đúng ngôn ngữ được yêu cầu
    /// </summary>
    public interface ILocalizedEntity
    {

    }
}
