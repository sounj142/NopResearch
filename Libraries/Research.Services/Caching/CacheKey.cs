
namespace Research.Services.Caching
{
    /// <summary>
    /// Lớp này là "trại tập trung" nơi sẽ khai báo tất cả các mẫu key cache được dùng trong Research.Services. Việc khai báo này giúp
    /// tránh trùng key và dễ quản lý so với cách khai báo rời rạc ở những nơi cần dùng
    /// </summary>
    public static class CacheKey
    {
        #region Localization
        /// <summary>
        /// Key dùng để cache tất cả các LocaleStringResource theo từng nhóm language id ?
        /// {0} : LanguageId
        /// </summary>
        public const string LOCALSTRINGRESOURCES_ALL_KEY = "Nop.lsr.all-{0}"; // dịch: Nop - LocaleStringResource - tất cả - id ngôn ngữ

        /// <summary>
        /// Cache cụ thể từng LocaleStringResource theo cặp khóa key - language id
        /// {0} : Language id
        /// {1}: resource key ( ResourceName )
        /// </summary>
        public const string LOCALSTRINGRESOURCES_BY_RESOURCENAME_KEY = "Nop.lsr.{0}-{1}";

        /// <summary>
        /// Tiết đầu ngữ của key được dùng làm điều kiện để xóa cache cho LocaleStringResource
        /// </summary>
        public const string LOCALSTRINGRESOURCES_PATTERN_KEY = "Nop.lsr.";

        #endregion

        #region Language

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : get from static cache
        /// </remarks>
        public const string LANGUAGES_BY_ID_KEY = "Nop.language.id-{0}-{1}"; // key dùng cho per request cache

        public const string LANGUAGES_ALL_HASHIDDEN_KEY_PATTERN = "Nop.language.all-"; // key dùng để clear riêng cho StoreMapping

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : Store Id
        /// </remarks>
        public const string LANGUAGES_ALL_HASHIDDEN_KEY = LANGUAGES_ALL_HASHIDDEN_KEY_PATTERN + "{0}-{1}-{2}"; // key dùng cho per request cache, dựa trên dữ liệu all static cache


        public const string LANGUAGES_ALL_KEY = "Nop.language.all"; // key duy nhất dùng cho static cache toàn bộ các language
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string LANGUAGES_PATTERN_KEY = "Nop.language.";

        #endregion

        #region Settings

        /// <summary>
        /// Key for caching
        /// </summary>
        public const string SETTINGS_ALL_KEY = "Nop.setting.all";

        /// <summary>
        /// Key để cache từng loại setting riêng lẻ theo type và storeId
        /// {0} : Store Id
        /// {1} : type assembly full name
        /// </summary>
        public const string SETTINGS_BY_TYPE = "Nop.setting.type.{0}.{1}"; 
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string SETTINGS_PATTERN_KEY = "Nop.setting.";

        #endregion

        #region UrlRecord

        /// <summary>
        /// Key để cache theo bộ 3 khóa entityId - entityName - languageId
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// {2} : language ID
        /// </remarks>
        public const string URLRECORD_ACTIVE_BY_ID_NAME_LANGUAGE_KEY = "Nop.urlrecord.active.id-name-language-{0}-{1}-{2}";

        /// <summary>
        /// Key để cache trong trường hợp cache tất cả. Khi đó dữ liệu trong đối tượng sẽ là 2 từ điển phục vụ
        /// việc tra theo 2 hướng, theo slug và theo bộ 3 key
        /// </summary>
        public const string URLRECORD_ALL_KEY = "Nop.urlrecord.all";

        /// <summary>
        /// Key để cache theo slug
        /// </summary>
        /// <remarks>
        /// {0} : slug
        /// </remarks>
        public const string URLRECORD_BY_SLUG_KEY = "Nop.urlrecord.active.slug-{0}";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string URLRECORD_PATTERN_KEY = "Nop.urlrecord.";

        #endregion

        #region LocalizedEntity

        /// <summary>
        /// Key cache giá trị theo bộ tứ
        /// </summary>
        /// <remarks>
        /// {0} : language ID
        /// {1} : entity ID
        /// {2} : locale key group
        /// {3} : locale key
        /// </remarks>
        public const string LOCALIZEDPROPERTY_KEY = "Nop.localizedproperty.value-{0}-{1}-{2}-{3}";
        /// <summary>
        /// Key cache tất cả
        /// </summary>
        public const string LOCALIZEDPROPERTY_ALL_KEY = "Nop.localizedproperty.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string LOCALIZEDPROPERTY_PATTERN_KEY = "Nop.localizedproperty.";

