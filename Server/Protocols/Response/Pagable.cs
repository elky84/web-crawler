using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Pagable : Header
    {
        public long Total { get; set; }

        public int Offset { get; set; }

        public int Limit { get; set; }

        public string Sort { get; set; }

        public bool Asc { get; set; }
    }
}
