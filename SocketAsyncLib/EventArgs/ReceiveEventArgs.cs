using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    public class ReceiveEventArgs:EventArgs
    {
        /// <summary>
        /// 接收到的数据包或者是根据协议返回的完整数据包(具体以事件类型为准)
        /// </summary>
        public byte[] Receive { get; set; }
        /// <summary>
        /// 当前SOCKET OBJECT
        /// </summary>
        public SocketSession Session { get; set; }
    }
}
