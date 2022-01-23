using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class SourceMulti : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<Common.Source> Datas { get; set; }
    }
}
