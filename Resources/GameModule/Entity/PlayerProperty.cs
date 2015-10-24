using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// 角色初始属性值
    /// </summary>
    [Serializable]
    public class PlayerProperty : APCProperty
    {
        public PlayerProperty()
        { }

        /// <summary>
        /// 力量property
        /// </summary>
        public int LiLiang
        { get; set; }

        /// <summary>
        /// 体质
        /// </summary>
        public int TiZhi
        { get; set; }

        /// <summary>
        /// 智力
        /// </summary>
        public int ZhiLi
        { get; set; }

        /// <summary>
        /// 精神
        /// </summary>
        public int JingShen
        { get; set; }

        /// <summary>
        /// 伤害抗性.角色吸收最终伤害的能力（固定值）
        /// </summary>
        public int KangShangHai
        {
            get;
            set;
        }

        /// <summary>
        /// 发动攻击
        /// </summary>
        /// <param name="pro">被攻击者</param>
        /// <param name="lv"></param>
        /// <returns></returns>
        public int SendGongJi(PlayerProperty pro, double lv)
        {
            int v = (int)((this.BaoJi * lv) * (1 - pro.WuLiXiShou) - pro.KangShangHai);
            if (v <= 0) v = 1;
            return v;
        }

        /// <summary>
        /// 发出反击
        /// </summary>
        /// <param name="pro">被反击者</param>
        /// <returns></returns>
        public int SendFangJi(PlayerProperty pro)
        {
            int v = (int)(this.BaoJi * (1 - pro.WuLiXiShou) - pro.KangShangHai);
            if (v <= 0) v = 1;
            return v;
        }

        public override Variant ToVariant()
        {
            Variant v = base.ToVariant();
            v["LiLiang"] = this.LiLiang;
            v["TiZhi"] = this.TiZhi;
            v["ZhiLi"] = this.ZhiLi;
            v["JingShen"] = this.JingShen;
            v["KangShangHai"] = this.KangShangHai;
            return v;
        }

        /// <summary>
        /// 加成.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public override void Add(Variant v)
        {
            if (v != null && v.Count > 0)
            {
                base.Add(v);
                LiLiang += v.GetIntOrDefault("LiLiang");
                TiZhi += v.GetIntOrDefault("TiZhi");
                ZhiLi += v.GetIntOrDefault("ZhiLi");
                JingShen += v.GetIntOrDefault("JingShen");
                KangShangHai += v.GetIntOrDefault("KangShangHai");
            }
        }

        /// <summary>
        /// 加成.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public override void Add(APCProperty v)
        {
            base.Add(v);
            PlayerProperty p = v as PlayerProperty;
            if (p != null)
            {
                LiLiang += p.LiLiang;
                TiZhi += p.TiZhi;
                ZhiLi += p.ZhiLi;
                JingShen += p.JingShen;
                KangShangHai += p.KangShangHai;
            }
        }

        /// <summary>
        /// 减成
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public override void Subtract(APCProperty v)
        {
            base.Subtract(v);
            PlayerProperty p = v as PlayerProperty;
            if (p != null)
            {
                LiLiang -= p.LiLiang;
                TiZhi -= p.TiZhi;
                ZhiLi -= p.ZhiLi;
                JingShen -= p.JingShen;
                KangShangHai -= p.KangShangHai;
            }
        }

        /// <summary>
        /// 获取更新的值.
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public Variant GetChange(PlayerProperty old)
        {
            if (old == null) return this.GetChange();
            Variant v = base.GetChange(old);
            if (LiLiang != old.LiLiang) v.Add("LiLiang", LiLiang);
            if (TiZhi != old.TiZhi) v.Add("TiZhi", TiZhi);
            if (ZhiLi != old.ZhiLi) v.Add("ZhiLi", ZhiLi);
            if (JingShen != old.JingShen) v.Add("JingShen", JingShen);
            if (KangShangHai != old.KangShangHai) v.Add("KangShangHai", KangShangHai);
            return v;
        }

        /// <summary>
        /// 获取更新的值.
        /// </summary>
        /// <param name="old"></param>
        /// <returns></returns>
        public override Variant GetChange()
        {
            Variant v = base.GetChange();
            if (LiLiang != 0) v.Add("LiLiang", LiLiang);
            if (TiZhi != 0) v.Add("TiZhi", TiZhi);
            if (ZhiLi != 0) v.Add("ZhiLi", ZhiLi);
            if (JingShen != 0) v.Add("JingShen", JingShen);
            if (KangShangHai != 0) v.Add("KangShangHai", KangShangHai);
            return v;
        }

        public override void WriteProperty(IExternalWriter writer)
        {
            base.WriteProperty(writer);
            writer.WriteKey("LiLiang");
            writer.WriteInt(LiLiang);
            writer.WriteKey("TiZhi");
            writer.WriteInt(TiZhi);
            writer.WriteKey("ZhiLi");
            writer.WriteInt(ZhiLi);
            writer.WriteKey("JingShen");
            writer.WriteInt(JingShen);
        }
    }
}

//LiLiang:      int,
//TiZhi:        int,
//ZhiLi:        int,
//JingShen:     int,
//KangShangHai: int,

//ShengMing:    int,
//MoFa:         int,
//GongJi:       int,
//MoFaGongJi:   int,
//FangYu:       int,
//MoFaFangYu:   int,

//WuLiXiShou:   double,
//MoFaXiShou:   double,
//FanJiLv:      double,
//HeJiLv:       double,
//TaoPaoLv:     double,
//BaoJiShangHai:double,
//HeJiShangHai: double,

//SuDu:         int,
//BaoJi:        int,
//MingZhong:    int,
//ShanBi:       int,

//BaoJiLv:      double,
//MingZhongLv:  double,
//ShanBiLv:     double,