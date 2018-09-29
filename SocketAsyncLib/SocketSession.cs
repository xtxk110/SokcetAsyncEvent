using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace SocketAsyncLib
{
    public class SocketSession
    {
        /// <summary>
        /// Id with Socket
        /// </summary>
        public string SessionId { get; set; }
        /// <summary>
        /// Socket
        /// </summary>
        public Socket Client { get; set; }

    }
}
