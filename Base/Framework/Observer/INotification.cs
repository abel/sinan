using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Observer
{
    /// <summary>
    /// 消息
    /// </summary>
    public interface INotification
    {
        /// <summary>
        /// The name of the <c>INotification</c> instance
        /// </summary>
        /// <remarks>No setter, should be set by constructor only</remarks>
        string Name { get; }

        /// <summary>
        /// The body of the <c>INotification</c> instance
        /// </summary>
        IList Body { get; }
    }
}
