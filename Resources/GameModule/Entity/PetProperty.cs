using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Util;
using Sinan.GameModule;

namespace Sinan.Entity
{
    /// <summary>
    /// 宠物属性.
    /// </summary>
    [Serializable]
    public class PetProperty : PlayerProperty
    {
        /// <summary>
        /// 资质
        /// </summary>
        public int ZiZhi { set; get; }
        /// <summary>
        /// 成长度
        /// </summary>
        public int ChengChangDu { set; get; }
        /// <summary>
        /// 经验
        /// </summary>
        public int Experience { set; get; }

        public override Variant ToVariant()
        {
            Variant v = base.ToVariant();
            //基本属性 最大/当前
            v["ShengMing"] = MakeMV(ShengMing, ShengMing);
            v["MoFa"] = MakeMV(MoFa, MoFa);
            v["GongJi"] = MakeMV(GongJi, GongJi);
            v["MoFaGongJi"] = MakeMV(MoFaGongJi, MoFaGongJi);
            v["FangYu"] = MakeMV(FangYu, FangYu);
            v["MoFaFangYu"] = MakeMV(MoFaFangYu, MoFaFangYu);
            return v;
        }

        /// <summary>
        /// 将属性转化为动态对象
        /// </summary>
        /// <param name="level">等级</param>
        /// <returns></returns>
        public Variant ToVariant(int level)
        {
            Variant v = this.ToVariant();
            int maxExp = PetAccess.Instance.GetPetLevelMsg(level, 6);
            v["Experience"] = MakeMV(maxExp, this.Experience);
            return v;
        }

        /// <summary>
        /// 将属性转化为动态对象
        /// </summary>
        /// <param name="level">等级</param>
        /// <param name="sm">当前生命</param>
        /// <param name="mf">当前魔法</param>
        /// <param name="IsUp">是否升级</param>
        /// <returns>true升级</returns>
        public Variant ToVariant(int level, int sm, int mf)
        {
            Variant v = base.ToVariant();
            if (ShengMing < sm)
                v["ShengMing"] = MakeMV(ShengMing, ShengMing);
            else
                v["ShengMing"] = MakeMV(ShengMing, sm);


            if (MoFa < mf)
                v["MoFa"] = MakeMV(MoFa, MoFa);
            else
                v["MoFa"] = MakeMV(MoFa, mf);

            v["GongJi"] = MakeMV(GongJi, GongJi);
            v["MoFaGongJi"] = MakeMV(MoFaGongJi, MoFaGongJi);
            v["FangYu"] = MakeMV(FangYu, FangYu);
            v["MoFaFangYu"] = MakeMV(MoFaFangYu, MoFaFangYu);
            return v;
        }



        #region  MakeMV
        static Variant MakeMV(int M, int V)
        {
            Variant mv = new Variant(2);
            mv["M"] = M;
            mv["V"] = V;
            return mv;
        }
        static Variant MakeMV(double M, int V)
        {
            Variant mv = new Variant(2);
            mv["M"] = (int)M;
            mv["V"] = V;
            return mv;
        }
        static Variant MakeMV(double M, double V)
        {
            Variant mv = new Variant(2);
            mv["M"] = M;
            mv["V"] = V;
            return mv;
        }
        #endregion
        public static PlayerProperty CreatePlayerProperty(Variant v)
        {
            PlayerProperty life = new PlayerProperty();

            life.LiLiang = v.GetIntOrDefault("LiLiang");
            life.TiZhi = v.GetIntOrDefault("TiZhi");
            life.ZhiLi = v.GetIntOrDefault("ZhiLi");
            life.JingShen = v.GetIntOrDefault("JingShen");
            life.KangShangHai = v.GetIntOrDefault("KangShangHai");

            //-----------------------
            life.WuLiXiShou = v.GetDoubleOrDefault("WuLiXiShou");
            life.MoFaXiShou = v.GetDoubleOrDefault("MoFaXiShou");
            life.FanJiLv = v.GetDoubleOrDefault("FanJiLv");
            life.HeJiLv = v.GetDoubleOrDefault("HeJiLv");
            life.TaoPaoLv = v.GetDoubleOrDefault("TaoPaoLv");
            life.BaoJiShangHai = v.GetDoubleOrDefault("BaoJiShangHai");
            life.HeJiShangHai = v.GetDoubleOrDefault("HeJiShangHai");

            life.SuDu = v.GetIntOrDefault("SuDu");
            life.BaoJi = v.GetIntOrDefault("BaoJi");
            life.MingZhong = v.GetIntOrDefault("MingZhong");
            life.ShanBi = v.GetIntOrDefault("ShanBi");

            life.BaoJiLv = v.GetDoubleOrDefault("BaoJiLv");
            life.MingZhongLv = v.GetDoubleOrDefault("MingZhongLv");
            life.ShanBiLv = v.GetDoubleOrDefault("ShanBiLv");
            //----------------------------------
            life.ShengMing = v.GetVariantOrDefault("ShengMing").GetIntOrDefault("M");
            life.MoFa = v.GetVariantOrDefault("MoFa").GetIntOrDefault("M");
            life.GongJi = v.GetVariantOrDefault("GongJi").GetIntOrDefault("V");
            life.MoFaGongJi = v.GetVariantOrDefault("MoFaGongJi").GetIntOrDefault("V");
            life.FangYu = v.GetVariantOrDefault("FangYu").GetIntOrDefault("V");
            life.MoFaFangYu = v.GetVariantOrDefault("MoFaFangYu").GetIntOrDefault("V");

            return life;
        }

