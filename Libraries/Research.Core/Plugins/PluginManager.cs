using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using System.Web.Compilation;
using System.Web.Hosting;
using Research.Core.ComponentModel;
using Research.Core.Plugins;


// QUAN TRỌNG: MVC 5 đã chỉ còn hỗ trợ Full Trust nên đối với các phiên bản NOP mới, sẽ ko còn hỗ trợ medium trust nữa. Ngoài ra, medium
// trust đã là 1 kỹ thuật lỗi thời, nên bất cứ 1 nhà phân phối host chuyên nghiệp nào cũng sẽ bỏ cái này và hỗ trợ full trust
// => Toàn bộ phần code liên quan đến medium trust ở đây đều chỉ code để .. ngắm mà thôi chứ ko đc sử dụng
// http://www.nopcommerce.com/boards/t/31717/does-nopcommerce-33-runs-in-medium-trust-hosting-or-needs-full-trust-hosting.aspx
// http://stackoverflow.com/questions/16849801/is-trying-to-develop-for-medium-trust-a-lost-cause
// https://support.microsoft.com/en-us/kb/2698981



//Contributor: Umbraco (http://www.umbraco.com). Thanks a lot! 
//SEE THIS POST for full details of what this does - http://shazwazza.com/post/Developing-a-plugin-framework-in-ASPNET-with-medium-trust.aspx

// bằng chỉ thị này, ta đưa hàm PluginManager.Initialize() lên thành hàm được triệu gọi trước cả Appliacation_Start(). Ta muốn đưa code
// vào thực thi ở vị trí này là để có thể đăng ký các dll của plugin với AppDomain tại trước thời điểm chạy của BuildManager của MVC.
// Bằng cách đó, ta sẽ load động được các plugin khi mà không đặt các dll của plugin vào thư mục /bin
// Tùy vào mức độ trust, là medium hay full mà ta chọn 1 trong 2 biện pháp sau:
// + Full : Copy thẳng tất cả các dll plugin vào thư mục tạm AppDomain DynamicDirectory, và thế là xong
// + Medium : Copy tất cả các dll plugin vào ~/Plugins/bin, add chúng vào cho BuildManager
[assembly: PreApplicationStartMethod(typeof(PluginManager), "Initialize")]


namespace Research.Core.Plugins
{
    // các method ở đây là static nên đương nhiên ko có unit test

    // Nhận xét: Xem ra các hàm Install(), Uninstall của IPlugin chỉ được gọi trong các giao diện quản lý việc cài đặt plugin chứ ko cần
    // thiết phải được gọi bởi hệ thống
    // Tất cả những gì hệ thống làm để load plugin là như sau :
    // + Quét qua toàn bộ thư mục ~/Plugin và load tất cả các plugin có thể có vào AppDomain, đăng ký assembly với BuildManager
    // + Tùy theo plugin có được điểm danh trong InstallerPlugin.txt hay ko mà PluginDescriptor.Installer của nó bằng true hay false
    // và cũng chỉ có thế, chứ tất cả các plugin vẫn đc load vào AppDomain như thường
    // + Khác biệt giữa các plugin đc install và ko đc install sẽ đến ở các tao tác như : Đăng ký routing, đăng ký ITask, v..v..
    // nói chung là những plugin ko đc install sẽ bị loại bỏ ra khỏi những tiến trình đăng ký này. Chúng chỉ đc load vào app domain
    // và để đó thôi chứ ko được sử dụng


    // nhận xét: PluginManager xử lý theo cách như sau : Copy tất cả các file dll của tất cả các plugin vào thư mục tạm ( DynamicDirectory/
    // hoặc ~/Plugin/bin tùy mức độ trust level ), và chỉ copy mỗi các file dll mà thôi. Sau đó load assembly các file dll đã copy đó và 
    // đăng ký chúng với BuildManager, đồng thời vẫn duy trì các đường dẫn đến thư mục plugin chứa dll gốc ( VD: ~/Plugins/Shipping.CanadaPost )
    //. Chúng ta sẽ từ thư mục này lấy ra các tài nguyên cần thiết như view, image ..v...v..v..
    // Hệ thống luôn duy trì 1 IList<PluginDescriptor> chính là danh sách các plugin hiện đang load trong AppDomain. Mỗi PluginDescriptor
    // luôn giữ trong nó nhiều thứ : 1 FileInfo đến file dll gốc, 1 Assembly trỏ đến asembly sau khi đc load vào AppDomain, 1 Type trỏ
    // đến kiểu của lớp cài đặt IPlugin, ..v..v...
    
