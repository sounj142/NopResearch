using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Research.Core
{
    /// <summary>
    /// Lớp cơ sở trừu tượng cho mọi thực thể nghiệp vụ trong ứng dụng
    /// </summary>
    public abstract partial class BaseEntity
    {
        /// <summary>
        /// Id, bằng cách qui định property này, chúng ta sẽ xây dựng 1 CSDL mà ở đó mọi bảng đều có duy nhất 1 khóa chính
        /// là Id có kiểu int
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Override lại phương thức so sánh nguyên thủy của object. Bằng cách này, các phép so sánh .Equals( bao gồm cả == ) trên 
        /// BaseEntity sẽ thực hiện so sánh dựa trên kiểu và Id. Do đó, nếu 2 đối tượng BaseEntity có địa chỉ bộ nhớ khác nhau, nhưng lại
        /// kiểu sao cho kiểu của đối tượng này là cơ sở của kiểu đối tượng kia và chung Id thì chúng sẽ đc coi là bằng nhau
        /// </summary>
        public override bool Equals(object obj)
        {
            //if (obj is BaseEntity) return Equals((BaseEntity)obj);
            //return base.Equals(obj);
            // hoàn toàn từ bỏ Equal() của object
            return Equals(obj as BaseEntity);
        }

        public virtual bool Equals(BaseEntity obj)
        {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;

            if(!IsTransient(this) && !IsTransient(obj) && Equals(this.Id, obj.Id))
            {
                var thisType = this.GetType(); //// Khác ở đây
                var otherType = obj.GetType();
                return thisType.IsAssignableFrom(otherType) || otherType.IsAssignableFrom(thisType);
            }
            return false;
        }
        /// <summary>
        /// Kiểm tra đối tượng có là tạm thời ko ( ID = 0)
        /// </summary>
        public static bool IsTransient(BaseEntity obj)
        {
            return obj != null && Equals(obj.Id, default(int));
        }
        /// <summary>
        /// Lấy mã băm theo Id nếu đối tượng ko phải tạm thời ( Id != 0)
        /// </summary>
        public override int GetHashCode()
        {
            if(Equals(Id, default(int))) return base.GetHashCode();
            return Id.GetHashCode();
        }
        /// <summary>
        /// Đảm bảo rằng phép so sánh == được thực hiện theo cách so sánh kiểu phù hợp dựa vào Id
        /// </summary>
        public static bool operator == (BaseEntity x, BaseEntity y)
        {
            return Equals(x, y);
        }
        /// <summary>
        /// Đảm bảo rằng phép so sánh != được thực hiện theo cách so sánh kiểu phù hợp dựa vào Id
        /// </summary>
        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
    }
}
