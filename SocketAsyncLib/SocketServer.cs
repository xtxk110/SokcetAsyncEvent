using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using LogLib;
using System.Threading;
using System.Collections.Concurrent;

namespace SocketAsyncLib
{
    /// <summary>
    /// TCP SOCKET服务
    /// </summary>
    public class SocketServer
    {
        private Socket Server;
        private EndPoint Local;
        private BufferManage Buffer;//SocketAsyncEventArgs对象管理
        private ConcurrentDictionary<string, SocketSession> _dic = new ConcurrentDictionary<string, SocketSession>();// 存储SOCKET连接 key:sessionid
        private ConcurrentDictionary<string, MySocketAsyncEventArgs> _argsDic = new ConcurrentDictionary<string, MySocketAsyncEventArgs>();// 存储SocketAsyncEventArgs key:sessionid
        /// <summary>
        /// 存储SOCKET连接 key:sessionid
        /// </summary>
        //public ConcurrentDictionary<string,SocketSession> SessionDic { get { return _dic; } }
        /// <summary>
        /// SOCKET监听事件,成功返回监听地址;失败返回错误信息
        /// </summary>
        public event EventHandler ListenEvent;
        /// <summary>
        /// 新SOCKET连接事件,
        /// </summary>
        public event EventHandler NewSessionEvent;
        /// <summary>
        /// 接收到数据包事件(未过滤)
        /// </summary>
        public event EventHandler ReceiveEvent;
        /// <summary>
        /// 根据相关协议过滤返回的可用的完整数据包(未实现)
        /// </summary>
        public event EventHandler RecieveFilterEvent;
        /// <summary>
        /// 发送数据包成功事件
        /// </summary>
        public event EventHandler SendEvent;
        /// <summary>
        /// SOCKET关闭事件
        /// </summary>
        public event EventHandler CloseSessionEvent;

        private int _MaxListen = 10000;
        /// <summary>
        /// 最大监听数量,默认10000
        /// </summary>
        public int MaxListen { get { return _MaxListen; } set { _MaxListen = value; } }

        private int _BufferSize = 4096;
        /// <summary>
        /// 接收缓存大小,默认4096;根据实际传输数据大小设置
        /// </summary>
        public int BufferSize { get { return _BufferSize;} set { _BufferSize = value; } }

        private int _CurConn = 0;
        /// <summary>
        /// SOCKET当前连接数
        /// </summary>
        public int CurrentConnection { get { return _CurConn; } }


        /// <summary>
        /// 以IPV4网络格式地址初始化TCP SOCKET
        /// <param name="ip">IPv4地址</param>
        /// <param name="port">端口号</param>
        /// </summary>
        public SocketServer(string ip, int port) : this(ip, port, AddressFamily.InterNetwork) { }
        /// <summary>
        /// 以IPV4网络格式地址初始化TCP SOCKET
        /// </summary>
        /// <param name="ip">IP地址</param>
        /// <param name="port">端口号</param>
        /// <param name="addressFamily">SOCKET地址类型:AddressFamily.InterNetwork,AddressFamily.InterNetworkV6</param>
        public SocketServer(string ip,int port,AddressFamily addressFamily)
        {
            Server = new Socket(addressFamily, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                Local = new IPEndPoint(IPAddress.Parse(ip), port);
                Server.Bind(Local);
            }catch(Exception e) { Log.WriteLog(e.TargetSite + "->" + e.Message, LogType.ERROR); }
        }
        /// <summary>
        /// 启动SOCKET服务
        /// </summary>
        public void Start()
        {
            try
            {
                Buffer = new BufferManage(MaxListen, BufferSize);
                Server.Listen(MaxListen);
                ListenEvent?.Invoke(null, new ListenEventArgs { Message = Server.LocalEndPoint.ToString() });
                StartAccept(null);
            }
            catch(Exception e)
            {
                Log.WriteLog(e.TargetSite + "->" + e.Message, LogType.ERROR);
            }
        }
        /// <summary>
        /// 接入连接
        /// </summary>
        /// <param name="saea"></param>
        private void StartAccept(SocketAsyncEventArgs saea)
        {
            if (saea == null)
            {
                saea = new SocketAsyncEventArgs();
                saea.Completed += Accept_Completed;
            } else
                saea.AcceptSocket = null;
            bool asyncFlag = Server.AcceptAsync(saea);
            if (!asyncFlag)
                ProcessAccept(saea);
        }
        private void Accept_Completed(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }
        /// <summary>
        /// 接入连接具体操作
        /// </summary>
        /// <param name="saea"></param>
        private void ProcessAccept(SocketAsyncEventArgs saea)
        {
            try
            {
                Interlocked.Increment(ref _CurConn);//改当前连接数
                string guid = Guid.NewGuid().ToString("N");
                SocketSession session = new SocketSession { Client = saea.AcceptSocket, SessionId = guid }; 
                _dic.TryAdd(guid, session);//添加SokcetSession
                MySocketAsyncEventArgs msaea = Buffer.GetSocketEvent();//从池中获取一个SocketAsyncEventArgs
                msaea.UserToken = session;
                msaea.SetBuffer(Buffer.Buffer, msaea.CurrentIndex * this.BufferSize, this.BufferSize);//设置连接的缓冲区
                msaea.Completed -= IO_Completed;
                msaea.Completed += IO_Completed;
                _argsDic.TryAdd(guid, msaea);//添加SocketAsyncEventArgs

                NewSessionEvent?.Invoke(null, new SessionEventArgs { Type = SessionType.NEW, Endpoint = saea.AcceptSocket.RemoteEndPoint.ToString(), Session = session });
                bool resultFlag= saea.AcceptSocket.ReceiveAsync(msaea);
                if (!resultFlag)
                    ProcessReceive(msaea);
            }catch(Exception e) { Log.WriteLog(e.TargetSite + "->" + e.Message, LogType.INFO); }

            StartAccept(saea);
        }

