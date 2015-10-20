using Research.Core;
using Research.Core.Domain.Common;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Research.Services.Common
{
    public partial class GenericAttributeService : BaseService<GenericAttribute>, IGenericAttributeService
    {
        #region Field, property and ctor

        private readonly IGenericAttributeCacheWriter _cacheWriter;

        public GenericAttributeService(IRepository<GenericAttribute> repository,
            IEventPublisher eventPublisher,
            IGenericAttributeCacheWriter cacheWriter)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
        }

        #endregion

        #region Methods

        public IList<GenericAttribute> GetAttributesForEntity(int entityId, string keyGroup)
        {
            return _cacheWriter.GetAttributesForEntity(entityId, keyGroup, () => {
                return _repository.Table
                    .Where(p => p.EntityId == entityId && p.KeyGroup == keyGroup)
                    .ToList();
            });
        }

        public void SaveAttribute<TPropType>(BaseEntity entity, string key, TPropType value, int storeId = 0, bool saveChange = true)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (string.IsNullOrEmpty(key)) throw new ArgumentException("key must not empty or null");

            // lấy về tên thực sự của lớp nguyên bản, ko phải là tên của lớp proxy do EF tự tạo
            string keyGroup = _repository.GetUnproxiedEntityType(entity).Name;

            // đọc từ per request cache để lấy ra property nếu như nó đã có trong database. 
            // Khác với Nop, ở đây ta dùng so sánh phân biệt hoa thường cho key
            var prop = GetAttributesForEntity(entity.Id, keyGroup)
                .FirstOrDefault(p => p.StoreId == storeId && string.Equals(key, p.Key, StringComparison.InvariantCulture));

            string valueStr = CommonHelper.To<string>(value);
            if (prop != null)
            {
                // property đã tồn tại trước đó => cập nhật giá trị mới
                if (string.IsNullOrEmpty(valueStr)) // ko xài IsNullOrWhiteSpace ?
                    Delete(prop, saveChange, saveChange);
                else
                {
                    prop.Value = valueStr;
                    Update(prop, saveChange, saveChange);
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(valueStr))
                {
                    // ko có, tạo mới
                    Insert(new GenericAttribute
                    {
                        EntityId = entity.Id,
                        Key = key,
                        KeyGroup = keyGroup,
                        StoreId = storeId,
                        Value = valueStr
                    }, saveChange, saveChange);
                }
            }
        }

        //public virtual string GetKeyGroupName(BaseEntity entity)
        //{
        //    if (entity == null) return null;
        //    return _repository.GetUnproxiedEntityType(entity).Name;
        //}

        public virtual IList<GenericAttribute> GetAttributesForEntity(BaseEntity entity)
        {
            if (entity == null) return new List<GenericAttribute>();
            return GetAttributesForEntity(entity.Id, _repository.GetUnproxiedEntityType(entity).Name);
        }

        public TPropType GetAttribute<TPropType>(BaseEntity entity, string key, int storeId = 0)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            string keyGroup = _repository.GetUnproxiedEntityType(entity).Name;

            return _cacheWriter.GetAttributesForEntity<TPropType>(entity.Id, keyGroup, key, storeId, () => {
                var props = GetAttributesForEntity(entity.Id, keyGroup);
                //little hack here (only for unit testing). we should write ecpect-return rules in unit tests for such cases
                if (props == null) return default(TPropType);

                var prop = props.FirstOrDefault(p => p.StoreId == storeId
                    && string.Equals(p.Key, key, StringComparison.InvariantCulture));
                if (prop == null || string.IsNullOrEmpty(prop.Value)) return default(TPropType);

                // nhờ có phép chuyển kiểu này mà TPropType có thể là 1 class tự định nghĩa gồm có nhiều property phức tạp, chứ ko nhất
                // thiết phải là 1 kiểu cơ bản. Chỉ cần viết cho TPropType 1 TypeConverter phù hợp, đồng thời đăng ký bộ TypeConvert
                // đó trong CommonHelper.GetNopCustomTypeConverter()
                // Dữ liệu của đối tượng TPropType khi đó sẽ được chuyển thành chuỗi string lưu trong 1 row của GenericAttribute,
                // quá trình chuyển đổi qua lại đó sẽ do bộ TypeConverter chịu trách nhiệm
                return CommonHelper.To<TPropType>(prop.Value);
            });
        }

        public virtual void SaveChange(bool publishEvent = true)
        {
            if (_unitOfWork.SaveChanges() > 0 && publishEvent)
                _eventPublisher.EntityAllChange((GenericAttribute)null);
        }

        #endregion
    }
}
