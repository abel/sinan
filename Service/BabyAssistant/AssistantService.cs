using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Xml;
using SharpSvn;
using Sinan.BabyAssistant.Command;

namespace Sinan.BabyAssistant
{
    [System.ComponentModel.DesignerCategory("Class")]
    public class AssistantService : ServiceBase
    {
        private NetworkCredential nc;
        private Config config;
        private Timer timer;
        private string local;
        private DateTime updateTime;
        //定时期时长
        private int timecount = 1000;

        public AssistantService()
        {
            this.ServiceName = "AssistantService";
        }

        public void Start(string[] args)
        {
            OnStart(args);
        }

        protected override void OnStart(string[] args)
        {
            Log.LogInfo("启动服务:" + this.ServiceName);
            //svn账号/密码
            nc = new NetworkCredential();
            nc.UserName = ConfigurationManager.AppSettings["username"];
            nc.Password = ConfigurationManager.AppSettings["password"];

            config = new Config();
            config.XPath = ConfigurationManager.AppSettings["svnpath"];
            config.SvnLocalPath = ConfigurationManager.AppSettings["localpath"];

            //得到基目录
            local = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "47baby");

            //注册认处理器
            if (timer == null)
            {
                int tc = Convert.ToInt32(ConfigurationManager.AppSettings["timecount"]);
                timecount = timecount > tc ? timecount : tc;
                timer = new Timer(Pull, null, timecount, Timeout.Infinite);
            }
        }

        protected override void OnStop()
        {
            timer.Dispose();
            timer = null;
            Log.LogInfo("停止服务:" + this.ServiceName);
        }

        /// <summary>
        /// 开始更新
        /// </summary>
        private void Pull(object o)
        {
            try
            {
                //svn更新
                if (!Update(config.XPath, config.SvnLocalPath))
                {
                    return;
                }

                //更新成功后,比较两个文件
                string m = Path.Combine(local, "check.xml");
                string n = Path.Combine(config.SvnLocalPath, "check.xml");
                //如果两个文件相同,则不需要更新
                if (FileHelper.FileEquals(m, n))
                {
                    return;
                }

                //存在更新
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(n);
                XmlNode node = xmlDoc.SelectSingleNode("//date");
                //得到当前时间
                DateTime dt = DateTime.Now;
                int upgent = Convert.ToInt32(node.Attributes["upgent"].InnerText);
                DateTime time = DateTime.Parse(node.Attributes["time"].InnerText);
                DateTime upgenttime = DateTime.Parse(node.Attributes["upgenttime"].InnerText);
                switch (upgent)
                {
                    case 0:
                        //普通更新
                        updateTime = time;
                        break;
                    case 1:
                        //定时更新
                        updateTime = upgenttime;
                        break;
                    case 2:
                        //立即更新
                        updateTime = dt;
                        break;
                }

                if (!SameMinute(dt, updateTime))
                {
                    return;
                }

                Log.LogInfo(Log.StartUpdate);
                Dictionary<string, int> diffFiles = GetChangeFiles(config.SvnLocalPath, local);
                //FilterFiles(diffFiles);

                if (diffFiles.Count == 0)
                {
                    //表示没有变化
                    File.Copy(n, m, true);
                    return;
                }
                SyncFiles(diffFiles);
            }
            catch (Exception err)
            {
                Log.LogInfo(err);
            }
            finally
            {
                if (timer != null)
                    timer.Change(timecount, Timeout.Infinite);
            }
        }

        /// <summary>
        /// 过滤文件
        /// </summary>
        /// <param name="diffFiles"></param>
        private void FilterFiles(Dictionary<string, int> diffFiles, List<string> filterName)
        {
            foreach (var item in diffFiles.Keys.ToArray())
            {
                if (filterName != null)
                {
                    bool isbreak = false;
                    foreach (string name in filterName)
                    {
                        if (item.IndexOf(name) > 0)
                        {
                            isbreak = true;
                            break;
                        }
                    }
                    if (isbreak) continue;
                }
            }
        }

