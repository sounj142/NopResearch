using System.Collections.Generic;

namespace Research.Core.Domain
{
    /// <summary>
    /// Lớp dùng làm lớp cơ sở chứa thông tin kết quả trả về cho những thao tác trên domain model
    /// </summary>
    public class BaseResultViewModel
    {
        public IList<string> Errors { get; set; }

        public BaseResultViewModel() 
        {
            this.Errors = new List<string>();
        }

        public bool Success 
        {
            get { return this.Errors.Count == 0; }
        }

        public void AddError(string error) 
        {
            this.Errors.Add(error);
        }
    }
}
