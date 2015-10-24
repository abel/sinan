using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 打明怪
    /// </summary>
    sealed public class SceneApcMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                FightCommand.FightSceneApc,
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
            if (note.Player == null) return;
            if (note.Player.Scene == null) return;
            switch (note.Name)
            {
                case FightCommand.FightSceneApc:
                    FightSceneApc(note);
                    return;
                default:
                    return;
            }
        }
        #endregion

        void FightSceneApc(UserNote note)
        {
            PlayerBusiness player = note.Player;

            if (player.AState == ActionState.Fight || player.TeamJob == TeamJob.Member)
            {
                return;
            }

            string apcid = note.GetString(0);
            SceneApc apc = SceneApcProxy.FindOne(apcid);
            if (apc == null)
            {
                //怪物已被击杀
                player.Call(FightCommand.FightFalseR, apcid, TipManager.GetMessage(ClientReturn.FightSceneApc1));
                return;
            }
            if (apc.SceneID != player.SceneID)
            {
                //你已跨场景
                player.Call(FightCommand.FightFalseR, apcid, TipManager.GetMessage(ClientReturn.FightSceneApc2));
                return;
            }
            VisibleApc hideApc = apc.Apc;
            if (hideApc == null)
            {
                //怪物已被击杀
                player.Call(FightCommand.FightFalseR, apcid, TipManager.GetMessage(ClientReturn.FightSceneApc3));
                return;
            }
            int subID = note.GetInt32(1);
            if (!apc.TryFight(subID))
            {
                //怪物在战斗中,不能攻击
                player.Call(FightCommand.FightFalseR, apcid, TipManager.GetMessage(ClientReturn.FightSceneApc4));
                return;
            }

            //进入战斗
            player.FightTime = DateTime.UtcNow;
            object[] objs;

            if (player.Scene.SceneType == SceneType.Rob)
            {
                //夺宝奇兵
                SceneRob sr = player.Scene as SceneRob;
                objs = new object[] { FightType.RobAPC, hideApc, hideApc.ID, apc, subID, sr.Part };
            }
            else if (player.Scene.SceneType == SceneType.Pro)
            {
                //守护战争
                ScenePro sr = player.Scene as ScenePro;
                objs = new object[] { FightType.ProAPC, hideApc, hideApc.ID, apc, subID, sr.Part };
            }
            else
            {
                objs = new object[] { FightType.SceneAPC, hideApc, hideApc.ID, apc, subID };
            }
            UserNote note2 = new UserNote(player, FightCommand.IntoBattle, objs);
            Notifier.Instance.Publish(note2);
        }
    }
}
