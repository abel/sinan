using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 家园
    /// </summary>
    public sealed class SceneHome : SceneBusiness
    {
        /// <summary>
        /// 家园地图ID
        /// </summary>
        public const string DefaultID = "MAP_C001";

        /// <summary>
        /// 家园默认传送阵
        /// </summary>
        public static string DefaultPin = "P_583357099";

        public override SceneType SceneType
        {
            get { return SceneType.Home; }
        }

        public SceneHome(GameConfig scene)
            : base(scene)
        {
            m_showAll = false;
        }

        SceneHome(SceneHome scene)
            : base(scene)
        {
            m_showAll = false;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneHome(this);
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
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.SceneHome1));
                return false;
            }
            if (player.Fight != null)
            {
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.SceneHome2));
                return false;
            }
            if (player.Scene != null)
            {
                switch (player.Scene.SceneType)
                {
                    case SceneType.Home:
                    case SceneType.City:
                    case SceneType.Outdoor:
                        break;
                    default:
                        {
                            player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.SceneHome3));
                            return false;
                        }
                }
            }
            return true;
        }

        protected override Util.Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            player.ShowID = player.PID;
            return base.CreateSceneInfo(player, newlogin);
        }

        /// <summary>
        /// 成功进入场景
        /// </summary>
        /// <param name="note"></param>
        protected override void IntoSceneSuccess(PlayerBusiness player, Variant sceneinfo)
        {
            player.Call(ClientCommand.IntoSceneR, true, sceneinfo, EmptyPlayerList);
            player.FightTime = DateTime.UtcNow.AddSeconds(5);
            player.Online = true;
            player.Save();
        }
    }
}
