﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    public class SocketSession
    {
        public string SessionId { get; set; }

        public Socket Client { get; set; }
    }
}
