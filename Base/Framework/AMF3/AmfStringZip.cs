using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.AMF3
{
    /// <summary>
    /// 字符串压缩类,使用字典
    /// </summary>
    public class AmfStringZip
    {
        readonly protected int m_capacity;
        protected string[] m_decodeStrings;
        protected Dictionary<string, int> m_encodeStrings;

        public int Capacity
        {
            get { return m_capacity; }
        }

        public AmfStringZip(int capacity)
        {
            m_capacity = capacity;
            m_decodeStrings = new string[capacity];
            m_encodeStrings = new Dictionary<string, int>(capacity << 1);
        }

        public string ReadString(int handle)
        {
            return m_decodeStrings[handle];
        }

        public bool ReadIndex(string v, out int handle)
        {
            return m_encodeStrings.TryGetValue(v, out handle);
        }

        protected virtual bool AddKey(string key)
        {
            int index = m_encodeStrings.Count;
            if (index < m_capacity)
            {
                m_decodeStrings[index] = key;
                m_encodeStrings.Add(key, index);
                return true;
            }
            return false;
        }

        public virtual void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string item = sr.ReadLine();
                while (item != null)
                {
                    string key = JsonConvert.DeserializeObject<Variant>(item).ToString();
                    if (!AddKey(key))
                    {
                        return;
                    }
                }
            }
        }
    }
}
