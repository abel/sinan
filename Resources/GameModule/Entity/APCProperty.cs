using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// APC战斗属性.
    /// </summary>
    [Serializable]
    public class APCProperty
    {
        /// <summary>
        /// 最大等级
        /// </summary>
        public const int MaxLevel = 160;

        ///// <summary>
        ///// 当前生命
        ///// </summary>
        //public int HP
        //{ get; set; }

        ///// <summary>
        /////  当前魔法
        ///// </summary>
        //public int MP
        //{ get; set; }

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int ShengMing
        { get; set; }

        /// <summary>
        /// 最大魔法值
        /// </summary>
        public int MoFa
        { get; set; }

        /// <summary>
        /// 物理攻击
        /// </summary>
        public int GongJi
        { get; set; }

        /// <summary>
        /// 魔法攻击
        /// </summary>
        public int MoFaGongJi
        { get; set; }

        /// <summary>
        /// 物理防御
        /// </summary>
        public int FangYu
        { get; set; }

        /// <summary>
        /// 魔法防御
        /// </summary>
        public int MoFaFangYu
        { get; set; }

        /// <summary>
        /// 物理吸收
        /// </summary>
        public double WuLiXiShou
        { get; set; }

        /// <summary>
        /// 魔法吸收
        /// </summary>
        public double MoFaXiShou
        { get; set; }

        /// <summary>
        /// 反击率
        /// </summary>
        public double FanJiLv
        { get; set; }

        /// <summary>
        /// 合击率
        /// </summary>
        public double HeJiLv
        { get; set; }

        /// <summary>
        /// 逃跑率
        /// </summary>
        public double TaoPaoLv
        { get; set; }

        /// <summary>
        /// 暴击伤害倍数
        /// </summary>
        public double BaoJiShangHai
        { get; set; }

        /// <summary>
        /// 合击伤害倍数
        /// </summary>
        public double HeJiShangHai
        { get; set; }

        /// <summary>
        /// 速度
        /// </summary>
        public int SuDu
        { get; set; }

        /// <summary>
        /// 暴击
        /// </summary>
        public int BaoJi
        { get; set; }

        /// <summary>
        /// 命中
        /// </summary>
        public int MingZhong
        { get; set; }

        /// <summary>
        /// 闪避
        /// </summary>
        public int ShanBi
        { get; set; }

        /// <summary>
        /// 暴击率
        /// </summary>
        public double BaoJiLv
        { get; set; }

        /// <summary>
        /// 命中率
        /// </summary>
        public double MingZhongLv
        { get; set; }

        /// <summary>
        /// 闪避率
        /// </summary>
        public double ShanBiLv
        { get; set; }

        /// <summary>
        /// 加成.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual void Add(Variant v)
        {
            if (v != null && v.Count > 0)
            {
                ShengMing += v.GetIntOrDefault("ShengMing");
                MoFa += v.GetIntOrDefault("MoFa");
                GongJi += v.GetIntOrDefault("GongJi");
                MoFaGongJi += v.GetIntOrDefault("MoFaGongJi");
                FangYu += v.GetIntOrDefault("FangYu");
                MoFaFangYu += v.GetIntOrDefault("MoFaFangYu");

                WuLiXiShou += v.GetDoubleOrDefault("WuLiXiShou");
                MoFaXiShou += v.GetDoubleOrDefault("MoFaXiShou");
                FanJiLv += v.GetDoubleOrDefault("FanJiLv");
                HeJiLv += v.GetDoubleOrDefault("HeJiLv");
                TaoPaoLv += v.GetDoubleOrDefault("TaoPaoLv");
                BaoJiShangHai += v.GetDoubleOrDefault("BaoJiShangHai");
                HeJiShangHai += v.GetDoubleOrDefault("HeJiShangHai");

                SuDu += v.GetIntOrDefault("SuDu");
                BaoJi += v.GetIntOrDefault("BaoJi");
                MingZhong += v.GetIntOrDefault("MingZhong");
                ShanBi += v.GetIntOrDefault("ShanBi");

                BaoJiLv += v.GetDoubleOrDefault("BaoJiLv");
                MingZhongLv += v.GetDoubleOrDefault("MingZhongLv");
                ShanBiLv += v.GetDoubleOrDefault("ShanBiLv");
            }
        }

        /// <summary>
        /// 加成.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual void Add(APCProperty v)
        {
            if (v != null)
            {
                ShengMing += v.ShengMing;
                MoFa += v.MoFa;
                GongJi += v.GongJi;
                MoFaGongJi += v.MoFaGongJi;
                FangYu += v.FangYu;
                MoFaFangYu += v.MoFaFangYu;
                WuLiXiShou += v.WuLiXiShou;
                MoFaXiShou += v.MoFaXiShou;
                FanJiLv += v.FanJiLv;
                HeJiLv += v.HeJiLv;
                TaoPaoLv += v.TaoPaoLv;
                BaoJiShangHai += v.BaoJiShangHai;
                HeJiShangHai += v.HeJiShangHai;
                SuDu += v.SuDu;
                BaoJi += v.BaoJi;
                MingZhong += v.MingZhong;
                ShanBi += v.ShanBi;
                BaoJiLv += v.BaoJiLv;
                MingZhongLv += v.MingZhongLv;
                ShanBiLv += v.ShanBiLv;
            }
        }

        /// <summary>
        /// 减成
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public virtual void Subtract(APCProperty v)
        {
            if (v != null)
            {
                ShengMing -= v.ShengMing;
                MoFa -= v.MoFa;
                GongJi -= v.GongJi;
                MoFaGongJi -= v.MoFaGongJi;
                FangYu -= v.FangYu;
                MoFaFangYu -= v.MoFaFangYu;
                WuLiXiShou -= v.WuLiXiShou;
                MoFaXiShou -= v.MoFaXiShou;
                FanJiLv -= v.FanJiLv;
                HeJiLv -= v.HeJiLv;
                TaoPaoLv -= v.TaoPaoLv;
                BaoJiShangHai -= v.BaoJiShangHai;
                HeJiShangHai -= v.HeJiShangHai;
                SuDu -= v.SuDu;
                BaoJi -= v.BaoJi;
                MingZhong -= v.MingZhong;
                ShanBi -= v.ShanBi;
                BaoJiLv -= v.BaoJiLv;
                MingZhongLv -= v.MingZhongLv;
                ShanBiLv -= v.ShanBiLv;
            }
        }

        /// <summary>
        /// 获取更新的值.
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        protected Variant GetChange(APCProperty old)
        {
            Variant v = new Variant();
            if (ShengMing != old.ShengMing) v.Add("ShengMing", ShengMing);
            if (MoFa != old.MoFa) v.Add("MoFa", MoFa);
            if (GongJi != old.GongJi) v.Add("GongJi", GongJi);
            if (MoFaGongJi != old.MoFaGongJi) v.Add("MoFaGongJi", MoFaGongJi);
            if (FangYu != old.FangYu) v.Add("FangYu", FangYu);
            if (MoFaFangYu != old.MoFaFangYu) v.Add("MoFaFangYu", MoFaFangYu);

            if (WuLiXiShou != old.WuLiXiShou) v.Add("WuLiXiShou", WuLiXiShou);
            if (MoFaXiShou != old.MoFaXiShou) v.Add("MoFaXiShou", MoFaXiShou);
            if (FanJiLv != old.FanJiLv) v.Add("FanJiLv", FanJiLv);
            if (HeJiLv != old.HeJiLv) v.Add("HeJiLv", HeJiLv);
            if (TaoPaoLv != old.TaoPaoLv) v.Add("TaoPaoLv", TaoPaoLv);
            if (BaoJiShangHai != old.BaoJiShangHai) v.Add("BaoJiShangHai", BaoJiShangHai);
            if (HeJiShangHai != old.HeJiShangHai) v.Add("HeJiShangHai", HeJiShangHai);

            if (SuDu != old.SuDu) v.Add("SuDu", SuDu);
            if (BaoJi != old.BaoJi) v.Add("BaoJi", BaoJi);
            if (MingZhong != old.MingZhong) v.Add("MingZhong", MingZhong);
            if (ShanBi != old.ShanBi) v.Add("ShanBi", ShanBi);

            if (BaoJiLv != old.BaoJiLv) v.Add("BaoJiLv", BaoJiLv);
            if (MingZhongLv != old.MingZhongLv) v.Add("MingZhongLv", MingZhongLv);
            if (ShanBiLv != old.ShanBiLv) v.Add("ShanBiLv", ShanBiLv);
            return v;
        }

        /// <summary>
        /// 获取更新的值.
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public virtual Variant GetChange()
        {
            Variant v = new Variant(5);
            if (ShengMing != 0) v.Add("ShengMing", ShengMing);
            if (MoFa != 0) v.Add("MoFa", MoFa);
            if (GongJi != 0) v.Add("GongJi", GongJi);
            if (MoFaGongJi != 0) v.Add("MoFaGongJi", MoFaGongJi);
            if (FangYu != 0) v.Add("FangYu", FangYu);
            if (MoFaFangYu != 0) v.Add("MoFaFangYu", MoFaFangYu);

            if (WuLiXiShou != 0) v.Add("WuLiXiShou", WuLiXiShou);
            if (MoFaXiShou != 0) v.Add("MoFaXiShou", MoFaXiShou);
            if (FanJiLv != 0) v.Add("FanJiLv", FanJiLv);
            if (HeJiLv != 0) v.Add("HeJiLv", HeJiLv);
            if (TaoPaoLv != 0) v.Add("TaoPaoLv", TaoPaoLv);
            if (BaoJiShangHai != 0) v.Add("BaoJiShangHai", BaoJiShangHai);
            if (HeJiShangHai != 0) v.Add("HeJiShangHai", HeJiShangHai);

            if (SuDu != 0) v.Add("SuDu", SuDu);
            if (BaoJi != 0) v.Add("BaoJi", BaoJi);
            if (MingZhong != 0) v.Add("MingZhong", MingZhong);
            if (ShanBi != 0) v.Add("ShanBi", ShanBi);

            if (BaoJiLv != 0) v.Add("BaoJiLv", BaoJiLv);
            if (MingZhongLv != 0) v.Add("MingZhongLv", MingZhongLv);
            if (ShanBiLv != 0) v.Add("ShanBiLv", ShanBiLv);
            return v;
        }

        public virtual Variant ToVariant()
        {
            Variant v = new Variant();
            v["ShengMing"] = this.ShengMing;
            v["MoFa"] = this.MoFa;
            v["GongJi"] = this.GongJi;
            v["MoFaGongJi"] = this.MoFaGongJi;
            v["FangYu"] = this.FangYu;
            v["MoFaFangYu"] = this.MoFaFangYu;
            v["WuLiXiShou"] = this.WuLiXiShou;
            v["MoFaXiShou"] = this.MoFaXiShou;
            v["FanJiLv"] = this.FanJiLv;
            v["HeJiLv"] = this.HeJiLv;
            v["TaoPaoLv"] = this.TaoPaoLv;
            v["BaoJiShangHai"] = this.BaoJiShangHai;
            v["HeJiShangHai"] = this.HeJiShangHai;
            v["SuDu"] = this.SuDu;
            v["BaoJi"] = this.BaoJi;
            v["MingZhong"] = this.MingZhong;
            v["ShanBi"] = this.ShanBi;
            v["BaoJiLv"] = this.BaoJiLv;
            v["MingZhongLv"] = this.MingZhongLv;
            v["ShanBiLv"] = this.ShanBiLv;
            return v;
        }

        public virtual void WriteProperty(IExternalWriter writer)
        {
            writer.WriteKey("SuDu");
            writer.WriteInt(SuDu);
            writer.WriteKey("BaoJi");
            writer.WriteInt(BaoJi);
            writer.WriteKey("MingZhong");
            writer.WriteInt(MingZhong);
            writer.WriteKey("ShanBi");
            writer.WriteInt(ShanBi);

            writer.WriteKey("GongJi");
            writer.WriteInt(GongJi);
            writer.WriteKey("MoFaGongJi");
            writer.WriteInt(MoFaGongJi);
            writer.WriteKey("FangYu");
            writer.WriteInt(FangYu);
            writer.WriteKey("MoFaFangYu");
            writer.WriteInt(MoFaFangYu);

            writer.WriteKey("WuLiXiShou");
            writer.WriteDouble(WuLiXiShou);
            writer.WriteKey("MoFaXiShou");
            writer.WriteDouble(MoFaXiShou);

            //writer.WriteKey("ShengMing");
            //MVPair.WritePair(writer, ShengMing, HP);
            //writer.WriteKey("MoFa");
            //MVPair.WritePair(writer, MoFa, MP);
        }
    }
}
