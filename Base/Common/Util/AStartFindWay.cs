using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Util
{
    public class AStartFindWay
    {
        readonly int w;
        readonly int h;
        readonly byte[,] map;

        public AStartFindWay(byte[,] map)
        {
            h = map.GetLength(0);
            w = map.GetLength(1);
            this.map = map;
        }

        //开启列表
        List<APoint> m_openList = new List<APoint>();

        //关闭列表
        List<APoint> m_closeList = new List<APoint>();

        //从开启列表查找F值最小的节点
        private APoint GetMinFFromOpenList()
        {
            APoint Pmin = null;
            foreach (APoint p in m_openList)
            {
                if (Pmin == null || Pmin.G + Pmin.H > p.G + p.H)
                {
                    Pmin = p;
                }
            }
            return Pmin;
        }

        //判断一个点是否为障碍物
        private bool IsBar(APoint p, byte[,] map)
        {
            return map[p.y, p.x] == 0;
        }

        //判断关闭列表是否包含一个坐标的点
        private bool IsInCloseList(int x, int y)
        {
            foreach (APoint p in m_closeList)
            {
                if (p.x == x && p.y == y)
                {
                    return true;
                }
            }
            return false;
        }

        //从关闭列表返回对应坐标的点
        private APoint GetPointFromCloseList(int x, int y)
        {
            foreach (APoint p in m_closeList)
            {
                if (p.x == x && p.y == y)
                {
                    return p;
                }
            }
            return null;
        }

        //判断开启列表是否包含一个坐标的点
        private bool IsInOpenList(int x, int y)
        {
            foreach (APoint p in m_openList)
            {
                if (p.x == x && p.y == y)
                {
                    return true;
                }
            }
            return false;
        }

        //从开启列表返回对应坐标的点
        private APoint GetPointFromOpenList(int x, int y)
        {
            foreach (APoint p in m_openList)
            {
                if (p.x == x && p.y == y)
                {
                    return p;
                }
            }
            return null;
        }

        //计算某个点的G值
        private int GetG(APoint p)
        {
            if (p.father == null)
            {
                return 0;
            }
            if (p.x == p.father.x || p.y == p.father.y)
            {
                return p.father.G + 10;
            }
            else
            {
                return p.father.G + 14;
            }
        }

        //计算某个点的H值
        private int GetH(APoint p, APoint pb)
        {
            return Math.Abs(p.x - pb.x) + Math.Abs(p.y - pb.y);
        }

        //检查当前节点附近的节点
        private void CheckP8(APoint p0, APoint pb)
        {
            for (int xt = p0.x - 1; xt <= p0.x + 1; xt++)
            {
                for (int yt = p0.y - 1; yt <= p0.y + 1; yt++)
                {
                    //排除超过边界和等于自身的点
                    if ((xt >= 0 && xt < w && yt >= 0 && yt < h) && !(xt == p0.x && yt == p0.y))
                    {
                        //排除障碍点和关闭列表中的点
                        if (map[yt, xt] != 0 && !IsInCloseList(xt, yt))
                        {
                            if (IsInOpenList(xt, yt))
                            {
                                APoint pt = GetPointFromOpenList(xt, yt);
                                int G_new = 0;
                                if (p0.x == pt.x || p0.y == pt.y)
                                {
                                    G_new = p0.G + 10;
                                }
                                else
                                {
                                    G_new = p0.G + 14;
                                }
                                if (G_new < pt.G)
                                {
                                    m_openList.Remove(pt);
                                    pt.father = p0;
                                    pt.G = G_new;
                                    m_openList.Add(pt);
                                }
                            }
                            else
                            {
                                //不在开启列表中
                                APoint pt = new APoint();
                                pt.x = xt;
                                pt.y = yt;
                                pt.father = p0;
                                pt.G = GetG(pt);
                                pt.H = GetH(pt, pb);
                                m_openList.Add(pt);
                            }
                        }
                    }
                }
            }
        }

        public APoint FindWay(APoint pa, APoint pb)
        {
            m_openList.Add(pa);
            while (!(IsInOpenList(pb.x, pb.y) || m_openList.Count == 0))
            {
                APoint p0 = GetMinFFromOpenList();
                if (p0 == null)
                {
                    return null;
                }
                m_openList.Remove(p0);
                m_closeList.Add(p0);
                CheckP8(p0, pb);
            }
            APoint p = GetPointFromOpenList(pb.x, pb.y);
            if (p == null)
            {
                return null;
            }
            pb.father = p;
            return pb;
        }

        public void SaveWay(APoint pb)
        {
            APoint p = pb;
            while (p.father != null)
            {
                p = p.father;
                map[p.y, p.x] = 3;
            }
        }

        public void PrintMap()
        {
            for (int a = 0; a < h; a++)
            {
                for (int b = 0; b < w; b++)
                {
                    if (map[a, b] == 1) Console.Write("█");
                    else if (map[a, b] == 3) Console.Write("★");
                    else if (map[a, b] == 4) Console.Write("○");
                    else Console.Write("  ");
                }
                Console.Write("\n");
            }
        }
    }

    public class APoint
    {
        public int y;
        public int x;
        public int G;
        public int H;

        public APoint()
        {
        }
        public APoint(int x0, int y0, int G0, int H0, APoint F)
        {
            x = x0;
            y = y0;
            G = G0;
            H = H0;
            father = F;
        }
        public APoint father;
    }

}
