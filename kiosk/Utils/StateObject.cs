using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace s_oil.Utils
{
    internal class StateObject
    {
        /// <summary>
        /// Client socket
        /// </summary>
        public Socket workSocket = null;

        /// <summary>
        /// 수신 버퍼의 크기
        /// </summary>
        public const int BufferSize = 1024;

        /// <summary>
        /// 수신 버퍼
        /// </summary>
        public byte[] buffer = new byte[BufferSize];

        /// <summary>
        /// 수신된 데이터 문자열
        /// </summary>
        public StringBuilder sb = new StringBuilder();
    }
}
