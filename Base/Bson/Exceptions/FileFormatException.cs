using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MongoDB.Bson {
    /// <summary>
    /// Represents a BSON exception.
    /// </summary>
    [Serializable]
    public class FileFormatException : Exception {
        #region constructors
        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        public FileFormatException()
            : base() {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        public FileFormatException(
            string message
        )
            : base(message) {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public FileFormatException(
            string message,
            Exception innerException
        )
            : base(message, innerException) {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class.
        /// </summary>
        /// <param name="format">The error message format string.</param>
        /// <param name="args">One or more args for the error message.</param>
        public FileFormatException(
            string format,
            params object[] args
        )
            : base(string.Format(format, args)) {
        }

        /// <summary>
        /// Initializes a new instance of the BsonException class (this overload used by deserialization).
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        public FileFormatException(
            SerializationInfo info,
            StreamingContext context
        )
            : base(info, context) {
        }
        #endregion
    }
}
