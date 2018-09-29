using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;

namespace SocketAsyncLib
{ 
    public class SendEventArgs:EventArgs
    {
        /// <summary>
        /// 当前SOCKET OBJECT
        /// </summary>
        public SocketSession Session { get; set; }
    }
}
