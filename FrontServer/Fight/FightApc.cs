using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗怪
    /// </summary>
    public partial class FightApc : FightObject
    {
        protected Apc m_apc;
        protected IList m_ai;
        protected DateTime m_deadTime;
        protected FightObject m_killer;

        /// <summary>
        /// 杀死APC的玩家
        /// </summary>
        public FightObject Killer
        {
            get { return m_killer; }
            set
            {
                m_killer = value;
                m_deadTime = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// 死亡时间
        /// </summary>
        public DateTime DeadTime
        {
            get { return m_deadTime; }
        }

        public override FighterType FType
        {
            get { return FighterType.APC; }
        }

        /// <summary>
        /// APC类型ID
        /// </summary>
        public string ApcID
        {
            get { return m_apc.ID; }
        }

        public Apc APC
        {
            get { return m_apc; }
        }

        public FightApc(int p, Apc apc)
        {
            this.P = p;
            this.ID = ObjectId.GenerateNewId().ToString(); //Guid.NewGuid().ToString("N");
            this.m_apc = apc;
            this.m_ai = apc.Value.GetValueOrDefault<IList>("AI");
            this.Name = apc.Name;
            this.Level = apc.Level;
            this.Skin = apc.Skin;
            PlayerProperty property = new PlayerProperty();
            property.Add(apc.Value);
            this.Life = property;
            m_hp = property.ShengMing;
            m_mp = property.MoFa;

            if (m_apc.Value != null)
            {
                this.Talk = NewTalk(m_apc.Value.GetVariantOrDefault("Start"));
            }
        }

        /// <summary>
        /// 获取奖励
        /// </summary>
        /// <returns></returns>
        public void GetAwards(Dictionary<string, int> awards)
        {
            //前2个为角色经验和宠物经验,后四个为币
            GetAward("P1exp", awards);
            GetAward("P2exp", awards);
            GetAwardPMM("Bond", awards);
            GetAwardPMM("Score", awards);
            Variant award;
            if (m_apc.Value.TryGetValueT<Variant>("Award", out award) && award != null)
            {
                Award.GetPackets(award, awards);
            }
        }

        /// <summary>
        /// 几率/最小值/最大值模式
        /// </summary>
        /// <param name="name"></param>
        /// <param name="awards"></param>
        private void GetAwardPMM(string name, Dictionary<string, int> awards)
        {
            Variant v = m_apc.Value.GetVariantOrDefault(name);
            if (v != null)
            {
                int value = Award.GetAwardCount(v);
                if (value > 0)
                {
                    int old;
                    awards.TryGetValue(name, out old);
                    awards[name] = old + value;
                }
            }
        }

        private void GetAward(string name, Dictionary<string, int> awards)
        {
            int value = m_apc.Value.GetIntOrDefault(name);
            if (value > 0)
            {
                int old;
                awards.TryGetValue(name, out old);
                awards[name] = old + value;
            }
        }

        public override bool ExistsSkill(string name)
        {
            return true;
        }

        /// <summary>
        /// 检查技能的使用条件
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="gs"></param>
        /// <returns></returns>
        public override bool CheckJingNeng(int current, out GameConfig gc)
        {
            string skillID = Action.Parameter;
            gc = string.IsNullOrEmpty(skillID) ? null : GameConfigAccess.Instance.FindOneById(skillID);
            return gc != null;
        }
    }
}
