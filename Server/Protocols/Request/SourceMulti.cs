using System.Collections.Generic;

namespace Server.Protocols.Request
{
    public class SourceMulti : EzAspDotNet.Protocols.RequestHeader
    {
        public List<Common.Source> Datas { get; set; }
    }
}
