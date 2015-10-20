namespace Research.Core.Interface.Service
{
    /// <summary>
    /// User agent helper interface
    /// </summary>
    public partial interface IUserAgentHelper
    {
        /// <summary>
        /// Chỉ ra request hiện hành có phải là do 1 search enginer tạo ra hay ko 
        /// </summary>
        bool IsSearchEngine();
    }
}
