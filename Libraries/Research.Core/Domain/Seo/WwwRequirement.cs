
namespace Research.Core.Domain.Seo
{
    /// <summary>
    /// Miêu tả cho đòi hỏi tiền tố "www." dành cho các url của ứng dụng. Có thể là tùy ý, có thể bắt buộc phải có,
    /// có thể bắt buộc phải ko có
    /// </summary>
    public enum WwwRequirement
    {
        /// <summary>
        /// Doesn't matter (do nothing)
        /// </summary>
        NoMatter = 0,
        /// <summary>
        /// Pages should have WWW prefix
        /// </summary>
        WithWww = 10,
        /// <summary>
        /// Pages should not have WWW prefix
        /// </summary>
        WithoutWww = 20
    }
}
