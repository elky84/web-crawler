using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class CodeList : Header
    {
        public List<string> Datas = new List<string>();
    }
}
