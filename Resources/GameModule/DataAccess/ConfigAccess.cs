using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.GameModule
{
    /// <summary>
    /// 访问配置文件.
    /// </summary>
    sealed public partial class GameConfigAccess : VariantBuilder<GameConfig>, IConfigManager
    {
        readonly static GameConfigAccess m_instance = new GameConfigAccess();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static GameConfigAccess Instance
        {
            get { return m_instance; }
        }

        GameConfigAccess()
            : base("GameConfig")
        { }

        ConcurrentDictionary<string, GameConfig> m_configs = new ConcurrentDictionary<string, GameConfig>();

        /// <summary>
        /// 获取配置内容
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public List<GameConfig> FindByIDList(IList ids)
        {
            List<GameConfig> configs = new List<GameConfig>(ids.Count);
            foreach (var id in ids)
            {
                GameConfig config;
                if (m_configs.TryGetValue(id.ToString(), out config))
                {
                    configs.Add(config);
                }
            }
            return configs;
        }

        public List<Variant> FindAllVariant()
        {
            return m_collection.FindAllAs<Variant>().ToList();
        }

        /// <summary>
        /// 获取所有配置信息
        /// </summary>
        /// <param name="mainType">主类型</param>
        /// <param name="subType">子类型</param>
        /// <returns></returns>
        public List<GameConfig> Find(string mainType, string subType = "")
        {
            List<GameConfig> configs = new List<GameConfig>();
            foreach (var item in m_configs)
            {
                var config = item.Value;
                if (config.MainType == mainType)
                {
                    if (string.IsNullOrEmpty(subType) || config.SubType == subType)
                    {
                        configs.Add(config);
                    }
                }
            }
            return configs;
        }

        public override GameConfig FindOneById(string ID)
        {
            if (string.IsNullOrEmpty(ID))
            {
                return null;
            }
            GameConfig config;
            m_configs.TryGetValue(ID, out config);
            return config;
        }

        /// <summary>
        /// 扩展基本配置[得到机率]
        /// </summary>
        /// <param name="name">操作名称</param>
        /// <param name="value">操作类型</param>
        /// <param name="number">需要道具数量</param>       
        /// <param name="goodsid">需要的道具</param>
        /// <returns>附加机率</returns>
        public double FindExtend(string name, string value, out int number, out string goodsid)
        {
            GameConfig gc = FindOneById("ED_00001");
            if (gc == null)
            {
                number = 0;
                goodsid = "";
                return 0;
            }
            Variant v = gc.Value.GetVariantOrDefault(name);
            if (v == null)
            {
                number = 0;
                goodsid = "";
                return 0;
            }

            Variant t = v.GetVariantOrDefault(value);
            int max = t.GetIntOrDefault("Max");
            double uplv = t.GetDoubleOrDefault("UpLv");

            number = max;
            goodsid = v.GetStringOrDefault("GoodsID");
            return uplv;
        }

        /// <summary>
        /// 得到相关配置信息
        /// </summary>
        /// <param name="mainType">主类型</param>
        /// <param name="subType">子类型</param>
        /// <param name="name">名称</param>
        /// <returns></returns>
        public GameConfig Find(string mainType, string subType, string name)
        {
            foreach (var item in m_configs)
            {
                var config = item.Value;
                if (config.MainType == mainType && config.Name == name)
                {
                    if (string.IsNullOrEmpty(subType) || config.SubType == subType)
                    {
                        return config;
                    }
                }
            }
            return null;
        }


        Dictionary<string, int> dic = new Dictionary<string, int>(1000);

        /// <summary>
        /// 得到物品堆叠数
        /// </summary>
        /// <param name="goodsid">道具id</param>
        /// <returns></returns>
        public int GetStactCount(string goodsid)
        {
            int count = 0;
            try
            {
                if (!dic.TryGetValue(goodsid, out count))
                {
                    GameConfig gc = FindOneById(goodsid);
                    if (gc != null)
                    {
                        Variant v = gc.Value;
                        if (v != null)
                        {
                            count = v.GetIntOrDefault("StactCount");
                            dic.Add(goodsid, count);
                        }
                    }
                }
            }
            catch { }
            return count;
        }

        /// <summary>
        /// 得到道具购买价格
        /// </summary>
        /// <param name="goodsid">道具id</param>
        /// <returns></returns>
        public int GetPrice(string goodsid)
        {
            GameConfig gc = FindOneById(goodsid);
            if (gc == null)
                return 0;
            Variant v = gc.Value;
            if (v == null)
                return 0;
            Variant price = v.GetVariantOrDefault("Price");
            if (price == null)
                return 0;
            Variant buy = price.GetVariantOrDefault("Buy");
            if (buy == null)
                return 0;
            return buy.GetIntOrDefault("Score");
        }

        Dictionary<string, GameConfig> allBufferUI;
        public GameConfig FindBuffer(string name)
        {
            if (allBufferUI == null)
            {
                allBufferUI = new Dictionary<string, GameConfig>();
                var v = Find(MainType.Buffer);
                foreach (var item in v)
                {
                    allBufferUI.Add(item.Name, item);
                }
            }
            GameConfig config;
            allBufferUI.TryGetValue(name, out config);
            return config;
        }

        public void Load(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string x = sr.ReadToEnd();
                Variant v = JsonConvert.DeserializeObject<Variant>(x);
                if (v != null)
                {
                    GameConfig gc = new GameConfig();
                    string id = Path.GetFileNameWithoutExtension(path);
                    gc.ID = string.Intern(id);
                    gc.MainType = string.Intern(v.GetStringOrDefault("MainType"));
                    gc.SubType = string.Intern(v.GetStringOrDefault("SubType"));
                    gc.UI = v.GetVariantOrDefault("UI");
                    gc.Value = v.GetVariantOrDefault("Value");
                    gc.Ver = v.GetIntOrDefault("Ver");
                    gc.Name = v.GetStringOrDefault("Name");
                    gc.Modified = v.GetDateTimeOrDefault("Modified");

                    m_configs[id] = gc;
                }
            }
        }

        public void Unload(string path)
        {
            string id = Path.GetFileNameWithoutExtension(path);
            GameConfig gc;
            m_configs.TryRemove(id, out gc);
        }

        public const char ObjectHead = '{';
        public const char ObjectEnd = '}';
        public const char ArrayHead = '[';
        public const char ArrayEnd = ']';
        public const char Quote = '"';
        public const char KeySplit = ':';
        public const char ValueSplit = ',';
        public const char Escape = '\\';

        public static bool CheckKey(IDictionary<string, object> v)
        {
            foreach (var item in v)
            {
                foreach (var c in item.Key)
                {
                    bool r = (c == '{' || c == '}' || c == '"' || c == ':' || c == ',' || c == '\\');
                    if (r)
                    {
                        return false;
                    }
                }
                if (item.Value is IDictionary<string, object>)
                {
                    bool r = CheckKey(item.Value as IDictionary<string, object>);
                    if (!r)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
