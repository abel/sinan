using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理场景上的明雷
    /// </summary>
    partial class SceneBusiness
    {
        /// <summary>
        /// 场景上是否有箱子
        /// </summary>
        public bool HaveBox
        {
            get;
            set;
        }

        /// <summary>
        /// 场景上是否有明怪
        /// </summary>
        public bool HaveApc
        {
            get;
            set;
        }

        /// <summary>
        /// 获取随机降生点
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public Point RandomBronPoint(Rectangle range)
        {
            List<Point> all;
            if (range == Rectangle.Empty)
            {
                all = m_walk;
            }
            else
            {
                all = m_walk.FindAll(x => range.Contains(x));
                if (all.Count == 0)
                {
                    all = m_walk;
                }
            }
            if (all.Count == 0)
            {
                return Point.Empty;
            }
            int index = NumberRandom.Next(0, all.Count);
            return all[index];
        }

        /// <summary>
        /// 得到指定范围内可行走的数据
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Point> GetWalkPoints(Rectangle range)
        {
            List<Point> all;
            if (range == Rectangle.Empty)
            {
                all = m_walk;
            }
            else
            {
                all = m_walk.FindAll(x => range.Contains(x));
            }
            return all;
        }
    }
}
