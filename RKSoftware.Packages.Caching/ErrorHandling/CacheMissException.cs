using System;

namespace RKSoftware.Packages.Caching.ErrorHandling
{
    /// <summary>
    /// This exception type is used to notify if cache miss happened (no object found in cache).
    /// </summary>
    public class CacheMissException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMissException"/> class.
        /// </summary>
        public CacheMissException()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMissException"/> class.
        /// </summary>
        /// <param name="message">Message that describes error happened.</param>
        public CacheMissException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheMissException"/> class.
        /// </summary>
        /// <param name="message">Message that describes error happened.</param>
        /// <param name="innerException">Inner exception that is the reason of this exception.</param>
        public CacheMissException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