    public partial class PluginManager
    {
        #region Fields, Properties, Ctors

        /// <summary>
        /// đường dẫn đến file txt chứa danh sách các plugin đã được cài đặt. Tên chứa trong file này chính là tên nhận diện SystemName.
        /// File chứa nhiều dòng, mỗi dòng là SystemName của 1 plugin
        /// </summary>
        private const string InstalledPluginsFilePath = "~/App_Data/InstalledPlugins.txt";

        /// <summary>
        /// Đường dẫn vật lý nơi sẽ chứa tất cả các plugin sau khi đã được biên dịch thành mã thực thi ( khác với project plugin là nơi
        /// chứa source code ). Đường dẫn này sẽ là Folder /Plugins tính từ thư mục gốc Web ( cùng cấp với /bin , /Views ... ).
        /// Nhìn chung, sau khi biên dịch xong thì chúng ta cần copy ( tự động ? ) các mã biên dịch của các plugin vào đây, nhưng vẫn giữ
        /// nguyên cấu trúc thư mục của từng plugin
        /// </summary>
        private const string PluginsPath = "~/Plugins";

        /// <summary>
        /// Đường dẫn vật lý nơi mà ta sẽ tiến hành copy dll của tất cả plugin vào, để tạo ra 1 bản copy, dùng để đăng ký với AppDoman
        /// và giải quyết vấn đề "DLL file locking" (ko cho update plugin khi ứng dụng chạy) trong trường hợp medium trust như đã để cập ở link đầu file.
        /// Đường dẫn này là thư mục con /bin của PluginsPath. Sẽ cần lưu ý khi tiến hành copy các dll là con của /PluginsPath vào
        /// /PluginsPath/bin vì bản thân bin cũng là con của PluginsPath
        /// </summary>
        private const string ShadowCopyPath = "~/Plugins/bin";

        





        /// <summary>
        /// Dùng để khóa ghi, đảm bảo cho chỉ 1 luồng duy nhất được phép thực thi vào đoạn code được khóa này bảo vệ
        /// </summary>
        private static ReaderWriterLockSlim Locker = new ReaderWriterLockSlim();

        /// <summary>
        /// thư mục tướng ứng với ShadowCopyPath ???
        /// Thư mục này sẽ chỉ được dùng để copy dll vào trong chế độ medium trust ?
        /// </summary>
        private static DirectoryInfo _shadowCopyFolder;

        /// <summary>
        /// Thư mục gốc nơi chưa các plugin ( tương ứng với PluginsPath )
        /// </summary>
        private static DirectoryInfo _pluginRootFolder;

        /// <summary>
        /// Yêu cầu xóa sạch toàn bộ thư mục shadow copy mỗi khi ứng dụng khởi chạy ?
        /// </summary>
        private static bool _clearShadowDirectoryOnStartup;




        /// <summary>
        /// Returns a collection of all referenced plugin assemblies that have been shadow copied
        /// Tập hợp tất cả các plugin đã được load vào AppDomain
        /// </summary>
        public static IList<PluginDescriptor> ReferencedPlugins { get; set; }

        /// <summary>
        /// Returns a collection of all plugin which are not compatible with the current version
        /// Tập hợp các plugin mà nó ko tương thích với phiên bản Nop hiện hành. Đây là các bản plugin chỉ hỗ trợ các phiên bản Nop cũ
        /// </summary>
        public static IList<string> IncompatiblePlugins { get; set; }


        #endregion

        #region Utilities

