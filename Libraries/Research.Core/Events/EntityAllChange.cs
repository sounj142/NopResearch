namespace Research.Core.Events
{
    /// <summary>
    /// Mô tả cho sự kiện "thay đổi qui mô lớn" trên bảng T. Sự kiện này sẽ đòi hỏi clear cache ở mức độ cao nhất, toàn diện nhất
    /// 
    /// Bởi vì đây là sự kiện qui mô lớn, thường do 1 danh sách đối tượng phát sinh chứ ko phải 1 đối tượng cụ thể, nên ta
    /// chỉ cần truyền vào T=null làm tham số cho hàm tạo
    /// </summary>
    public class EntityAllChange<T> where T:BaseEntity
    {
        public EntityAllChange(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; private set; }
    }
}
