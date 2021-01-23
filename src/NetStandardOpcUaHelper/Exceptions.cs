using System;

namespace NetStandardOpcUaHelper
{
    /// <summary>
    /// Custom exception for when a session has not been created
    /// </summary>
    public class SessionNotCreatedException : Exception
    {
        /// <summary>
        /// Create new instance of <see cref="SessionNotCreatedException"/>
        /// </summary>
        public SessionNotCreatedException()
            : base(message: Text.SessionNotCreated)
        {
        }
    }
}