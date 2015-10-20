using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Research.Core;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Events;

namespace Research.Services
{
    /// <summary>
    /// Định nghĩa lớp service chứa các chức năng cơ sở dùng chung
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BaseService<T> : IBaseService<T> where T: BaseEntity
    {
        protected readonly IRepository<T> _repository;
        protected readonly IUnitOfWork _unitOfWork;
        protected readonly IEventPublisher _eventPublisher;

        protected BaseService(IRepository<T> repository, IEventPublisher eventPublisher)
        {
            this._repository = repository;
            _unitOfWork = repository.UnitOfWork;
            _eventPublisher = eventPublisher;
        }

        /// <summary>
        /// saveChange: có save change xuống database hay ko
        /// publishEvent: có ném ra sự kiện hay ko, chỉ có ý nghĩa khi saveChange = true
        /// </summary>
        public virtual void Delete(T entity, bool saveChange, bool publishEvent)
        {
            _repository.Delete(entity);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityDeleted(entity);
            }
        }

        public virtual void Delete(T entity)
        {
            Delete(entity, true, true);
        }

        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            Delete(_repository.Table.Where(where).ToList(), true, true);
        }

        public virtual void Delete(IList<T> entities, bool saveChange, bool publishEvent)
        {
            if (entities == null || entities.Count == 0) return;
            _repository.Delete(entities);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityAllChange(entities[0]);
            }
        }

        public virtual void Delete(IList<T> entities)
        {
            Delete(entities, true, true);
        }

        public virtual T GetById(params object[] keyValues)
        {
            return _repository.GetById(keyValues);
        }

        public virtual T GetById(int id)
        {
            if (id <= 0) return null;
            return _repository.GetById(id);
        }

        public virtual IQueryable<T> GetAll()
        {
            return _repository.Table;
        }

        public virtual IQueryable<T> GetAllNoTracking()
        {
            return _repository.TableNoTracking;
        }

        public virtual void Insert(T entity, bool saveChange, bool publishEvent)
        {
            _repository.Insert(entity);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityInserted(entity);
            }
        }

        public virtual void Insert(T entity)
        {
            Insert(entity, true, true);
        }

        public virtual void Insert(IList<T> entities, bool saveChange, bool publishEvent)
        {
            if (entities == null || entities.Count == 0) return;
            _repository.Insert(entities);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityAllChange(entities[0]);
            }
        }

        public virtual void Insert(IList<T> entities)
        {
            Insert(entities, true, true);
        }

        public virtual void Update(T entity, bool saveChange, bool publishEvent)
        {
            _repository.Update(entity);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityUpdated(entity);
            }
        }

        public virtual void Update(T entity)
        {
            Update(entity, true, true);
        }

        public virtual void Update(IList<T> entities, bool saveChange, bool publishEvent)
        {
            if (entities == null || entities.Count == 0) return;
            _repository.Update(entities);
            if (saveChange)
            {
                _unitOfWork.SaveChanges();
                if (publishEvent && _eventPublisher != null) _eventPublisher.EntityAllChange(entities[0]);
            }
        }

        public virtual void Update(IList<T> entities)
        {
            Update(entities, true, true);
        }

        public virtual IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params Expression<Func<T, object>>[] includeProperties)
        {
            return _repository.IncludeProperties(anotherData, includeProperties);
        }

        public virtual IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params string[] includePaths)
        {
            return _repository.IncludeProperties(anotherData, includePaths);
        }
    }
}