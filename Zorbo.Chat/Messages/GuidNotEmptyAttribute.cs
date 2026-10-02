using System.ComponentModel.DataAnnotations;

namespace Zorbo.Chat.Messages
{
    public sealed class GuidNotEmptyAttribute : ValidationAttribute
    {
        public override bool IsValid(object value) {
            if (value is Guid guid)
                return guid != Guid.Empty;
            return false;
        }
    }
}