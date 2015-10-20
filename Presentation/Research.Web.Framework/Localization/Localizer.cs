
namespace Research.Web.Framework.Localization
{
    /// <summary>
    /// Định nghĩa Localizer là tên 1 hàm delegete, mà cụ thể sẽ là cú pháp @T() trong view. Hàm này sẽ nhận vào tham số là 1 chuỗi resourceKey, và trả về
    /// kết quả là resourcevalue liên kết với chuỗi đó. Trường hợp args.Length>0 và resourcevalue có dạng {0}...{1}... thì sẽ dùng các tham số
    /// args để điềm vào cho resourcevalue bằng hàm string.Format()
    /// Các tham số args cho phép ta dễ dàng làm việc với các chuỗi resource có dùng ký tự giữ chỗ
    /// </summary>
    public delegate LocalizedString Localizer(string resourceKey, params object[] args);
}
