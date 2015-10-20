
namespace Research.Core
{
    
    public static class Extensions
    {
        /// <summary>
        /// Xây dựng extension method tương tự string.IsNullOrEmpty áp dụng cho kiểu T?, trong đó T là struct ( int? , double? .... )
        /// </summary>
        public static bool IsNullOrDefault<T>(this T? value) where T:struct
        {
            return default(T).Equals(value.GetValueOrDefault());
        }
    }
}
