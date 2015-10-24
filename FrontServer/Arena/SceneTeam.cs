using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 队伍竞技场
    /// </summary>
    public class SceneTeam : SceneArena
    {
        public override SceneType SceneType
        {
            get { return SceneType.ArenaTeam; }
        }

        public SceneTeam(GameConfig scene)
            : base(scene)
        {
        }

        protected SceneTeam(SceneTeam scene)
            : base(scene)
        {
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneTeam(this);
        }
    }
}
