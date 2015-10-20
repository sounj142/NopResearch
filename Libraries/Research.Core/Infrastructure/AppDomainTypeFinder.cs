using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Class tìm những kiểu cần thiết bằng cách lặp qua các assembly trong AppDomain hiện hành . Chỉ những assembly mà tên của nó
    /// phù hợp với 1 patterm cho trước thì assembly đó mới đc đưa vào kiểm tra tìm lớp, và có thể cung cấp 1 danh sách tùy chọn 
    /// các asembly luôn được kiểm tra. Nói chung là thường chịu trách nhiệm lọc và tìm kiếm các kiểu phù hợp trong quá trình Nop
    /// khởi chạy
    /// </summary>
    public class AppDomainTypeFinder: ITypeFinder
    {

        #region Properties

        /// <summary>
        /// bỏ qua ngoại lên reflection ?
        /// </summary>
        private bool ignoreReflectionErrors = true;

        /// <summary>
        /// AppDomain để tìm kiếm kiểu trong nó
        /// </summary>
        public virtual AppDomain App
        {
            get { return AppDomain.CurrentDomain; }
        }

        /// <summary>
        /// Cho biết Nop có nên lặp qua các assembly trong appdomain để tìm những type cần thiết hay ko. Mẫu loadingPattern
        /// sẽ đc dùng khi lặp qua các assembly này
        /// </summary>
        public bool LoadAppDomainAssemblies { get; set; }
        /// <summary>
        /// Những assembly bắt đầu bằng System, mscorlib, Microsoft, ...v.v... sẽ ko được kiểm tra bởi chúng ta biết chắc đó là 
        /// assembly hệ thống
        /// </summary>
        public string AssemblySkipLoadingPattern { get; set; }
        /// <summary>
        /// Mẫu các assembly cần kiểm tra : 1 chuỗi ký tự bất kỳ ko có dấu xuống dòng
        /// </summary>
        public string AssemblyRestrictToLoadingPattern { get; set; }
        /// <summary>
        /// Danh sách các assemby sẽ đc load ở thời điểm chạy, bổ sung vào d/sách đã đc load trong appdomain ( load = kiểm tra ? )
        /// Câu hỏi: Liệu đây là kiểm tra hay là Load thực sự, tức là load mã nhị phân của Assembly vào bộ nhớ và chạy
        /// </summary>
        public IList<string> AssemblyNames { get; set; }

        #endregion

        #region Methods
        public AppDomainTypeFinder()
        {
            LoadAppDomainAssemblies = true;
            AssemblyNames = new List<string>();
            AssemblySkipLoadingPattern = "^System|^mscorlib|^Microsoft|^AjaxControlToolkit|^Antlr3|^Autofac|^AutoMapper|^Castle|^ComponentArt|^CppCodeProvider|^DotNetOpenAuth|^EntityFramework|^EPPlus|^FluentValidation|^ImageResizer|^itextsharp|^log4net|^MaxMind|^MbUnit|^MiniProfiler|^Mono.Math|^MvcContrib|^Newtonsoft|^NHibernate|^nunit|^Org.Mentalis|^PerlRegex|^QuickGraph|^Recaptcha|^Remotion|^RestSharp|^Rhino|^Telerik|^Iesi|^TestDriven|^TestFu|^UserAgentStringLibrary|^VJSharpCodeProvider|^WebActivator|^WebDev|^WebGrease";
            AssemblyRestrictToLoadingPattern = null; // null : Ko giới hạn
        }
        /// <summary>
        /// Lấy về những assembly phù hợp với cấu hình hiện hành của lớp
        /// </summary>
        /// <returns>Trả về 1 danh sách các asembly nên đc load bởi Nop Factory</returns>
        public virtual IList<Assembly> GetAssemblies()
        {
            IList<string> addedAssemblyNames;
            return GetAssemblies(out addedAssemblyNames);
        }

        protected virtual IList<Assembly> GetAssemblies(out IList<string> addedAssemblyNames)
        {
            addedAssemblyNames = new List<string>();
            var assemblies = new List<Assembly>();

            if (LoadAppDomainAssemblies) AddAssembliesInAppDomain((List<string>)addedAssemblyNames, assemblies);
            AddConfiguredAssemblies((List<string>)addedAssemblyNames, assemblies);
            return assemblies;
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(assignTypeFrom, GetAssemblies(), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            var result = new List<Type>();
            try
            {
                foreach(var a in assemblies)
                {
                    Type[] types = null;
                    try
                    {
                        types = a.GetTypes();
                    }catch
                    {
                        // EF6 ko cho phép lấy về những kiểu ( ném ngoại lệ nếu đc yêu cầu )
                        if (!ignoreReflectionErrors) throw;
                    }
                    if(types != null)
                    {
                        foreach(var t in types)
                        {
                            if(assignTypeFrom.IsAssignableFrom(t) || (assignTypeFrom.IsGenericTypeDefinition && 
                                DoesTypeImplementOpenGeneric(t, assignTypeFrom)))
                                if (!t.IsInterface)
                                {
                                    if (onlyConcreteClasses)
                                    {
                                        if (t.IsClass && !t.IsAbstract) result.Add(t);
                                    }
                                    else result.Add(t);
                                }
                        }
                    }
                }
            }catch(ReflectionTypeLoadException ex)
            {
                var fail = new Exception(string.Join(Environment.NewLine, ex.LoaderExceptions.Select(x => x.Message)), ex);
                Debug.WriteLine(fail.Message, fail);
                throw fail;
            }

            return result;
        }

        public IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), onlyConcreteClasses);
        }

        public IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true)
        {
            return FindClassesOfType(typeof(T), assemblies, onlyConcreteClasses);
        }
        #endregion

        #region Utilities
        /// <summary>
        /// Lặp qua tất cả Assembly trong AppDomain, và nếu tên của nó khớp với partem cấu hình thì add nó vào list
        /// </summary>
        private void AddAssembliesInAppDomain(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            foreach(var assembly in App.GetAssemblies()) //// khác ở đây
                if (Matches(assembly.FullName) && !addedAssemblyNames.Contains(assembly.FullName))
                {
                    addedAssemblyNames.Add(assembly.FullName);
                    assemblies.Add(assembly);
                }
        }

        public virtual bool Matches(string assemblyFullName)
        {
            return !Matches(assemblyFullName, AssemblySkipLoadingPattern) && (AssemblyRestrictToLoadingPattern == null ||
                Matches(assemblyFullName, AssemblyRestrictToLoadingPattern));
        }

        protected virtual bool Matches(string assemblyFullName, string pattern)
        {
            return Regex.IsMatch(assemblyFullName, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        /// <summary>
        /// Add những assembly đc cấu hình rõ ràng trong List
        /// </summary>
        protected virtual void AddConfiguredAssemblies(List<string> addedAssemblyNames, List<Assembly> assemblies)
        {
            if(AssemblyNames != null)
            {
                foreach (string assemblyName in AssemblyNames)
                {
                    var assembly = Assembly.Load(assemblyName);
                    if(!addedAssemblyNames.Contains(assembly.FullName))
                    {
                        addedAssemblyNames.Add(assembly.FullName);
                        assemblies.Add(assembly);
                    }
                }
            }
        }
        /// <summary>
        /// Kiểm tra xem type có phải là kiểu cụ thể hóa của kiểu generic openGeneric
        /// </summary>
        protected virtual bool DoesTypeImplementOpenGeneric(Type type, Type openGeneric)
        {
            try
            {
                var genericTypeDefinition = openGeneric.GetGenericTypeDefinition();
                foreach(var implementedInterface in type.FindInterfaces((objType, objCriteria) => true, null))
                    if (implementedInterface.IsGenericType)
                    {
                        if (genericTypeDefinition.IsAssignableFrom(implementedInterface.GetGenericTypeDefinition()))
                            return true; //// khác ở đây
                    }
                return false;
            }catch
            {
                return false;
            }
        }
        /// <summary>
        /// Đảm bảo rằng những assembly thỏa mãn đkiện so khớp ở trong folder được chỉ ra sẽ đc load vào app domain
        /// </summary>
        /// <param name="directoryPath">Đường dẫn vật lý của thư mục chứa các dll cần load</param>
        protected virtual void LoadMatchingAssemblies(string directoryPath)
        {
            IList<string> loadedAssemblyNames;
            var assemblies = GetAssemblies(out loadedAssemblyNames);
            if (!Directory.Exists(directoryPath)) return;
            
            foreach(string dllPath in Directory.GetFiles(directoryPath, "*.dll"))
            {
                try
                {
                    var assembly = AssemblyName.GetAssemblyName(dllPath);
                    if (assembly != null && Matches(assembly.FullName) && !loadedAssemblyNames.Contains(assembly.FullName))
                    {
                        App.Load(assembly); //// nên add assembly.FullName vào loadedAssemblyNames để tránh gọi App.Load nhiều lần ?
                        loadedAssemblyNames.Add(assembly.FullName);
                    }
                }catch(BadImageFormatException ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        #endregion
    }
}
