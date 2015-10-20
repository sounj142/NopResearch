using System;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.Web.SessionState;

namespace Research.Core.Fakes
{
    /// <summary>
    /// Gỉa lập Session. Kế thừa HttpSessionStateBase, định nghĩa 1 collection mới và override lại tất cả các method quan trọng để
    /// chuyển hướng qua xài collection này, ko xài đến collection cũng như code của lớp cơ sở
    /// </summary>
    public class FakeHttpSessionState : HttpSessionStateBase
    {
        /// <summary>
        /// Định nghĩa 1 collection data mới, toàn bộ thao tác sẽ tương tác trên collection này, mọi method mức base sẽ bị thay thế
        /// </summary>
        private readonly SessionStateItemCollection _sessionItems;

        /// <summary>
        /// Sở dĩ để sessionItems được truyền vào từ bên ngoài ( có thể thông qua DI ) là để tiện lợi cho unit test ? Để khi unittest,
        /// chúng ta có thể chuẩn bị 1 đối tượng SessionStateItemCollection, sau đó truyền vào cho hàm tạo của FakeHttpSessionState,
        /// sau đó chỉ cần thường xuyên theo dõi đối tượng SessionStateItemCollection là có thể biết Session có hoạt động đúng hay ko ?
        /// </summary>
        public FakeHttpSessionState(SessionStateItemCollection sessionItems)
        {
            if (sessionItems == null) throw new ArgumentNullException("sessionItems");
            _sessionItems = sessionItems;
        }

        public override int Count
        {
            get { return _sessionItems.Count; }
        }

        public override NameObjectCollectionBase.KeysCollection Keys
        {
            get
            {
                return _sessionItems.Keys;
            }
        }

        public override object this[string name]
        {
            get
            {
                return _sessionItems[name];
            }
            set
            {
                _sessionItems[name] = value;
            }
        }

        public bool Exists(string key)
        {
            return _sessionItems[key] != null;
        }

        public override object this[int index]
        {
            get
            {
                return _sessionItems[index];
            }
            set
            {
                _sessionItems[index] = value;
            }
        }

        public override void Add(string name, object value)
        {
            _sessionItems[name] = value;
        }

        public override IEnumerator GetEnumerator()
        {
            return _sessionItems.GetEnumerator();
        }

        public override void Remove(string name)
        {
            _sessionItems.Remove(name);
        }
    }
}