using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GMServer;

namespace GMServer
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.ProcessExit += new EventHandler(CurrentDomain_ProcessExit);
            GMApplication.InitServer();
            GMApplication.Instance.Start();

            while (true)
            {
                string message = Console.ReadLine();
                if (message == "exit")
                {
                    System.Environment.Exit(0);
                }
            }
        }

        static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            GMApplication.Instance.Stop();
        }
    }
}