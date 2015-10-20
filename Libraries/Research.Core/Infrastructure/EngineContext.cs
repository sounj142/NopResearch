using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using Research.Core.Configuration;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Lớp sẽ đọc config của web, theo cấu hình trong config, sẽ tạo ra đối tượng IEngine tương ứng và gọi đến IEngine.Initialize()
    /// 
    /// Để sử dụng, đơn giản gọi đến EngineContext.Current là ok, thao tác gọi đến Initialize() sẽ được gọi ngầm định ở lần gọi đầu tiên
    /// </summary>
    public class EngineContext
    {
        // vì lớp Singleton<IEngine> là duy nhất nên có thể dùng cách này để định nghĩa
        // nhanh 1 singleton
        public static IEngine Current
        {
            get
            {
                if (Singleton<IEngine>.Instance == null) // nếu chưa có IEngine thì yêu cầu khởi tạo
                    Initialize(false);
                return Singleton<IEngine>.Instance;
            }
        }

        protected static IEngine CreateEngineInstance(NopConfig config)
        {
            if(config != null && !string.IsNullOrEmpty(config.EngineType)) // nếu có yêu cầu dùng 1 IEngine khác với IEngine mặc định
            {
                var engineType = Type.GetType(config.EngineType);
                if(engineType == null)
                    throw new ConfigurationErrorsException("The type '" + config.EngineType + "' could not be found. Please check the configuration at /configuration/nop/engine[@engineType] or check for missing assemblies.");
                else if(!typeof(IEngine).IsAssignableFrom(engineType))
                    throw new ConfigurationErrorsException("The type '" + engineType + "' doesn't implement 'Nop.Core.Infrastructure.IEngine' and cannot be configured in /configuration/nop/engine[@engineType] for that purpose.");
                return (IEngine)Activator.CreateInstance(engineType);
            }

            // dùng IEngine mặc định
            return new NopEngine();
        }

        /// <summary>
        /// Đọc thông tin cấu hình trong webconfig và khởi tạo thể hiện IEngine phù hợp, và triệu gọi đến IEngine.Initialize()
        /// Hàm này sẽ đc lazy loading từ EngineContext.Current, ko cần phải gọi thực thi tường minh
        /// </summary>
        /// <param name="forceRecreate">Cướng ép tạo lại IEngine cho dù đã có IEngine được tạo trước đó. Hạn chế dùng</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IEngine Initialize(bool forceRecreate)
        {
            if (Singleton<IEngine>.Instance == null || forceRecreate) // chỉ tạo mới IEngine nếu chưa có hoặc có yêu cầu cụ thể
            {
                // đọc thông tin cấu hình từ webconfig
                var config = ConfigurationManager.GetSection("NopConfig") as NopConfig; // ép kiểu sẽ thành công, do có mục cấu hình section trong web config qui định thẻ <NopConfig> sẽ đc ánh xạ thành đối tượng NopConfig
                Singleton<IEngine>.Instance = CreateEngineInstance(config);
                Singleton<IEngine>.Instance.Initialize(config);
            }
            return Singleton<IEngine>.Instance;
        }

        /// <summary>
        /// Cho phép chuyển sang sử dụng IEngine khác ( nên hay ko nên, có lẽ chỉ nên áp dụng ở AppStart, nêu muốn tạo động 1
        /// IEngine mà ko dùng thông tin cấu hình trong webconfig ). Nói chung là ko nên dùng
        /// </summary>
        /// <param name="engine"></param>
        /// <remarks>Only use this method if you know what you're doing.</remarks>
        public static void Replace(IEngine engine)
        {
            Singleton<IEngine>.Instance = engine;
        }
    }
}
