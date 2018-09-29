using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAsyncLib
{
    public enum CloseReason
    {
        /// <summary>
        /// 服务端主动关闭
        /// </summary>
        ACTIVE_CLOSE=0,
        /// <summary>
        /// 客户端连接主动关闭
        /// </summary>
        PASSIVE_CLOSE=1
    }
}
