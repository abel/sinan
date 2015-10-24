using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 个人竞技场
    /// </summary>
    class ScenePerson : SceneArena
    {
        public override SceneType SceneType
        {
            get { return SceneType.ArenaPerson; }
        }

        public ScenePerson(GameConfig scene)
            : base(scene)
        {
        }

        protected ScenePerson(ScenePerson scene)
            : base(scene)
        {
        }

        public override SceneBusiness CreateNew()
        {
            return new ScenePerson(this);
        }
    }
}
