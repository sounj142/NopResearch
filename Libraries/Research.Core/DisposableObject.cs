using System;

namespace Research.Core
{
    public class DisposableObject: IDisposable
    {
        private bool isDisposed;

        ~DisposableObject()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed && disposing) DisposeCore();
            isDisposed = true;
        }

        /// <summary>
        /// Thường thì override cái này để đưa vào những hành động cần làm lúc Dispose
        /// </summary>
        protected virtual void DisposeCore()
        {
        }
    }
}
