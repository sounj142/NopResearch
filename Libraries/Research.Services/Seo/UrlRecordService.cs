using Research.Core;
using Research.Core.Domain.Localization;
using Research.Core.Domain.Seo;
using Research.Core.Interface.Data;
using Research.Core.Interface.Service;
using Research.Services.Caching.Models;
using Research.Services.Caching.Writer;
using Research.Services.Events;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Research.Services.Seo
{
    public class UrlRecordService : BaseService<UrlRecord>, IUrlRecordService
    {
        #region Fields, Property and Ctors

        private readonly IUrlRecordCacheWriter _cacheWriter;
        private readonly LocalizationSettings _localizationSettings;

        public UrlRecordService(IRepository<UrlRecord> repository,
            IUrlRecordCacheWriter cacheWriter,
            LocalizationSettings localizationSettings,
            IEventPublisher eventPublisher)
            : base(repository, eventPublisher)
        {
            _cacheWriter = cacheWriter;
            _localizationSettings = localizationSettings;
        }

        #endregion

        #region Utilities

        protected virtual UrlRecordCachePackage GetAllUrlRecordsCached()
        {
            return _cacheWriter.GetAll(() => {
                var result = new UrlRecordCachePackage ();
                result.Initialize(_repository.TableNoTracking);
                return result;
            });
        }

        /// <summary>
        /// Thay thế chuỗi slug bằng 1 chuỗi slug mới nếu kiểm tra phát hiện trùng slug. Slug thay thế có dạng slug-số
        /// Hàm đủ thông minh để thay thế chuỗi slug dạng
        /// sach-2 thành sach-3. Trả về 1 đối tượng khác null nếu quyết định rằng đã tìm được 1 chuỗi UrlRecord sẵn có phù hợp 
        /// mà ko phải ghi mới
        /// 
        /// Hàm được thiết kế riêng để gọi từ SaveSlug, ko nên gọi tùy tiện từ bên ngoài
        /// </summary>
        protected virtual UrlRecord ChangeConflictSlug<T>(T entity, ref string slug, int languageId)
            where T : BaseEntity, ISlugSupported
        {
            string entityName = typeof(T).Name;
            int entityId = entity.Id;

            int digit;
            string prefixSlug, newSlug;
            int index = slug.LastIndexOf('-');
            if (index > 0 && int.TryParse(slug.Substring(index + 1), out digit) && 0 <= digit && digit < int.MaxValue / 2)
            {
                digit++;
                prefixSlug = slug.Substring(0, index + 1);
            }
            else
            {
                digit = 1;
                prefixSlug = slug + "-";
            }

            // lấy ra danh sách các slug có thể trùng hiện đang có để so sánh và phát sinh ra slug mới
            var sameRecords = _repository.Table
                .Where(p => p.Slug.StartsWith(prefixSlug))
                .OrderBy(p => p.Slug)
                .ToList();
            // nếu trong danh sách có những url record cho chính entity thì sử dụng chúng, ưu tiên dùng record active
            // nếu ko có thì ưu tiên record có slug nhỏ nhất
            var tempRecords = sameRecords
                .Where(p => p.EntityId == entityId &&
                string.Equals(p.EntityName, entityName, StringComparison.InvariantCulture) && p.LanguageId == languageId)
                .OrderByDescending(p => p.IsActive)
                .ThenBy(p => p.Slug, StringComparer.InvariantCultureIgnoreCase)
                .FirstOrDefault();
            if (tempRecords != null)
            {
                // tìm thấy urlRecord phù hợp => sử dụng luôn
                return tempRecords;
            }
            // ko tìm thấy, sinh chuỗi slug mới
            index = 0;
            newSlug = prefixSlug + digit;
            while (index < sameRecords.Count)
            {
                int compare = string.Compare(newSlug, sameRecords[index].Slug, StringComparison.InvariantCultureIgnoreCase);
                if (compare < 0)
                    break;
                else if (compare > 0) index++;
                else
                {
                    index++;
                    digit++;
                    newSlug = prefixSlug + digit;
                }
            }
            slug = newSlug; // ok, thay thế chuỗi slug hiện tại bằng chuỗi slug mới
            return null;
        }

        #endregion

        #region Methods

        public UrlRecord GetBySlug(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;
            return _repository.Table.FirstOrDefault(p => p.Slug == slug);
        }

        public UrlRecordForCaching GetBySlugCached(string slug)
        {
            if (string.IsNullOrEmpty(slug)) return null;

            if(_localizationSettings.LoadAllUrlRecordsOnStartup)
            {
                var allRecords = GetAllUrlRecordsCached();
                return allRecords.Search(slug); // sở dĩ ở đây ko cache theo khóa slug là do thao tác này thường chỉ thực hiện 1 lần
                // khi giải quyết request đến, ko đủ giá trị để cache
            }
            else
            {
                return _cacheWriter.GetBySlug(slug, () => {
                    var urlRecord = _repository.TableNoTracking.FirstOrDefault(p => p.Slug == slug);
                    return UrlRecordForCaching.Transform(urlRecord);
                });
            }
        }

        public IPagedList<UrlRecord> GetAllUrlRecords(string slug, int pageIndex, int pageSize)
        {
            var query = _repository.Table;
            if (!string.IsNullOrWhiteSpace(slug)) 
                query = query.Where(p => p.Slug.Contains(slug));
            query = query.OrderBy(p => p.Slug);
            return new PagedList<UrlRecord>(query, pageIndex, pageSize);
        }

        public string GetActiveSlugCached(int entityId, string entityName, int languageId)
        {
            if (_localizationSettings.LoadAllUrlRecordsOnStartup)
            {
                string result;
                if (_cacheWriter.TryGetByKeys(entityId, entityName, languageId, out result)) return result;

                // vì thao tác lấy slug theo bộ 3 được thực hiện rất nhiều lần trong các thao tác sinh link trong view nên ta sẽ ưu
                // tiên cache chúng vào static cache để tăng tốc
                // Vì chúng ta sẽ cache cả null ( hack bằng cách chuyển thành ""), nên cần đảm bảo bộ 3 này đến từ database. Nếu bộ 3 
                // này đến từ 1 nguồn đểu thì có thể gây ga cache tràn bộ nhớ
                var allRecords = GetAllUrlRecordsCached();
                return _cacheWriter.GetByKeys(entityId, entityName, languageId, 
                    () => allRecords.Search(entityId, entityName, languageId));
            }
            else
            {
                return _cacheWriter.GetByKeys(entityId, entityName, languageId, () =>
                {
                    var slug = _repository
                        .TableNoTracking
                        .Where(p => p.IsActive && p.EntityId == entityId
                            && p.EntityName == entityName && p.LanguageId == languageId)
                        .OrderByDescending(p => p.Id)
                        .Select(p => p.Slug)
                        .FirstOrDefault();
                    return slug ?? string.Empty;
                });
            }
        }

        private void SaveOrDeleteUnuseUrlRecord(UrlRecord record, DateTime currentDate)
        {
            if (_localizationSettings.MinHoursForUrlSlugEverlasting > 0 && record.CreateDate.HasValue &&
                                record.CreateDate.Value.AddHours(_localizationSettings.MinHoursForUrlSlugEverlasting) > currentDate)
                _repository.Delete(record);
            else
            {
                record.IsActive = false;
                _repository.Update(record);
            }
        }

        public UrlRecord SaveSlug<T>(T entity, string slug, int languageId, bool autoAddTailDigit = false)
            where T : BaseEntity, ISlugSupported
        {
            if (entity == null) throw new ArgumentNullException("entity");
            if (string.IsNullOrWhiteSpace(slug)) return null; // ko lưu chuỗi slug rỗng
            slug = slug.Trim();

            string entityName = typeof(T).Name;
            int entityId = entity.Id;
            DateTime currentDate = DateTime.UtcNow;

            // tìm những đối tượng entity KHÁC đang dùng cùng 1 chuỗi slug nếu có 
            // kiểm tra lại câu truy vấn chỗ này
            var conflictRecords = _repository.Table
                .Where(p => p.Slug == slug &&
                    (p.EntityId != entityId || p.EntityName != entityName || p.LanguageId != languageId))
                .FirstOrDefault();
            if (conflictRecords != null)
            {
                if (!autoAddTailDigit)
                    return null; // nếu xảy ra trùng mà lại ko được phép tự ý sửa lại chuỗi slug thì sẽ từ chối ghi để tránh sai sót dữ liệu

                // thay thế chuỗi slug bằng chuỗi mới, nếu tìm thấy 1 UrlRecord có slug ở định dạng phù hợp thì sử dụng luôn
                var result = ChangeConflictSlug(entity, ref slug, languageId);
                if (result != null)
                {
                    if (!result.IsActive)
                    {
                        // liệu có những record khác đang active hay ko ?
                        var activeRecords = _repository.Table
                            .Where(p => p.EntityId == entityId && p.EntityName == entityName && p.LanguageId == languageId && p.IsActive)
                            .ToList();
                        foreach (var p in activeRecords)
                            SaveOrDeleteUnuseUrlRecord(p, currentDate);

                        result.IsActive = true;
                        Update(result); // đã clear cache
                    }

                    return result;
                }
            }

            // ok, đến đây có thể đảm bảo là slug ko bị trùng ( vấn đề truy vấn đồng thời gì gì đó ko tính nhá =]] )


            // tìm kiếm tất cả các UrlRecord đại diện cho entity nếu có
            var allRecord = _repository.Table
                .Where(p => p.EntityId == entityId && p.EntityName == entityName && p.LanguageId == languageId)
                .OrderByDescending(p => p.Id)
                .ToList();
            // lấy ra trường đang active
            var activeRecord = allRecord.FirstOrDefault(p => p.IsActive);
            
            // nếu không có 1 trường urlrecord nào đang active đại diện cho entity 
            if (activeRecord == null)
            {
                // tìm xem có 1 urlRecord nào unactive mà đang sử dụng chính chuỗi slug được yêu cầu hay ko, nếu có thì active nó lên
                var oldUrlRecord = allRecord.FirstOrDefault(p => string.Equals(slug, p.Slug, StringComparison.InvariantCultureIgnoreCase));
                if (oldUrlRecord != null)
                {
                    oldUrlRecord.IsActive = true;
                    Update(oldUrlRecord);
                    return oldUrlRecord;
                }
                // Ko có, tạo ra 1 urlRecord mới và lưu vào database
                var result = new UrlRecord
                {
                    CreateDate = currentDate,
                    EntityId = entityId,
                    EntityName = entityName,
                    IsActive = true,
                    LanguageId = languageId,
                    Slug = slug
                };
                Insert(result);
                return result;
            }else
            {
                //Nếu có 1 Url Record đang Active

                // nếu nó đang dùng chính chuỗi slug thì ko có gì để làm cả
                if (string.Equals(slug, activeRecord.Slug, StringComparison.InvariantCultureIgnoreCase)) 
                    return activeRecord;

                // Ngược lại thì xem xét khả năng thay thế hay ghi mới active record tùy theo cấu hình

                // tìm xem có 1 urlRecord nào mà đang sử dụng chính chuỗi slug được yêu cầu hay ko, nếu có thì active nó lên sử dụng
                var oldUrlRecord = allRecord.FirstOrDefault(p => string.Equals(slug, p.Slug, StringComparison.InvariantCultureIgnoreCase));
                if (oldUrlRecord != null)
                {
                    SaveOrDeleteUnuseUrlRecord(activeRecord, currentDate); // Xóa hoặc ẩn current record
                    oldUrlRecord.IsActive = true;
                    Update(oldUrlRecord);
                    return oldUrlRecord;
                }

                // 1 trong 2 phương án: Ghi đè lên activeRecord hoặc ẩn nó đi và ghi 1 trường mới
                if (_localizationSettings.MinHoursForUrlSlugEverlasting > 0 && activeRecord.CreateDate.HasValue &&
                                activeRecord.CreateDate.Value.AddHours(_localizationSettings.MinHoursForUrlSlugEverlasting) > currentDate)
                {
                    activeRecord.CreateDate = currentDate;
                    activeRecord.LastUnactive = null;
                    activeRecord.Slug = slug;
                    Update(activeRecord);
                    return activeRecord;
                }
                else
                {
                    activeRecord.IsActive = false;
                    activeRecord.LastUnactive = currentDate;
                    _repository.Update(activeRecord);

                    var result = new UrlRecord
                    {
                        CreateDate = currentDate,
                        EntityId = entityId,
                        EntityName = entityName,
                        IsActive = true,
                        LanguageId = languageId,
                        Slug = slug
                    };

                    Insert(result);
                    return result;
                }
            }
        }

        #endregion
    }
}
