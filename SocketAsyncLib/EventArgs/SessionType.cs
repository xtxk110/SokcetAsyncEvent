using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAsyncLib
{
    public enum SessionType
    {
        /// <summary>
        /// SOCKET连接
        /// </summary>
        NEW=0,
        /// <summary>
        /// SOCKET关闭
        /// </summary>
        CLOSED=1
    }
}
