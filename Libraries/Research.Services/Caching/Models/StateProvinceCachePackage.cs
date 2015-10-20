using Research.Core.Domain.Directory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Caching.Models
{
    /// <summary>
    /// Đối tượng dùng để cache tất cả các StateProvince vào static cache, cung cấp cơ chế cho phép xác định nhanh danh sách các StateProvince
    /// thuộc về 1 countryId cho trước
    /// </summary>
    public class StateProvinceCachePackage
    {
        public IList<StateProvince> StateProvinces { get; private set; }

        /// <summary>
        /// Từ điển chỉ mục lưu chỉ số của StateProvince đầu tiên thuộc countryId
        /// </summary>
        private IDictionary<int, int> _countryIndexDict = new Dictionary<int, int>();

        public StateProvinceCachePackage(IQueryable<StateProvince> stateProvinces)
        {
            if (stateProvinces == null) throw new ArgumentNullException("stateProvinces");

            StateProvinces = stateProvinces.OrderBy(p => p.CountryId)
                    .ThenBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Name)
                    .ToList();
            // khởi tạo dữ liệu ban đầu cho từ điên chỉ mục
            int lastCountryId = 0;
            for (int i = 0; i < StateProvinces.Count; i++)
                if (StateProvinces[i].CountryId != lastCountryId)
                {
                    lastCountryId = StateProvinces[i].CountryId;
                    _countryIndexDict[lastCountryId] = i;
                }
        }

        /// <summary>
        /// Hàm cho phép lấy nhanh được danh sách các StateProvince thuộc về countryId, đã đc sắp tăng theo StateProvince.Name
        /// </summary>
        public IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool showHidden = false)
        {
            var result = new List<StateProvince>();
            if (countryId <= 0) return result;

            int index;
            if (_countryIndexDict.TryGetValue(countryId, out index))
            {
                while (index < StateProvinces.Count && StateProvinces[index].CountryId == countryId)
                {
                    if (showHidden || StateProvinces[index].Published)
                        result.Add(StateProvinces[index].MakeClone());
                    index++;
                }
            }
            return result;
        }
    }
}
