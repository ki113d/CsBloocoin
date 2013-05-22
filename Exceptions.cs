using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ki113d.CsBloocoin.Exceptions {

    /// <summary>
    /// Base exception.
    /// </summary>
    [Serializable]
    public class Error : Exception {
        public string ErrorMessage {
            get {
                return base.Message.ToString();
            }
        }

        public Error( string errorMessage )
            : base(errorMessage) { }

        public Error( string errorMessage, Exception innerEx )
            : base(errorMessage, innerEx) { }
    }

    /// <summary>
    /// To be thrown when a socket times out.
    /// </summary>
    [Serializable]
    public class ConnectionTimeoutException : Error {

        public ConnectionTimeoutException( string errorMessage )
            : base(errorMessage) { }

        public ConnectionTimeoutException( string errorMessage, Exception innerEx )
            : base(errorMessage, innerEx) { }
    }

    /// <summary>
    /// To be thrown when the user supplies the wrong arguments to a client command.
    /// </summary>
    [Serializable]
    public class InvalidArgumentException : Error {

        public InvalidArgumentException( string errorMessage )
            : base(errorMessage) { }

        public InvalidArgumentException( string errorMessage, Exception innerEx )
            : base(errorMessage, innerEx) { }
    }

}
