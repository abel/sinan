using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 属性计算公式类
    /// </summary>
    public class PropertyHelper
    {
        /// <summary>
        /// 生成完整的玩家属性
        /// </summary>
        /// <param name="roleID">角色ID</param>
        /// <param name="level">玩家等级</param>
        /// <param name="life">附加加成属性</param>
        /// <returns></returns>
        static public PlayerProperty CreateProperty(string roleID, int level, PlayerProperty life)
        {
            level = Math.Min(level, APCProperty.MaxLevel);
            //0级属性.只受直接加成的影响
            //伤害抗性
            life.KangShangHai += 0;
            //反击率
            life.FanJiLv = Math.Min(1, life.FanJiLv + 0.05);
            //合击率
            life.HeJiLv = Math.Min(1, life.HeJiLv + 0.5);
            //逃跑率f
            life.TaoPaoLv = Math.Min(1, life.TaoPaoLv + 0.8);

            //暴击伤害倍数1.5
            life.BaoJiShangHai = life.BaoJiShangHai + 1.5;
            //合击伤害倍数1.2
            life.HeJiShangHai = life.HeJiShangHai + 1.2;

            InitLevel1(roleID, level, life);
            return life;
        }

        /// <summary>
        /// 初始化第1层用户属性.
        /// </summary>
        static void InitLevel1(string roleID, int lv, PlayerProperty life)
        {
            if (roleID == "1")
            {
                life.LiLiang = life.LiLiang + (2 + 3 * lv);
                life.TiZhi = life.TiZhi + (2 + 2 * lv);
                life.ZhiLi = life.ZhiLi + 2;
                life.JingShen = life.JingShen + (2 + lv);

                life.SuDu = life.SuDu + (5 + 5 * lv);
                life.BaoJi = life.BaoJi + (45 + 5 * lv);
                life.MingZhong = life.MingZhong + (47 + 3 * lv);
                life.ShanBi = life.ShanBi + (48 + 2 * lv);
            }
            else if (roleID == "2")
            {
                life.LiLiang = life.LiLiang + 2;
                life.TiZhi = life.TiZhi + (2 + lv);
                life.ZhiLi = life.ZhiLi + (2 + 3 * lv);
                life.JingShen = life.JingShen + (2 + 2 * lv);

                life.SuDu = life.SuDu + (7 + 3 * lv);
                life.BaoJi = life.BaoJi + (46 + 4 * lv);
                life.MingZhong = life.MingZhong + (47 + 3 * lv);
                life.ShanBi = life.ShanBi + (45 + 5 * lv);
            }
            else if (roleID == "3")
            {
                life.LiLiang = life.LiLiang + ((2 + lv));
                life.TiZhi = life.TiZhi + (2 + 2 * lv);
                life.ZhiLi = life.ZhiLi + (2 + 2 * lv);
                life.JingShen = life.JingShen + (2 + lv);

                life.SuDu = life.SuDu + (6 + 4 * lv);
                life.BaoJi = life.BaoJi + (47 + 3 * lv);
                life.MingZhong = life.MingZhong + (46 + 4 * lv);
                life.ShanBi = life.ShanBi + (46 + 4 * lv);
            }
            InitLevel2(roleID, lv, life);
        }

        /// <summary>
        /// 初始化第2层用户属性.
        /// </summary>
        static void InitLevel2(string roleID, int lv, PlayerProperty life)
        {
            //二级属性.受1级属性和直接加成的影响..
            //暴击率
            life.BaoJiLv = life.BaoJiLv + life.BaoJi * 0.0001;
            //命中率
            life.MingZhongLv = life.MingZhongLv + life.MingZhong * 0.0001;
            //闪避率
            life.ShanBiLv = life.ShanBiLv + life.ShanBi * 0.0001;

            if (roleID == "1")
            {
                life.ShengMing = life.ShengMing + (35 + 150 * lv + 3 * life.LiLiang + 8 * life.TiZhi);
                life.MoFa = life.MoFa + (3 * life.ZhiLi + 4 * life.JingShen);
                life.GongJi = life.GongJi + life.LiLiang + 15;
                life.MoFaGongJi = life.MoFaGongJi + life.ZhiLi;
                life.FangYu = life.FangYu + life.TiZhi;
                life.MoFaFangYu = life.MoFaFangYu + life.JingShen;
            }
            else if (roleID == "2")
            {
                life.ShengMing = life.ShengMing + (35 + 100 * lv + 2 * life.LiLiang + 6 * life.TiZhi);
                life.MoFa = life.MoFa + (3 * life.ZhiLi + 7 * life.JingShen);
                life.GongJi = life.GongJi + life.LiLiang + 15;
                life.MoFaGongJi = life.MoFaGongJi + life.ZhiLi;
                life.FangYu = life.FangYu + life.TiZhi;
                life.MoFaFangYu = life.MoFaFangYu + life.JingShen;
            }
            else if (roleID == "3")
            {
                life.ShengMing = life.ShengMing + (35 + 120 * lv + 2 * life.LiLiang + 8 * life.TiZhi);
                life.MoFa = life.MoFa + (3 * life.ZhiLi + 5 * life.JingShen);
                life.GongJi = life.GongJi + life.LiLiang + 15;
                life.MoFaGongJi = life.MoFaGongJi + life.ZhiLi;
                life.FangYu = life.FangYu + life.TiZhi;
                life.MoFaFangYu = life.MoFaFangYu + life.JingShen;
            }
            InitLevel3(roleID, lv, life);
        }

        /// <summary>
        /// 初始化第3层用户属性.
        /// </summary>
        static void InitLevel3(string roleID, int lv, PlayerProperty life)
        {
            //三级属性.受2级属性和直接加成的影响
            //物理吸收
            life.WuLiXiShou = life.WuLiXiShou + ((1.0 * life.FangYu) / (3000 + life.FangYu));
            //魔法吸收
            life.MoFaXiShou = life.MoFaXiShou + ((1.0 * life.MoFaFangYu) / (3000 + life.MoFaFangYu));
        }
    }
}
