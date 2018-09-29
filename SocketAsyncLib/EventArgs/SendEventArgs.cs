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
        /// 发送的数据包
        /// </summary>
        public byte[] Receive { get; set; }
        /// <summary>
        /// 当前SOCKET OBJECT
        /// </summary>
        public SocketSession Session { get; set; }
    }
}
