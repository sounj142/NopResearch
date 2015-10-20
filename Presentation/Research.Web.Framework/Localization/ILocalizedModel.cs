using System.Collections.Generic;

namespace Research.Web.Framework.Localization
{
    /// <summary>
    /// ?????
    /// </summary>
    public interface ILocalizedModel
    {
    }

    /// <summary>
    /// Tức là 1 cái ILocalizedModel[T] cũng là 1 ILocalizedModel, đồng thời có thêm đặc điểm của riêng nó
    /// => Mọi ILocalizedModel[T] đều là ILocalizedModel, và có thể ép kiểu nó về ILocalizedModel hay coi nó là 1 đối tượng ILocalizedModel
    /// </summary>
    public interface ILocalizedModel<TLocalizedModel>: ILocalizedModel
    {
        /// <summary>
        /// 1 danh sách các đối tượng TLocalizedModel ?
        /// </summary>
        IList<TLocalizedModel> Locales { get; set; }
    }
}
