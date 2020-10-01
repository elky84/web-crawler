using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Server.Code;
using System;

namespace Server.Protocols.Common
{
    public class Header
    {
        public string Id { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Updated { get; set; }
    }
}
