
namespace Research.Core.Events
{
    /// <summary>
    /// 1 thùng chứa cho những đối tượng được insert mới vào database. Tức là nếu như có 1 đối tượng product được insert mới vào database
    /// thì ta sẽ tạo ra 1 đối tương EntityInserted[Product](product) bao lấy đối tượng này, và dùng làm đối tượng xử lý sự kiện
    /// 
    /// Khi đó EntityInserted<T> là sự kiện và EntityInserted<T>.Entity chính là thực thể của sự kiện
    /// </summary>
    public class EntityInserted<T> where T:BaseEntity
    {
        public EntityInserted(T entity)
        {
            this.Entity = entity;
        }

        public T Entity { get; private set; }
    }
}
