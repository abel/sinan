
using System.Collections.Generic;
using Sinan.GameModule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif


namespace Sinan.FrontServer
{
    /// <summary>
    /// 箱子代理
    /// </summary>
    sealed public class BoxProxy
    {
        /// <summary>
        /// 所有箱子(Key:箱子ID,Value: BoxBusiness) 
        /// </summary>
        static readonly ConcurrentDictionary<string, BoxBusiness> m_boxs = new ConcurrentDictionary<string, BoxBusiness>();

        /// <summary>
        /// 加载Box
        /// </summary>
        public static void LoadBox()
        {
            foreach (var v in BoxManager.Instance.Values)
            {
                SceneBusiness scenes;
                if (ScenesProxy.TryGetScene(v.SceneID, out scenes))
                {
                    BoxBusiness bb;
                    if (scenes.SceneType == SceneType.Ectype ||
                        scenes.SceneType == SceneType.SubEctype ||
                        scenes.SceneType == SceneType.Instance
                        )
                    {
                        bb = new BoxBusiness(v);
                    }
                    else
                    {
                        bb = new BoxShare(v);
                    }
                    bb.InitBron(scenes.Walking, ScenePinManager.Instance.GetAllPinRang(v.SceneID));
                    m_boxs[v.ID] = bb;
                    scenes.HaveBox = true;
                }
            }
        }

        static public bool TryGetBox(string boxID, out BoxBusiness box)
        {
            return m_boxs.TryGetValue(boxID, out box);
        }

        static public List<BoxBusiness> GetSceneBox(string sceneID)
        {
            List<BoxBusiness> boxs = new List<BoxBusiness>();
            foreach (var v in m_boxs.Values)
            {
                if (v.CanOpen && v.SceneID == sceneID)
                {
                    boxs.Add(v);
                }
            }
            return boxs;
        }
    }
}
