using System;
using System.Collections.Generic;
using Sinan.Core;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 场景代理类.管理所有加载的场景
    /// </summary>
    static public partial class ScenesProxy
    {
        /// <summary>
        /// 本服务器承载的场景
        /// </summary>
        static readonly Dictionary<string, SceneBusiness> m_scenes = new Dictionary<string, SceneBusiness>();

        /// <summary>
        /// 场景之间的可达信息(通过传送阵)
        /// "A",{"B","C"} 表示A场景可以直接到达B或C场景
        /// </summary>
        static readonly Dictionary<string, List<string>> m_sceneWay = new Dictionary<string, List<string>>();

        #region 初始化.加载场景
        /// <summary>
        /// 初始化.加载场景
        /// </summary>
        /// <param name="app"></param>
        static public void LoadScenes(Application app)
        {
            var lists = GameConfigAccess.Instance.Find(MainType.Map);
            foreach (var item in lists)
            {
                InitScene(item);
            }
            //设置父子副本关系
            InitSubEctype();

            // 加载道路,必须先加载传送阵
            LoadSceneWays();

            //加载宝箱
            BoxProxy.LoadBox();

            //加载明雷
            SceneApcProxy.LoadSceneApc();

            // 检查配置
            CheckConfig();
            //已改为直接处理
            //StartWork();
        }

        private static void InitScene(GameConfig item)
        {
            string sceneID = item.ID;
            SceneBusiness scene;
            switch ((SceneType)Int32.Parse(item.SubType))
            {
                case SceneType.City:
                    scene = new SceneCity(item);
                    break;
                case SceneType.Outdoor:
                    scene = new SceneOutdoor(item);
                    break;
                case SceneType.Ectype:
                    OnlyOne(item);
                    scene = new SceneEctype(item);
                    break;
                case SceneType.SubEctype:
                    OnlyOne(item);
                    scene = new SceneSubEctype(item);
                    break;
                case SceneType.Home:
                    scene = new SceneHome(item);
                    break;
                case SceneType.Battle:
                    scene = new SceneBattle(item);
                    break;
                case SceneType.Rob:
                    scene = new SceneRob(item);
                    break;
                case SceneType.Pro:
                    OnlyOne(item);
                    scene = new ScenePro(item);
                    break;
                case SceneType.ArenaPerson:
                    scene = new ScenePerson(item);
                    break;
                case SceneType.ArenaFamily:
                    scene = new SceneFamily(item);
                    break;
                case SceneType.ArenaTeam:
                    scene = new SceneTeam(item);
                    break;
                case SceneType.Instance:
                    scene = new SceneInstance(item);
                    break;
                default:
                    return;
            }
            if (item.UI != null && item.UI.ContainsKey("RoadObject"))
            {
                Variant v = item.UI["RoadObject"] as Variant;
                if (v == null)
                {
                    LogWrapper.Warn(string.Format("Scene{0},no walking info", scene.Name));
                    return;
                }
                scene.InitWalk(v);
            }

            m_scenes.Add(sceneID, scene);
            //const int line = 2;
            //SceneBusiness[] scenes = new SceneBusiness[line];
            //scenes[0] = scene;
            //for (int i = 1; i < line; i++)
            //{
            //    scenes[i] = scene.CreateNew();
            //}
            //m_scenes.Add(sceneID, scenes);
        }

        static void OnlyOne(GameConfig scene)
        {
            Variant v = scene.Value.GetVariantOrDefault("Config");
            if (v == null)
            {
                v = new Variant();
                scene.Value["Config"] = v;
            }
            v["MinMember"] = 1;
            v["MaxMember"] = 1;
        }

        static void InitSubEctype()
        {
            foreach (var item in m_scenes.Values)
            {
                SceneSubEctype subEctype = item as SceneSubEctype;
                if (subEctype != null)
                {
                    int maxStay = 0;
                    SceneBusiness x;
                    if (m_scenes.TryGetValue(subEctype.EctypeID, out x))
                    {
                        SceneEctype ectype = x as SceneEctype;
                        if (ectype != null)
                        {
                            maxStay = ectype.MaxStay;
                        }
                        else
                        {
                            Console.WriteLine("Scene{0},father err", subEctype.Name);
                        }
                    }
                    //foreach (SceneSubEctype v in item)
                    //{
                    //    v.IsOverTime = maxStay > 0;
                    //}
                }
            }
        }
        #endregion

        #region 检查场景的行走和传送配置
        static void CheckConfig()
        {
            foreach (var pin in ScenePinManager.Instance.Values)
            {
                if (!CheckWalk(pin.SceneA, pin.X, pin.Y))
                {
                    LogWrapper.Warn(String.Format("Scene{0},Pin:{1} {2},{3} unreach",
                        pin.SceneA, pin.ID, pin.X, pin.Y));
                }
                if (!CheckWalk(pin.SceneB, pin.ScenebX, pin.ScenebY))
                {
                    Console.WriteLine("Scene{0},Pin:{1}to scene {2} {3},{4} can't walk",
                        pin.SceneA, pin.ID, pin.SceneB, pin.ScenebX, pin.ScenebY);
                }
            }

            foreach (var item in m_scenes.Values)
            {
                SceneBusiness scene = item;
                if (!CheckWalk(scene.ID, scene.BornX, scene.BornY))
                {
                    LogWrapper.Warn(String.Format("Scene{0},born err:{1},{2}",
                        scene.Name, scene.BornX, scene.BornY));
                }
                Destination des = scene.PropDestination;
                if (des != null)
                {
                    if (!CheckWalk(des.SceneID, des.X, des.Y))
                    {
                        LogWrapper.Warn(String.Format("Scene{0},prop {1}:{2},{3} can't walk",
                            scene.Name, des.SceneID, des.X, des.Y));
                    }
                }
                des = scene.DeadDestination;
                if (des != null)
                {
                    if (!CheckWalk(des.SceneID, des.X, des.Y))
                    {
                        LogWrapper.Warn(String.Format("Scene{0},dead {1}:{2},{3} can't walk",
                           scene.Name, des.SceneID, des.X, des.Y));
                    }
                }
            }
        }

        private static bool CheckWalk(string scenesID, int x, int y)
        {
            SceneBusiness scenes;
            if (m_scenes.TryGetValue(scenesID, out scenes))
            {
                if (x == 0 && y == 0)
                {
                    return true;
                }
                return scenes.CheckWalk(x, y);
            }
            return false;
        }
        #endregion

        /// <summary>
        /// 获取指定ID的SceneBusiness
        /// </summary>
        /// <param name="sceneID"></param>
        /// <param name="scene"></param>
        /// <returns></returns>
        static public bool TryGetScene(string sceneID, out SceneBusiness scene, int line = 0)
        {
            SceneBusiness obj;
            if (m_scenes.TryGetValue(sceneID, out obj))
            {
                scene = obj;
                return true;
            }
            scene = null;
            return false;
        }

        /// <summary>
        /// 得到各场景的在线人数
        /// </summary>
        /// <returns></returns>
        static public Dictionary<string, int> OnlineCount()
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            foreach (var scenes in m_scenes.Values)
            {
                //int count = scenes.Sum(x => x.Players.Count);
                int count = scenes.Players.Count;
                if (count > 0)
                {
                    result.Add(scenes.ID, count);
                }
            }
            return result;
        }

        #region 开启线程..处理场景事务..
        //static private void StartWork()
        //{
        //    //处理场景业务的线程数.
        //    const int m_threadCount = 4;
        //    for (int i = 0; i < m_threadCount; i++)
        //    {
        //        //Task.Factory.StartNew(SceneStart);
        //        Thread thread = new Thread(new ThreadStart(SceneStart));
        //        thread.Start();
        //    }
        //}

        //static void SceneStart()
        //{
        //    while (true)
        //    {
        //        bool process = false;
        //        foreach (var item in m_scenesList)
        //        {
        //            if (item.ExecWork())
        //            {
        //                process = true;
        //            }
        //        }
        //        if (process)
        //        {
        //            System.Threading.Thread.Sleep(0);
        //        }
        //        else
        //        {
        //            System.Threading.Thread.Sleep(1);
        //        }
        //    }
        //}
        #endregion
    }
}
