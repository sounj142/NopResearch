using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Research.Core.Configuration
{
    public static class SettingExtensions
    {
        /// <summary>
        /// Hàm từ cú pháp m => m.Name sẽ lấy về [Tên kiểu của m].Name dùng làm khóa cho việc lưu trữ trong bảng Setting
        /// 
        /// Đây chính là cách mà entity framework sử dụng trong mấy cái như Orderby(p => p.Id), để có thể lấy ra tên property "Id"
        /// và sử dụng xây dựng câu truy vấn Sql
        /// </summary>
        public static string GetSettingKey<T, TPropType>(this T entity, Expression<Func<T, TPropType>> keySelector)
            where T : ISettings, new()
        {
            var member = keySelector.Body as MemberExpression; // lấy ra phần thân của biểu thức lambda ?
            if (member == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a method, not a property.", keySelector));

            var propInfo = member.Member as PropertyInfo;
            if (propInfo == null)
                throw new ArgumentException(string.Format("Expression '{0}' refers to a field, not a property.", keySelector));
            return typeof(T).Name + "." + propInfo.Name;
        }
    }
}
