using System.Collections.Generic;
using System.IO;
using System.Linq;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.SkillModule
{
    /// <summary>
    /// 技能...
    /// </summary>
    sealed public class SkillMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                SkillCommand.GetSkill,
                SkillCommand.GetSkills,
                SkillCommand.StudySkill,
                SkillCommand.LoadSkill,
            };
        }
        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            //已连接的用户可以执行的操作.
            switch (note.Name)
            {
                case SkillCommand.GetSkills:
                    GetSkills(note);
                    return;
                case SkillCommand.GetSkill:
                    GetSkill(note);
                    return;
                case SkillCommand.StudySkill:
                    StudySkill(note);
                    return;
                case SkillCommand.LoadSkill:
                    LoadSkill(note);
                    return;
                default:
                    return;
            }
        }

        private void LoadSkill(UserNote note)
        {
            try
            {
                string skillID = note.GetString(0);
                string path = System.AppDomain.CurrentDomain.BaseDirectory;
                path = Path.Combine(ConfigLoader.Config.DirGame, "Skill", skillID);
                if (File.Exists(path))
                {
                    using (FileStream fs = File.OpenRead(path))
                    {
                        byte[] key = new byte[fs.Length];

                        fs.Read(key, 0, key.Length);
                        note.Session.SendAsync(key);
                        note.Session.Close();
                    }
                }
            }
            catch { }
        }
        #endregion

        #region 处理客户端的调用

        /// <summary>
        /// 学习技能
        /// </summary>
        /// <param name="note"></param>
        private void StudySkill(UserNote note)
        {
            string skillID = note.GetString(0);

            GameConfig c = GameConfigAccess.Instance.FindOneById(skillID);
            if (c == null) return;

            PlayerEx skill = note.Player.Skill;
            int oldLev = skill.Value.GetIntOrDefault(skillID);
            if (oldLev == 0) //第一次学习检查职业限制
            {
                //RoleLimit:职业限制 无限制"";  狂战士"1"; 魔弓手"2"; 神祭师"3" 
                string roleID = c.UI.GetStringOrDefault("RoleLimit");
                if (roleID == "1" || roleID == "2" || roleID == "3")
                {
                    if (note.Player.RoleID != roleID)
                    {
                        note.Call(SkillCommand.StudySkillR, false, TipManager.GetMessage(SkillReturn.StudySkill1), oldLev);
                        return;
                    }
                }
            }

            //已达最大等级检查
            int maxLev = c.UI.GetIntOrDefault("Level");
            Variant studyLimit = c.UI.GetVariantOrDefault(oldLev.ToString());
            if (studyLimit == null || oldLev >= maxLev)
            {
                note.Call(SkillCommand.StudySkillR, false, TipManager.GetMessage(SkillReturn.StudySkill3), oldLev);
                return;
            }

            //学习所需角色等级检查
            int studyLev = studyLimit.GetIntOrDefault("StudyLev");
            if (studyLev > note.Player.Level)
            {
                note.Call(SkillCommand.StudySkillR, false, string.Format(TipManager.GetMessage(SkillReturn.StudySkill4), studyLev), oldLev);
                return;
            }

            //前置技能检查
            string preSkill = studyLimit.GetStringOrDefault("PreSkill");
            if (!string.IsNullOrEmpty(preSkill))
            {
                int preLev = studyLimit.GetIntOrDefault("PreLev");
                if (preLev > 0)
                {
                    GameConfig preConfig = GameConfigAccess.Instance.FindOneById(preSkill);
                    if (preConfig != null)
                    {
                        int check = skill.Value.GetIntOrDefault(preSkill);
                        if (preLev > check)
                        {
                            note.Call(SkillCommand.StudySkillR, false,
                                string.Format(TipManager.GetMessage(SkillReturn.StudySkill5), preConfig.Name, preLev), oldLev);
                            return;
                        }
                    }
                }
            }

            //学习消耗技能书检查
            string book = c.UI.GetStringOrDefault("Book");
            if (!string.IsNullOrEmpty(book))
            {
                if (BurdenManager.GoodsCount(note.Player.Value["B0"] as PlayerEx, book) == 0)
                {
                    note.Call(SkillCommand.StudySkillR, false, TipManager.GetMessage(SkillReturn.StudySkill2), oldLev);
                    return;
                }
            }

            //积分消耗检查
            int studyCost = studyLimit.GetIntOrDefault("StudyCost");
            if (note.Player.Score < studyCost || (!note.Player.AddScore(-studyCost, FinanceType.SkillStudy)))
            {
                note.Call(SkillCommand.StudySkillR, false, string.Format(TipManager.GetMessage(SkillReturn.StudySkill6), studyCost), oldLev);
                return;
            }

            skill.Value[skillID] = oldLev + 1;
            skill.Save();
            note.Call(SkillCommand.StudySkillR, true, skillID, oldLev + 1);
            //扣除技能书,每一级都要消耗一本
            if (!string.IsNullOrEmpty(book))
            {
                PlayerEx b0 = note.Player.Value.GetValueOrDefault<PlayerEx>("B0");
                BurdenManager.Remove(b0, book);
                Variant v = new Variant(1);
                v["B0"] = b0;
                note.Call(BurdenCommand.BurdenListR, v);
            }
            note.Player.RefeshSkill();
        }

        /// <summary>
        /// 获取所有技能信息
        /// </summary>
        /// <param name="note"></param>
        private void GetSkills(UserNote note)
        {
            object[] ids = note[0] as object[];
            if (ids == null || ids.Length == 0)
            {
                var skills = GameConfigAccess.Instance.Find(MainType.Skill, string.Empty);
                note.Call(SkillCommand.GetSkillsR, true, skills.Select(x => new SkillSimple(x)));
            }
            else
            {
                var skill = GameConfigAccess.Instance.FindByIDList(ids);
                note.Call(SkillCommand.GetSkillsR, true, skill.Select(x => new SkillSimple(x)));
            }
        }

        /// <summary>
        /// 获取单个技能的信息
        /// </summary>
        /// <param name="note"></param>
        private void GetSkill(UserNote note)
        {
            string id = note.GetString(0);
            var skill = GameConfigAccess.Instance.FindOneById(id);
            note.Call(SkillCommand.GetSkillR, true, new GameConfigUI(skill));
        }

        #endregion
    }
}
