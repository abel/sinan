using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions.FluentDate;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 单人副本
    /// </summary>
    public sealed class SceneEctype : SceneBattle
    {
        public override SceneType SceneType
        {
            get { return SceneType.Ectype; }
        }

        public SceneEctype(GameConfig scene)
            : base(scene)
        {
            m_showAll = false;
        }

        SceneEctype(SceneEctype scene)
            : base(scene)
        {
            m_showAll = false;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneEctype(this);
        }

        /// <summary>
        /// 进入检查
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        protected override bool IntoCheck(PlayerBusiness player)
        {
            if (player.Team != null)
            {
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.IntoLimit1));
                return false;
            }
            if (player.Fight != null)
            {
                //player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.SceneHome2));
                return false;
            }
            if (m_intoLimit != null)
            {
                string msg;
                if (!m_intoLimit.IntoCheck(player, out msg))
                {
                    player.Call(ClientCommand.IntoSceneR, false, null, msg);
                    return false;
                }

                if (!m_intoLimit.IntoDeduct(player, out msg))
                {
                    player.Call(ClientCommand.IntoSceneR, false, null, msg);
                    return false;
                }
            }
            return true;
        }

        protected override Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            player.ShowID = player.PID;
            Variant scene = base.CreateSceneInfo(player, newlogin);
            if (!newlogin) //从其它场景进入..非新登入方式.
            {
                player.WriteDaily(PlayerBusiness.DailyMap, this.ID);
                player.Ectype.Value["Killed"] = new List<string>();
                player.Ectype.Save();
            }
            scene["Killed"] = player.Ectype.Value.GetValue<IList>("Killed");
            return scene;
        }

        /// <summary>
        /// 场景切换检查
        /// </summary>
        /// <param name="player"></param>
        /// <param name="inScene"></param>
        /// <param name="transmitType">0:道具/超时回城, 1:死亡回城 , 2:转送阵, 3: NPC</param>
        /// <returns></returns>
        public override bool TransferCheck(PlayerBusiness player, SceneBusiness inScene, TransmitType transmitType)
        {
            //检查必杀的怪是否完成..
            if (transmitType == TransmitType.Pin)
            {
                IList killed = player.Ectype.Value.GetValue<IList>("Killed");
                string needKill = NpcManager.Instance.MustKill(this.ID, killed);
                if (needKill != null)
                {
                    string msg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype4), needKill);
                    player.Call(ClientCommand.IntoSceneR, false, null, msg);
                    return false;
                }
            }
            return base.TransferCheck(player, inScene, transmitType);
        }

        /// <summary>
        /// 检查必杀怪
        /// </summary>
        /// <param name="player"></param>
        /// <param name="npcID"></param>
        /// <returns></returns>
        protected override bool CheckFight(PlayerBusiness player, string npcID)
        {
            IList killed = player.Ectype.Value.GetValue<IList>("Killed");
            if (killed.Contains(npcID))
            {
                player.Call(FightCommand.FightFalseR, npcID, string.Format(TipManager.GetMessage(ClientReturn.SceneEctype5)));
                return false;
            }
            string needKill = NpcManager.Instance.MustKill(this.ID, killed, npcID);
            if (needKill != null)
            {
                string msg = string.Format(TipManager.GetMessage(ClientReturn.SceneEctype4), needKill);
                player.Call(FightCommand.FightFalseR, npcID, msg);
                return false;
            }
            //需发送通知:
            return needKill == null;
        }


        /// <summary>
        /// 成功进入场景
        /// </summary>
        /// <param name="note"></param>
        protected override void IntoSceneSuccess(PlayerBusiness player, Variant sceneinfo)
        {
            player.Call(ClientCommand.IntoSceneR, true, sceneinfo, EmptyPlayerList);
            player.FightTime = DateTime.UtcNow.AddSeconds(10);
            player.Online = true;
            player.Save();

            if (this.HaveApc)
            {
                IList apcs = SceneApcProxy.GetSceneApc(this.ID);
                player.Call(ClientCommand.RefreshApcR, this.ID, apcs);
            }
            if (this.HaveBox)
            {
                IList boxs = BoxProxy.GetSceneBox(this.ID);
                player.Call(ClientCommand.RefreshBoxR, this.ID, boxs);
            }
        }
    }
}
