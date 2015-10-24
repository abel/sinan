using System;
using System.Collections.Generic;
using System.Linq;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 场景代理类.管理所有加载的场景
    /// </summary>
    static partial class ScenesProxy
    {
        /// <summary>
        /// 空路径
        /// </summary>
        static readonly List<string> emptyPaht = new List<string>(0);

        /// <summary>
        /// 路径缓存
        /// </summary>
        static readonly Dictionary<string, List<Tuple<int, int, List<string>>>> cachePath = new Dictionary<string, List<Tuple<int, int, List<string>>>>();

        /// <summary>
        /// 转换传送阵地图
        /// </summary>
        /// <param name="scene"></param>
        private static void LoadSceneWays()
        {
            m_sceneWay.Clear();
            foreach (var v in ScenePinManager.Instance.Values)
            {
                if (v.AutoPath)
                {
                    List<string> x;
                    if (!m_sceneWay.TryGetValue(v.SceneA, out x))
                    {
                        x = new List<string>();
                        m_sceneWay.Add(v.SceneA, x);
                    }
                    string bid = v.SceneB;
                    if (!string.IsNullOrEmpty(bid))
                    {
                        if (!x.Contains(bid))
                        {
                            if (!m_sceneWay.ContainsKey(bid))
                            {
                                m_sceneWay.Add(bid, new List<string>());
                            }
                            x.Add(bid);
                        }
                    }
                }
            }
            CachePath();
        }

        /// <summary>
        /// 缓存所有寻路
        /// </summary>
        static public void CachePath()
        {
            List<string> secenes = m_sceneWay.Keys.ToList();
            foreach (var sceneA in secenes)
            {
                foreach (var sceneB in secenes)
                {
                    if (sceneA == sceneB) continue;
                    List<Tuple<int, int, List<string>>> caches = new List<Tuple<int, int, List<string>>>();
                    try
                    {
                        var paths = AutoFindWay.FindAllPath<string>(m_sceneWay, sceneA, sceneB);
                        paths.Sort(delegate(List<string> x, List<string> y)
                        {
                            return x.Count - y.Count;
                        });
                        foreach (var path in paths)
                        {
                            var x = FindScenePin(path);
                            if (x != null)
                            {
                                caches.Add(x);
                            }
                        }
                        if (caches.Count > 0)
                        {
                            cachePath.Add(sceneA + sceneB, caches);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogWrapper.Warn(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 填充传送阵
        /// </summary>
        /// <param name="path"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        private static Tuple<int, int, List<string>> FindScenePin(List<string> path)
        {
            int minLev = 0;
            int maxLev = int.MaxValue;
            for (int i = 0; i < path.Count - 1; i++)
            {
                SceneBusiness scenes;
                if (!m_scenes.TryGetValue(path[i], out scenes))
                {
                    return null;
                }
                string next = path[i + 1];
                //var pin = scenes.Pins.Find(x => x.SceneB == next);
                var pin = ScenePinManager.Instance.FindPin(path[i], next);
                if (pin == null)
                {
                    return null;
                }
                SceneBusiness sceneB;
                if (!m_scenes.TryGetValue(next, out sceneB))
                {
                    return null;
                }
                minLev = Math.Max(minLev, sceneB.MinLev);
                if (sceneB.MaxLev > 0)
                {
                    maxLev = Math.Min(maxLev, sceneB.MaxLev);
                }
                //更换为转送阵ID
                path[i] = pin.ID;
            }
            //如果所有的场景都找到传送阵,最后一个为场景ID,不再使用
            if (path.Count > 0)
            {
                path.RemoveAt(path.Count - 1);
            }
            return new Tuple<int, int, List<string>>(minLev, maxLev, path);
        }

        static public List<string> FindPath(string sceneA, string sceneB, int level = 0)
        {
            List<Tuple<int, int, List<string>>> paths;
            if (cachePath.TryGetValue(sceneA + sceneB, out paths))
            {
                foreach (var path in paths)
                {
                    if (level <= 0) return path.Item3;
                    if (level >= path.Item1 && level <= path.Item2)
                    {
                        return path.Item3;
                    }
                }
                if (paths.Count > 0)
                {
                    return paths[0].Item3;
                }
            }
            return emptyPaht;
        }
    }
}
