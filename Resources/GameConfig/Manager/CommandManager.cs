using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Sinan.AMF3;
using Sinan.FastConfig;

namespace Sinan.GameModule
{
    /// <summary>
    /// 提示信息配置
    /// </summary>
    sealed public class CommandManager : AmfStringZip, IConfigManager
    {
        /// <summary>
        /// 最大命令(不包括该值)
        /// </summary>
        public const int MaxCommand = 8000;

        /// <summary>
        /// 执行命令的基本控制功能
        /// (用户必须间隔指定时间才能执行同一命令)
        /// </summary>
        readonly public static int[] ControlTicks = new int[MaxCommand];

        readonly static CommandManager m_instance = new CommandManager();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static CommandManager Instance
        {
            get { return m_instance; }
        }

        private CommandManager()
            : base(MaxCommand)
        {
        }

        public override void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string[] decodeStrings = new string[m_capacity];
                Dictionary<string, int> encodeStrings = new Dictionary<string, int>(m_capacity);
                string item = sr.ReadLine();
                while (item != null)
                {
                    if (item.Length > 3 && item[0] != '/' && item[0] != '*')
                    {
                        string[] values = item.Split(new char[] { ' ', ',', ':', '"' }, StringSplitOptions.RemoveEmptyEntries);
                        if (values.Length >= 2)
                        {
                            int command;
                            if (int.TryParse(values[0], out command) && command >= 0 && command < MaxCommand)
                            {
                                string key = values[1];
                                key = String.Intern(key);
                                if (key != string.Empty)
                                {
                                    if (m_encodeStrings.ContainsKey(key))
                                    {
                                        Console.WriteLine("Duplicate command:" + key);
                                    }
                                    encodeStrings[key] = command;
                                    decodeStrings[command] = key;
                                }
                                if (values.Length >= 3)
                                {
                                    int d;
                                    if (int.TryParse(values[2], out d))
                                    {
                                        ControlTicks[command] = d * 10000;
                                    }
                                }
                                else
                                {
                                    ControlTicks[command] = 100 * 10000; //默认100毫秒
                                }
                            }
                        }
                    }
                    item = sr.ReadLine();
                }
                m_decodeStrings = decodeStrings;
                m_encodeStrings = encodeStrings;
            }
        }

        public void Unload(string path)
        {
            Array.Clear(m_decodeStrings, 0, m_decodeStrings.Length);
            m_encodeStrings.Clear();
        }
    }
}
