using Research.Core.Domain.Localization;
using System.Collections.Generic;

namespace Research.Core.Interface.Service
{
    public partial interface ILanguageService
    {
        /// <summary>
        /// Lấy về tất cả các ngôn ngữ từ cache static/database và cache vào per request cache
        /// </summary>
        /// <param name="showHidden">Có lấy những language bị ẩn hay ko</param>
        /// <param name="storeId">Load những language được cho phép bởi storeid, =0 sẽ load lên tất cả các trường</param>
        /// <returns></returns>
        IList<Language> GetAllLanguages(bool getFromStaticCache = true, bool showHidden = false, int storeId = 0);

        /// <summary>
        /// Lấy về ngôn ngữ theo id từ cache static/hoặc trực tiếp từ database và cache vào per request cache
        /// </summary>
        Language GetById(int id, bool getFromStaticCache);

        void Delete(Language language);

        void Insert(Language language);

        void Update(Language entity);
    }
}
