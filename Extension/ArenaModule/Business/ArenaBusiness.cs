
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.ArenaModule.Business
{    
    public class ArenaBusiness
    {
        /// <summary>
        /// 竞技场列表
        /// </summary>
        static ConcurrentDictionary<string, ArenaBase> m_doc = new ConcurrentDictionary<string, ArenaBase>();

        /// <summary>
        /// 竞技场列表
        /// </summary>
        public static ConcurrentDictionary<string, ArenaBase> ArenaList 
        {
            get { return m_doc; }
        }

        /// <summary>
        /// 创建竞技场
        /// </summary>
        /// <param name="playerid">角色</param>        
        /// <param name="model"></param>        
        /// <returns>0表示创建成功,1该竞技场已经存在,2你已经创建有竞技场</returns>
        public int CreateArenaBase(string playerid, ArenaBase model)
        {
            if (IsAreneBase(playerid))
                return 2;

            if (!m_doc.TryAdd(model.SoleID, model))
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// 判断是否已经创建竞技场
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <returns></returns>
        private bool IsAreneBase(string playerid)
        {
            foreach (ArenaBase ab in m_doc.Values)
            {
                if (ab.PlayerID == playerid)
                    return true;
            }
            return false;
        }
        
    }
}
