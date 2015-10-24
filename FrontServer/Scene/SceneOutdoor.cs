using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Observer;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 野外
    /// </summary>
    public sealed class SceneOutdoor : SceneBusiness
    {
        public override SceneType SceneType
        {
            get { return SceneType.Outdoor; }
        }

        public SceneOutdoor(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
        }

        SceneOutdoor(SceneOutdoor scene)
            : base(scene)
        {
            m_showAll = true;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneOutdoor(this);
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
