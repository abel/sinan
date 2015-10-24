using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 组队副本场景
    /// </summary>
    public class SceneInstance : SceneBusiness
    {
        public override SceneType SceneType
        {
            get { return SceneType.Instance; }
        }

        public SceneInstance(GameConfig scene)
            : base(scene)
        {
            m_showAll = false;
        }

        SceneInstance(SceneInstance scene)
            : base(scene)
        {
            m_showAll = false;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneInstance(this);
        }

        protected override Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            Variant scene = base.CreateSceneInfo(player, newlogin);
            var t = player.TeamInstance;
            if (t != null && t.OverTime.HasValue)
            {
                player.ShowID = t.ID;
                scene["OverTime"] = t.OverTime;
            }
            return scene;
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="note"></param>
        protected override bool WalkEvent(PlayerBusiness player)
        {
            var t = player.TeamInstance;
            if (t != null && t.OverTime.HasValue && t.OverTime < DateTime.UtcNow)
            {
                player.TeamInstance = null;
                //超时结束副本
                t.Over();
                return false;
            }
            return true;
        }

        /// <summary>
        /// 退出场景(玩家仍在线)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool ExitScene(PlayerBusiness player)
        {
            string playerID = player.ID;
            if (m_players.TryRemove(playerID, out player) && player != null)
            {
                //发送通知
                CallAll(player.ShowID, ClientCommand.ExitSceneR, new object[] { playerID });
                return true;
            }
            return false;
        }
    }
}
