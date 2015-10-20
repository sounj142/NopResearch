using Research.Core.Domain.Directory;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Models;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Directory
{
    public partial class StateProvinceService : BaseService<StateProvince>, IStateProvinceService
    {
        #region field, ctor, property

        private readonly IStateProvinceCacheWriter _cacheWriter;

        public StateProvinceService(IRepository<StateProvince> repository,
            IEventPublisher eventPublisher,
            IStateProvinceCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region ultilities

        /// <summary>
        /// Danh sách đc sắp ưu tiên theo countryId
        /// 
        /// Chú ý: bởi vì StateProvince hỗ trợ đa ngôn ngữ thông qua ILocalizedEntityService, nên sắp xếp theo Name ở đây là chưa chính xác
        /// Khi chuyển về 1 ngôn ngữ cụ thể, cần phải sắp xếp lại 1 lần nữa theo DisplayOrder và Name sau khi dịch để có đc 1 list chính
        /// xác nhất
        /// </summary>
        protected virtual StateProvinceCachePackage GetAllFromStaticCache()
        {
            return _cacheWriter.GetAllFromStaticCache(() => {
                return new StateProvinceCachePackage(_repository.TableNoTracking);
            });
        }

        #endregion

        #region method

        public virtual StateProvince GetStateProvinceById(int id, bool getFromStaticCache = true)
        {
            if (id <= 0) return null;
            return _cacheWriter.GetStateProvinceById(id, getFromStaticCache, () => {
                if (getFromStaticCache)
                {
                    // cách search tuần tự này hơi bị chậm, nếu thao tác get by id được thực hiện nhiều lần thì nên xem xét 
                    // thêm cấu trúc Dict hoặc IList theo id vào static cache để tăng tốc

                    return StateProvince.MakeClone(GetAllFromStaticCache()
                        .StateProvinces.FirstOrDefault(p => p.Id == id));
                }
                else return _repository.GetById(id);
            });
        }

        public virtual StateProvince GetStateProvinceByAbbreviation(int countryId, string abbreviation, bool getFromStaticCache = true)
        {
            // hiện sẽ coi như ko có kết quả nếu tên viết tắt là rỗng
            if(string.IsNullOrEmpty(abbreviation)) return null;

            if (getFromStaticCache)
            {
                return StateProvince.MakeClone(GetAllFromStaticCache().StateProvinces
                    .FirstOrDefault(p => p.CountryId == countryId &&
                        string.Equals(abbreviation, p.Abbreviation, StringComparison.InvariantCultureIgnoreCase)));
            }
            else return _repository.Table
               .FirstOrDefault(p => p.CountryId == countryId && p.Abbreviation == abbreviation);
        }

        public virtual IList<StateProvince> GetStateProvincesByCountryId(int countryId, bool autoOrder = true,
            bool showHidden = false, bool getFromStaticCache = true)
        {
            if (countryId <= 0) return new StateProvince[0];

            return _cacheWriter.GetStateProvincesByCountryId(countryId, autoOrder, showHidden, getFromStaticCache, () =>
            {
                if (getFromStaticCache)
                {
                    return GetAllFromStaticCache().GetStateProvincesByCountryId(countryId, showHidden);
                }
                else
                {
                    var query = _repository.Table.Where(p => p.CountryId == countryId);
                    if (!showHidden) query = query.Where(p => p.Published);
                    if (autoOrder) query = query.OrderBy(p => p.DisplayOrder)
                            .ThenBy(p => p.Name);
                    return query.ToList();
                }
            });
        }

        public virtual IList<StateProvince> GetStateProvinces(bool autoOrder = true, bool showHidden = false,
            bool getFromStaticCache = true)
        {
            if (getFromStaticCache)
            {
                IEnumerable<StateProvince> allStateProvince = GetAllFromStaticCache().StateProvinces;

                if (!showHidden) allStateProvince = allStateProvince.Where(p => p.Published);
                return allStateProvince.Select(p => p.MakeClone()).ToList();
            }
            else
            {
                var query = _repository.Table;
                if (!showHidden) query = query.Where(p => p.Published);
                if (autoOrder) query = query.OrderBy(p => p.CountryId)
                    .ThenBy(p => p.DisplayOrder)
                    .ThenBy(p => p.Name);
                return query.ToList();
            }
        }

        #endregion
    }
}
