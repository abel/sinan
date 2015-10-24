using Sinan.Data;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 活动场景
    /// </summary>
    abstract public class ScenePart : SceneBusiness
    {
        /// <summary>
        /// 活动对象
        /// </summary>
        protected PartBusiness m_part;

        public PartBusiness Part
        {
            get { return m_part; }
        }

        public ScenePart(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
        }

        protected ScenePart(ScenePart scene)
            : base(scene)
        { }

        public virtual void Start(PartBusiness part)
        {
            m_part = part;
            SceneApcProxy.LoadPartApcs(this, m_part.Part.ID);
        }

        /// <summary>
        /// 活动结束.
        /// </summary>
        /// <returns></returns>
        public virtual bool End()
        {
            bool over = true;
            foreach (var player in m_players.Values)
            {
                FightBase f = player.Fight;
                if (f != null)
                {
                    f.ForcedOver();
                }
                this.TownGate(player, TransmitType.OverTime);
            }
            SceneApcProxy.UnloadPartApcs(this, m_part.Part.ID);
            return over;
        }
    }
}