        /// <summary>
        /// Lấy về tất cả các file cấu hình plugin từ 1 đường dẫn thư mục cha pluginFolder cho trước. Tức là khi có đường dẫn ~/Plugin/
        /// ta sẽ trả về danh sách các file cấu hình của các plugin con, đi kèm với đó là cấu hình plugin đã đc parse thành PluginDescriptor.
        /// Chú ý là sẽ chỉ lấy ra các file cấu hình ở mức con trực tiếp của ~/Plugin/, vd ~/Plugin/ExternalAuth.Facebook, ko lấy sâu hơn
        /// 
        /// Kết quả của hàm được đảm bảo ko bao giờ lấy vào thư mục /Plugin/bin
        /// </summary>
        /// <param name="pluginFolder">Plugin directory info</param>
        /// <returns>Original and parsed description files, sắp xếp theo thứ tự tăng dần của DisplayOrrder và FriendlyName</returns>
        private static IList<KeyValuePair<FileInfo, PluginDescriptor>> GetDescriptionFilesAndDescriptors(DirectoryInfo pluginFolder)
        {
            if (pluginFolder == null) throw new ArgumentNullException("pluginFolder");

            // danh sách các cặp <file description, nội dung PluginDescriptor sau khi được parse >
            var result = new List<KeyValuePair<FileInfo, PluginDescriptor>>();
            foreach (var descriptionFile in pluginFolder.GetFiles("Description.txt", SearchOption.AllDirectories))
                if(IsPackagePluginFolder(descriptionFile.Directory))
                {
                    var pluginDescriptor = PluginFileParser.ParsePluginDescriptionFile(descriptionFile.FullName);
                    result.Add(new KeyValuePair<FileInfo, PluginDescriptor>
                        (descriptionFile, pluginDescriptor));
                }

            //sort list by display order. NOTE: Lowest DisplayOrder will be first i.e 0 , 1, 1, 1, 5, 10
            //it's required: http://www.nopcommerce.com/boards/t/17455/load-plugins-based-on-their-displayorder-on-startup.aspx
            
            // gọi đến hàm CompareTo đã được override của PluginDescriptor
            result.Sort((a, b) => a.Value.CompareTo(b.Value));
            return result;
        }

        /// <summary>
        /// Kiểm tra xem 1 file dll đã được load vào assembly hay chưa, bằng cách so sánh File Name của file dll với danh sách file
        /// name của các assembly trong appDomain hiện hành
        /// </summary>
        private static bool IsAlreadyLoaded(FileInfo fileInfo)
        {
            try
            {
                string fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileInfo.FullName);
                if(fileNameWithoutExt == null)
                    throw new Exception(string.Format("Cannot get file extnension for {0}", fileInfo.Name));

                foreach(var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    // phép kiểm tra dựa vào tên file này thực sự tỏ ra khá miễn cưỡng. Sẽ ra sao nếu các Plugin đặt tên trùng nhau ?
                    string assemblyName = assembly.FullName
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .FirstOrDefault();
                    if (string.Equals(assemblyName, fileNameWithoutExt, StringComparison.InvariantCultureIgnoreCase))
                        return true;
                }
            }catch(Exception ex)
            {
                Debug.WriteLine("Cannot validate whether an assembly is already loaded. " + ex);
            }
            return false;
        }

        /// <summary>
        /// Đăng ký 1 file dll của plugin vào appdomain, bao gồm các bước copy file dll vào thư mục tạm, sau đó là gọi load assembly
        /// cho file đã được copy đó, cuối cùng là đăng ký assembly đã load được với BuildManager.
        /// Kết quả là assembly của file đã copy được
        /// </summary>
        public static Assembly PerformFileDeploy(FileInfo plug, DirectoryInfo shadowCopyPlugFolder, bool isFullTrust)
        {
            if (plug == null) throw new ArgumentNullException("plug");
            if(plug.Directory == null || plug.Directory.Parent == null)
                throw new InvalidOperationException("The plugin directory for the " + plug.Name +
                                                    " file exists in a folder outside of the allowed nopCommerce folder heirarchy");

            // copy file plugin vào thư mục phù hợp tùy vào mức trust level
            var shadowCopiedPlug = isFullTrust ?
                InitializeFullTrust(plug, shadowCopyPlugFolder) :
                InitializeMediumTrust(plug, shadowCopyPlugFolder);

            //we can now register the plugin definition
            var shadowCopiedAssembly = Assembly.Load(AssemblyName.GetAssemblyName(shadowCopiedPlug.FullName));

            //add the reference to the build manager
            Debug.WriteLine("Adding to BuildManager: '{0}'", shadowCopiedAssembly.FullName);
            BuildManager.AddReferencedAssembly(shadowCopiedAssembly);

            return shadowCopiedAssembly;
        }


