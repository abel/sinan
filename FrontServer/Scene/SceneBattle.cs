using System;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战场(支持超时传出.)
    /// </summary>
    public class SceneBattle : SceneBusiness
    {
        public override SceneType SceneType
        {
            get { return SceneType.Battle; }
        }

        readonly int m_maxStay;

        /// <summary>
        /// 在场景中的最大停留时间(秒)
        /// </summary>
        public int MaxStay
        {
            get { return m_maxStay; }
        }

        public SceneBattle(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
            Variant v = scene.Value.GetVariantOrDefault("Config");
            if (v != null)
            {
                m_maxStay = v.GetIntOrDefault("MaxStay");
                if (m_maxStay < 0) m_maxStay = 0;
                if (DeadDestination == null)
                {
                    LogWrapper.Warn("Unknown dead go home:" + this.ID);
                }
            }
        }

        protected SceneBattle(SceneBattle scene)
            : base(scene)
        {
            m_showAll = true;
            this.m_maxStay = scene.m_maxStay;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneBattle(this);
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="note"></param>
        protected override bool WalkEvent(PlayerBusiness player)
        {
            if (this.MaxStay > 0)
            {
                // 超时转出.
                if (player.Ectype.Value.GetDateTimeOrDefault("OverTime") < DateTime.UtcNow)
                {
                    this.TownGate(player, TransmitType.OverTime);
                    return false;
                }
            }
            return base.WalkEvent(player);
        }

        protected override Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            Variant scene = base.CreateSceneInfo(player, newlogin);
            if (newlogin) //(断线后登录进入)
            {
                if (this.MaxStay > 0)
                {
                    scene["OverTime"] = player.Ectype.Value.GetDateTimeOrDefault("OverTime");
                }
            }
            else //正常进入
            {
                if (this.MaxStay > 0)
                {
                    DateTime overTime = DateTime.UtcNow.AddSeconds(this.MaxStay);
                    player.Ectype.Value["OverTime"] = overTime;
                    scene["OverTime"] = overTime;
                }
            }
            return scene;
        }
    }
}
