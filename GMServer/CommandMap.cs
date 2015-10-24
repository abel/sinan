using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.AMF3;

namespace Sinan.GMServer
{
    /// <summary>
    /// 提示信息配置
    /// </summary>
    sealed public class CommandMap : AmfStringZip
    {
        /// <summary>
        /// 最大命令(不包括该值)
        /// </summary>
        public const int MaxCommand = 8000;

        public CommandMap() :
            base(8000)
        { }

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
                                        Console.WriteLine("命令重复:" + key);
                                    }
                                    encodeStrings[key] = command;
                                    decodeStrings[command] = key;
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