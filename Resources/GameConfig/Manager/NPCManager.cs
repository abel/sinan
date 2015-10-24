using System.Collections;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class NpcManager : ConfigManager<Npc>
    {
        readonly static NpcManager m_instance = new NpcManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static NpcManager Instance
        {
            get { return m_instance; }
        }

        NpcManager()
        {
        }

        protected override void CreateT(Variant v)
        {
            Npc npc = new Npc(v);
            if (!string.IsNullOrEmpty(npc.SceneID))
            {
                m_configs[npc.ID] = npc;
            }
        }

        public Variant FindTaskNpc(string sceneID, string npcID, string taskID)
        {
            Npc npc;
            if (m_configs.TryGetValue(npcID, out npc))
            {
                return npc.GetTaskOrDefault(taskID, sceneID);
            }
            return null;
        }

        /// <summary>
        /// 检查必须Kill的怪
        /// </summary>
        /// <param name="sceneID">场景</param>
        /// <param name="killed">已被清除了的怪</param>
        /// <param name="killLev">检查的清除等级</param>
        /// <returns></returns>
        public string MustKill(string sceneID, IList killed, int killLev = 1)
        {
            foreach (var npc in m_configs.Values)
            {
                if (npc.KillLev > 0 && npc.KillLev <= killLev && npc.SceneID == sceneID)
                {
                    if (killed == null || (!killed.Contains(npc.ID)))
                    {
                        return npc.Name ?? string.Empty;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// 跟怪战斗前检查必须杀死的怪
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="killed"></param>
        /// <param name="npcID"></param>
        /// <returns></returns>
        public string MustKill(string sceneID, IList killed, string npcID)
        {
            Npc npc;
            if (m_configs.TryGetValue(npcID, out npc) && npc.KillLev > 1)
            {
                return MustKill(sceneID, killed, npc.KillLev);
            }
            return null;
        }
    }
}
