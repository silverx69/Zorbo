using System.ComponentModel.DataAnnotations;

namespace Zorbo.Net.Messages
{
    public class MessageValidator
    {
        public static bool Validate(object message, out List<ValidationResult> results) {
            results = [];
            return Validator.TryValidateObject(message, new(message), results, true);
        }
    }
}