using System.Data.Common;

namespace Research.Core.Data
{
    /// <summary>
    /// Đại diện cho 1 data provider, chẳng hạn như Sql Server Data provider, Sql CE Data provider ?
    /// DBContext sẽ làm việc với các provider này để có thể tương tác dữ liệu ?
    /// </summary>
    public partial interface IDataProvider
    {
        /// <summary>
        /// Initialize connection factory
        /// </summary>
        void InitConnectionFactory();

        /// <summary>
        /// Set database initializer
        /// </summary>
        void SetDatabaseInitializer();

        /// <summary>
        /// Initialize database
        /// </summary>
        void InitDatabase();

        /// <summary>
        /// A value indicating whether this data provider supports stored procedures
        /// </summary>
        bool StoredProceduredSupported { get; }

        /// <summary>
        /// Gets a support database parameter object (used by stored procedures)
        /// </summary>
        /// <returns>Parameter</returns>
        DbParameter GetParameter();
    }
}
