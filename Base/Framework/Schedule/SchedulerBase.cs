using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Sinan.Log;

namespace Sinan.Schedule
{
    public abstract class SchedulerBase : ISchedule, IDisposable
    {
        protected readonly object m_locker = new object();
        protected System.Threading.Timer m_timer;
        protected int m_dueTime, m_period;

        int cur_period;

        protected SchedulerBase(int dueTime, int period)
        {
            m_dueTime = dueTime;
            m_period = period;
        }

        void Exec(object ojb)
        {
            if (Monitor.TryEnter(m_locker))
            {
                try
                {
                    Exec();
                    //if (cur_period != m_period)
                    //{
                    //    cur_period = m_period;
                    //    m_timer.Change(cur_period, cur_period);
                    //}
                }
                catch { }
                finally
                {
                    Monitor.Exit(m_locker);
                }
            }
        }

        protected abstract void Exec();


        public virtual void Start()
        {
            lock (m_locker)
            {
                System.Threading.Timer temp = m_timer;
                if (temp != null)
                {
                    temp.Dispose();
                }
                cur_period = m_period;
                m_timer = new System.Threading.Timer(Exec, null, m_dueTime, cur_period);
            }
        }

        public virtual void Close()
        {
            lock (m_locker)
            {
                System.Threading.Timer temp = m_timer;
                if (temp != null)
                {
                    m_timer = null;
                    temp.Dispose();
                }
            }
        }

        void IDisposable.Dispose()
        {
            Close();
        }


        /// <summary>
        /// 从程序集加载所有定时任务.
        /// </summary>
        /// <param name="a"></param>
        public static List<ISchedule> LoadAllSchedule(Assembly a)
        {
            List<ISchedule> set = new List<ISchedule>();
            foreach (Type t in a.GetExportedTypes())
            {
                try
                {
                    if ((!t.IsAbstract) && (!t.IsGenericType)
                        && t.GetInterface("Sinan.Schedule.ISchedule") != null)
                    {
                        const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                        var defaultConstructor = t.GetConstructor(bindingFlags, null, new Type[0], null);
                        ISchedule sub = defaultConstructor.Invoke(null) as ISchedule;
                        //ISchedule sub = System.Activator.CreateInstance(t) as ISchedule;
                        if (sub != null)
                        {
                            set.Add(sub);
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
        /// 加载指定路径下所有dll文件中的定时任务
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static List<ISchedule> LoadAllSchedules(string path)
        {
            List<ISchedule> dic = new List<ISchedule>();
            if ((!string.IsNullOrEmpty(path)) && Directory.Exists(path))
            {
                String[] files = System.IO.Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly);
                foreach (var file in files)
                {
                    try
                    {
                        Assembly a = Assembly.LoadFrom(file);
                        var set = LoadAllSchedule(a);
                        dic.AddRange(set);
                    }
                    catch (System.Exception ex)
                    {
                        LogWrapper.Error(ex);
                    }
                }
                return dic;
            }
            return dic;
        }
    }
}
