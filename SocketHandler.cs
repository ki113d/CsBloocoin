using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ki113d.CsBloocoin {

    /// <summary>
    /// Handles all socket operations.
    /// </summary>
    public class SocketHandler : IDisposable {

        private TcpClient client;
        private String ip;
        private int port;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="ip">The IP address or hostname to connect to.</param>
        /// <param name="port">The port on which the server listens.</param>
        public SocketHandler( String ip, int port ) {
            client = new TcpClient();
            this.ip = ip;
            this.port = port;
        }

        /// <summary>
        /// Connects to the given server.
        /// </summary>
        /// <returns>Returns whether the connection was a success.</returns>
        public Boolean connect() {
            IAsyncResult result = client.BeginConnect(ip, port, null, null);
            bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1), true);
            return success;
        }

        /// <summary>
        /// Attempts to send given data to the server.
        /// </summary>
        /// <param name="str">The string data to be sent to the server.</param>
        /// <returns>Whether the send action was a success.</returns>
        public Boolean send( String str ) {
            NetworkStream stream = client.GetStream();
            stream.WriteTimeout = 1000;
            try {
                Byte[] data = Encoding.ASCII.GetBytes(str);
                stream.Write(data, 0, data.Length);
            } catch (TimeoutException ohShit) {
                throw new Exceptions.ConnectionTimeoutException(ohShit.Message);
            }
            return true;
        }

        /// <summary>
        /// Attempts to receive data from the server.
        /// </summary>
        /// <returns>The string data received from the server.</returns>
        public String receive() {
            Int32 bytes;
            Byte[] data = new Byte[1024];
            String responseData = String.Empty;
            try {
                NetworkStream stream = client.GetStream();
                stream.ReadTimeout = 1000;
                bytes = stream.Read(data, 0, data.Length);
            } catch (TimeoutException ohShit) {
                throw new Exceptions.ConnectionTimeoutException(ohShit.Message);
            }
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            responseData = responseData.Replace(System.Environment.NewLine, String.Empty);
            return responseData;
        }

        /// <summary>
        /// Dispose method required by implemented interface.
        /// </summary>
        public void Dispose() {
            client.Close();
            client = null;
        }
    }
}
