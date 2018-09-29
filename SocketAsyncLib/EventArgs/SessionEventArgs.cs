using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    public class SessionEventArgs:EventArgs
    {
        /// <summary>
        /// SOCKET连接或关闭
        /// </summary>
        public SessionType Type { get; set; }
        /// <summary>
        /// SOKCET OBJECT
        /// </summary>
        public SocketSession Session { get; set; }
        /// <summary>
        /// 客户端链接地址
        /// </summary>
        public string Endpoint { get; set; }
        /// <summary>
        /// SOCKET 关闭原因
        /// </summary>
        public CloseReason Reason { get; set; }
    }
}
