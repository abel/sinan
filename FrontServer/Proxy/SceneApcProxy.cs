using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Schedule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 场景怪物代理
    /// (管理场景上所有的明怪)
    /// </summary>
    sealed public class SceneApcProxy : SchedulerBase
    {
        HashSet<string> scenes = new HashSet<string>();
        private SceneApcProxy()
            : base(10 * 1000, 5 * 1000)
        {
        }

        protected override void Exec()
        {
            try
            {
                foreach (var item in m_activeApcs.Values)
                {
                    if (item.TryActiveApc())
                    {
                        scenes.Add(item.SceneID);
                    }
                }
                foreach (var sceneID in scenes)
                {
                    SceneBusiness scene;
                    if (ScenesProxy.TryGetScene(sceneID, out scene))
                    {
                        IList apcs = GetSceneApc(scene.ID);
                        scene.CallAll(0, ClientCommand.RefreshApcR, new object[] {scene.ID, apcs });
                        scene.HaveApc = apcs.Count > 0;
                    }
                }
            }
            finally
            {
                scenes.Clear();
            }
        }

        /// <summary>
        /// 所有明怪产生规则
        /// </summary>
        static readonly Dictionary<string, SceneApc> m_sceneApcs = new Dictionary<string, SceneApc>();

        /// <summary>
        /// 场景上的活动怪
        /// </summary>
        static readonly ConcurrentDictionary<string, SceneApc> m_activeApcs = new ConcurrentDictionary<string, SceneApc>();

        /// <summary>
        /// 加载所有明雷
        /// </summary>
        public static void LoadSceneApc()
        {
            foreach (GameConfig v in GameConfigAccess.Instance.Find(MainType.SceneAPC))
            {
                SceneApc bb = new SceneApc(v);
                if (bb.InitApc())
                {
                    m_sceneApcs[bb.ID] = bb;
                    if (bb.SubType == "Normal")
                    {
                        bb.ResetApc();
                        m_activeApcs.TryAdd(bb.ID, bb);
                    }
                }
            }
        }

        /// <summary>
        /// 得到场景上的明雷
        /// </summary>
        /// <param name="sceneID"></param>
        /// <returns></returns>
        static public List<SceneApc> GetSceneApc(string sceneID)
        {
            List<SceneApc> apcs = new List<SceneApc>();
            foreach (var v in m_activeApcs)
            {
                if (v.Value.SceneID == sceneID)
                {
                    apcs.Add(v.Value);
                }
            }
            return apcs;
        }

        /// <summary>
        /// 得到明雷
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        static public SceneApc FindOne(string id)
        {
            SceneApc sb;
            if (m_activeApcs.TryGetValue(id, out sb))
            {
                return sb;
            }
            return null;
        }

        /// <summary>
        /// 加载活动怪
        /// </summary>
        /// <param name="scene"></param>
        static public void LoadPartApcs(SceneBusiness scene, string partID)
        {
            foreach (SceneApc bb in m_sceneApcs.Values)
            {
                if (bb.SceneID == scene.ID && bb.SubType == partID)
                {
                    bb.ResetApc();
                    m_activeApcs[bb.ID] = bb;
                    scene.HaveApc = true;
                }
            }
            IList apcs = GetSceneApc(scene.ID);
            scene.CallAll(0, ClientCommand.RefreshApcR, new object[] { scene.ID, apcs });
        }

        /// <summary>
        /// 移除活动怪
        /// </summary>
        /// <param name="scene"></param>
        static public void UnloadPartApcs(SceneBusiness scene, string partID)
        {
            bool haveAPc = false;
            foreach (SceneApc sb in m_activeApcs.Values)
            {
                if (sb.SceneID == scene.ID)
                {
                    if (sb.SubType == partID)
                    {
                        SceneApc t;
                        m_activeApcs.TryRemove(sb.ID, out t);
                    }
                    else
                    {
                        haveAPc = true;
                    }
                }
            }
            scene.HaveApc = haveAPc;
            IList apcs = GetSceneApc(scene.ID);
            scene.CallAll(0, ClientCommand.RefreshApcR, new object[] {scene.ID, apcs });
        }
    }
}
