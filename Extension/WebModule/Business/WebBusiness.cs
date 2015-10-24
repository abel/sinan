using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

using Sinan.FrontServer;
using Sinan.FastJson;
using Sinan.Data;
using Sinan.Util;
using Sinan.Command;
using System.Collections;
using Sinan.GameModule;
using System.Xml;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.WebModule.Business
{
    class WebBusiness
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();        
        /// <summary>
        /// 激合码奖励
        /// </summary>
        /// <param name="note"></param>
        public static void CodeAward(UserNote note)
        {                                
            PlayerBusiness pb = note.Player;
            string soleid = pb.ID + "CodeAward";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                string code = note.GetString(0);
                Variant ps = new Variant();
                ps["code"] = code;
                ps["opuid"] = pb.ID;
                ps["opopenid"] = pb.UserID;
                ps["name"] = pb.Name;
                ps["worldid"] = ConfigLoader.Config.Zoneid;
                XmlNodeList nodes = MallAccess.QueryPostWebService(ConfigLoader.Config.WebAddress, "JhACode", ps);
                if (nodes == null) 
                {
                    return;
                }
                string msg = nodes[1].InnerText;
                if (string.IsNullOrEmpty(msg))
                {
                    note.Call(WebCommand.CodeAwardR, false, msg);
                    return;
                }
                Variant v = JsonConvert.DeserializeObject<Variant>(msg);
                if (v.GetIntOrDefault("ok") == 0)
                {
                    note.Call(WebCommand.CodeAwardR, false, v.GetStringOrDefault("message"));
                    return;
                }

                //取得
                IList info = v.GetValue<IList>("message");
                List<Variant> goodsList = new List<Variant>();

                foreach (Variant item in info)
                {
                    string goodsid = item.GetStringOrDefault("GoodsID");
                    int number = item.GetIntOrDefault("Number");
                    string isbinding = item.GetStringOrDefault("IsBinding");
                    Variant gs = new Variant();
                    gs.Add("G", goodsid);
                    gs.Add("A", number);
                    gs.Add("E", goodsid);
                    gs.Add("H", isbinding);
                    if (number > 0)
                    {
                        goodsList.Add(gs);
                    }
                }

                string title = v.GetStringOrDefault("title");
                string content = v.GetStringOrDefault("content");
                int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                if (EmailAccess.Instance.SendEmail(title, TipManager.GetMessage(PetsReturn.StealPet12), pb.ID, pb.Name, content, string.Empty, goodsList, reTime))
                {
                    if (pb.Online)
                    {
                        pb.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(pb.ID));
                    }
                }
                note.Call(WebCommand.CodeAwardR, true, "");
            }
            finally 
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 取得活动列表
        /// </summary>
        /// <param name="note"></param>
        public static void PartList()
        {
            Variant ps = new Variant();
            ps["worldid"] = ConfigLoader.Config.Zoneid;
            XmlNodeList nodes = MallAccess.QueryPostWebService(ConfigLoader.Config.WebAddress, "GetPart", ps);
            if (nodes == null)
                return;
            string msg = nodes[1].InnerText;
            if (string.IsNullOrEmpty(msg))
                return;
            Variant v = JsonConvert.DeserializeObject<Variant>(msg);
            if (v == null)
                return;
            if (v.GetIntOrDefault("ok") == 0)
                return;
            //取得活动列表
            IList info = v.GetValue<IList>("message");
            if (info == null)
                return;
            if (info.Count == 0)
                return;
            MallAccess.UpdatePartList(info);
        } 
    }
}
