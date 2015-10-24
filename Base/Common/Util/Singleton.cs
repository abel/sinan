using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Sinan.Util
{
    /// <summary>
    /// 单例模式
    /// </summary>
    /// <typeparam name="T">请确保T有默认构造函数</typeparam>
    public static class Singleton<T> where T : class //, new()
    {
        readonly static T m_instance;

        static Singleton()
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
            var defaultConstructor = typeof(T).GetConstructor(bindingFlags, null, new Type[0], null);
            if (defaultConstructor != null)
            {
                m_instance = defaultConstructor.Invoke(null) as T;
            }
        }

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static T Instance
        {
            get { return m_instance; }
        }
    }
}
