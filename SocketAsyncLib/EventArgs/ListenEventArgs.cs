using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SocketAsyncLib
{
    /// <summary>
    /// 监听事件参数
    /// </summary>
    public class ListenEventArgs:EventArgs
    {
        /// <summary>
        /// 监听地址信息
        /// </summary>
        public string Message { get; set; }
    }
}
