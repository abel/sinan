using System;
using Sinan.Command;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 打怪类活动
    /// </summary>
    public sealed class HuntBusiness : PartBusiness
    {
        public HuntBusiness(Part part)
            : base(part)
        {
        }

        public override void Start(DateTime startTime, DateTime endTime)
        {
            m_start = startTime;
            m_end = endTime;
            PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { this });
            foreach (SceneBusiness scene in m_scenes)
            {
                if (scene != null)
                {
                    SceneApcProxy.LoadPartApcs(scene, m_part.ID);
                }
            }
        }

        /// <summary>
        /// 结束活动
        /// </summary>
        /// <returns></returns>
        public override void End()
        {
            base.End();
            foreach (SceneBusiness scene in m_scenes)
            {
                SceneApcProxy.UnloadPartApcs(scene, m_part.ID);
            }
        }
    }
}