using System;
using System.ServiceProcess;
using System.Threading;

namespace Sinan._47babyServer
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            int work, comp;
            System.Threading.ThreadPool.GetMinThreads(out work, out comp);
            Console.WriteLine(string.Format("原最小线程{0},IO:{1}", work, comp));
            System.Threading.ThreadPool.GetMaxThreads(out work, out comp);
            Console.WriteLine(string.Format("原最大线程{0},IO:{1}", work, comp));

            BabyService baby = new BabyService();

            if (args.Length > 0)
            {
                baby.Start(args);
                System.Threading.Thread.Sleep(Timeout.Infinite);
            }
            else
            {
                ServiceBase[] ServicesToRun = new ServiceBase[] { baby };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
