using System;
using System.Web;

namespace Research.Core.Fakes
{
    public static class Extensions
    {
        /// <summary>
        /// Chỉ ra 1 đối tượng HttpContextBase có phải là Fake hay ko
        /// </summary>
        public static bool IsFakeContext(this HttpContextBase context)
        {
            if(context == null) throw new ArgumentNullException("context");
            return context is FakeHttpContext;
        }
    }
}
