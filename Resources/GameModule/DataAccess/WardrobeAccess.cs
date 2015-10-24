using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class WardrobeAccess
    {
        private static string SubType = "ShiZhuang";
        
        /// <summary>
        /// 取得当前上线时装
        /// </summary>
        /// <returns></returns>
        public static HashSet<string> Wardrobe() 
        {
            HashSet<string> hs = new HashSet<string>();
            //得取时装列表
            List<GameConfig> list = GameConfigAccess.Instance.Find(MainType.Mall, SubType);
            foreach (GameConfig gc in list)
            {
                Variant v = gc.Value;
                if (v == null) 
                    continue;

                DateTime dt = DateTime.UtcNow;
                string ud = v.GetStringOrDefault("UpDate");                
                if (!string.IsNullOrEmpty(ud))
                {
                    DateTime update = v.GetDateTimeOrDefault("UpDate").ToUniversalTime();
                    if (dt < update)
                    {
                        //没有上架
                        continue;
                    }              
                }

                string dd = v.GetStringOrDefault("DownDate");                
                if (!string.IsNullOrEmpty(dd))
                {
                    DateTime endDate = v.GetDateTimeOrDefault("DownDate").ToUniversalTime();
                    if (endDate < dt)
                    {
                        //下架时间
                        continue;
                    }            
                }
                string goodsid = gc.Value.GetStringOrDefault("GoodsID");
                if (string.IsNullOrEmpty(goodsid))
                    continue;
                if (!MallAccess.HS.Contains(goodsid))
                    continue;
                hs.Add(goodsid);
            }
            return hs;
        }
    }
}
