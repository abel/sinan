using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Util;

namespace Sinan.Observer
{
    public class Notification : INotification
    {
        protected string m_name;
        protected IList m_body;

        #region Constructors

        /// <summary>
        /// Constructs a new notification with the specified name, default body and type
        /// </summary>
        /// <param name="name">The name of the <c>Notification</c> instance</param>
        public Notification(string name)
            : this(name, null)
        { }

        /// <summary>
        /// Constructs a new notification with the specified name and body, with the default type
        /// </summary>
        /// <param name="name">The name of the <c>Notification</c> instance</param>
        /// <param name="body">The <c>Notification</c>s body</param>
        public Notification(string name, IList body)
        {
            m_name = name;
            m_body = body;
        }

        #endregion

        #region Accessors
        /// <summary>
        /// The name of the <c>Notification</c> instance
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// The body of the <c>Notification</c> instance
        /// </summary>
        /// <remarks>This accessor is thread safe</remarks>
        public IList Body
        {
            get { return m_body; }
            //set { m_body = value; }
        }
        #endregion

        #region 快速访问的辅助方法
        public int Count
        {
            get { return m_body == null ? 0 : m_body.Count; }
        }

        public object this[int index]
        {
            get { return m_body[index]; }
            set { m_body[index] = value; }
        }

        public string GetString(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                object b = m_body[index];
                if (b != null)
                {
                    return b.ToString();
                }
            }
            return null;
        }

        public int GetInt32(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return Convert.ToInt32(m_body[index]);
            }
            return 0;
        }

        public Int64 GetInt64(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return Convert.ToInt64(m_body[index]);
            }
            return 0;
        }

        public Boolean GetBoolean(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return Convert.ToBoolean(m_body[index]);
            }
            return false;
        }

        public double GetDouble(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return Convert.ToDouble(m_body[index]);
            }
            return 0;
        }

        public Variant GetVariant(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return (m_body[index]) as Variant;
            }
            return null;
        }

        public T GetValue<T>(int index)
        {
            if (m_body != null && index < m_body.Count)
            {
                return (T)(m_body[index]);
            }
            return default(T);
        }
        #endregion
    }
}