        #endregion

        #region Store

        /// <summary>
        /// Cache tất cả các Store
        /// </summary>
        public const string STORES_ALL_KEY = "Nop.stores.all";

        /// <summary>
        /// Cache riêng từng store theo id, dùng để cache perRequest theo id
        /// </summary>
        /// <remarks>
        /// {0} : store ID
        /// </remarks>
        public const string STORES_BY_ID_KEY = "Nop.stores.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string STORES_PATTERN_KEY = "Nop.stores.";

        #endregion

        #region LocalizedEntity

        /// <summary>
        /// Key cho phép cache theo cặp khóa entityId, entityName, chứa danh sách các store mà đối tượng entity cho phép truy cập
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        public const string STOREMAPPING_BY_ENTITYID_NAME_KEY = "Nop.storemapping.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string STOREMAPPING_PATTERN_KEY = "Nop.storemapping.";

        #endregion

        #region LocalizedEntity

        /// <summary>
        /// Key để có thể cache theo bộ khóa entity id - loại entity - property name - store id
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : key group
        /// {2} : property name
        /// {3} : store id
        /// </remarks>
        public const string GENERICATTRIBUTE_BYENTITY_KEY = "Nop.genericattribute.{0}-{1}-{2}-{3}";

        /// <summary>
        /// Key để có thể cache theo cặp khóa entity id - loại entity ( như vậy trong cache sẽ có dữ liệu là 1 list các cặp 
        /// [property name - store id ] --- value
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : key group
        /// </remarks>
        public const string GENERICATTRIBUTE_KEY = "Nop.genericattribute.{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string GENERICATTRIBUTE_PATTERN_KEY = "Nop.genericattribute.";

        #endregion

        #region Currency

        /// <summary>
        /// Cache per request theo Id
        /// </summary>
        /// <remarks>
        /// {0} : currency ID
        /// {1} : data from cache static ?
        /// </remarks>
        public const string CURRENCIES_BY_ID_KEY = "Nop.currency.id-{0}-{1}";

        /// <summary>
        /// Cache per request theo currencyCode
        /// </summary>
        /// <remarks>
        /// {0} : currency Code
        /// </remarks>
        public const string CURRENCIES_BY_CODE = "Nop.currency.code-{0}-{1}";

        public const string CURRENCIES_ALL_KEY_WITH_HIDDEN_PATTERN = "Nop.currency.all-";

        /// <summary>
        /// Cache per request theo tình trạng hidden.
        /// Dữ liệu cache ở key này có dính dáng đến store mapping nên mỗi khi có thay đổi trên store mapping cần clear key này
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// {1} : Store Id
        /// </remarks>
        public const string CURRENCIES_ALL_KEY_WITH_HIDDEN = CURRENCIES_ALL_KEY_WITH_HIDDEN_PATTERN + "{0}-{1}-{2}";
        
        /// <summary>
        /// Cache static tất cả các currency
        /// </summary>
        public const string CURRENCIES_ALL_KEY = "Nop.currency.all";

        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string CURRENCIES_PATTERN_KEY = "Nop.currency.";

        #endregion

        #region Customer

        public const string CUSTOMERS_BY_ID = "Nop.customer.id-{0}";

        public const string CUSTOMERS_BY_GUID = "Nop.customer.guid-{0}";

        public const string CUSTOMERS_BY_EMAIL = "Nop.customer.email-{0}";

        public const string CUSTOMERS_BY_SYSTEMNAME = "Nop.customer.systemname-{0}";

        public const string CUSTOMERS_BY_USERNAME = "Nop.customer.username-{0}";

        public const string CUSTOMERS_PATTERN_KEY = "Nop.customer.";

        #endregion

        #region CustomerRole

        
        public const string CUSTOMERROLES_BY_ID = "Nop.customerrole.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : show hidden records?
        /// </remarks>
        public const string CUSTOMERROLES_ALL_WITH_HIDDEN_KEY = "Nop.customerrole.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : system name
        /// </remarks>
        public const string CUSTOMERROLES_BY_SYSTEMNAME_KEY = "Nop.customerrole.systemname-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string CUSTOMERROLES_PATTERN_KEY = "Nop.customerrole.";

        #endregion

        #region AclRecord

