using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Research.Core.Infrastructure;

namespace Research.Web.Framework.Themes
{
    public abstract class ThemeableVirtualPathProviderViewEngine : VirtualPathProviderViewEngine
    {
        #region Fields and properties and Ctors
        // hàm lấy phần mở rộng của file mà đc trỏ tới bởi đường dẫn ảo
        internal Func<string, string> GetExtensionThunk;

        private readonly string[] _emptyLocations = null; // hằng dùng để đặt tên cho 1 mảng rỗng

        protected ThemeableVirtualPathProviderViewEngine()
        {
            GetExtensionThunk = VirtualPathUtility.GetExtension;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Chịu trách nhiệm tìm về đường dẫn file view .cshtml tương ứng với 1 request
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="locations"></param>
        /// <param name="areaLocations"></param>
        /// <param name="locationsPropertyName"></param>
        /// <param name="name">Tên view có thể ở dạng "Index" hoặc "~/View/Home/Action.cshtml"</param>
        /// <param name="controllerName"></param>
        /// <param name="theme"></param>
        /// <param name="cacheKeyPrefix"></param>
        /// <param name="useCache"></param>
        /// <param name="searchedLocations">danh sách các đường dẫn để tìm view, null nếu thao tác tìm path thành công</param>
        protected virtual string GetPath(ControllerContext controllerContext, string[] locations, string[] areaLocations, 
            string locationsPropertyName, string name, string controllerName, string theme, string cacheKeyPrefix,
            bool useCache, out string[] searchedLocations)
        {
            searchedLocations = _emptyLocations; // khởi tạo == rỗng
            if(string.IsNullOrEmpty(name)) return string.Empty; 
            string areaName = GetAreaName(controllerContext.RouteData); // lấy về tên area nếu có

            //little hack to get nop's admin area to be in /Administration/ instead of /Nop/Admin/ or Areas/Admin/

            // hiểu rồi, tức là ta đặt các controller trong Research.Admin vào 1 Area và đặt tên cho area đó là "Admin", trong khi 
            // thực ra Area nằm trong thư mục Administration, cho nên ở đây ta hack 1 chút để mỗi khi muốn tìm view/layout cho
            // Admin area, hãy đến các đường dẫn ~/Administration/Views/.... mà tìm
            if(string.Equals(areaName, "admin", StringComparison.InvariantCultureIgnoreCase))
            {
                var newLocations = new string[(areaLocations == null ? 0 : areaLocations.Length) + 2];
                newLocations[0] = "~/Administration/Views/{1}/{0}.cshtml"; // chèn 2 đường dẫn này vào đầu để ưu tiên tìm đầu tiên
                newLocations[1] = "~/Administration/Views/Shared/{0}.cshtml";
                if(areaLocations != null)
                {
                    for(int i=0; i<areaLocations.Length; i++)
                        newLocations[i+2] = areaLocations[i];
                }
                areaLocations = newLocations;
            }

            bool flag = !string.IsNullOrEmpty(areaName); // true nếu có area
            var viewLocations = GetViewLocations(locations, flag ? areaLocations : null);
            if(viewLocations.Count == 0) // nếu ko có bất kỳ đường dẫn nào để tìm view
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, "Properties cannot be null or empty.", new object[] { locationsPropertyName }));

            bool flagTwo = IsSpecificPath(name); // name bắt đầu bằng dấu ~ hoặc dấu /
            string key = this.CreateCacheKey(cacheKeyPrefix, name, flagTwo ? string.Empty : controllerName, areaName, theme);
            if(useCache)
            {
                string cached = this.ViewLocationCache.GetViewLocation(controllerContext.HttpContext, key);
                if (cached != null) return cached;
            }
            if(!flagTwo) // get path ở dạng tên view ở dạng chung chẳng hạn "Index"
                return this.GetPathFromGeneralName(controllerContext, viewLocations, name, controllerName, areaName, theme, key, ref searchedLocations);
            // get path khi tên view ở dạng đường dẫn, chẳng hạn ~/View/Home/Action.cshtml
            return this.GetPathFromSpecificName(controllerContext, name, key, ref searchedLocations);
        }

        /// <summary>
        /// Chịu trách nhiệm lấy về area từ dữ liệu routing routeData
        /// </summary>
        protected virtual string GetAreaName(RouteData routeData)
        {
            object obj;
            if(routeData.DataTokens.TryGetValue("area", out obj)) return obj as string;
            return GetAreaName(routeData.Route);
        }

