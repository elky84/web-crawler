﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Protocols.Response
{
    public class Notification : Header
    {
        public Common.Notification Data { get; set; }
    }
}
