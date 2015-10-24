using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Data;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 城市
    /// </summary>
    public sealed class SceneCity : SceneBusiness
    {
        /// <summary>
        /// 默认城市,凡海城
        /// </summary>
        public const string DefaultID = "MAP_A001";

        public override SceneType SceneType
        {
            get { return SceneType.City; }
        }

        public SceneCity(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
        }

        SceneCity(SceneCity scene)
            : base(scene)
        {
            m_showAll = true;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneCity(this);
        }

        /// <summary>
        /// 退出场景(玩家仍在线)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool ExitScene(PlayerBusiness player)
        {
            player.SaveBaseScine();
            return base.ExitScene(player);
        }
    }
}