        /// <summary>
        /// 同步文件
        /// </summary>
        /// <param name="diffFiles"></param>
        private void SyncFiles(Dictionary<string, int> diffFiles)
        {
            bool restart = NeedStop(diffFiles);
            //需要重启
            if (restart)
            {
                ProssStop(config.ServerList.Values);
                Log.LogInfo("所有进程已结束");
            }
            bool logcheck = true;
            foreach (Server info in config.ServerList.Values)
            {
                Copy(diffFiles, config.SvnLocalPath, info.Path, logcheck, config.NoUpdate);
                logcheck = false;
            }
            if (restart)
            {
                ProssStart();
            }
            Copy(diffFiles, config.SvnLocalPath, local, false, new List<string> { "exe.config" });
            Log.LogInfo(Log.UpdateFinish);
        }

        /// <summary>
        /// 根据文件和配置检查服务是否需要停止
        /// </summary>
        /// <param name="diffFiles"></param>
        /// <returns></returns>
        private bool NeedStop(Dictionary<string, int> diffFiles)
        {
            bool needStop = false;
            foreach (var item in diffFiles)
            {
                if (item.Key.IndexOf(".dll") > 0 || item.Key.IndexOf(".exe") > 0)
                {
                    needStop = true;
                    break;
                }
                foreach (string tmp in config.RestartPath)
                {
                    if (item.Key.IndexOf(tmp) > 0)
                    {
                        needStop = true;
                        break;
                    }
                }
            }
            return needStop;
        }

        #region 基本方法
        static private void Copy(Dictionary<string, int> dic, string sourcePath, string targetPath, bool logcheck, List<string> filterName = null)
        {
            foreach (var item in dic)
            {
                if (filterName != null)
                {
                    bool isbreak = false;
                    foreach (string name in filterName)
                    {
                        if (item.Key.IndexOf(name) > 0)
                        {
                            isbreak = true;
                            break;
                        }
                    }
                    if (isbreak) continue;
                }

                string file = item.Key.Replace(sourcePath, targetPath);
                //要求删除的文件
                if (item.Value == 2)
                {
                    File.Delete(file);
                    if (logcheck)
                    {
                        Log.LogInfo("删除:" + item.Key);
                    }
                }
                else
                {
                    if (FileHelper.CopyFile(item.Key, file))
                    {
                        if (logcheck)
                        {
                            Log.LogInfo("修改:" + item.Key);
                        }
                    }
                }
            }
            //文件夹验证
            DelEmptyDirectory(targetPath);
        }



        /// <summary>
        /// 删除空文件夹
        /// </summary>
        /// <param name="path">根目录</param>
        static private void DelEmptyDirectory(string path)
        {
            DirectoryInfo info = new DirectoryInfo(path);
            foreach (DirectoryInfo c in info.GetDirectories("*", SearchOption.AllDirectories))
            {
                if (c.FullName.IndexOf(".svn") > 0)
                    continue;
                if (Directory.Exists(c.FullName))
                {
                    //删除没有文件的目录
                    if (Directory.GetFiles(c.FullName, "*", SearchOption.AllDirectories).Length == 0)
                    {
                        Directory.Delete(c.FullName, true);
                    }
                }
            }
        }

        /// <summary>
        /// 判断两个时间是否同分钟
        /// </summary>
        /// <param name="m"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        static private bool SameMinute(DateTime m, DateTime n)
        {
            return (m.Year == n.Year &&
            m.Month == n.Month &&
            m.Day == n.Day &&
            m.Hour == n.Hour &&
            m.Minute == n.Minute);
        }

        /// <summary>
        /// 进程停止
        /// </summary>
        /// <returns></returns>
        static private bool ProssStop(IEnumerable<Server> servers)
        {
            foreach (Server info in servers)
            {
                Log.LogInfo(string.Format(Log.StopService, info.Name));
                bool result = ServiceHelper.Stop(info.Name);
                Log.LogInfo(string.Format(result ? Log.StopSucess : Log.StopFail, info.Name));
            }
            //判断进程是否停完
            foreach (Server info in servers)
            {
                string file = ServiceHelper.GetServicePath(info.Name).Trim('"');
                if (string.IsNullOrEmpty(file))
                {
                    continue;
                }
                while (ServiceHelper.ProcessExists(file))
                {
                    Log.LogInfo(string.Format("等待进程结束:{0}", file));
                    System.Threading.Thread.Sleep(10000);
                }
                Log.LogInfo(string.Format("进程已结束:{0}", file));
            }
            return true;
        }


