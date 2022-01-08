using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public class ResultCode : EzAspDotNet.Code.ResultCode
    {
        public ResultCode(int id, string name) : base(id, name)
        {
        }


        public static ResultCode UsingSourceId = new(10000, "UsingSourceId");
        public static ResultCode UsingRssId = new(10001, "UsingRssId");
        public static ResultCode UsingNotificationId = new(10002, "UsingNotificationId");
        public static ResultCode NotFoundSource = new(10003, "NotFoundSource");
    }
}
