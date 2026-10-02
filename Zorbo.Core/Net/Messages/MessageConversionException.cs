namespace Zorbo.Net.Messages
{
    public class MessageConversionException : Exception
    {
        const string defaultMessage = "Failed to convert message. See inner exception for details.";

        public MessageConversionException(string message)
            : base(message) { }

        public MessageConversionException(Exception innerException)
            : base(defaultMessage, innerException) { }

        public MessageConversionException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}