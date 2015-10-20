using System;
using Research.Core;
using Research.Core.Data;

namespace Research.Data
{
    public partial class EfDataProviderManager : BaseDataProviderManager
    {
        public EfDataProviderManager(DataSettings settings) // lấy settings qua DI
            : base(settings)
        { }

        /// <summary>
        /// Hàm tạo và rả về IDataProvider phù hợp với cấu hình DataSettings
        /// </summary>
        public override IDataProvider LoadDataProvider()
        {
            var providerName = Settings.DataProvider;
            if(string.IsNullOrWhiteSpace(providerName)) throw new ResearchException("Data Settings doesn't contain a providerName");

            // đoạn code quyết định tạo IDataProvider ở đây
            switch(providerName.ToLowerInvariant())
            {
                case "sqlserver":
                    return new SqlServerDataProvider();
                case "sqlce":
                    return new SqlCeDataProvider();
            }
            throw new ResearchException(string.Format("Not supported dataprovider name: {0}", providerName));
        }
    }
}
