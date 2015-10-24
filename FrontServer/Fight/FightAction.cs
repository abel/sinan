using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗行为
    /// </summary>
    public class FightAction : VariantExternalizable
    {
        //为-1时表示已出招
        public const int HasAction = -1;

        public FightAction(ActionType actionType, string id, int fightCount)
        {
            this.Sender = id;
            this.ActionType = actionType;
            this.FightCount = fightCount;
        }

        public FightAction(Variant v, int fightCount)
        {
            this.ActionType = (ActionType)(v.GetIntOrDefault("fType"));
            this.Sender = v.GetStringOrDefault("handler");
            object obj;
            if (v.TryGetValueT("target", out obj))
            {
                this.Target = (obj == null ? string.Empty : obj.ToString());
            }
            if (v.TryGetValueT("parameter", out obj))
            {
                this.Parameter = (obj == null ? string.Empty : obj.ToString());
            }
            this.FightCount = fightCount;
        }

        /// <summary>
        /// 动作类型
        /// </summary>
        public ActionType ActionType
        {
            get;
            set;
        }

        /// <summary>
        /// 攻击对象
        /// </summary>
        public string Target
        {
            get;
            set;
        }

        /// <summary>
        /// 发起者ID
        /// </summary>
        public string Sender
        {
            get;
            set;
        }

        /// <summary>
        /// 如果FType为0，这儿是技能的ID，
        /// 如果FType为2, 这儿是捕捉网ID;
        /// 如果FType为6, 这儿是战斗包袱的格子"P?";
        /// </summary>
        public string Parameter
        {
            get;
            set;
        }

        /// <summary>
        /// 如果为技能.此处为技能等级
        /// </summary>
        public int SkillLev
        {
            get;
            set;
        }

        /// <summary>
        /// 如果为技能,此处为使用后剩余的魔法
        /// </summary>
        public int MP
        {
            get;
            set;
        }

        /// <summary>
        /// 攻击序号
        /// </summary>
        public int ActionID
        {
            get;
            set;
        }

        /// <summary>
        /// 回合数(为-1时表示已出招..)
        /// </summary>
        public int FightCount
        {
            get;
            set;
        }

        /// <summary>
        /// 说话
        /// </summary>
        public int Say
        {
            get;
            set;
        }

        public List<ActionResult> Result
        {
            get;
            set;
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            base.WriteAmf3(writer);

            if (this.ActionType != ActionType.TaoPao)
            {
                writer.WriteKey("Target");
                writer.WriteUTF(Target);

                writer.WriteKey("Parameter");
                writer.WriteUTF(Parameter);

                writer.WriteKey("ActionID");
                writer.WriteInt(ActionID);

                if (this.ActionType == 0)
                {
                    writer.WriteKey("SkillLev");
                    writer.WriteInt(SkillLev);
                }
                if (Result != null)
                {
                    writer.WriteKey("Result");
                    writer.WriteValue(Result);
                }
            }

            if (Say > 0)
            {
                writer.WriteKey("Say");
                writer.WriteInt(Say);
            }
            writer.WriteKey("fType");
            writer.WriteInt((int)(this.ActionType));

            writer.WriteKey("ID");
            writer.WriteUTF(Sender);
        }

        public FightAction CopyNew()
        {
            FightAction action = new FightAction(this.ActionType, this.Sender, this.FightCount);
            action.Parameter = this.Parameter;
            action.SkillLev = this.SkillLev;
            action.Target = this.Target;
            return action;
        }

    }
}
