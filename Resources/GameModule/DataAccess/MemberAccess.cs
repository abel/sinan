using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 得到会员的基本配置
    /// </summary>
    public class MemberAccess
    {
        /// <summary>
        /// 会员基本配置
        /// </summary>
        private static string id = "M_0000";
        private static GameConfig gc = GameConfigAccess.Instance.FindOneById(id);

        /// <summary>
        /// 得到会员基本配置
        /// </summary>
        /// <returns></returns>
        public static GameConfig MemberGC
        {
            get
            { 
                return gc; 
            }
        }

        /// <summary>
        /// 取得会员最大值
        /// </summary>
        /// <returns></returns>
        public static int MemberMax() 
        {            
            Variant v = gc.Value;
            //得到最大等级      
            int max = 0;
            if (v != null)
            {
                foreach (var item in v)
                {
                    int cur = 0;
                    if (int.TryParse(item.Key, out cur) && max < cur)
                    {
                        max = cur;
                    }
                }
            }
            return max;
        }

        /// <summary>
        /// 得取会员对
        /// </summary>
        /// <param name="mlevel">会员等级</param>
        /// <returns>对应等级基本信息</returns>
        public static Variant MemberInfo(int mlevel)
        {
            if (gc != null)
            {
                Variant v = gc.Value;
                if (v != null)
                {
                    return v.GetVariantOrDefault(mlevel.ToString());
                }
            }
            return null;
        }

    }
}
