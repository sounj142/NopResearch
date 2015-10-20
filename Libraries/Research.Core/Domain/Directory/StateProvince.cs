using Research.Core.Domain.Localization;

namespace Research.Core.Domain.Directory
{
    /// <summary>
    /// Represents a state/province.
    /// 
    /// Với giả định rằng chúng ta sẽ chỉ nhập vào tỉnh thành của 1 số rất ít các quốc gia chúng ta quan tâm, và để trống số lượng lớn còn lại
    /// , nên lượng tỉnh thành sẽ ko quá nhiều, ta sẽ cache static tất cả ( ngược lại cần có cờ ở các country để chỉ rõ nên cache static
    /// StateProvince đi kèm với nó hay ko, nhưng phướng án cache 1 phần này khá phức tạp, ko cần dùng đến )
    /// 
    /// 
    /// Đối với khóa ngoại Country - StateProvince, ta thấy mối quan hệ này thường là từ country tìm lấy tất cả các state,
    /// cho nên ta sẽ cache static tất cả các StateProvince, sắp xếp theo country Id, và sau đó mới đến DisplayIndex. Như thế, bằng 1
    /// phép tìm kiếm nhị phân, ta sẽ tìm được vị trí bắt đầu của dãy các state thuộc country Id được yêu cầu nếu có, và lấy
    /// ra danh sách các StateProvince thuộc country, kết quả đc per request cache. Bằng cách đó, ta đảm bảo đc tính rời giữa 2 tập
    /// Country và StateProvince khi cache static
    /// </summary>
    public partial class StateProvince : BaseEntity, ILocalizedEntity
    {
        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int CountryId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the abbreviation
        /// </summary>
        public string Abbreviation { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entity is published
        /// </summary>
        public bool Published { get; set; }

        /// <summary>
        /// Gets or sets the display order
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public virtual Country Country { get; set; }

        public StateProvince MakeClone()
        {
            return new StateProvince
            {
                Abbreviation = this.Abbreviation,
                CountryId = this.CountryId,
                DisplayOrder = this.DisplayOrder,
                Id = this.Id,
                Name = this.Name,
                Published = this.Published,
                Country = null // ko copy country
            };
        }

        public static StateProvince MakeClone(StateProvince stateProvince)
        {
            if (stateProvince == null) return null;
            return stateProvince.MakeClone();
        }
    }
}
