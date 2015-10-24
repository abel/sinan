using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 祝福值基本配置
    /// </summary>
    public class ZhuFuAccess
    {
        /// <summary>
        /// 会员基本配置
        /// </summary>
        private static string id = "ZF_000001";
        private static GameConfig gc = GameConfigAccess.Instance.FindOneById(id);

        /// <summary>
        /// 得到会员基本配置
        /// </summary>
        /// <returns></returns>
        public static GameConfig ZhuFuGC
        {
            get
            {
                return gc;
            }
        }

        /// <summary>
        /// 取得祝福基本信息
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="level">阶级</param>
        /// <returns></returns>
        public static Variant ZhuFuInfo(string name, int level)
        {
            if (gc == null)
                return null;
            Variant v = gc.Value;
            if (v == null)
                return null;
            Variant info = v.GetVariantOrDefault(name);
            if (info == null)
                return null;
            return info.GetVariantOrDefault(level.ToString());
        }
    }
}
