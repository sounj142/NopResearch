using Research.Core.Caching;
using Research.Core.Infrastructure;
using Research.Core.Interface.Service;
using Research.Core.Plugins;
using System;
using System.Collections.Generic;

namespace Research.Services.Events
{
    /// <summary>
    /// Lớp đóng vai trò nhà sản xuất các sự kiện, được sử dụng chủ yếu ở phần insert/update/delete, dùng để phát ra sự kiện mỗi khi thay đổi các
    /// dữ liệu bị cache
    /// 
    /// Hệ thống sẽ chỉ có  đối tượng IEventPublisher duy nhất, singleton
    /// </summary>
    public class EventPublisher : IEventPublisher
    {
        /// <summary>
        /// Đối tượng dùng để lấy ra danh sách các consumer đăng ký nhận sự kiện
        /// </summary>
        private readonly ISubscriptionService _subscriptionService;

        public EventPublisher(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        /// <summary>
        /// Nhà sản xuất sẽ đăng ký sự kiện bằng hàm này. Hàm sẽ chịu trách nhiệm duyệt qua tất cả các consumer có đăng ký nhận sự kiện
        /// và gọi lần lượt đến chúng
        /// </summary>
        public virtual void Publish<T>(T eventMessage)
        {
            // nguyên bản :
            //foreach (var consumer in _subscriptionService.GetSubscriptions<T>())
            //    PublishToConsumer(consumer, eventMessage);


            // Hệ interface IConsumer đã được sửa lại để làm 1 nhiệm vụ duy nhất: Đăng ký các mẫu chuỗi cache cần remove vào 1 list để
            // có thể được remove 1 lần duy nhất, ở trong 1 khóa lock được bảo vệ. Bằng cách đó, ta đảm bảo là cache được remove 1 cách nhanh chóng
            // và trọn vẹn nhất 


            PublishCommonProcess((staticCachePrefixes, perRequestCachePrefixes) =>
            {
                PublishToAllConsumer<T>(eventMessage, staticCachePrefixes, perRequestCachePrefixes);
            });
        }

        public virtual void Publish<T1, T2>(T1 eventMessage1, T2 eventMessage2)
        {
            PublishCommonProcess((staticCachePrefixes, perRequestCachePrefixes) =>
            {
                PublishToAllConsumer<T1>(eventMessage1, staticCachePrefixes, perRequestCachePrefixes);
                PublishToAllConsumer<T2>(eventMessage2, staticCachePrefixes, perRequestCachePrefixes);
            });
        }

        public virtual void Publish<T1, T2, T3>(T1 eventMessage1, T2 eventMessage2, T3 eventMessage3)
        {
            PublishCommonProcess((staticCachePrefixes, perRequestCachePrefixes) =>
            {
                PublishToAllConsumer<T1>(eventMessage1, staticCachePrefixes, perRequestCachePrefixes);
                PublishToAllConsumer<T2>(eventMessage2, staticCachePrefixes, perRequestCachePrefixes);
                PublishToAllConsumer<T3>(eventMessage3, staticCachePrefixes, perRequestCachePrefixes);
            });
        }

        protected virtual void PublishToAllConsumer<T>(T eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            if (eventMessage != null)
            {
                foreach (var consumer in _subscriptionService.GetSubscriptions<T>())
                    PublishToConsumer(consumer, eventMessage, staticCachePrefixes, perRequestCachePrefixes);
            }
        }

        protected virtual void PublishCommonProcess(Action<IList<string>, IList<string>> addKeyToClearCacheListAction)
        {
            var staticCachePrefixes = new List<string>();
            var perRequestCachePrefixes = new List<string>();

            addKeyToClearCacheListAction(staticCachePrefixes, perRequestCachePrefixes);

            var engine = EngineContext.Current;
            // clear cache duy nhất 1 lần với lock
            if (staticCachePrefixes.Count > 0)
            {
                var staticCache = engine.Resolve<ICacheManager>();
                staticCache.RemoveByKeysStartsWith(staticCachePrefixes);
            }
            if (perRequestCachePrefixes.Count > 0)
            {
                var perRequestCache = engine.Resolve<IPerRequestCacheManager>();
                perRequestCache.RemoveByKeysStartsWith(perRequestCachePrefixes);
            }
        }

        /// <summary>
        /// Thông báo sự kiện đến consumer bằng cách triệu gọi hàm HandleEvent trên consumer
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="consumer">Đối tượng nhận sự kiện</param>
        /// <param name="eventMessage">Sự kiện</param>
        protected virtual void PublishToConsumer<T>(ICacheConsumer<T> consumer, T eventMessage,
            IList<string> staticCachePrefixes, IList<string> perRequestCachePrefixes)
        {
            // bỏ qua không gọi đến hàm xử lý sự kiện nếu consumer là kiểu thuộc về 1 plugin mà ko được instance
            var pluginDescriptor = PluginManager.FindPlugin(consumer.GetType());
            if (pluginDescriptor != null && !pluginDescriptor.Installed) return;

            try
            {
                // gọi hàm xử lý sự kiện của consumer
                consumer.HandleEvent(eventMessage, staticCachePrefixes, perRequestCachePrefixes);
            }catch(Exception ex)
            {
                var logger = EngineContext.Current.Resolve<ILogger>();
                logger.Error(ex.Message, ex);
            }
        }
    }
}
