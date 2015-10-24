using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace Sinan.BabyAssistant.Command
{
    class Config
    {
        private string _xpath;
        private string _localpath;
        private Dictionary<string, Server> _serverlist = new Dictionary<string, Server>();
        private List<string> _restartpath=new List<string>();
        private List<string> _noupdate = new List<string>();

        /// <summary>
        /// svn服务地址
        /// </summary>
        public string XPath 
        {
            get { return _xpath; }
            set { _xpath = value; }
        }

        /// <summary>
        /// svn本地文件
        /// </summary>
        public string SvnLocalPath 
        {
            get { return _localpath; }
            set { _localpath = value; }
        }

        /// <summary>
        /// 服务列表
        /// </summary>
        public Dictionary<string, Server> ServerList 
        {
            get { return _serverlist; }
            set { _serverlist = value; }
        }

        /// <summary>
        /// 需要重启服务的目录
        /// </summary>
        public List<string> RestartPath
        {
            get { return _restartpath; }
            set { _restartpath = value; }
        }

        /// <summary>
        /// 不需要更新内容
        /// </summary>
        public List<string> NoUpdate 
        {
            get { return _noupdate; }
            set { _noupdate = value; }
        }

        public Config() 
        {
            GetSericeList();
        }

        /// <summary>
        /// 取得基本配置
        /// </summary>
        private void GetSericeList()
        {
            XmlDocument doc = new XmlDocument();
            string xpath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Command", "sm.xml");
            doc.Load(xpath);

            XmlNodeList list = doc.SelectNodes("//list");
            foreach(XmlNode node in list)
            {
                foreach (XmlNode n in node.ChildNodes) 
                {
                    switch (n.Name) 
                    {
                        case "server":
                            Server model = new Server();
                            model.Name = n.Attributes["name"].Value;
                            model.Path = n.Attributes["path"].Value;
                            if (_serverlist.ContainsKey(model.Name))
                                continue;
                            _serverlist.Add(model.Name, model);
                            break;
                        case "restart":
                            _restartpath.Add(n.InnerText);
                            break;
                        case "noupdate":
                            _noupdate.Add(n.InnerText);
                            break;
                    }
                }
            }
        }
    }
}
