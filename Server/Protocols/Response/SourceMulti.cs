using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class SourceMulti : Header
    {
        public List<Common.Source> SourceDatas { get; set; }
    }
}
