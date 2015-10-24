using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    public class FightPlayer : FightObject
    {
        /// <summary>
        /// 玩家类型
        /// </summary>
        public override FighterType FType
        {
            get { return FighterType.Player; }
        }

        int m_wg, m_mg, m_wf, m_mf;
        /// <summary>
        /// 强力(增加物理攻击)
        /// </summary>
        public override int WG
        {
            get { return m_wg; }
        }

        /// <summary>
        /// 魔能(增加魔法攻击)
        /// </summary>
        public override int MG
        {
            get { return m_mg; }
        }

        /// <summary>
        /// 坚韧(增加物理防御)
        /// </summary>
        public override int WF
        {
            get { return m_wf; }
        }

        /// <summary>
        /// 坚韧(增加魔法防御)
        /// </summary>
        public override int MF
        {
            get { return m_mf; }
        }

        double m_cp;
        /// <summary>
        /// 增加宠物捕捉几率
        /// </summary>
        public double CP
        {
            get { return m_cp; }
        }

        double m_expa;
        /// <summary>
        /// 双倍经验
        /// </summary>
        public double ExpA
        {
            get { return m_expa; }
        }

        protected Variant m_skills;

        /// <summary>
        /// 战斗结果
        /// </summary>
        public FightResult FResult
        {
            get;
            set;
        }

        protected FightPlayer(PlayerBusiness player)
        {
            m_player = player;
            m_skills = player.Skill.Value;
            PlayerEx assist = player.Assist;
            if (assist != null && assist.Value != null)
            {
                m_wg = GetAssistInt(assist.Value.GetVariantOrDefault("WG"));
                m_mg = GetAssistInt(assist.Value.GetVariantOrDefault("MG"));
                m_wf = GetAssistInt(assist.Value.GetVariantOrDefault("WF"));
                m_mf = GetAssistInt(assist.Value.GetVariantOrDefault("MF"));
                m_cp = GetAssistDouble(assist.Value.GetVariantOrDefault("CP"));
                m_expa = GetDouleExp(assist.Value);
            }
        }

        public FightPlayer(int p, PlayerBusiness player)
            : this(player)
        {
            this.P = p;
            this.ID = player.ID;
            this.Level = player.Level;
            this.Name = player.Name;
            this.Life = player.Life;            
            m_hp = player.HP;
            m_mp = player.MP;
            this.Skin = player.RoleID.ToString();
            this.m_fixBuffer = player.FixBuffer;
            player.UseClapnet = 0;
            player.CheckAutoFight();
            FResult = FightResult.Lose;
        }

        public override bool ExistsSkill(string name)
        {
            return m_player.Skill.Value.ContainsKey(name);
        }

        private int GetAssistInt(Variant v)
        {
            if (v == null) return 0;
            if (v.GetDateTimeOrDefault("T") >= DateTime.UtcNow)
            {
                return v.GetIntOrDefault("A");
            }
            return 0;
        }

        private double GetAssistDouble(Variant v)
        {
            if (v == null) return 0;
            if (v.GetDateTimeOrDefault("T") >= DateTime.UtcNow)
            {
                return v.GetDoubleOrDefault("A");
            }
            return 0;
        }

        /// <summary>
        /// 获取双倍经验系数
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        protected virtual double GetDouleExp(Variant v)
        {
            if (v == null) return 0;
            Variant exp = v.GetVariantOrDefault("RExp");
            if (exp == null) return 0;
            if (exp.GetDateTimeOrDefault("T") >= DateTime.UtcNow)
            {
                return exp.GetDoubleOrDefault("A");
            }
            return 0;
        }

        /// <summary>
        /// 检查技能的使用条件
        /// </summary>
        /// <param name="fighter"></param>
        /// <param name="gs"></param>
        /// <returns></returns>
        public override bool CheckJingNeng(int current, out GameConfig gs)
        {
            string skillID = m_action.Parameter;
            if (m_skills == null || string.IsNullOrEmpty(skillID))
            {
                gs = null;
                return false;
            }

            gs = GameConfigAccess.Instance.FindOneById(skillID);
            if (gs == null) return false;

            // 检查技能限制...
            // 技能冻结回合数: int(被动技能为0,主动技能大于0)
            int maxUse = gs.UI.GetIntOrDefault("MaxUse");
            if (maxUse == 0) return false;

            int level = m_skills.GetIntOrDefault(skillID);
            if (level <= 0) return false;
            m_action.SkillLev = level;

            // HP小于指定值时不能使用 HPLimit: int
            int hpLimit = gs.Value.GetIntOrDefault("HPLimit");
            if (this.HP <= hpLimit) return false;

            // 获取技能等级
            Variant obj = gs.Value.GetVariantOrDefault(level.ToString());
            if (obj == null) return false;

            int mpCost = obj.GetIntOrDefault("MPCost");
            if (this.MP < mpCost) return false;

            // 使用消耗的生命值 HPCost:int
            int hpCost = obj.GetIntOrDefault("HPCost");
            if (this.HP <= hpCost) return false;

            if (maxUse > 1) //需冻结的技能
            {
                int used;
                if (SkillUsed.TryGetValue(skillID, out used))
                {
                    if (used + maxUse > current) return false;
                }
                SkillUsed[skillID] = current;
            }
            this.AddHPAndMP(-hpCost, -mpCost);
            m_action.MP = this.MP;
            return true;
        }

        public override void ExitFight(bool changeLife)
        {
            m_online = false;
            if (changeLife && this.Level == m_player.Level)
            {
                //回写HP和MP
                m_player.HP = m_hp;
                m_player.MP = m_mp;
                m_player.ExitFight(true, m_hp, m_mp);
            }
            else
            {
                m_player.ExitFight(false, m_hp, m_mp);
            }
        }

        protected override void WriteAmf3(AMF3.IExternalWriter writer)
        {
            base.WriteAmf3(writer);
            if (FType == FighterType.Player)
            {
                writer.WriteKey("AutoFight");
                writer.WriteInt(m_player.AutoFight);
            }
        }
    }
}
