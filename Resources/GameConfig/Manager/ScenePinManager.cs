using System.Collections.Generic;
using System.Drawing;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    sealed public class ScenePinManager : ConfigManager<ScenePin>
    {
        readonly static ScenePinManager m_instance = new ScenePinManager();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static ScenePinManager Instance
        {
            get { return m_instance; }
        }

        ScenePinManager()
        {
        }

        protected override void CreateT(Variant v)
        {
            ScenePin npc = new ScenePin(v);
            m_configs[npc.ID] = npc;
        }

        public ScenePin GetPin(string pinid, string sceneid)
        {
            ScenePin pin;
            if (m_configs.TryGetValue(pinid, out pin))
            {
                if (sceneid == null || pin.SceneA == sceneid)
                {
                    return pin;
                }
            }
            return null;
        }


        public ScenePin FindPin(string sceneA, string sceneB)
        {
            foreach (var pin in m_configs.Values)
            {
                if (pin.SceneA == sceneA && pin.SceneB == sceneB)
                {
                    return pin;
                }
            }
            return null;
        }

        public List<Rectangle> GetAllPinRang(string scene)
        {
            List<Rectangle> rangs = new List<Rectangle>();
            foreach (var pin in m_configs.Values)
            {
                if (pin.SceneA == scene)
                {
                    rangs.Add(pin.Range);
                }
            }
            return rangs;
        }

    }
}
