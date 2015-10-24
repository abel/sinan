using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Command
{
    /// <summary>
    /// (35XX)
    /// </summary>
    public class SkillCommand
    {
        /// <summary>
        /// 获取所有技能信息
        /// </summary>
        public const string GetSkills = "getSkills";
        public const string GetSkillsR = "s.getSkillsR";

        /// <summary>
        /// 获取单个技能信息
        /// </summary>
        public const string GetSkill = "getSkill";
        public const string GetSkillR = "s.getSkillR";

        /// <summary>
        /// 学习技能
        /// </summary>
        public const string StudySkill = "studySkill";
        public const string StudySkillR = "s.studySkillR";

        /// <summary>
        /// 加载单个技能信息
        /// </summary>
        public const string LoadSkill = "loadSkill";
        public const string LoadSkillR = "s.loadSkillR";
    }

    public class SkillReturn 
    {
        /// <summary>
        /// 该职业不能学习该技能
        /// </summary>
        public const int StudySkill1 = 35001;
        /// <summary>
        /// 缺少技能书
        /// </summary>
        public const int StudySkill2 = 35002;
        /// <summary>
        /// 已达最大等级
        /// </summary>
        public const int StudySkill3 = 35003;
        /// <summary>
        /// 所需等级{0},你的等级不足
        /// </summary>
        public const int StudySkill4 = 35004;
        /// <summary>
        /// 需学习前置技能[0]到[1]级!
        /// </summary>
        public const int StudySkill5 = 35005;
        /// <summary>
        /// 所需石币{0},你的石币不足
        /// </summary>
        public const int StudySkill6 = 35006;
    }
}
