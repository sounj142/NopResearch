
namespace Research.Web.Framework.Localization
{
    public interface ILanguageUrlStandardize
    {
        /// <summary>
        /// Cần đc gọi 1 lần duy nhất trong Begin_Request. Ghi chú là hàm sẽ ko làm bất cứ điều gì nếu hệ thống ko hỗ trợ phân đoạn
        /// ngôn ngữ
        /// 
        /// [[[Trong trường hợp hệ thống dùng url có phân đoạn ngôn ngữ]]], hàm sẽ gọi RewritePath để đảm bảo 
        /// AppRelativeCurrentExecutionFilePath bị loại bỏ phân đoạn ngôn ngữ nếu có
        /// </summary>
        void RewriteAppRelativeCurrentExecutionFilePath();
    }
}
