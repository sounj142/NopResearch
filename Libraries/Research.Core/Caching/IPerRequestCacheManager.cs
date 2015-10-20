
namespace Research.Core.Caching
{
    /// <summary>
    /// Interface dành riêng cho PerRequestCache, từ giờ nhắc tới ICacheManager sẽ là static cache, IPerRequestCacheManager sẽ là
    /// per request cache. Đơn giản rõ ràng, ko đánh lận con đen
    /// </summary>
    public interface IPerRequestCacheManager : ICacheManager
    {
    }
}