        /// <summary>
        /// Used to initialize plugins when running in Full Trust
        /// Hàm được dùng để khởi chạy 1 file dll plugin khi hệ thống chạy trong chế độ Full Trust
        /// 
        /// Công việc mà nó làm đơn giản là tìm cách copy file dll plugin [plug] vào thư mục shadowCopyPlugFolder ( là AppDomain DynamicDirectory )
        /// </summary>
        /// <param name="plug">Đường dẫn file dll plugin nguyên bản. Nếu plugin có nhiều dll thì ta cũng sẽ copy tất cả chúng từng cái 1 ???</param>
        /// <param name="shadowCopyPlugFolder">Thư mục nơi file plugin đc shadow copy tới</param>
        /// <returns></returns>
        private static FileInfo InitializeFullTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            // ở chế độ Full Trust, chúng ta chỉ đơn giản là copy file plug vào shadowCopyPlugFolder, ko kiểm tra xem trong folder
            // có tồn tại file cũ hay ko, 1 phần vì quyền Full Trust, 1 phần vì thư mục shadowCopyPlugFolder là 1 thư mục động 
            // được tạo lúc thực thi app ???
            return InitializeCommon(plug, shadowCopyPlugFolder);
        }

        
        /// <summary>
        /// Used to initialize plugins when running in Medium Trust
        /// 
        /// Công việc mà nó làm đơn giản là tìm cách copy file dll plugin [plug] vào thư mục shadowCopyPlugFolder, thường là thư mục
        /// ~/Plugins/bin
        /// </summary>
        private static FileInfo InitializeMediumTrust(FileInfo plug, DirectoryInfo shadowCopyPlugFolder)
        {
            bool shouldCopy = true;
            var shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));

            // kiểm tra xem file với đường dẫn shadowCopiedPlug đã có hay chưa, và nếu nó có, kiểm tra xem đây có phải là 1 phiên bản update
            //, và nếu ko phải thì sẽ ko copy
            if (shadowCopiedPlug.Exists)
            {
                // sẽ tốt hơn nếu sử dụng LastWriteTimeUTC, nhưng ko phải tất cả các hệ thống file đều có property này
                // có thể sẽ chính xác hơn khi chúng ta sinh và kiểm tra file hash ???
                bool areFilesIdentical = shadowCopiedPlug.CreationTimeUtc.Ticks >= plug.CreationTimeUtc.Ticks;
                if (areFilesIdentical)
                {
                    shouldCopy = false;
                    Debug.WriteLine("Not copying; files appear identical: '{0}'", shadowCopiedPlug.Name);
                }
                else
                {
                    // File shadowCopiedPlug được coi là cũ hơn và cần đc xóa để thay thế bằng file mới
                    // More info: http://www.nopcommerce.com/boards/t/11511/access-error-nopplugindiscountrulesbillingcountrydll.aspx?p=4#60838

                    Debug.WriteLine("New plugin found; Deleting the old file: '{0}'", shadowCopiedPlug.Name);
                    File.Delete(shadowCopiedPlug.FullName);
                }
            }

            if (shouldCopy)
                return InitializeCommon(plug, shadowCopyPlugFolder, shadowCopiedPlug);

            return shadowCopiedPlug;
        }


        private static FileInfo InitializeCommon(FileInfo plug, DirectoryInfo shadowCopyPlugFolder, FileInfo shadowCopiedPlug = null)
        {
            if (shadowCopiedPlug == null)
                shadowCopiedPlug = new FileInfo(Path.Combine(shadowCopyPlugFolder.FullName, plug.Name));
            try
            {
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            catch (IOException)
            {
                Debug.WriteLine(shadowCopiedPlug.FullName + " đã bị lock, đang thử rename file");
                //this occurs when the files are locked,
                //for some reason devenv locks plugin files some times and for another crazy reason you are allowed to rename them
                //which releases the lock, so that it what we are doing here, once it's renamed, we can re-shadow copy
                try
                {
                    string oldFileName = shadowCopiedPlug.FullName + Guid.NewGuid().ToString("N") + ".old";
                    File.Move(shadowCopiedPlug.FullName, oldFileName);
                }
                catch (IOException ex)
                {
                    throw new IOException(shadowCopiedPlug.FullName + " đổi tên thất bại, ko thể khởi chạy plugin", ex);
                }
                //ok, we've made it this far, now retry the shadow copy
                File.Copy(plug.FullName, shadowCopiedPlug.FullName, true);
            }
            return shadowCopiedPlug;
        }


        /// <summary>
        /// Determines if the folder is a bin plugin folder for a package
        /// Hàm kiểm tra xem 1 thư mục có phải là thư mục chủ của 1 plugin nào đó hay ko. Thư mục này phải là con trực tiếp của thư mục
        /// gốc ~/Plugins, đồng thời ko phải là thư mục bin ~/Plugins/bin
        /// </summary>
        private static bool IsPackagePluginFolder(DirectoryInfo folder)
        {
            if (folder == null || folder.Parent == null) return false;
            if (!string.Equals(folder.Parent.FullName, _pluginRootFolder.FullName, StringComparison.InvariantCultureIgnoreCase)) return false;
            // thêm vào 1 kiểm tra để đảm bảo plugin gốc ko được search từ folder bin chứa file shadow copy
            if (string.Equals(folder.FullName, _shadowCopyFolder.FullName, StringComparison.InvariantCultureIgnoreCase)) return false;
            return true;
        }

        /// <summary>
        /// Lấy về đường dẫn vật lý của file InstalledPlugins.txt, nơi chứa danh sách các plugin đã Install ( các plugin ko chứa trong
        /// danh sách này dù tìm thấy cũng sẽ ko install ??? )
        /// </summary>
        private static string GetInstalledPluginsFilePath()
        {
            return HostingEnvironment.MapPath(InstalledPluginsFilePath);
        }

        #endregion

        #region Methods


        /// <summary>
        /// Hàm sẽ đc triệu gọi đầu tiên trong ứng dụng, chịu trách nhiệm shadow copy các dll của plugin nếu cần thiết và thực hiện 
        /// load các dll của plugin vào AppDomain, đồng thời đăng ký các assembly plugin với BuildManager
        /// </summary>
        public static void Initialize()
        {
            // thiết lập khóa ghi để chỉ cho phép 1 luồng xử lý duy nhất được phép chạy qua đoạn code này tại 1 thời điểm
            // liệu hàm sẽ đc triệu gọi 1 lần duy nhất trước Application Start hay có thể chạy thêm nhiều lần khác nữa khi mà ứng dụng
            // đang chạy ?
            // Có vẻ như sẽ chỉ chạy 1 lần duy nhất trước thời điểm app start
            using(new WriteLockDisposable(Locker))
            {
                // lấy về mức độ trust của application
                bool isFullTrust = CommonHelper.GetTrustLevel() == AspNetHostingPermissionLevel.Unrestricted;
                // lấy về đường dẫn nơi chúng ta sẽ copy các file plugin dll vào, tùy mức độ trust mà đó có thể là thư mục ~/Plugins/bin
                // hoặc thư mục DynamicDirectory của appDomain
                DirectoryInfo realShadowCopyPlugFolder;
                if (isFullTrust)
                {
                    Debug.WriteLine("=============== Trust level is FULL TRUST ===============");
                    Debug.WriteLine("+++AppDomain.CurrentDomain.DynamicDirectory = " + AppDomain.CurrentDomain.DynamicDirectory);
                    realShadowCopyPlugFolder = new DirectoryInfo(AppDomain.CurrentDomain.DynamicDirectory);
                }
                else
                {
                    Debug.WriteLine("=============== Trust level is MEDIUM TRUST ===============");
                    realShadowCopyPlugFolder = new DirectoryInfo(HostingEnvironment.MapPath(ShadowCopyPath));
                }



                // TODO: Add verbose exception handling / raising here since this is happening on app startup and could
                // prevent app from starting altogether

                _pluginRootFolder = new DirectoryInfo(HostingEnvironment.MapPath(PluginsPath)); // thư mục gốc chứa plugin ~/Plugins
                _shadowCopyFolder = new DirectoryInfo(HostingEnvironment.MapPath(ShadowCopyPath)); // thư mục shadow cho medium trust ~/Plugins/bin
                string installedPluginFilePath = GetInstalledPluginsFilePath(); // đường dẫn file InstalledPlugins.txt nơi chứa tên SystemName của các plugin đang đc install

                var referencedPlugins = new List<PluginDescriptor>(); // danh sách các plugin đang được load vào AppDomain của hệ thống ?
                var incompatiblePlugins = new List<string>(); // danh sách SystemName các plugin ko tương thích với phiên bản hiện hành ?

                string clearShadowDirectoryOnStartupStr = ConfigurationManager.AppSettings["ClearPluginsShadowDirectoryOnStartup"];
                _clearShadowDirectoryOnStartup = !string.IsNullOrWhiteSpace(clearShadowDirectoryOnStartupStr) &&
                    Convert.ToBoolean(clearShadowDirectoryOnStartupStr);

                try
                {
                    // lấy về danh sách SystemName của các plugin đang đc đánh dấu là install trong file InstallerPlugins.txt
                    var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(installedPluginFilePath);

                    Debug.WriteLine("Creating shadow copy folder and querying for dlls");
                    
                    // đảm bảo rằng các thư mục ~/Plugins ; ~/Plugins/bin đã được tạo
                    if (!_pluginRootFolder.Exists)
                        Directory.CreateDirectory(_pluginRootFolder.FullName);
                    if (!_shadowCopyFolder.Exists)
                        Directory.CreateDirectory(_shadowCopyFolder.FullName);

                    // lấy về danh sách tất cả các file đang có trong  ~/Plugins/bin và clear nếu được yêu cầu
                    if (_clearShadowDirectoryOnStartup || isFullTrust) // sẽ xóa sạch khi có cấu hình cụ thể hoặc khi ở mức full trust, bởi full trust ko cần phải đặt file vào ~/Plugins/bin !
                    {
                        var binFiles = _shadowCopyFolder.GetFiles("*", SearchOption.AllDirectories);
                        // xóa sạch các file cũ trong ~/Plugins/bin
                        foreach (var fileInfo in binFiles)
                        {
                            Debug.WriteLine("Deleting " + fileInfo.Name);
                            try
                            {
                                File.Delete(fileInfo.FullName);
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("Error deleting file " + fileInfo.Name + ". Exception: " + ex);
                            }
                        }
                    }


                    // duyệt qua tất cả các file miêu tả plugin có trong các thư mục con trực tiếp của ~/Plugins. Kiểm tra các plugin này,
                    // load plugin vào AppDomain nếu plugin là phù hợp. Tùy theo SystemName của plugin có trong file InstalledPlugin.txt
                    // hay ko mà thiết lập giá trị cho trường Installed tương ứng. Chú ý là việc có mặt hay ko trong InstalledPlugin.txt
                    // sẽ ko ảnh hưởng đến việc plugin được shadow copy và load assembly vào AppDomain
                    foreach (var dfd in GetDescriptionFilesAndDescriptors(_pluginRootFolder))
                    {
                        var descriptionFile = dfd.Key;
                        var pluginDescriptor = dfd.Value;

                        // đảm bảo rằng plugin phải hỗ trợ Nop phiên bản hiện hành
                        if (!pluginDescriptor.SupportedVersions.Contains(NopVersion.CurrentVersion, StringComparer.InvariantCultureIgnoreCase))
                        {
                            // add plugin vào danh sách các plugin out of date
                            incompatiblePlugins.Add(pluginDescriptor.SystemName);
                            continue;
                        }

                        // 1 vài kiểm tra khác
                        if(string.IsNullOrWhiteSpace(pluginDescriptor.SystemName))
                            throw new Exception(string.Format("A plugin '{0}' has no system name. Try assigning the plugin a unique name and recompiling.", descriptionFile.FullName));
                        // lỗi xảy ra khi cùng 1 plugin xuất hiện nhiều hơn 1 lần trong thư mục ~/Plugins
                        if(referencedPlugins.Contains(pluginDescriptor))
                            throw new Exception(string.Format("A plugin with '{0}' system name is already defined", pluginDescriptor.SystemName));

                        // kiểm tra xem nếu tên SystemName của plugin được khai báo trong file InstalledPlugins.txt thì sẽ thiết lập
                        // cho plugin là đã cài đặt
                        pluginDescriptor.Installed = installedPluginSystemNames.Any(p => string.Equals(p,
                            pluginDescriptor.SystemName, StringComparison.InvariantCulture));

                        try
                        {
                            if(descriptionFile.Directory == null)
                                throw new Exception(string.Format("Directory cannot be resolved for '{0}' description file", descriptionFile.Name));

                            // lấy về danh sách tất cả các file dll có trong thư mục của plugin hiện hành. Sẽ chỉ lấy các file dll có trong thư mục gốc
                            // của plugin, ko lấy nhầm bất kỳ file nào trong nhánh shadow copy ~/Plugins/bin
                            var pluginFiles = descriptionFile.Directory.GetFiles("*.dll", SearchOption.TopDirectoryOnly)
                                //.Where(f => IsPackagePluginFolder(f.Directory)) // sẽ chỉ lấy những file dll ở thư mục gốc của plugin, vd như thưc mục ~/Plugins/ExternalAuth.Facebook ( là con trực tiếp của ~/Plugins )
                                .ToList();

                            // lấy ra file dll chính của plugin
                            var mainPluginFile = pluginFiles
                                .FirstOrDefault(f => f.Name.Equals(pluginDescriptor.PluginFileName, StringComparison.InvariantCultureIgnoreCase));
                            if(mainPluginFile == null)
                                throw new Exception(string.Format("Plugin '{0}' don't have main plugin file '{1}'", pluginDescriptor.SystemName, pluginDescriptor.PluginFileName));
                            pluginDescriptor.OriginalAssemblyFile = mainPluginFile; // lưu lại file nguyên gốc khi chưa copy

                            // shadow copy file dll chính của plugin, đồng thời load Assembly nó lên và đăng ký nó với BuildManager
                            pluginDescriptor.ReferencedAssembly = PerformFileDeploy(mainPluginFile, realShadowCopyPlugFolder, isFullTrust);

                            // load tất cả những file dll khác ko phải là file dll chính còn lại của plugin
                            foreach (var plugin in pluginFiles
                                .Where(f => !f.Name.Equals(mainPluginFile.Name, StringComparison.InvariantCultureIgnoreCase)))
                                if (!IsAlreadyLoaded(plugin))
                                    PerformFileDeploy(plugin, realShadowCopyPlugFolder, isFullTrust);


                            // tìm trong Assembly của file dll chính của plugin ( file name cho bởi pluginDescriptor.PluginFileName )
                            // để lấy ra class đầu tiên cài đặt giao diện IPlugin mà là 1 lớp thông thường, ghi nhận class đó vào
                            // pluginDescriptor.PluginType như thể là cổng vào của plugin, nơi mà các phương thức Install, UnInstall của
                            // plugin được gọi
                            foreach(var t in pluginDescriptor.ReferencedAssembly.GetTypes())
                                if(typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && t.IsClass && !t.IsAbstract)
                                {
                                    pluginDescriptor.PluginType = t;
                                    break;
                                }

                            // ghi nhận plugin đã đc load vào AppDomain
                            referencedPlugins.Add(pluginDescriptor);
                        }catch(ReflectionTypeLoadException ex)
                        {
                            string msg = string.Empty;
                            foreach (var e in ex.LoaderExceptions)
                                msg += e.Message + Environment.NewLine;
                            var fail = new Exception(msg, ex);
                            Debug.WriteLine(fail.Message);
                            throw fail;
                        }
                    }

                }catch(Exception ex)
                {
                    string msg = string.Empty;
                    for (var e = ex; e != null; e = e.InnerException)
                        msg += e.Message + Environment.NewLine;

                    var fail = new Exception(msg, ex);
                    Debug.WriteLine(fail.Message);
                    throw fail;
                }

                // ghi nhận lại giá trị cho các tập static
                ReferencedPlugins = referencedPlugins; 
                IncompatiblePlugins = incompatiblePlugins;
            }
        }

        /// <summary>
        /// Đánh dấu 1 plugin là installed
        /// </summary>
        public static void MarkPluginAsInstalled(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName)) throw new ArgumentNullException("systemName");

            string filePath = GetInstalledPluginsFilePath();
            IList<string> installedPluginSystemNames;
            if (!File.Exists(filePath))
            {
                using (File.Create(filePath))
                {
                    //we use 'using' to close the file after it's created
                }
                installedPluginSystemNames = new List<string>();
            }
            else
            {
                installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(filePath);
                for (int i = 0; i < installedPluginSystemNames.Count; i++)
                    if (string.Equals(installedPluginSystemNames[i], systemName, StringComparison.InvariantCultureIgnoreCase))
                        return;
            }

            installedPluginSystemNames.Add(systemName);
            PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
        }

        /// <summary>
        /// Đánh dấu 1 plugin là Uninstalled
        /// </summary>
        public static void MarkPluginAsUninstalled(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName)) throw new ArgumentNullException("systemName");

            string filePath = GetInstalledPluginsFilePath();
            if (File.Exists(filePath))
            {
                var installedPluginSystemNames = PluginFileParser.ParseInstalledPluginsFile(filePath);

                for (int i = 0; i < installedPluginSystemNames.Count; i++)
                    if (string.Equals(installedPluginSystemNames[i], systemName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        installedPluginSystemNames.RemoveAt(i);
                        PluginFileParser.SaveInstalledPluginsFile(installedPluginSystemNames, filePath);
                        break;
                    }
            }
        }

        /// <summary>
        /// Đánh dấu tất cả các plugin là đã bị gỡ bỏ
        /// Để làm điều này, ta xóa bỏ file cho bởi đường dẫn InstalledPluginsFilePath. Chỉ những plugin có SystemName được liệt kê
        /// trong file InstalledPluginsFilePath này mới được load asembly vào AppDoamin, những plugin khác sẽ ko được phép load vào appDomain
        /// </summary>
        public static void MarkAllPluginsAsUninstalled()
        {
            string filePath = GetInstalledPluginsFilePath();
            if (File.Exists(filePath)) File.Delete(filePath);
        }

        /// <summary>
        /// Find a plugin descriptor by some type which is located into its assembly
        /// Kiểm tra xem nếu providerType là 1 phần của 1 plugin nào đó thì trả về PluginDescriptor của plugin đó
        /// </summary>
        /// <param name="providerType">Provider type</param>
        /// <returns>Plugin descriptor</returns>
        public static PluginDescriptor FindPlugin(Type providerType)
        {
            if (providerType == null) throw new ArgumentNullException("providerType");

            var pluginDescriptors = ReferencedPlugins;
            if (pluginDescriptors == null) return null;
            foreach (var descriptor in pluginDescriptors)
                if (descriptor.ReferencedAssembly != null &&
                    string.Equals(providerType.Assembly.FullName, descriptor.ReferencedAssembly.FullName, StringComparison.InvariantCulture))
                    return descriptor;

            return null;
        }

        #endregion
    }
}
