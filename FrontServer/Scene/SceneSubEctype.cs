using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 单人子副本
    /// </summary>
    public sealed class SceneSubEctype : SceneBusiness
    {
        public override SceneType SceneType
        {
            get { return SceneType.SubEctype; }
        }

        /// <summary>
        /// 所属副本ID
        /// </summary>
        string m_ectypeID;

        /// <summary>
        /// 父场景
        /// </summary>
        public string EctypeID
        {
            get { return m_ectypeID; }
        }

        public SceneSubEctype(GameConfig scene)
            : base(scene)
        {
            IsOverTime = true;
            m_showAll = false;
            Variant v = scene.Value.GetVariantOrDefault("Config");
            m_ectypeID = v.GetStringOrDefault("FatherScene");
            if (string.IsNullOrEmpty(m_ectypeID))
            {
                LogWrapper.Warn("father err:" + this.ID);
            }
        }

        SceneSubEctype(SceneSubEctype scene)
            : base(scene)
        {
            m_showAll = false;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneSubEctype(this);
        }

        /// <summary>
        /// 超时传出.
        /// </summary>
        public bool IsOverTime
        {
            get;
            set;
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="note"></param>
        protected override bool WalkEvent(PlayerBusiness player)
        {
            if (IsOverTime)
            {
                DateTime overTime = player.Ectype.Value.GetDateTimeOrDefault("OverTime");
                if (overTime < DateTime.UtcNow)
                {
                    // 副本超时转出.
                    this.TownGate(player, TransmitType.OverTime);
                    return false;
                }
            }
            return base.WalkEvent(player);
        }

        protected override Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            player.ShowID = player.PID;
            Variant scene = base.CreateSceneInfo(player, newlogin);
            if (!newlogin) //从其它场景进入
            {
                player.Ectype.Value["Killed"] = new List<string>();
            }
            if (IsOverTime)
            {
                DateTime overTime = player.Ectype.Value.GetDateTimeOrDefault("OverTime");
                scene["OverTime"] = overTime;
            }
            scene["Killed"] = player.Ectype.Value.GetValue<IList>("Killed");
            player.Ectype.Save();
            return scene;
        }


        /// <summary>
        /// 场景切换检查
        /// </summary>
        /// <param name="player"></param>
        /// <param name="inScene"></param>
        /// <param name="transmitType"></param>
        /// <returns></returns>
        public override bool TransferCheck(PlayerBusiness player, SceneBusiness inScene, TransmitType transmitType)
        {
            //检查必杀的怪是否完成..
            if (transmitType == TransmitType.Pin)
            {
                IList killed = player.Ectype.Value.GetValue<IList>("Killed");
                string npcName = NpcManager.Instance.MustKill(this.ID, killed);
                if (npcName != null)
                {
                    player.Call(ClientCommand.IntoSceneR, false, null, string.Format(TipManager.GetMessage(ClientReturn.SceneEctype4), npcName));
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
            if (killed == null)
            {
                killed = new List<string>();
                player.Ectype.Value["Killed"] = killed;
                player.Ectype.Save();
            }
            else if (killed.Contains(npcID))
            {
                //不能重复打怪
                player.Call(FightCommand.FightFalseR, npcID, string.Format(TipManager.GetMessage(ClientReturn.SceneEctype5)));
                return false;
            }
            string needKill = NpcManager.Instance.MustKill(this.ID, killed, npcID);
            if (needKill != null)
            {
                player.Call(FightCommand.FightFalseR, npcID, string.Format(TipManager.GetMessage(ClientReturn.SceneEctype4), needKill));
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
            return true;
        }
    }
}
