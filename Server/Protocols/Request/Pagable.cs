using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Request
{
    public class Pagable
    {
        public int Offset { get; set; } = 0;

        public int Limit { get; set; } = 20;

        public string Sort { get; set; }

        public bool Asc { get; set; }
    }
}