         /// <summary>
        /// Chịu trách nhiệm lấy về area từ định nghĩa của 1 route
        /// </summary>
        protected virtual string GetAreaName(RouteBase route)
        {
            var area = route as IRouteWithArea;
            if(area != null) return area.Area;
            var rou = route as Route;
            if(rou != null && rou.DataTokens != null) return rou.DataTokens["area"] as string;
            return null;
        }

        /// <summary>
        /// Lấy danh sách các đường dẫn để tìm view với tham số vào là 
        /// danh sách các đường dẫn tìm view và danh sách các đường dẫn tìm area
        /// 
        /// Nói chung công việc chỉ là nối 2 list lại với nhau, với Area được đưa vào trước
        /// </summary>
        protected virtual IList<ViewLocation> GetViewLocations(string[] viewLocationFormats, string[] areaViewLocationFormats)
        {
            var list = new List<ViewLocation>();
            if (areaViewLocationFormats != null)
                list.AddRange(areaViewLocationFormats.Select(s => new AreaAwareViewLocation(s)));
            if (viewLocationFormats != null)
                list.AddRange(viewLocationFormats.Select(s => new ViewLocation(s)));

            return list;
        }

        /// <summary>
        /// Trả về true nếu path bắt đầu bằng ~ hoặc /
        /// </summary>
        protected virtual bool IsSpecificPath(string path)
        {
            char c = path[0];
            if (c != '~') return c == '/';
            return true;
        }

