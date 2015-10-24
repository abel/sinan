using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理单个场景的业务
    /// </summary>
    partial class SceneBusiness
    {
        /// <summary>
        /// 强制PK
        /// </summary>
        /// <param name="note"></param>
        protected virtual void FightPK(UserNote note)
        {
            if (m_fightType == FightType.NotPK)
            {
                return;
            }

            PlayerBusiness player = note.Player;
            if (player.AState == ActionState.Fight || player.RedName || player.TeamJob == TeamJob.Member)
            {
                return;
            }

            string pkID = note.GetString(0);
            PlayerBusiness b;
            if (m_players.TryGetValue(pkID, out b))
            {
                if (b.Fight != null)
                {
                    return;
                }
                if (player.Team != null && player.Team == b.Team)
                {
                    return;
                }
                DateTime now = DateTime.UtcNow;
                //PK保护时间
                if (m_fightType == FightType.PK)
                {
                    if (player.GetPKProtect() > now)
                    {
                        player.Call(ClientCommand.SendActivtyR, new object[] { "T01", TipManager.GetMessage(ClientReturn.PKProtect) });
                        return;
                    }
                    if (b.LastPK > now || b.GetPKProtect() > now)
                    {
                        player.Call(ClientCommand.SendActivtyR, new object[] { "T01", TipManager.GetMessage(ClientReturn.PKProtect) });
                        return;
                    }
                }

                player.FightTime = DateTime.UtcNow;
                UserNote note2 = new UserNote(note, FightCommand.IntoBattlePK, new object[] { m_fightType, b });
                Notifier.Instance.Publish(note2);
            }
        }

        /// <summary>
        /// 请求切磋
        /// </summary>
        /// <param name="note"></param>
        protected void FightCC(UserNote note)
        {
            PlayerBusiness player = note.Player;
            if (player.AState == ActionState.Fight || player.TeamJob == TeamJob.Member)
            {
                return;
            }
            string pkID = note.GetString(0);
            PlayerBusiness b;
            if (m_players.TryGetValue(pkID, out b))
            {
                if (b.Fight != null) return;
                b.Call(FightCommand.FightCCR, note.PlayerID);
            }
        }

        /// <summary>
        /// 回复切磋
        /// </summary>
        /// <param name="note"></param>
        protected void FightReplyCC(UserNote note)
        {
            PlayerBusiness player = note.Player;
            if (player.AState == ActionState.Fight || player.TeamJob == TeamJob.Member)
            {
                return;
            }
            string pkID = note.GetString(1);
            PlayerBusiness b;
            if (m_players.TryGetValue(pkID, out b))
            {
                bool a = note.GetBoolean(0);
                if (!a)
                {
                    //不同意切磋
                    b.Call(FightCommand.FightReplyCCR, false, note.PlayerID);
                    return;
                }
                player.FightTime = DateTime.UtcNow;
                UserNote note2 = new UserNote(note, FightCommand.IntoBattlePK, new object[] { FightType.CC, b });
                Notifier.Instance.Publish(note2);
            }
        }

        /// <summary>
        /// 打任务NPC
        /// </summary>
        /// <param name="note"></param>
        protected void FightTaskApc(UserNote note)
        {
            PlayerBusiness player = note.Player;
            if (player.AState == ActionState.Fight || player.TeamJob == TeamJob.Member)
            {
                return;
            }

            string npcID = note.GetString(0);

            //打明怪
            if (!CheckFight(player, npcID))
            {
                return;
            }

            string taskID = note.Count > 1 ? note.GetString(1) : string.Empty;
            //生成APC 
            Variant task = NpcManager.Instance.FindTaskNpc(this.ID, npcID, taskID);
            if (task == null)
            {
                //无此任务,不能打怪
                player.Call(FightCommand.FightFalseR, npcID, string.Format(TipManager.GetMessage(ClientReturn.SceneBusiness1)));
                return;
            }
            player.FightTime = DateTime.UtcNow;
            UserNote note2 = new UserNote(note, FightCommand.IntoBattle, new object[] { FightType.TaskAPC, task, npcID });
            Notifier.Instance.Publish(note2);
        }

        /// <summary>
        /// 检查能否跟NPC战斗
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npcID"></param>
        /// <returns></returns>
        protected virtual bool CheckFight(PlayerBusiness player, string npcID)
        {
            return true;
        }
    }
}
