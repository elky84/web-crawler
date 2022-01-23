using System.Collections.Generic;

namespace Server.Protocols.Response
{
    public class CodeList : EzAspDotNet.Protocols.ResponseHeader
    {
        public List<string> Datas = new();
    }
}
