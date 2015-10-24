using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FastConfig;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class StringFilter : IConfigManager
    {
        readonly static StringFilter m_instance = new StringFilter();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static StringFilter Instance
        {
            get { return m_instance; }
        }

        private StringFilter() { }

        IWordFilter m_trieWord;

        /// <summary>
        /// 检查是否包含非法字符
        /// </summary>
        /// <param name="name"></param>
        /// <returns>找到的第1个非法字符.没有则返回string.Empty</returns>
        public string GetBadWord(string name)
        {
            return m_trieWord.FindOne(name);
        }

        public IEnumerable<string> FindAll(string text)
        {
            return m_trieWord.FindAll(text);
        }

        /// <summary>
        /// 替换非法字符
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ReplacetBadWord(string name)
        {
            return m_trieWord.Replace(name, '*');
        }

        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                //IWordFilter trieFilter = new FastFilter();
                IWordFilter trieFilter = new TrieFilter();
                string key = sr.ReadLine();
                while (key != null)
                {
                    if (key != string.Empty)
                    {
                        //x.Add(key);
                        trieFilter.AddKey(key);
                    }
                    key = sr.ReadLine();
                }
                m_trieWord = trieFilter;
            }
        }

        public void Unload(string fullPath)
        {
        }
    }
}