        protected virtual string CreateCacheKey(string prefix, string name, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, ":ViewCacheEntry:{0}:{1}:{2}:{3}:{4}:{5}", base.GetType().AssemblyQualifiedName, prefix, name, controllerName, areaName, theme);
        }

        /// <summary>
        /// Tìm đường dẫn file view nếu name là 1 tên action, kiểu như "Index"
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="locations">Danh sách các đường dẫn cần tìm, bao gồm đường dẫn area và đường dẫn thông thường. 
        /// Mỗi đường dẫn có dạng "~/Themes/{2}/Views/{1}/{0}.cshtml"</param>
        /// <param name="name"></param>
        /// <param name="controllerName"></param>
        /// <param name="areaName"></param>
        /// <param name="theme"></param>
        /// <param name="cacheKey"></param>
        /// <param name="searchedLocations">Trả về danh sách các đường dẫn được tìm, mối cái có dạng /Theme/Views/Home/Action.cshtml</param>
        /// <returns></returns>
        protected virtual string GetPathFromGeneralName(ControllerContext controllerContext, IList<ViewLocation> locations,
            string name, string controllerName, string areaName, string theme, string cacheKey, ref string[] searchedLocations)
        {
            searchedLocations = new string[locations.Count];
            for (int i = 0; i < locations.Count; i++)
            {
                // 1 link trong locations có dạng "~/Themes/{2}/Views/{1}/{0}.cshtml", ta sẽ tha các ký tự giữ chỗ để xác định được
                // đường dẫn đầy đủ, sau đó kiểm tra nếu tồn tại file thì trả về và thông báo tìm được
                string str = locations[i].Format(name, controllerName, areaName, theme);
                if (this.FileExists(controllerContext, str))
                {
                    searchedLocations = _emptyLocations; // gán null để đánh dấu là tìm thấy
                    this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, str);
                    return str;
                }
                searchedLocations[i] = str;
            }
            return string.Empty; // nếu chạy đến tận đây có nghĩa là tìm ko thấy view
        }
        /// <summary>
        /// Tìm đường dẫn file view nếu name là 1 đường dẫn cụ thể, dạng ~/Theme/Views/Home/Action.cshtml
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="name"></param>
        /// <param name="cacheKey"></param>
        /// <param name="searchedLocations"></param>
        /// <returns></returns>
        protected virtual string GetPathFromSpecificName(ControllerContext controllerContext, string name, 
            string cacheKey, ref string[] searchedLocations)
        {
            // kiểm tra file phải tồn tại và phần mở rộng file đc hỗ trợ
            if (!this.FilePathIsSupported(name) || !this.FileExists(controllerContext, name))
            {
                searchedLocations = new[] { name }; // ko tìm đc, trả về đường dẫn đc kiểm tra
                return string.Empty;
            }
            this.ViewLocationCache.InsertViewLocation(controllerContext.HttpContext, cacheKey, name);
            return name;
        }

        protected virtual bool FilePathIsSupported(string virtualPath)
        {
            var fileExtensions = this.FileExtensions;
            if (fileExtensions == null) return true;

            string str = this.GetExtensionThunk(virtualPath).TrimStart('.');
            return fileExtensions.Contains(str, StringComparer.InvariantCultureIgnoreCase);
        }

        /// <summary>
        /// Lấy về theme hiện hành băng cách sử dụng IEnginer để lấy về IThemeContext, và gọi WorkingThemeName()
        /// </summary>
        /// <returns></returns>
        protected virtual string GetCurrentTheme()
        {
            return EngineContext.Current.Resolve<IThemeContext>().WorkingThemeName;
        }

        /// <summary>
        /// Tìm đầy đủ thông tin đường dẫn render cho view có tên viewName ( tên action hoặc chuỗi virtual path đầy đủ ), 
        /// và layout masterName
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        protected virtual ViewEngineResult FindThemeableView(ControllerContext controllerContext, 
            string viewName, string masterName, bool useCache)
        {
            if (controllerContext == null) throw new ArgumentNullException("controllerContext");
            if (viewName == null) throw new ArgumentNullException("viewName");
            
            string[] strArray, strArray2;
            var theme = GetCurrentTheme();
            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            string str2 = this.GetPath(controllerContext, this.ViewLocationFormats, this.AreaViewLocationFormats, 
                "ViewLocationFormats", viewName, requiredString, theme, "View", useCache, out strArray);
            // nếu masterName == rỗng ( ko có lay out ) thì hàm sẽ rất nhanh trả về rỗng
            string str3 = this.GetPath(controllerContext, this.MasterLocationFormats, this.AreaMasterLocationFormats, 
                "MasterLocationFormats", masterName, requiredString, theme, "Master", useCache, out strArray2);
            if(!string.IsNullOrEmpty(str2) && (string.IsNullOrEmpty(masterName) || !string.IsNullOrEmpty(str3)))
            {
                // nếu tìm được đầy đủ path cho cả view và master ( nếu có master)
                return new ViewEngineResult(this.CreateView(controllerContext, str2, str3), this);
            }
            // không tìm được đầy đủ view và master ?, báo lỗi
            IEnumerable<string> paths;
            if(strArray == null)
                paths = strArray2;
            else paths = strArray2 == null ? strArray : strArray.Union(strArray2);
            return new ViewEngineResult(paths);
        }

        /// <summary>
        /// Trả về 1 đối tượng ViewEngineResult là kết quả của việc tìm đường dẫn cho partial view và tạo ra 1 partial view dựa trên
        /// nội dung của file đó
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="partialViewName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        protected virtual ViewEngineResult FindThemeablePartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            if (controllerContext == null) throw new ArgumentNullException("controllerContext");
            if (partialViewName == null) throw new ArgumentNullException("partialViewName");

            string[] strArray;
            var theme = GetCurrentTheme();
            string requiredString = controllerContext.RouteData.GetRequiredString("controller");
            string str2 = this.GetPath(controllerContext, this.PartialViewLocationFormats, this.AreaPartialViewLocationFormats,
                "PartialViewLocationFormats", partialViewName, requiredString, theme, "Partial", useCache, out strArray);
            if (string.IsNullOrEmpty(str2))
                return new ViewEngineResult(strArray);
            else return new ViewEngineResult(this.CreatePartialView(controllerContext, str2), this);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Cài đặt lại method FindView để cho phép tìm view theo qui ước riêng của ứng dụng, trong đó sẽ chuyển thư mục chứa view mặc
        /// định sang 1 vị trí khác và cho phép cung cấp nhiều theme
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewName"></param>
        /// <param name="masterName"></param>
        /// <param name="useCache"></param>
        /// <returns></returns>
        public override ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return FindThemeableView(controllerContext, viewName, masterName, useCache);
        }

        public override ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return FindThemeablePartialView(controllerContext, partialViewName, useCache);
        }
        
        #endregion
    }

    public class ViewLocation
    {
        protected readonly string _virtualPathFormatString;

        public ViewLocation(string virtualPathFormatString)
        {
            _virtualPathFormatString = virtualPathFormatString;
        }

        public virtual string Format(string viewName, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, theme);
        }
    }

    public class AreaAwareViewLocation: ViewLocation
    {
        public AreaAwareViewLocation(string virtualPathFormatString)
            : base(virtualPathFormatString)
        {
        }
        public override string Format(string viewName, string controllerName, string areaName, string theme)
        {
            return string.Format(CultureInfo.InvariantCulture, _virtualPathFormatString, viewName, controllerName, areaName, theme);
        }
    }
}
