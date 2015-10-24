﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Extensions;

namespace Sinan.Util
{
    public class TrieNode
    {
        internal protected bool m_end;
        protected Dictionary<char, TrieNode> m_values;
        public TrieNode()
        {
            m_values = new Dictionary<char, TrieNode>();
        }

        public bool TryGetValue(char c, out TrieNode node)
        {
            return m_values.TryGetValue(c, out node);
        }

        public TrieNode Add(char c)
        {
            TrieNode subnode;
            if (!m_values.TryGetValue(c, out subnode))
            {
                subnode = new TrieNode();
                m_values.Add(c, subnode);
            }
            return subnode;
        }
    }

    public class TrieFilter : TrieNode, Sinan.Util.IWordFilter
    {
        /// <summary>
        /// 添加关键字
        /// </summary>
        /// <param name="key"></param>
        public void AddKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            TrieNode node = this;
            for (int i = 0; i < key.Length; i++)
            {
                char c = key[i].GetSimp();
                node = node.Add(c);
            }
            node.m_end = true;
        }

        /// <summary>
        /// 检查是否包含非法字符
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <returns>找到的第1个非法字符.没有则返回string.Empty</returns>
        public string FindOne(string text)
        {
            for (int head = 0; head < text.Length; head++)
            {
                int index = head;
                TrieNode node = this;
                while (node.TryGetValue(text[index].GetSimp(), out node))
                {
                    if (node.m_end)
                    {
                        return text.Substring(head, index - head + 1);
                    }
                    if (text.Length == ++index)
                    {
                        break;
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// 查找所有非法字符
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public List<string> FindAll(string text)
        {
            List<string> result = new List<string>();
            for (int head = 0; head < text.Length; head++)
            {
                int index = head;
                TrieNode node = this;
                while (node.TryGetValue(text[index].GetSimp(), out node))
                {
                    if (node.m_end)
                    {
                        result.Add(text.Substring(head, index - head + 1));
                    }
                    if (text.Length == ++index)
                    {
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 替换非法字符
        /// </summary>
        /// <param name="text"></param>
        /// <param name="mask">用于代替非法字符</param>
        /// <returns>替换后的字符串</returns>
        public string Replace(string text, char mask = '*')
        {
            char[] chars = null;
            for (int head = 0; head < text.Length; head++)
            {
                int index = head;
                TrieNode node = this;
                while (node.TryGetValue(text[index].GetSimp(), out node))
                {
                    if (node.m_end)
                    {
                        if (chars == null) chars = text.ToArray();
                        for (int i = head; i <= index; i++)
                        {
                            chars[i] = mask;
                        }
                        head = index;
                    }
                    if (text.Length == ++index)
                    {
                        break;
                    }
                }
            }
            return chars == null ? text : new string(chars);
        }
    }
}
