using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Code
{
    public enum ResultCode
    {
        Success,
        UsingSourceId,
        UsingNotificationId,
        NotImplementedYet,
        NotFoundSource,
        UnknownException
    }
}