        /// <summary>
        /// 数据发送接收操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void IO_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e is MySocketAsyncEventArgs msaea)
            {
                try
                {
                    switch (msaea.LastOperation)
                    {
                        case SocketAsyncOperation.Receive:
                            ProcessReceive(msaea);
                            break;
                        case SocketAsyncOperation.Send:
                            ProcessSend(msaea);
                            break;
                        default:
                            break;
                    }
                }catch(Exception e1) { Log.WriteLog(e1.TargetSite + "->" + e1.Message, LogType.ERROR); }
            }
        }
        /// <summary>
        /// 接收具体操作
        /// </summary>
        /// <param name="msaea"></param>
        private void ProcessReceive(MySocketAsyncEventArgs msaea)
        {
            SocketSession session = (SocketSession)msaea.UserToken;
            // e.BytesTransferred获取套接字操作中传输的字节数。不大于0,则对方SOCKET已关闭 
            if (msaea.BytesTransferred > 0)
            {
                if (msaea.SocketError == SocketError.Success)
                {
                    if (ReceiveEvent != null)
                    {
                        byte[] data = new byte[msaea.BytesTransferred];
                        System.Buffer.BlockCopy(Buffer.Buffer, msaea.Offset, data, 0, msaea.BytesTransferred);
                        ReceiveEvent(null, new ReceiveEventArgs { Receive = data, Session = session });
                    }
                }
                else
                    Log.WriteLog(msaea.SocketError.ToString(), LogType.INFO);

                ////接收下一个数据包
                bool resultFlag = session.Client.ReceiveAsync(msaea);
                if (!resultFlag)
                    ProcessReceive(msaea);
            }
            else
                CloseClientSocket(msaea,CloseReason.PASSIVE_CLOSE);
        }
        /// 数据发送
        /// </summary>
        /// <param name="sendData">send data byte[]</param>
        /// <param name="sessionId">SocketSession sessionId</param>
        public void Send(string sessionId,byte[] sendData)
        {
            bool flag1= _dic.TryGetValue(sessionId, out SocketSession session);
            bool flag2= _argsDic.TryGetValue(sessionId, out MySocketAsyncEventArgs args);
            if (!flag1 || !flag2)
                Log.WriteLog("提供的SESSIONID已失效", LogType.INFO);
            int sendLen = sendData.Length;
            System.Buffer.BlockCopy(sendData, 0, args.Buffer, args.Offset, sendLen);
            try
            {
                args.SetBuffer(args.Offset, sendLen);
                var resultFlag = session.Client.SendAsync(args);
                if (!resultFlag)
                    ProcessSend(args);
            }catch(Exception e) { Log.WriteLog(e.TargetSite + "->" + e.Message, LogType.ERROR); }
        }
        /// <summary>
        /// 数据发送具体执行
        /// </summary>
        /// <param name="msae"></param>
        private void ProcessSend(MySocketAsyncEventArgs msae)
        {
            if (msae.SocketError == SocketError.Success)
            {
                if (SendEvent != null)
                {
                    SocketSession session = msae.UserToken as SocketSession;
                    SendEvent(null, new SendEventArgs { Session = session });
                }
            }
            else
            {
                CloseClientSocket(msae,CloseReason.PASSIVE_CLOSE);
            }
        }
        //关闭SOCKET连接
        private void CloseClientSocket(MySocketAsyncEventArgs msaea, CloseReason reason)
        {
            SocketSession session = (SocketSession)msaea.UserToken;
            CloseSessionEvent?.Invoke(null, new SessionEventArgs { Type = SessionType.CLOSED, Reason = reason, Endpoint = session.Client.RemoteEndPoint.ToString() });
            try { _dic.TryRemove(session.SessionId, out SocketSession outValue); _argsDic.TryRemove(session.SessionId, out MySocketAsyncEventArgs eventArgs); }//移除关闭的连接
            catch (Exception e) { Log.WriteLog(e.TargetSite + "->" + e.Message, LogType.ERROR); }
            try
            {
                session.Client.Shutdown(SocketShutdown.Send);
            }
            catch (Exception e) { }
            try
            {
                session.Client.Close();
            }
            catch (Exception e) { }

            Interlocked.Decrement(ref _CurConn);//当前连接数自减
            msaea.UserToken = null;
            Buffer.FreeSocketEvent(msaea);//把关闭的事件对象,放入集合中

        }
        /// <summary>
        /// <summary>
        /// 通过sessionId获取SocketSession
        /// </summary>
        /// <param name="sessionId">sessionId</param>
        /// <returns></returns>
        public SocketSession GetSession(string sessionId)
        {
            if (_dic.TryGetValue(sessionId, out SocketSession outValue))
                return outValue;
            else
                return null;
        }
        /// <summary>
        /// 关闭SOCKET服务
        /// </summary>
        public void Close()
        {
            try
            {
                Server.Shutdown(SocketShutdown.Both);
                Server.Close();
                Server.Dispose();
            }catch { }
            foreach(var item in _dic.Values)
            {
                try
                {
                    item.Client.Shutdown(SocketShutdown.Both);
                    item.Client.Close();
                    item.Client.Dispose();
                }
                catch { }
            }
            _dic.Clear();
            _argsDic.Clear();
            Buffer = null;
        }
    }
}
