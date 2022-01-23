using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Rss : EzAspDotNet.Protocols.ResponseHeader
    {
        public Common.Rss Data { get; set; }
    }
}
