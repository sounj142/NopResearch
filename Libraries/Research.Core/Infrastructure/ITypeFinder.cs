using System;
using System.Collections.Generic;
using System.Reflection;

namespace Research.Core.Infrastructure
{
    /// <summary>
    /// Class cài đặt giao diện này cho phép tìm kiếm những kiểu được chỉ định hoặc những kiểu kế thừa từ kiểu đó, trong
    /// phạm vi assembly được chỉ rõ. Tức là nó cho phép tìm kiếm động những class, chẳng hạn những class kế thừa từ BaseController
    /// trong phạm vi asembly Nop.Web, đồng thời cho phép load thêm các assemby từ các thư mục chỉ định vào appdomain
    /// </summary>
    public interface ITypeFinder // cài đặt đc sử dụng l
    {
        IList<Assembly> GetAssemblies();

        /// <summary>
        /// Trả về những class có kiểu là assignTypeFrom hoặc kế thừa từ kiểu này
        /// </summary>
        /// <param name="assignTypeFrom">kiểu cần tìm</param>
        /// <param name="onlyConcreteClasses">true: Chỉ lấy các kiểu thuộc loại có thể tạo thực thể ( là lơp và ko phải là abstract ). False: lấy hết các kiểu lớp </param>
        /// <returns>Kết quả trả về sẽ luôn khác null</returns>
        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType(Type assignTypeFrom, IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType<T>(bool onlyConcreteClasses = true);

        IEnumerable<Type> FindClassesOfType<T>(IEnumerable<Assembly> assemblies, bool onlyConcreteClasses = true);
    }
}
