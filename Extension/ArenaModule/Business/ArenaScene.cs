using System.Drawing;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.ArenaModule.Business
{
    public class ArenaScene
    {
        private string m_sceneid;
        private int m_mapwidth;
        private int m_mapheight;
        private  int m_gridwidth;

        private Point m_mapSize;
        private Point m_sceneSize;
        private  Point m_zero;

        /// <summary>
        /// 场景ID
        /// </summary>
        public string SceneID 
        {
            get { return m_sceneid; }
            set { m_sceneid = value; }
        }

        /// <summary>
        /// 地图宽度
        /// </summary>
        public int MapWidth 
        {
            get { return m_mapwidth; }
            set { m_mapwidth = value; }
        }

        /// <summary>
        /// 地图高度
        /// </summary>
        public int MapHeight
        {
            get { return m_mapheight; }
            set { m_mapheight = value; }
        }

        /// <summary>
        /// 网格宽度
        /// </summary>
        public int GridWidth 
        {
            get { return m_gridwidth; }
            set { m_gridwidth = value; }
        }

        /// <summary>
        /// 地图尺寸
        /// </summary>
        public Point MapSize 
        {
            get { return m_mapSize; }
            set { m_mapSize = value; }
        }

        public Point SceneSize 
        {
            get { return m_sceneSize; }
            set { m_sceneSize = value; }
        }

        public Point Zero 
        {
            get { return m_zero; }
            set { m_zero = value; }
        }

        /// <summary>
        /// 场景基本信息
        /// </summary>
        /// <param name="gc"></param>
        public ArenaScene(GameConfig gc)
        {
            m_sceneid = gc.ID;
            Variant ui = gc.UI;
            if (ui == null) 
                return;

            m_mapwidth = ui.GetIntOrDefault("MapWidth");
            m_mapheight = ui.GetIntOrDefault("MapHeight");
            m_gridwidth = ui.GetIntOrDefault("GridWidth");

            Point();
        }

        /// <summary>
        /// 得到地图相关信息
        /// </summary>
        private void Point() 
        {
            m_mapSize = new Point(m_mapwidth, m_mapheight);

            m_sceneSize = new Point();
            m_sceneSize.X = m_mapSize.X / m_gridwidth;
            m_sceneSize.Y = m_mapSize.Y / m_gridwidth;

            m_zero = new Point();
            m_zero.X = -m_sceneSize.X * m_gridwidth / 2;
            m_zero.Y = m_sceneSize.X * m_gridwidth / 4;
        }


        /// <summary>
        /// 得到屏幕坐标
        /// </summary>
        /// <param name="p"></param>
        public Point ToScreen(Point p)
        {
            p.X = p.X * m_gridwidth / 2;
            p.Y = p.Y * m_gridwidth / 2;

            int x = p.X - p.Y;
            int y = (p.X + p.Y) / 2;

            p.X = x - m_zero.X;
            p.Y = y - m_zero.Y;
            return p;
        }
    }
}
