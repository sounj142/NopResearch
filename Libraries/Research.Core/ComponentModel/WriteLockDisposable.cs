using System;
using System.Threading;

namespace Research.Core.ComponentModel
{
    /// <summary>
    /// Cung cấp 1 lớp mà có thể dùng để giữ 1 khóa ghi dùng lệnh using(). Cú pháp sử dụng như sau
    /// 1. Khai báo ReaderWriterLockSlim rwLock ở đâu đó, đây có thể là 1 biến static dùng làm khóa chung cho 1 cache
    /// 2. using(var locker = new WriteLockDisposable(rwLock) // sẽ yêu cầu khóa ghi
    /// {
    ///     // xử lý ở đây
    /// } // sẽ đảm bảo luôn nhả khóa ghi ở bất cứ tình huống nào
    /// </summary>
    public class WriteLockDisposable: IDisposable
    {
        private readonly ReaderWriterLockSlim _rwLock;

        public WriteLockDisposable(ReaderWriterLockSlim rwLock)
        {
            _rwLock = rwLock;
            _rwLock.EnterWriteLock();
        }
        public void Dispose()
        {
            _rwLock.ExitWriteLock();
        }
    }
}
