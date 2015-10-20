using Research.Core.Caching;
using Research.Core.Infrastructure;

namespace Research.Services.Caching
{
    /// <summary>
    /// Tác vụ clear cache định kỳ, có thể được cấu hình từ bảng ScheduleTask trong database
    /// </summary>
    public partial class ClearCacheTask : ITask
    {
        public void Execute()
        {
            var cacheManager = EngineContext.Current.Resolve<ICacheManager>();
            cacheManager.Clear();
        }
    }
}
