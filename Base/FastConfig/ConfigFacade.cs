using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Sinan.FastConfig
{
    public class ConfigFacade
    {
        private string m_path;
        private FileSystemWatcher m_watcher;
        private IConfigManager m_defaultManager;
        private Dictionary<string, IConfigManager> m_access = new Dictionary<string, IConfigManager>();
        private Dictionary<string, DateTime> m_filesTime = new Dictionary<string, DateTime>();

        /// <summary>
        /// 是否启用文件监听
        /// </summary>
        public bool Enable
        {
            get { return m_watcher.EnableRaisingEvents; }
            set { m_watcher.EnableRaisingEvents = value; }
        }

        public ConfigFacade(string path, string filter = "*", bool includeSubdirectories = true)
        {
            m_path = path;
            m_watcher = new FileSystemWatcher(path);
            m_watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            m_watcher.IncludeSubdirectories = includeSubdirectories;
            m_watcher.Filter = filter;
            m_watcher.Changed += new FileSystemEventHandler(FileChanged);
            m_watcher.Created += new FileSystemEventHandler(FileChanged);
            m_watcher.Deleted += new FileSystemEventHandler(FileDeleted);
            m_watcher.Renamed += new RenamedEventHandler(FileRenamed);
            //m_watcher.Error += new ErrorEventHandler(FileError);
        }

        //void FileError(object sender, ErrorEventArgs e)
        //{
        //    //throw new NotImplementedException();
        //}

        void FileRenamed(object sender, RenamedEventArgs e)
        {
            //Console.WriteLine("重命名:" + e.OldName + "   " + e.Name);
            IConfigManager oldFileAccess = FindProcessor(e.OldFullPath);
            if (oldFileAccess != null)
            {
                try
                {
                    oldFileAccess.Unload(e.OldFullPath);
                }
                catch { }
            }

            IConfigManager newFileAccess = FindProcessor(e.FullPath);
            if (newFileAccess != null)
            {
                try
                {
                    newFileAccess.Load(e.FullPath);
                }
                catch { }
            }
        }

        void FileDeleted(object sender, FileSystemEventArgs e)
        {
            //Console.WriteLine("删除:" + e.Name);
            IConfigManager access = FindProcessor(e.FullPath);
            if (access != null)
            {
                try
                {
                    access.Unload(e.FullPath);
                }
                catch { }
            }
        }

        void FileChanged(object sender, FileSystemEventArgs e)
        {
            if (!EnableEvent(e.FullPath)) return;
            //Console.WriteLine((e.ChangeType == WatcherChangeTypes.Changed ? "更改:" : "创建") + e.FullPath);
            IConfigManager access = FindProcessor(e.FullPath);
            if (access != null)
            {
                try
                {
                    access.Load(e.FullPath);
                }
                catch { }
            }
        }

        private bool checkFile(string fullPath)
        {
            return File.Exists(fullPath) && (!Path.GetDirectoryName(fullPath).Contains(".svn"));
        }

        private bool EnableEvent(string fullPath)
        {
            lock (m_watcher)
            {
                if (!checkFile(fullPath)) return false;
                string path = fullPath.ToLower();
                lock (m_filesTime)
                {
                    DateTime lastModifyTime = File.GetLastWriteTime(fullPath);
                    DateTime prevModifyTime;
                    if (m_filesTime.TryGetValue(path, out prevModifyTime))
                    {
                        if (lastModifyTime <= prevModifyTime)
                        {
                            return false;
                        }
                        else
                        {
                            m_filesTime[path] = lastModifyTime;
                        }
                    }
                    else
                    {
                        m_filesTime[path] = lastModifyTime;
                    }
                }
                return true;
            }
        }

        string[] SplitDirectory(string fullPath)
        {
            string sub = fullPath.Substring(m_path.Length);
            return sub.Split(new char[] { Path.DirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// 注册处理器
        /// </summary>
        /// <param name="access"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public bool RegistConfigProcessor(IConfigManager access, params string[] keys)
        {
            string key = keys.Aggregate((t, path) => t + Path.DirectorySeparatorChar + path);
            if (key == "*")
            {
                m_defaultManager = access;
            }
            else
            {
                m_access.Add(key, access);
            }
            return true;
        }

        /// <summary>
        /// 查找处理器 
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        IConfigManager FindProcessor(string fullPath)
        {
            IConfigManager access;
            string key = fullPath.Substring(m_path.Length + 1);
            while (!m_access.TryGetValue(key, out access))
            {
                int index = key.LastIndexOf(Path.DirectorySeparatorChar);
                if (index < 0)
                {
                    return m_defaultManager;
                    //m_access.TryGetValue("*", out access);
                }
                key = key.Substring(0, index);
            }
            return access;
        }

        public void LoadAll(Action<string> log = null)
        {
            var op = m_watcher.IncludeSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            String[] files = System.IO.Directory.GetFiles(m_path, m_watcher.Filter, op);
            foreach (var file in files)
            {
                if (checkFile(file))
                {
                    IConfigManager access = FindProcessor(file);
                    if (access != null)
                    {
                        try
                        {
                            access.Load(file);
                        }
                        catch (Exception err)
                        {
                            if (log != null)
                            {
                                log(err.Message + ":" + file);
                            }
                        }
                    }
                }
            }
        }
    }
}
