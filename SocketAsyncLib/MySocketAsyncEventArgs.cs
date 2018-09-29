using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    public class MySocketAsyncEventArgs:SocketAsyncEventArgs
    {
        /// <summary>
        /// 当前SOCKET连接的索引(接入的先后顺序),用以设置接收缓存起始位置
        /// </summary>
        public int CurrentIndex { get; set; }
    }
}
