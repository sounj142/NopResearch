using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using Research.Core;
using System.Diagnostics;
using System.Linq.Expressions;
using Research.Core.Interface.Data;
using System.Data.Entity.Core.Objects;

namespace Research.Data
{
    /// <summary>
    /// IRepository cài đặt bằng EF
    /// </summary>
    public partial class EfRepository<T> : IRepository<T> where T:BaseEntity
    {
        #region Fields and Ctors

        /// <summary>
        /// Đối tượng đại diện cho DbContext, cho phép thông qua nó truy cập CSDL
        /// </summary>
        protected readonly IDbContext _context;
        protected readonly IUnitOfWork _unitOfWork;

        /// <summary>
        /// Đại diện cho bảng dữ liệu "T" 
        /// </summary>
        protected IDbSet<T> _entities; 

        public EfRepository(IUnitOfWork unitOfWork) 
        {
            this._unitOfWork = unitOfWork;
            _context = (IDbContext)unitOfWork.GetIDbContext();
        }

        #endregion

        #region Properties

        public virtual IDbSet<T> Entities
        {
            get
            {
                if (_entities == null) _entities = _context.Set<T>(); // lazy loading
                return _entities;
            }
        }

        public IQueryable<T> Table
        {
            get { return Entities; }
        }

        public IQueryable<T> TableNoTracking
        {
            get { return Entities.AsNoTracking(); }
        }

        public IUnitOfWork UnitOfWork 
        {
            get { return _unitOfWork; }
        }

        #endregion

        #region Methods

        public virtual T GetById(params object[] keyValues)
        {
            //see some suggested performance optimization (not tested)
            //http://stackoverflow.com/questions/11686225/dbset-find-method-ridiculously-slow-compared-to-singleordefault-on-id/11688189#comment34876113_11688189
            return Entities.Find(keyValues);
        }

        /// <summary>
        /// Ghi chú: Repository trong Nop sẽ insert, update, delete ngay lập tức ngay khi phương thức tương ứng được gọi. Nó sẽ ko cư
        /// xử theo mẫu UnitOfWork, trong đó thao tác trên sẽ chỉ lưu dữ liệu tạm vào ngữ cảnh EF, để rồi thao tác SaveChange sẽ tạo
        /// ra 1 transaction, thực hiện tất cả các thao tác đã lưu trữ trong 1 đơn nguyên transaction duy nhất
        /// </summary>
        public virtual T Insert(T entity)
        {
            T result = null;
            ProcessRepositoryAction(entity, () =>
            {
                result = Entities.Add(entity);
            });
            return result;
        }

        public virtual void Insert(IEnumerable<T> entities)
        {
            ProcessRepositoryAction(entities, () =>
            {
                var _entities = Entities;
                foreach (var entity in entities) _entities.Add(entity);
            });
        }

        public virtual void Update(T entity)
        {
            ProcessRepositoryAction(entity, () =>
            {
                //if (_context.Entry(entity).State == EntityState.Detached) 
                Entities.Attach(entity);
                _context.Entry(entity).State = EntityState.Modified;
            });
        }

        public virtual void Update(IEnumerable<T> entities)
        {
            ProcessRepositoryAction(entities, () =>
            {
                foreach (var entity in entities)
                {
                    //if (_context.Entry(entity).State == EntityState.Detached) 
                    Entities.Attach(entity);
                    _context.Entry(entity).State = EntityState.Modified;
                }
            });
        }

        public virtual void Delete(T entity)
        {
            ProcessRepositoryAction(entity, () =>
            {
                var _entities = Entities;
                //if (_context.Entry(entity).State == EntityState.Detached) 
                _entities.Attach(entity);
                _entities.Remove(entity);
            });
        }

        public virtual void Delete(IEnumerable<T> entities)
        {
            ProcessRepositoryAction(entities, () =>
            {
                var _entities = Entities;
                foreach (var entity in entities)
                {
                    //if (_context.Entry(entity).State == EntityState.Detached) 
                    _entities.Attach(entity);
                    _context.Entry(entity).State = EntityState.Deleted;
                    //_entities.Remove(entity);
                }
            });
        }

        protected virtual void ProcessRepositoryAction(object entity, Action actionMethod)
        {
            if (entity == null) throw new ArgumentNullException("entity");
            try
            {
                actionMethod();
            }
            catch (DbEntityValidationException ex)
            {
                string msg = string.Empty;

                foreach (var validationErrors in ex.EntityValidationErrors)
                    foreach (var validationError in validationErrors.ValidationErrors)
                        msg += string.Format("Property: {0} Error: {1}{2}", validationError.PropertyName,
                            validationError.ErrorMessage, Environment.NewLine);
                var fail = new Exception(msg, ex);
                // Debug.WriteLine(fail.Message, fail);
                throw fail;
            }
        }

        public virtual IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> entities = anotherData ?? Entities;
            return entities.IncludeProperties(includeProperties);
        }

        public virtual IQueryable<T> IncludeProperties(IQueryable<T> anotherData, params string[] includePaths)
        {
            IQueryable<T> entities = anotherData ?? Entities;
            return entities.IncludeProperties(includePaths);
        }
        
        #endregion

        public virtual Type GetUnproxiedEntityType(BaseEntity entity)
        {
            return ObjectContext.GetObjectType(entity.GetType());
        }
    }
}