using System;
using System.Collections.Generic;
using System.Linq;
using Research.Core.Domain.Directory;
using Research.Core.Interface.Service;
using Research.Core.Interface.Data;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using Research.Core;

namespace Research.Services.Directory
{
    public partial class MeasureService : BaseService<MeasureDimension>, IMeasureService
    {
        #region Field, ctors, properties

        private readonly IRepository<MeasureWeight> _measureWeightRepository;
        private readonly IMeasureCacheWriter _cacheWriter;
        private readonly MeasureSettings _measureSettings;

        // khác Nop:
        // hệ thống sử dụng cơ chế cache giá trị của đơn vị đo mặc định vào 1 field của class nhằm tăng tốc độ tính toán
        // để đảm bảo hệ thống hoạt động đúng, MeasureService cần phải được đảm bảo có vòng đời InstancePerLifetimeScope
        private MeasureDimension _baseDimension;
        private MeasureWeight _baseWeight;

        private MeasureDimension BaseDimension
        {
            get
            {
                return _baseDimension ?? (_baseDimension = GetMeasureDimensionById(_measureSettings.BaseDimensionId));
            }
        }

        private MeasureWeight BaseWeight
        {
            get
            {
                return _baseWeight ?? (_baseWeight = GetMeasureWeightById(_measureSettings.BaseWeightId));
            }
        }

        public MeasureService(IRepository<MeasureDimension> repository,
            IEventPublisher eventPublisher,
            IRepository<MeasureWeight> measureWeightRepository,
            IMeasureCacheWriter cacheWriter,
            MeasureSettings measureSettings)
            :base(repository, eventPublisher)
        {
            _measureWeightRepository = measureWeightRepository;
            _cacheWriter = cacheWriter;
            _measureSettings = measureSettings;
        }

        #endregion

        #region Utilities

        protected virtual IList<MeasureDimension> GetAllMeasureDimensionsCached()
        {
            return _cacheWriter.GetAllMeasureDimensionsCached(() =>
                _repository.TableNoTracking.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name).ToList());
        }

