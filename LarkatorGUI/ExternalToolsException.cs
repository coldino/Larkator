using System;
using System.Runtime.Serialization;

namespace LarkatorGUI
{
    public class ExternalToolsException : ApplicationException
    {
        public ExternalToolsException()
        {
        }

        public ExternalToolsException(string message) : base(message)
        {
        }

        public ExternalToolsException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected ExternalToolsException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
