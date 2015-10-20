using System;
using System.Runtime.Serialization;

namespace Research.Core
{
    public class ResearchException : Exception
    {
        public ResearchException() 
        { 
        }

        public ResearchException(string message)
            : base(message)
        {
        }

        public ResearchException(string messageFormat, params object[] args)
            : base(string.Format(messageFormat, args))
        {
        }

        protected ResearchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ResearchException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
