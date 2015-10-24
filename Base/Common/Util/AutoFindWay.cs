using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Util
{
    /// <summary>
    /// 自动寻路
    /// </summary>
    static public class AutoFindWay
    {
        static public List<T> FindPath<T>(IDictionary<T, List<T>> map, T a, T b)
        {
            List<T> path = new List<T>(map.Count);
            path.Add(a);
            if (!FindPath(map, a, b, path))
            {
                path.Clear();
            }
            return path;
        }

        /// <summary>
        /// 自动路径寻路
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        static bool FindPath<T>(IDictionary<T, List<T>> map, T a, T b, IList<T> path)
        {
            T last = path[path.Count - 1];
            if (last.Equals(b))
            {
                return true;
            }
            List<T> paths;
            if (map.TryGetValue(last, out paths))
            {
                foreach (var next in paths)
                {
                    if (!path.Contains(next))
                    {
                        path.Add(next);
                        if (FindPath(map, a, b, path))
                        {
                            return true;
                        }
                        else
                        {
                            path.RemoveAt(path.Count - 1);
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// 最短路径寻路
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">路径数据</param>
        /// <param name="a">起点A</param>
        /// <param name="b">终点B</param>
        /// <param name="shortOut">找到小于等于指定长度的路径就短路(查找全部设为0)</param>
        /// <returns></returns>
        static public List<T> FindShortestPath<T>(IDictionary<T, List<T>> map, T a, T b, int shortOut = 0)
        {
            List<T> path = new List<T>(map.Count);
            List<List<T>> allPath = new List<List<T>>();
            path.Add(a);
            if (!FindShortestPath(map, a, b, path, allPath, shortOut))
            {
                path.Clear();
                return path;
            }
            List<T> result = allPath[0];
            for (int i = 1; i < allPath.Count; i++)
            {
                if (allPath[i].Count < result.Count)
                {
                    result = allPath[i];
                }
            }
            return result;
        }

        /// <summary>
        /// 寻找所有路径
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">路径数据</param>
        /// <param name="a">起点A</param>
        /// <param name="b">终点B</param>
        /// <param name="shortOut">找到小于等于指定长度的路径就短路(查找全部设为0)</param>
        /// <returns></returns>
        static public List<List<T>> FindAllPath<T>(IDictionary<T, List<T>> map, T a, T b, int shortOut = 0)
        {
            List<T> path = new List<T>(map.Count);
            List<List<T>> allPath = new List<List<T>>();
            path.Add(a);
            FindShortestPath(map, a, b, path, allPath, shortOut);
            return allPath;
        }


        /// <summary>
        /// 最短路径寻路
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="map">路径数据</param>
        /// <param name="a">起点A</param>
        /// <param name="b">终点B</param>
        /// <param name="path">当前行走路径</param>
        /// <param name="allPath">所有可行走路径</param>
        /// <param name="shortOut">找到小于等于指定长度的路径就短路(查找全部设为0)</param>
        /// <returns></returns>
        static bool FindShortestPath<T>(
            IDictionary<T, List<T>> map,
            T a, T b,
            IList<T> path,
            List<List<T>> allPath,
            int shortOut = 0)
        {
            T last = path[path.Count - 1];
            if (last.Equals(b))
            {
                allPath.Add(path.ToList());
                return true;
            }
            foreach (var next in map[last])
            {
                if (!path.Contains(next))
                {
                    path.Add(next);
                    if (FindShortestPath(map, a, b, path, allPath, shortOut))
                    {
                        if (path.Count <= shortOut)
                        {
                            return true;
                        }
                    }
                    path.Remove(next);
                }
            }
            return allPath.Count > 0;
        }
    }
}
