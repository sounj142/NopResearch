using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

namespace Research.Data
{
    public static class DataReaderExtensions
    {
        /// <summary>
        /// Extention method cho phép đọc vào dữ liệu từ 1 IDataReader ( thường là bảng kết quả trả về từ 1 truy vấn sql ),
        /// và xuất ra 1 IList với các property được gắn giá trị đầy đủ ( dùng reflection )
        /// </summary>
        /// <typeparam name="TType"></typeparam>
        /// <param name="reader">1 datareader đã đc mở và đang ở vị trí đọc</param>
        /// <param name="fieldsToSkip">tùy chọn: danh sách những property mà bạn ko muốn đọc vào. Phải chuyển về LowerCase</param>
        /// <param name="piList">Danh sách những property bạnh quan tâm, có thể cache sẵn để truyền vào nhằm tăng 1 phần hiệu năng
        /// , khóa string phải chuyển về lower case</param>
        /// <returns></returns>
        public static IList<TType> DataReaderToObjectList<TType>(this IDataReader reader, IList<string> fieldsToSkips = null,
            IDictionary<string, PropertyInfo> piList = null) where TType : new()
        {
            if (reader == null) return null;
            var result = new List<TType>();

            piList = GetPropertyDictionary<TType>(piList);
            while (reader.Read())
            {
                result.Add(DataReaderToObject<TType>(reader, fieldsToSkips, piList));
            }

            return result;
        }

        private static IDictionary<string, PropertyInfo> GetPropertyDictionary<TType>(IDictionary<string, PropertyInfo> piList)
        {
            if (piList == null)
            {
                piList = new Dictionary<string, PropertyInfo>();
                foreach (var property in typeof(TType).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                    piList.Add(property.Name.ToLowerInvariant(), property);
            }
            return piList;
        }

        /// <summary>
        /// fieldsToSkips: phải chuyển về in thường, piList : khóa tring phải chuyển về in thường
        /// </summary>
        public static void DataReaderToObject<TType>(this IDataReader reader, ref TType entity, IList<string> fieldsToSkips = null,
            IDictionary<string, PropertyInfo> piList = null) where TType: new()
        {
            if(reader.IsClosed)
                throw new InvalidOperationException("Data reader cannot be used because it's already closed");

            piList = GetPropertyDictionary<TType>(piList);
            for(int i=0; i<reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                if(!string.IsNullOrEmpty(name))
                {
                    name = name.ToLowerInvariant();
                    if(piList.ContainsKey(name) && (fieldsToSkips == null || !fieldsToSkips.Contains(name)))
                    {
                        var property = piList[name];
                        if(property != null && property.CanWrite)
                        {
                            var val = reader.GetValue(i);
                            property.SetValue(entity, val == DBNull.Value ? null : val);
                        }
                    }
                }
            }
        }

        public static TType DataReaderToObject<TType>(this IDataReader reader, IList<string> fieldsToSkips = null,
            IDictionary<string, PropertyInfo> piList = null) where TType : new()
        {
            TType entity = new TType();
            DataReaderToObject(reader, ref entity, fieldsToSkips, piList);
            return entity;
        }
    }
}
