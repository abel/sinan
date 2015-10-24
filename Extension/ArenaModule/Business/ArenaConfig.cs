using System.Collections;
using Sinan.Data;
using Sinan.Util;

namespace Sinan.ArenaModule.Business
{
    class ArenaConfig
    {
        private static GameConfig m_gc;
        private static IList m_win;
        private static IList m_petnumber;
        private static IList m_petlevel;
        private static IList m_group;
        private static IList m_scene;
        private static IList m_preparetime;
        private static IList m_gametime;
        private static Variant m_v;


        public static void ArenaInfo(GameConfig gc) 
        {
            m_gc = gc;
            m_v = m_gc.Value;
            m_win = m_v.GetValue<IList>("Win");
            m_petnumber = m_v.GetValue<IList>("PetNumber");
            m_petlevel = m_v.GetValue<IList>("PetLevel");
            m_group = m_v.GetValue<IList>("Group");
            m_scene = m_v.GetValue<IList>("Scene");
            m_preparetime = m_v.GetValue<IList>("PrepareTime");
            m_gametime = m_v.GetValue<IList>("GameTime");
        }

        /// <summary>
        /// 判断胜利方式
        /// </summary>
        /// <param name="win"></param>
        /// <returns></returns>
        public static bool IsWin(int win) 
        {
            if (m_win == null) 
                return false;
            if (m_win.Contains(win))
                return true;
            return false;
        }
        /// <summary>
        /// 指定参战宠物数量
        /// </summary>
        /// <param name="petnumber"></param>
        /// <returns></returns>
        public static bool IsPetNumber(int petnumber)
        {
            if (m_petnumber == null)
                return false;
            if (m_petnumber.Contains(petnumber))
                return true;
            return false;
        }

        /// <summary>
        /// 宠物等级选择
        /// </summary>
        /// <param name="petlevel"></param>
        /// <returns></returns>
        public static bool IsPetLevel(string petlevel) 
        {
            if (m_petlevel == null)
                return false;
            if (m_petlevel.Contains(petlevel))
                return true;
            return false;
        }

        /// <summary>
        /// 分组数量判断
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public static bool IsGroup(int group) 
        {
            if (m_group == null) 
                return false;
            if (m_group.Contains(group))
                return true;
            return false;
        }

        /// <summary>
        /// 地图判断
        /// </summary>
        /// <param name="scene"></param>
        /// <returns></returns>
        public static bool IsScene(string scene) 
        {
            if (m_scene == null)
                return false;
            if (m_scene.Contains(scene))
                return true;
            return false;
        }

        /// <summary>
        /// 竞技场开始时间选择
        /// </summary>
        /// <param name="preparetime"></param>
        /// <returns></returns>
        public static bool IsPrepareTime(int preparetime) 
        {
            if (m_preparetime == null)
                return false;
            if (m_preparetime.Contains(preparetime))
                return true;
            return false;
        }

        /// <summary>
        /// 竞技场结束时间选择
        /// </summary>
        /// <param name="gametime"></param>
        /// <returns></returns>
        public static bool IsGameTime(int gametime)
        {
            if (m_gametime == null)
                return false;
            if (m_gametime.Contains(gametime))
                return true;
            return false;
        }
    }
}
