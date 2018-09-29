using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogLib
{
    public enum LogType
    {
        /// <summary>
        /// 只记录错误日志
        /// </summary>
        ERROR=0,
        /// <summary>
        /// 一般的信息
        /// </summary>
        INFO=1,
        /// <summary>
        /// 只记录传送的数据
        /// </summary>
        DATA=2
    }
}
