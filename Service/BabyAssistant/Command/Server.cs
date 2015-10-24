using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.BabyAssistant.Command
{
    /// <summary>
    /// 服务列表
    /// </summary>
    class Server
    {
        private string _name;
        private string _path;
        
        /// <summary>
        /// 服务名称
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        /// <summary>
        /// 服务地址
        /// </summary>
        public string Path
        {
            get { return _path; }
            set { _path = value; }
        }
    }
}
