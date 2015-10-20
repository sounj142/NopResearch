using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Research.Core.Infrastructure;

namespace Research.Core.Plugins
{
    // việc khai báo 1 số property là internal set khiến cho nó chỉ có thể được thiết lập giá trị trong phạm vi của assembly khai báo nó,
    // tức là trong Research.Core. Vượt ngoài phạm vi Research.Core sẽ ko thể gọi đc phương thức set. Do đó, trong phạm vi plugin, sẽ
    // ko có cách nào thiết lập thủ công giá trị cho các property này. Cách duy nhất là thiết lập các giá trị này với hàm tạo 3 tham số

    /// <summary>
    /// Lớp chứa các miêu tả về plugin bao gồm các thông tin về tên, kiểu, phiên bản hỗ trợ, store hỗ trợ, v...v.v.v.
    /// </summary>
    public class PluginDescriptor : IComparable<PluginDescriptor>
    {
        #region Proprerties

        /// <summary>
        /// Plugin type
        /// Tên file .dll của plugin ?
        /// </summary>
        public virtual string PluginFileName { get; set; }

        /// <summary>
        /// Plugin type
        /// Trong file dll assembly chính của plugin, sẽ có duy nhất 1 lớp cài đặt giao diện IPlugin. Đó sẽ là điểm vào của plugin,
        /// nơi mà các phương thức Install, UnInstall được gọi
        /// </summary>
        public virtual Type PluginType { get; set; }

        /// <summary>
        /// The assembly that has been shadow copied that is active in the application
        /// Assemly đã đc shadow copy và đang là 1 thành phần của AppDomain
        /// </summary>
        public virtual Assembly ReferencedAssembly { get; internal set; }

        /// <summary>
        /// The original assembly file that a shadow copy was made from it
        /// Đường dẫn của file assembly nguyên bản của plugin
        /// </summary>
        public virtual FileInfo OriginalAssemblyFile { get; internal set; }

        /// <summary>
        /// Gets or sets the plugin group
        /// Gets or sets nhóm của plugin ?
        /// </summary>
        public virtual string Group { get; set; }

        /// <summary>
        /// Gets or sets the friendly name
        /// </summary>
        public virtual string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the system name
        /// Tên hệ thống của đối tượng, là tên nhận diện duy nhất, dùng để phân biệt các Plugin với nhau ???
        /// Tên này rất quan trọng, mỗi plugin nên có 1 tên riêng duy nhất để nhận diện, ko đc có 2 plugin trung tên này với nhau
        /// Tên này cũng để sinh hash cho đối tượng PluginDescriptor, cho phép dung PluginDescriptor như khóa của từ điển
        /// </summary>
        public virtual string SystemName { get; set; }

        /// <summary>
        /// Gets or sets the version
        /// </summary>
        public virtual string Version { get; set; }

        /// <summary>
        /// Gets or sets the supported versions of nopCommerce
        /// </summary>
        public virtual IList<string> SupportedVersions { get; set; }

        /// <summary>
        /// Gets or sets the author
        /// </summary>
        public virtual string Author { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public virtual int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the list of store identifiers in which this plugin is available. If empty, then this plugin is available in all stores
        /// Danh sách các storeId mà plugin này hỗ trợ. Nếu là rỗng thì tất cả các store đều ok
        /// </summary>
        public virtual IList<int> LimitedToStores { get; set; }

        /// <summary>
        /// Gets or sets the value indicating whether plugin is installed
        /// Giá trị chỉ ra rằng plugin đã đc Install hay chưa
        /// </summary>
        public virtual bool Installed { get; set; }

        #endregion

        #region Ctors and methods

        public PluginDescriptor()
        {
            this.SupportedVersions = new List<string>();
            this.LimitedToStores = new List<int>();
        }

        /// <summary>
        /// Hàm tạo 3 tham số trong đó có 2 tham số là internal set. Nếu ở ngoài phạm vi assembly Research.Core thì hàm tạo này là nơi duy nhất
        /// cho phép ta thiết lập giá trị cho các tham số internal set này
        /// </summary>
        public PluginDescriptor(Assembly referencedAssembly, FileInfo originalAssemblyFile, Type pluginType)
            :this()
        {
            this.ReferencedAssembly = referencedAssembly;
            this.OriginalAssemblyFile = originalAssemblyFile;
            this.PluginType = pluginType;
        }

        /// <summary>
        /// Hàm cho phép tạo ra thể hiện của đối tượng IPlugin có kiểu cho bởi property PluginType, và ép kiểu nó về kiểu T, 
        /// với T là 1 kiểu lớp kế thừa từ IPlugin.
        /// Cách làm này cho phép ta trước tiên đọc các file cấu hình để tạo ra đối tượng PluginDescriptor, sau đó triệu gọi đến
        /// phương thức Instance để yêu cầu PluginDescriptor phải tạo ra đối tượng IPlugin tương ứng với thông tin cấu hình của nó.
        /// Nếu tạo thành công thì ta sẽ gán chính PluginDescriptor thành Descriptor của IPlugin mà chúng ta tạo ra đc.
        /// 
        /// Như vậy, nếu được gọi nhiều lần thì có thể sẽ có nhiều thể hiện của IPlugin trỏ đến/dùng chung cùng 1 PluginDescriptor
        /// </summary>
        public virtual T Instance<T>() where T: class, IPlugin
        {
            object instance;
            var containerManager = EngineContext.Current.ContainerManager;
            // cố gắng dùng autofac để tạo ra đối tượng với type=PluginType
            if(!containerManager.TryResolve(PluginType, out instance))
            {
                instance = containerManager.ResolveUnregistered(PluginType);
            }

            // ép kiểu kết quả về T (IPlugin), nếu thành công thì thiết lập this trở thành đối tượng PluginDescriptor mô tả cho T
            var typeInstance = instance as T;
            if (typeInstance != null) typeInstance.PluginDescriptor = this;
            return typeInstance;
        }

        /// <summary>
        /// Gọi lại Instance[IPlugin], trong đó ko ép buộc phải ép kiểu thành 1 kiểu T nào đó cao siêu mà chỉ đơn giản là
        /// chuyển nó thành kiểu interface cơ bản nhất, IPlugin.
        /// Đây là hàm mà đều xài đc với tất cả các plugin
        /// </summary>
        public virtual IPlugin Instance()
        {
            return Instance<IPlugin>();
        }

        /// <summary>
        /// Cho phép so sánh 2 miêu tả plugin PluginDescriptor, trả về -1, 0, 1 tùy theo cái nào bé hơn. Nhờ đó ta có thể sắp xếp
        /// 1 list các PluginDescriptor theo thứ tự tăng dần theo DisplayOrder, và nếu DisplayOrder bằng nhau thì sẽ theo FriendlyName
        /// </summary>
        public int CompareTo(PluginDescriptor other)
        {
            if (DisplayOrder != other.DisplayOrder)
                return DisplayOrder.CompareTo(other.DisplayOrder);
            return FriendlyName.CompareTo(other.FriendlyName);
        }

        /// <summary>
        /// đã override để trả về FriendlyName
        /// </summary>
        public override string ToString()
        {
            return FriendlyName;
        }

        /// <summary>
        /// Chỉ bằng nhau khi đều là PluginDescriptor và có SystemName giống nhau
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as PluginDescriptor;
            return other != null && SystemName != null 
                && SystemName.Equals(other.SystemName, StringComparison.InvariantCulture);
        }

        public override int GetHashCode()
        {
            return SystemName.GetHashCode();
        }

        #endregion
    }
}
