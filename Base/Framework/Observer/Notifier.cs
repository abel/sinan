using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Sinan.Log;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.Observer
{
    /// <summary>
    /// 邮差.用于订阅和发布消息..
    /// </summary>
    /// </summary>
    public class Notifier : INotifier
    {
        #region 单例实现
        static Notifier _instantce;
        //static Amib.Threading.SmartThreadPool _smartThreadPool;

        public static Notifier Instance
        {
            get { return _instantce; }
        }
        static Notifier()
        {
            _instantce = new Notifier();
            //Amib.Threading.STPStartInfo stpStartInfo = new Amib.Threading.STPStartInfo();
            //stpStartInfo.IdleTimeout = 60 * 1000;
            //stpStartInfo.MinWorkerThreads = 8;
            //stpStartInfo.MaxWorkerThreads = 64;
            //stpStartInfo.EnableLocalPerformanceCounters = false;
            //_smartThreadPool = new Amib.Threading.SmartThreadPool(stpStartInfo);
        }
        #endregion

        ConcurrentDictionary<string, HashSet<ISubscriber>> m_subscriberMap;
        private Notifier()
        {
            m_subscriberMap = new ConcurrentDictionary<string, HashSet<ISubscriber>>();
        }

        #region IMediator 成员
        /// <summary>
        ///  订阅消息
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public bool Subscribe(ISubscriber subscriber)
        {
            foreach (string topic in subscriber.Topics())
            {
                Subscribe(topic, subscriber);
            }
            return true;
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public bool Subscribe(string topic, ISubscriber subscriber)
        {
            HashSet<ISubscriber> list;
            lock (m_subscriberMap)
            {
                if (!m_subscriberMap.TryGetValue(topic, out list))
                {
                    list = new HashSet<ISubscriber>();
                    if (!m_subscriberMap.TryAdd(topic, list))
                    {
                        m_subscriberMap.TryGetValue(topic, out list);
                    }
                }
                list.Add(subscriber);
            }
            return true;
        }

        /// <summary>
        ///  取消订阅
        /// </summary>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public bool UnSubscribe(ISubscriber subscriber)
        {
            foreach (string topic in subscriber.Topics())
            {
                UnSubscribe(topic, subscriber);
            }
            return true;
        }

        /// <summary>
        /// 取消订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="subscriber"></param>
        /// <returns></returns>
        public bool UnSubscribe(string topic, ISubscriber subscriber)
        {
            HashSet<ISubscriber> list;
            lock (m_subscriberMap)
            {
                if (m_subscriberMap.TryGetValue(topic, out list))
                {
                    return list.Remove(subscriber);
                }
            }
            return false;
        }

        /// <summary>
        /// 发布消息
        /// </summary>
        /// <param name="note"></param>
        /// <param name="async">是否异步执行</param>
        /// <returns></returns>
        public bool Publish(INotification note, bool async = false)
        {
            HashSet<ISubscriber> list;
            if (!m_subscriberMap.TryGetValue(note.Name, out list))
            {
                return false;
            }
            foreach (ISubscriber s in list)
            {
                try
                {
                    if (async)
                    {
                        //异步方式,使用线程池执行
                        //_smartThreadPool.QueueWorkItem(s.SafeExecute, note);
                        System.Threading.ThreadPool.UnsafeQueueUserWorkItem(s.SafeExecute, note);
                    }
                    else
                    {
                        s.SafeExecute(note);
                    }
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Error(note.Name + ex);
                }
            }
            return true;
        }
        #endregion


        /// <summary>
        /// 从程序集加载所有观察者.
        /// </summary>
        /// <param name="a"></param>
        public static HashSet<Type> LoadAllSubscribers(Assembly a)
        {
            HashSet<Type> set = new HashSet<Type>();
            foreach (Type t in a.GetExportedTypes())
            {
                try
                {
                    if ((!t.IsAbstract) && (!t.IsGenericType)
                        && t.GetInterface("Sinan.Observer.ISubscriber") != null)
                    {
                        ISubscriber sub = System.Activator.CreateInstance(t) as ISubscriber;
                        if (sub != null && set.Add(t))
                        {
                            Notifier.Instance.Subscribe(sub);
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Error(ex.InnerException ?? ex);
                }
            }
            return set;
        }

        /// <summary>
        /// 加载指定路径下所有dll文件中的观察者
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static Dictionary<Assembly, HashSet<Type>> LoadAllSubscribers(string path)
        {
            if ((!string.IsNullOrEmpty(path)) && Directory.Exists(path))
            {
                String[] files = System.IO.Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
                Dictionary<Assembly, HashSet<Type>> dic = new Dictionary<Assembly, HashSet<Type>>();
                foreach (var file in files)
                {
                    try
                    {
                        Assembly a = Assembly.LoadFrom(file);
                        HashSet<Type> set = Notifier.LoadAllSubscribers(a);
                        dic.Add(a, set);
                    }
                    catch (System.Exception ex)
                    {
                        LogWrapper.Error(ex);
                    }
                }
                return dic;
            }
            return null;
        }
    }
}