        /// <summary>
        /// 得到两个文件夹中的不同文件
        /// </summary>
        /// <param name="svnPath">基础文件夹</param>
        /// <param name="tempPath"></param>
        /// <returns>得到变化文件的信息</returns>
        static Dictionary<string, int> GetChangeFiles(string svnPath, string tempPath)
        {
            HashSet<string> svnFiles = GetAllFiles(svnPath);
            if (!Directory.Exists(tempPath))
            {
                //表示第一次复制,最新没有更新
                Directory.CreateDirectory(tempPath);
                foreach (string svnFile in svnFiles)
                {
                    string tempFile = svnFile.Replace(svnPath, tempPath);
                    File.Copy(svnFile, tempFile, true);
                }
                return new Dictionary<string, int>();
            }

            HashSet<string> tempFiles = GetAllFiles(tempPath);
            //变更文件信息
            Dictionary<string, int> dic = new Dictionary<string, int>();
            foreach (string item in svnFiles)
            {
                //替换文件路经
                string file = item.Replace(svnPath, tempPath);
                if (tempFiles.Contains(file))
                {
                    //表示发生变化
                    if (!FileHelper.FileEquals(item, file))
                    {
                        if (!dic.ContainsKey(item))
                        {
                            dic.Add(item, 0);
                        }
                    }
                }
                else
                {
                    //表示新加文件
                    dic.Add(item, 1);
                }
            }

            foreach (string item in tempFiles)
            {
                if (item.IndexOf("exe.config") >= 0)
                {
                    continue;
                }
                string file = item.Replace(tempPath, svnPath);
                if (!svnFiles.Contains(file) && (!dic.ContainsKey(item)))
                {
                    dic.Add(file, 2); //要删除的文件
                }
            }
            return dic;
        }

        /// <summary>
        /// 启动所有服务
        /// </summary>
        private void ProssStart()
        {
            foreach (Server info in config.ServerList.Values)
            {
                Log.LogInfo(string.Format(Log.StartService, info.Name));
                bool result = ServiceHelper.Start(info.Name);
                Log.LogInfo(string.Format(result ? Log.StartSucess : Log.StartFail, info.Name));
            }
        }

        /// <summary>
        /// svn更新
        /// </summary>
        /// <param name="xpath">svn地址</param>
        /// <param name="path">本地地址</param>
        /// <returns></returns>
        private bool Update(string xpath, string path)
        {
            using (SvnClient client = new SvnClient())
            {
                client.Authentication.DefaultCredentials = nc;
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                if (!existsSVN(path))
                {
                    Log.LogInfo("正在签出svn...");
                    if (client.CheckOut(new SvnUriTarget(xpath), path))
                    {
                        Log.LogInfo("svn签出成功...");
                        return true;
                    }
                    else
                    {
                        Log.LogInfo("svn签出失败...");
                        return false;
                    }
                }
                client.CleanUp(path);
                client.Revert(path); //?是否需要还原
                return client.Update(path);
            }
        }

        /// <summary>
        /// 检查指定路径及其上级目录是否存在.svn目录
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        static private bool existsSVN(string path)
        {
            string svn = Path.Combine(path, ".svn");
            if (Directory.Exists(svn))
            {
                return true;
            }
            var parent = Directory.GetParent(path);
            if (parent == null)
            {
                return false;
            }
            return existsSVN(parent.FullName);
        }

        /// <summary>
        /// 得到某文件下的所有文件
        /// </summary>
        /// <param name="xpath"></param>
        /// <returns>得到文件目录</returns>
        static private HashSet<string> GetAllFiles(string xpath)
        {
            string[] files = Directory.GetFiles(xpath, "*", SearchOption.AllDirectories);
            HashSet<string> hs = new HashSet<string>();
            foreach (var file in files)
            {
                //过滤.svn的文件
                if (file.IndexOf(".svn") == -1)
                {
                    hs.Add(file);
                }
            }
            return hs;
        }


        #endregion
    }
}