        public void AddPet(Variant v, int level)
        {
            //ShengMing:    double //生命
            //GongJi:       double //物理攻击
            //MoFaGongJi:   double //魔法攻击
            //SuDu:         double //速度
            //BaoJi:        double //暴击
            //MingZhong:    double //命中
            //ShanBi:       double //闪避
            ShengMing += (int)(v.GetDoubleOrDefault("ShengMing") * level);
            GongJi += (int)(v.GetDoubleOrDefault("GongJi") * level);
            MoFaGongJi += (int)(v.GetDoubleOrDefault("MoFaGongJi") * level);
            SuDu += (int)(v.GetDoubleOrDefault("SuDu") * level);
            BaoJi += (int)(v.GetDoubleOrDefault("BaoJi") * level);
            MingZhong += (int)(v.GetDoubleOrDefault("MingZhong") * level);
            ShanBi += (int)(v.GetDoubleOrDefault("ShanBi") * level);

            BaoJiShangHai += (v.GetDoubleOrDefault("BaoJiShangHai") * level);
            WuLiXiShou += (v.GetDoubleOrDefault("WuLiXiShou") * level);
            MoFaXiShou += (v.GetDoubleOrDefault("MoFaXiShou") * level);

            KangShangHai += (int)(v.GetDoubleOrDefault("KangShangHai") * level);
            MoFa += (int)(v.GetDoubleOrDefault("MoFa") * level);
            FangYu += (int)(v.GetDoubleOrDefault("FangYu") * level);
            MoFaFangYu += (int)(v.GetDoubleOrDefault("MoFaFangYu") * level);

            LiLiang += (int)(v.GetDoubleOrDefault("LiLiang") * level);
            TiZhi += (int)(v.GetDoubleOrDefault("TiZhi") * level);
            ZhiLi += (int)(v.GetDoubleOrDefault("ZhiLi") * level);
            JingShen += (int)(v.GetDoubleOrDefault("JingShen") * level);  
            
            TaoPaoLv += (v.GetDoubleOrDefault("TaoPaoLv") * level);

            //this.FanJiLv += (v.GetDoubleOrDefault("FanJiLv") * level);
            //this.HeJiLv += (v.GetDoubleOrDefault("HeJiLv") * level);
            //this.HeJiShangHai += (v.GetDoubleOrDefault("HeJiShangHai") * level);
            //this.BaoJiLv += (v.GetDoubleOrDefault("BaoJiLv") * level);
            //this.MingZhongLv += (v.GetDoubleOrDefault("MingZhongLv") * level);
            //this.ShanBiLv += (v.GetDoubleOrDefault("ShanBiLv") * level);
        }

        ///// <summary>
        ///// 添加家族技能
        ///// </summary>
        ///// <param name="v"></param>
        //public void AddPet(Variant v)
        //{
        //    if (v != null)
        //    {
        //        ShengMing += v.GetIntOrDefault("ShengMing");
        //        GongJi += v.GetIntOrDefault("GongJi");
        //        MoFaGongJi += v.GetIntOrDefault("MoFaGongJi");
        //        SuDu += v.GetIntOrDefault("SuDu");
        //        BaoJi += v.GetIntOrDefault("BaoJi");
        //        MingZhong += v.GetIntOrDefault("MingZhong");
        //        ShanBi += v.GetIntOrDefault("ShanBi");

        //        BaoJiShangHai += v.GetDoubleOrDefault("BaoJiShangHai");
        //        WuLiXiShou += v.GetDoubleOrDefault("WuLiXiShou");
        //        MoFaXiShou += v.GetDoubleOrDefault("MoFaXiShou");

        //        KangShangHai += v.GetIntOrDefault("KangShangHai");
        //        MoFa += v.GetIntOrDefault("MoFa");
        //        FangYu += v.GetIntOrDefault("FangYu");
        //        MoFaFangYu += v.GetIntOrDefault("MoFaFangYu");

        //        LiLiang += v.GetIntOrDefault("LiLiang");
        //        TiZhi += v.GetIntOrDefault("TiZhi");
        //        ZhiLi += v.GetIntOrDefault("ZhiLi");
        //        JingShen += v.GetIntOrDefault("JingShen");
        //    }
        //}
    }
}
