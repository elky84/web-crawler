using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Source : Header
    {
        public Common.Source Data { get; set; }
    }
}