        /// <summary>
        /// Cache danh sách các CustomerRoleId được cấp quyền riêng để tương tác với entity ( entity ID, entity name)
        /// </summary>
        /// <remarks>
        /// {0} : entity ID
        /// {1} : entity name
        /// </remarks>
        public const string ACLRECORD_BY_ENTITYID_NAME_KEY = "Nop.aclrecord.entityid-name-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string ACLRECORD_PATTERN_KEY = "Nop.aclrecord.";

        #endregion

        #region PermissionRecord

        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer role ID
        /// {1} : permission system name
        /// </remarks>
        public const string PERMISSIONS_ALLOWED_KEY = "Nop.permission.allowed-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string PERMISSIONS_PATTERN_KEY = "Nop.permission.";

        #endregion

        #region CustomerAttribute, CustomerAttributeValue

        /// <summary>
        /// Key for caching
        /// </summary>
        public const string CUSTOMERATTRIBUTES_ALL_KEY = "Nop.customerattribute.all";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute ID
        /// </remarks>
        public const string CUSTOMERATTRIBUTES_BY_ID_KEY = "Nop.customerattribute.id-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute ID
        /// </remarks>
        public const string CUSTOMERATTRIBUTEVALUES_ALL_KEY = "Nop.customerattributevalue.all-{0}";
        /// <summary>
        /// Key for caching
        /// </summary>
        /// <remarks>
        /// {0} : customer attribute value ID
        /// </remarks>
        public const string CUSTOMERATTRIBUTEVALUES_BY_ID_KEY = "Nop.customerattributevalue.id-{0}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string CUSTOMERATTRIBUTES_PATTERN_KEY = "Nop.customerattribute.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string CUSTOMERATTRIBUTEVALUES_PATTERN_KEY = "Nop.customerattributevalue.";

        #endregion

        #region StateProvince
        /// <summary>
        /// id - getFromStaticCache
        /// </summary>
        public const string STATEPROVINCES_BY_ID = "Nop.stateprovince.id-{0}-{1}";

        /// <summary>
        /// countryId - autoOrder - hidden - getFromStaticCache
        /// </summary>
        public const string STATEPROVINCES_BY_COUNTRYID_WITH_HIDDEN_KEY = "Nop.stateprovince.all-{0}-{1}-{2}-{3}";

        /// <summary>
        /// cache all
        /// </summary>
        public const string STATEPROVINCES_ALL_KEY = "Nop.stateprovince.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string STATEPROVINCES_PATTERN_KEY = "Nop.stateprovince.";

        #endregion

        #region StateProvince

        public const string COUNTRIES_ALL_BY_HIDDEN_KEY = "Nop.country.all-{0}-{1}-{2}";

        public const string COUNTRIES_BY_ID = "Nop.country.id-{0}-{1}";

        public const string COUNTRIES_ALL_KEY = "Nop.country.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string COUNTRIES_PATTERN_KEY = "Nop.country.";

        #endregion

        #region MeasureDimension, MeasureWeight

        /// <summary>
        /// Key for caching
        /// </summary>
        public const string MEASUREDIMENSIONS_ALL_KEY = "Nop.measuredimension.all";

        public const string MEASUREDIMENSIONS_ALL_KEY_PERREQUEST = "Nop.measuredimension.all-{0}";
        
        public const string MEASUREDIMENSIONS_BY_ID_KEY = "Nop.measuredimension.id-{0}-{1}";

        public const string MEASUREDIMENSIONS_BY_SYSTEM_KEYWORD = "Nop.measuredimension.syskey-{0}-{1}";
        /// <summary>
        /// Key for caching
        /// </summary>
        public const string MEASUREWEIGHTS_ALL_KEY = "Nop.measureweight.all";

        public const string MEASUREWEIGHTS_ALL_KEY_PERREQUEST = "Nop.measureweight.all-{0}";
        
        public const string MEASUREWEIGHTS_BY_ID_KEY = "Nop.measureweight.id-{0}-{1}";

        public const string MEASUREWEIGHTS_BY_SYSTEM_KEYWORD = "Nop.measureweight.syskey-{0}-{1}";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string MEASUREDIMENSIONS_PATTERN_KEY = "Nop.measuredimension.";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string MEASUREWEIGHTS_PATTERN_KEY = "Nop.measureweight.";

        #endregion

        #region ActivityLogType

        /// <summary>
        /// Key for caching
        /// </summary>
        public const string ACTIVITYTYPE_ALL_KEY = "Nop.activitytype.all";
        /// <summary>
        /// Key pattern to clear cache
        /// </summary>
        public const string ACTIVITYTYPE_PATTERN_KEY = "Nop.activitytype.";

        #endregion
    }
}