using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 家族竞技场
    /// </summary>
    class SceneFamily : SceneArena
    {
        public override SceneType SceneType
        {
            get { return SceneType.ArenaFamily; }
        }

        public SceneFamily(GameConfig scene)
            : base(scene)
        {
        }

        protected SceneFamily(SceneFamily scene)
            : base(scene)
        {
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneFamily(this);
        }
    }
}
