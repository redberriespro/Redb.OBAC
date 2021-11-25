using System;
using System.Runtime.Serialization;

namespace Redb.OBAC.Exceptions
{
    public class ObacException:Exception 
    {
        public ObacException()
        {
        }

        protected ObacException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ObacException(string message) : base(message)
        {
        }

        public ObacException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}