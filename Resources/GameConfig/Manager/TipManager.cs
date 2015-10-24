using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 提示信息配置
    /// </summary>
    sealed public class TipManager : IConfigManager
    {
        readonly static TipManager m_instance = new TipManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static TipManager Instance
        {
            get { return m_instance; }
        }
        private TipManager() { }

        Dictionary<string, string> m_keys;
        Dictionary<int, string> m_fast;

        public void Load(string path)
        {
            Variant tips = VariantWapper.LoadVariant(path);
            if (tips != null)
            {
                Dictionary<string, string> dic = new Dictionary<string, string>();
                Dictionary<int, string> fast = new Dictionary<int, string>();
                foreach (var item in tips)
                {
                    Variant value = item.Value as Variant;
                    if (value != null)
                    {
                        string v = value.GetStringOrEmpty("value");
                        dic.Add(item.Key, v);
                        int key;
                        if (int.TryParse(item.Key, out key))
                        {
                            fast.Add(key, v);
                        }
                    }
                }
                m_keys = dic;
                m_fast = fast;
                LoadBufferType(dic);
            }
        }

        /// <summary>
        /// 重新加载Buffer名
        /// </summary>
        /// <param name="keys"></param>
        static void LoadBufferType(Dictionary<string, string> keys)
        {
            string value;
            Type t = typeof(BufferType);
            FieldInfo[] fs = t.GetFields(BindingFlags.Static | BindingFlags.Public);
            foreach (FieldInfo f in fs)
            {
                string key = f.Name;
                if (keys.TryGetValue(key, out value))
                {
                    f.SetValue(null, value);
                }
            }
            if (keys.TryGetValue("PetSkillSuffix", out value))
            {
                SkillHelper.PetSkillSuffix = value;
            }
        }

        public void Unload(string path)
        {
            m_keys.Clear();
            m_fast.Clear();
        }

        /// <summary>
        /// 获取提示信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static public string GetMessage(int id)
        {
            string msg;
            if (m_instance.m_fast.TryGetValue(id, out msg))
            {
                if (string.IsNullOrEmpty(msg))
                    return id.ToString();
                return msg;
            }
            return id.ToString();
        }

        /// <summary>
        /// 获取提示信息
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        static public string GetMessage(string key)
        {
            string msg;
            if (m_instance.m_keys.TryGetValue(key, out msg))
            {
                return msg;
            }
            return key;
        }
    }
}