        protected virtual IList<MeasureWeight> GetAllMeasureWeightsCached()
        {
            return _cacheWriter.GetAllMeasureWeightsCached(() =>
                  _measureWeightRepository.TableNoTracking.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name).ToList());
        }

        #endregion

        #region Methods

        #region MeasureDimension

        public MeasureDimension GetMeasureDimensionById(int id, bool getFromStaticCache = true)
        {
            if (id <= 0) return null;

            return _cacheWriter.GetMeasureDimensionById(id, getFromStaticCache, () =>
            {
                return getFromStaticCache
                    ? MeasureDimension.MakeClone(GetAllMeasureDimensionsCached().FirstOrDefault(p => p.Id == id))
                    : _repository.GetById(id);
            });
        }

        public MeasureDimension GetMeasureDimensionBySystemKeyword(string systemKeyword, bool getFromStaticCache = true)
        {
            if (string.IsNullOrEmpty(systemKeyword)) return null;
            return _cacheWriter.GetMeasureDimensionBySystemKeyword(systemKeyword, getFromStaticCache, () =>
            {
                return getFromStaticCache
                        ? MeasureDimension.MakeClone(GetAllMeasureDimensionsCached()
                        .FirstOrDefault(p => systemKeyword.Equals(p.SystemKeyword, StringComparison.InvariantCultureIgnoreCase)))
                        : _repository.Table.FirstOrDefault(p => p.SystemKeyword == systemKeyword);
            });
        }

        public IList<MeasureDimension> GetAllMeasureDimensions(bool getFromStaticCache = true)
        {
            return _cacheWriter.GetAllMeasureDimensions(getFromStaticCache, () => {
                return getFromStaticCache
                    ? GetAllMeasureDimensionsCached().Select(p => p.MakeClone()).ToList()
                    : _repository.Table.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name).ToList();
            });
        }

        public decimal ConvertDimension(decimal quantity, MeasureDimension sourceMeasureDimension,
            MeasureDimension targetMeasureDimension, bool round = true, int digitNum = 2)
        {
            if (sourceMeasureDimension.Id != targetMeasureDimension.Id && quantity != decimal.Zero)
            {
                quantity = ConvertToPrimaryMeasureDimension(quantity, sourceMeasureDimension);
                quantity = ConvertFromPrimaryMeasureDimension(quantity, targetMeasureDimension);
            }
            return round ? decimal.Round(quantity, digitNum) : quantity;
        }

        public decimal ConvertToPrimaryMeasureDimension(decimal quantity, MeasureDimension sourceMeasureDimension)
        {
            var baseDimension = this.BaseDimension;
            if (sourceMeasureDimension.Id == baseDimension.Id || quantity == decimal.Zero) return quantity;
            if (sourceMeasureDimension.Ratio == decimal.Zero)
                throw new ResearchException(string.Format("Exchange ratio not set for dimension [{0}]", sourceMeasureDimension.Name));

            return quantity / sourceMeasureDimension.Ratio;
        }

        public decimal ConvertFromPrimaryMeasureDimension(decimal quantity, MeasureDimension targetMeasureDimension)
        {
            var baseDimension = this.BaseDimension;
            if (targetMeasureDimension.Id == baseDimension.Id || quantity == decimal.Zero) return quantity;
            if (targetMeasureDimension.Ratio == decimal.Zero)
                throw new ResearchException(string.Format("Exchange ratio not set for dimension [{0}]", targetMeasureDimension.Name));

            return quantity * targetMeasureDimension.Ratio;
        }

        #endregion

        #region MeasureWeight

        public void Delete(MeasureWeight measureWeight)
        {
            _measureWeightRepository.Delete(measureWeight);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityDeleted(measureWeight);
        }

        public MeasureWeight GetMeasureWeightById(int id, bool getFromStaticCache = true)
        {
            if (id <= 0) return null;

            return _cacheWriter.GetMeasureWeightById(id, getFromStaticCache, () =>
            {
                return getFromStaticCache
                    ? MeasureWeight.MakeClone(GetAllMeasureWeightsCached().FirstOrDefault(p => p.Id == id))
                    : _measureWeightRepository.GetById(id);
            });
        }

        public MeasureWeight GetMeasureWeightBySystemKeyword(string systemKeyword, bool getFromStaticCache = true)
        {
            if (string.IsNullOrEmpty(systemKeyword)) return null;
            return _cacheWriter.GetMeasureWeightBySystemKeyword(systemKeyword, getFromStaticCache, () =>
            {
                return getFromStaticCache
                        ? MeasureWeight.MakeClone(GetAllMeasureWeightsCached()
                        .FirstOrDefault(p => systemKeyword.Equals(p.SystemKeyword, StringComparison.InvariantCultureIgnoreCase)))
                        : _measureWeightRepository.Table.FirstOrDefault(p => p.SystemKeyword == systemKeyword);
            });
        }

        public IList<MeasureWeight> GetAllMeasureWeights(bool getFromStaticCache = true)
        {
            return _cacheWriter.GetAllMeasureWeights(getFromStaticCache, () =>
            {
                return getFromStaticCache
                    ? GetAllMeasureWeightsCached().Select(p => p.MakeClone()).ToList()
                    : _measureWeightRepository.Table.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name).ToList();
            });
        }

        public void Insert(MeasureWeight entity)
        {
            _measureWeightRepository.Insert(entity);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityInserted(entity);
        }

        public void Update(MeasureWeight entity)
        {
            _measureWeightRepository.Update(entity);
            _unitOfWork.SaveChanges();
            _eventPublisher.EntityUpdated(entity);
        }

        public decimal ConvertWeight(decimal quantity, MeasureWeight sourceMeasureWeight,
            MeasureWeight targetMeasureWeight, bool round = true, int digitNum = 2)
        {
            if (sourceMeasureWeight.Id != targetMeasureWeight.Id && quantity != decimal.Zero)
            {
                quantity = ConvertToPrimaryMeasureWeight(quantity, sourceMeasureWeight);
                quantity = ConvertFromPrimaryMeasureWeight(quantity, targetMeasureWeight);
            }
            return round ? decimal.Round(quantity, digitNum) : quantity;
        }

        public decimal ConvertToPrimaryMeasureWeight(decimal quantity, MeasureWeight sourceMeasureWeight)
        {
            var baseWeight = this.BaseWeight;
            if (sourceMeasureWeight.Id == baseWeight.Id || quantity == decimal.Zero) return quantity;
            if (sourceMeasureWeight.Ratio == decimal.Zero)
                throw new ResearchException(string.Format("Exchange ratio not set for weight [{0}]", sourceMeasureWeight.Name));

            return quantity / sourceMeasureWeight.Ratio;
        }

        public decimal ConvertFromPrimaryMeasureWeight(decimal quantity, MeasureWeight targetMeasureWeight)
        {
            var baseWeight = this.BaseWeight;
            if (targetMeasureWeight.Id == baseWeight.Id || quantity == decimal.Zero) return quantity;
            if (targetMeasureWeight.Ratio == decimal.Zero)
                throw new ResearchException(string.Format("Exchange ratio not set for Weight [{0}]", targetMeasureWeight.Name));

            return quantity * targetMeasureWeight.Ratio;
        }

        #endregion

        #endregion
    }
}
