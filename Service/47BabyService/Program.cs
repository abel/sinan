using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

static class Program
{
    /// <summary>
    /// 应用程序的主入口点。
    /// </summary>
    static void Main(string[] args)
    {
        if (args.Length > 0)
        {
#if mono
            //得到程序集的版本号.
            Assembly assem = Assembly.GetExecutingAssembly();
            AssemblyName v = assem.GetName();
            Console.WriteLine("Name:" + v.Name + ",Ver:" + v.Version.ToString());

            int work, comp;
            //System.Threading.ThreadPool.GetMinThreads(out work, out comp);
            //Console.WriteLine(string.Format("MinThreads{0},IO:{1}", work, comp));
            System.Threading.ThreadPool.GetMaxThreads(out work, out comp);
            Console.WriteLine(string.Format("MaxThreads{0},IO:{1}", work, comp));

            BabyService baby = new BabyService();
            baby.Start(args);
            System.Threading.Thread.Sleep(Timeout.Infinite);
#else
            InstallHelper.Description = "47babyServer";
            InstallHelper.Install(args);
#endif
        }
        else
        {
            var baby = new BabyService();
            ServiceBase.Run(new ServiceBase[] { baby });
        }
    }
}
