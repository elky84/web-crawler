using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;

namespace Server.Protocols.Response
{
    public class Header
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public ResultCode ResultCode { get; set; } = ResultCode.Success;

        public string ErrorMessage { get; set; }
    }
}
