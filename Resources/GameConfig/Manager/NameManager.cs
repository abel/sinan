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
    sealed public class NameManager : IConfigManager
    {
        readonly static NameManager m_instance = new NameManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static NameManager Instance
        {
            get { return m_instance; }
        }

        private NameManager() { }

        //保留名字.
        HashSet<string> m_reserver;
        byte[] m_whiteChar;

        /// <summary>
        /// 检查名字是否可用
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string CheckName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return TipManager.GetMessage(UserPlayerReturn.NameLimit1);
            }
            int maxLen = ConfigLoader.Config.MaxNameLen;
            int len = System.Text.Encoding.Default.GetByteCount(name);
            if (maxLen > 0 && len > maxLen)
            {
                return TipManager.GetMessage(UserPlayerReturn.NameLimit1);
            }

            foreach (char c in name)
            {
                //'\u0021' 到 '\u007e' 的字符
                //!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
                if (m_whiteChar[c] == 0)
                {
                    //return "不能使用字符:" + c;
                    return TipManager.GetMessage(UserPlayerReturn.NameLimit2) + c;
                }
            }
            //系统保留
            if (m_reserver != null && m_reserver.Contains(name))
            {
                return TipManager.GetMessage(UserPlayerReturn.NameLimit4);
            }
            //过滤用户名:
            string badWord = StringFilter.Instance.GetBadWord(name);
            if (badWord != string.Empty)
            {
                return TipManager.GetMessage(UserPlayerReturn.NameLimit3) + badWord;
            }
            int state = WordAccess.Instance.GetState(name);
            if (state > Word.AutoCount)
            {
                return TipManager.GetMessage(UserPlayerReturn.NameLimit4);
            }
            return string.Empty;
        }


        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                byte[] whiteChar = new byte[char.MaxValue + 1];
                HashSet<string> reserver = new HashSet<string>();
                string text = sr.ReadLine();
                while (text != null)
                {
                    text = text.Trim();
                    if (text.Length > 0 && (!text.StartsWith("//")) && (!text.StartsWith("--")))
                    {
                        if (text[0] == '#')
                        {
                            reserver.Add(text.Substring(1));
                        }
                        else
                        {
                            if (text.Length >= 10 && text[4] == '-' && (text[9] == ':' || text[9] == '：'))
                            {
                                //分割
                                UInt16 start, end;
                                UInt16.TryParse(text.Substring(0, 4), NumberStyles.HexNumber, null, out start);
                                UInt16.TryParse(text.Substring(5, 4), NumberStyles.HexNumber, null, out end);
                                if (start < end)
                                {
                                    for (char c = (char)start; c <= end; c++)
                                    {
                                        whiteChar[c] = 1;
                                    }
                                }
                            }
                            else
                            {
                                foreach (var c in text)
                                {
                                    whiteChar[c] = 1;
                                }
                            }
                        }
                    }
                    text = sr.ReadLine();
                }
                m_whiteChar = whiteChar;
                m_reserver = reserver;
            }
        }

        public void Unload(string fullPath)
        {
            m_reserver.Clear();
        }

    }
}
