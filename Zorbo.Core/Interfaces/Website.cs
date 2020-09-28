using Newtonsoft.Json;
using Zorbo.Core.Models;

namespace Zorbo.Core.Interfaces
{
    public class Website : ModelBase
    {
#pragma warning disable IDE0044 // Add readonly modifier
        string address = string.Empty;
        string caption = string.Empty;
#pragma warning restore IDE0044 // Add readonly modifier

        [JsonProperty("address", Required = Required.AllowNull)]
        public string Address {
            get { return address; }
            set { OnPropertyChanged(() => address, value); }
        }

        [JsonProperty("caption", Required = Required.AllowNull)]
        public string Caption {
            get { return caption; }
            set { OnPropertyChanged(() => caption, value); }
        }

        public Website() { }

        public Website(string address, string caption)
        {
            Address = address;
            Caption = caption;
        }
    }
}
