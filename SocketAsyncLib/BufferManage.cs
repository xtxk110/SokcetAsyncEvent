using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    /// <summary>
    /// AsyncEventArgs相关操作
    /// </summary>
    public class BufferManage
    {
        /// <summary>
        /// SOCKET连接最大记录数
        /// </summary>
        private int MaxConnection;
        /// <summary>
        /// 监听最大 数
        /// </summary>
        private int MaxListen { get; set; }
        /// <summary>
        /// 单个SOCKET连接的接收接缓存大小
        /// </summary>
        private int BufferSize { get; set; }
        /// <summary>
        /// MySocketAsyncEventArgs 池
        /// </summary>
        public Stack<MySocketAsyncEventArgs> SocketEventStack { get; set; }
        /// <summary>
        /// 所有连接的发送接收缓存
        /// </summary>
        public byte[] Buffer { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="maxListen">监听最大数</param>
        /// <param name="bufferSize">单个SOKCET连接缓存大小</param>
        public BufferManage(int maxListen,int bufferSize)
        {
            this.BufferSize = bufferSize;
            this.MaxListen = maxListen;
            SocketEventStack = new Stack<MySocketAsyncEventArgs>();
            Buffer = new byte[this.BufferSize * this.MaxListen];
            MaxConnection = -1;//初始化为-1,每增加一个连接,增加1,以此判断每个连接接收缓存的OFFSET
        }
        /// <summary>
        /// 获取一个空闲的SocketAsyncEventArgs
        /// </summary>
        /// <returns>SocketAsyncEventArgs</returns>
        public MySocketAsyncEventArgs GetSocketEvent()
        {
            if (SocketEventStack.Count > 0)
                return SocketEventStack.Pop();
            else
            {
                int value= Interlocked.Increment(ref MaxConnection);
                MySocketAsyncEventArgs eventArgs = new MySocketAsyncEventArgs { CurrentIndex = value };
                return eventArgs;
            }
        }
        /// <summary>
        /// 释放一个SocketAsyncEventArgs
        /// </summary>
        /// <param name="socketEvent">SocketAsyncEventArgs</param>
        public void FreeSocketEvent(SocketAsyncEventArgs socketEvent)
        {
            MySocketAsyncEventArgs se = socketEvent as MySocketAsyncEventArgs;
            if(se != null)
            {
                try { socketEvent.SetBuffer(0, 0);SocketEventStack.Push(se); }
                catch(Exception e) { LogLib.Log.WriteLog(e.TargetSite + "->" + e.Message, LogLib.LogType.ERROR); }
            }
            
        }
    }
}
