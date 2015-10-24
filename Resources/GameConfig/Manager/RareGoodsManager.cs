using System.Collections.Generic;
using System.IO;
using System.Text;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FastConfig;
using Sinan.FastJson;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 稀有物品管理
    /// </summary>
    public class RareGoodsManager : IConfigManager
    {
        readonly static RareGoodsManager m_instance = new RareGoodsManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static RareGoodsManager Instance
        {
            get { return m_instance; }
        }

        RareGoodsManager() { }

        /// <summary>
        /// 稀有物品获得推送消息
        /// </summary>
        Dictionary<string, string> m_rareGoods;
        string[] m_msg = new string[256];

        public void Load(string path)
        {
            Variant v = VariantWapper.LoadVariant(path);
            if (v != null)
            {
                //SetMsg(v, GoodsSource.KillApc);
                //SetMsg(v, GoodsSource.OpenBox);
                //SetMsg(v, GoodsSource.Clap);
                //SetMsg(v, GoodsSource.DrawCard);
                Dictionary<string, string> rareGoods = new Dictionary<string, string>();
                foreach (var item in v)
                {
                    rareGoods.Add(item.Key, item.Value.ToString());
                }
                m_rareGoods = rareGoods;
            }
        }

        public string GetMsgTo(string goodsID)
        {
            string position;
            m_rareGoods.TryGetValue(goodsID, out position);
            return position;
        }

        public void Unload(string path)
        {
        }

        public string GetMsg(string sceneName, string playerName, string goods, GoodsSource source)
        {
            var config = GameConfigAccess.Instance.FindOneById(goods);
            if (config == null)
            {
                return null;
            }
            int index = (int)source;
            switch (source)
            {
                case GoodsSource.KillApc:
                    index = GoodsReturn.RareKillApc;
                    break;
                case GoodsSource.OpenBox:
                    index = GoodsReturn.RareOpenBox;
                    break;
                case GoodsSource.Clap:
                    index = GoodsReturn.RareClap;
                    break;
                case GoodsSource.DrawCard:
                    index = GoodsReturn.RareDrawCard;
                    break;
                default:
                    return null;
            }
            string msg = TipManager.GetMessage(index);
            if (string.IsNullOrEmpty(msg))
            {
                return null;
            }
            return string.Format(msg, playerName, sceneName, config.Name);
        }

        //void SetMsg(Variant v, GoodsSource souce)
        //{
        //    try
        //    {
        //        string key = souce.ToString();
        //        string msg = v.GetStringOrDefault(key);
        //        v.Remove(key);
        //        if (string.IsNullOrEmpty(msg))
        //        {
        //            return;
        //        }
        //        //此处为了检查消息格式是否正确
        //        key = string.Format(msg, "playerName", "sceneName", "goodsName");
        //        m_msg[(int)souce] = msg;
        //    }
        //    catch { }
        //}
    }
}
