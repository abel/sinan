using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 活动基类
    /// </summary>
    public class PartBusiness : ExternalizableBase
    {
        protected Part m_part;
        protected DateTime m_start;
        protected DateTime m_end;

        /// <summary>
        /// 统计玩家胜利总次数
        /// Key:玩家ID, Value:胜利次数
        /// </summary>
        protected readonly ConcurrentDictionary<int, int> m_winers;

        /// <summary>
        /// 统计玩家连胜次数
        /// </summary>
        protected readonly ConcurrentDictionary<int, int> m_lWiners;

        /// <summary>
        /// 玩家进入记录
        /// </summary>
        protected readonly ConcurrentDictionary<int, int> m_playerInto;

        protected SceneBusiness m_mainScene;
        /// <summary>
        /// 活动地图
        /// </summary>
        protected readonly List<SceneBusiness> m_scenes;

        public Part Part
        {
            get { return m_part; }
        }

        /// <summary>
        /// 活动开始时间
        /// </summary>
        public DateTime StartTime
        {
            get { return m_start; }
        }

        /// <summary>
        /// 活动结束时间
        /// </summary>
        public DateTime EndTime
        {
            get { return m_end; }
        }

        protected bool m_over;
        /// <summary>
        /// 活动结束
        /// </summary>
        public bool Over
        {
            get { return m_over; }
        }

        public SceneBusiness MainScene
        {
            get { return m_mainScene; }
        }

        protected List<string> m_elements;
        /// <summary>
        /// 活动元素(跟具体的活动相关)
        /// 夺宝奇兵中为兑换光环的物品ID
        /// 守护战争中为守护凭证的ID
        /// </summary>
        public List<string> Elements
        {
            get { return m_elements; }
        }

        public PartBusiness(Part part)
        {
            m_part = part;

            m_winers = new ConcurrentDictionary<int, int>(Environment.ProcessorCount * 2, 512);
            m_lWiners = new ConcurrentDictionary<int, int>(Environment.ProcessorCount * 2, 512);
            m_playerInto = new ConcurrentDictionary<int, int>(Environment.ProcessorCount * 2, 512);

            m_scenes = new List<SceneBusiness>();
            for (int i = 0; i < part.Maps.Count; i++)
            {
                SceneBusiness scene;
                if (ScenesProxy.TryGetScene(part.Maps[i], out scene))
                {
                    if (i == 0)
                    {
                        m_mainScene = scene;
                    }
                    m_scenes.Add(scene);
                }
            }

            m_elements = new List<string>();
            ArrayList goods = part.Value.GetValueOrDefault<ArrayList>("Element");
            if (goods != null)
            {
                foreach (var v in goods)
                {
                    m_elements.Add(v.ToString());
                }
            }
        }

        /// <summary>
        /// 开始活动
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public virtual void Start(DateTime startTime, DateTime endTime)
        {
            m_start = startTime;
            m_end = endTime;
            PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { this });
        }

        /// <summary>
        /// 结束活动
        /// </summary>
        /// <returns></returns>
        public virtual void End()
        {
            m_over = true;
            PlayersProxy.CallAll(PartCommand.PartEndR, new object[] { this });
        }

        /// <summary>
        /// PK胜利排名
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        public int AddWiner(int pid)
        {
            m_lWiners.SetOrInc(pid, 1);
            return m_winers.SetOrInc(pid, 1);
        }

        public void AddLoser(int pid)
        {
            int t;
            m_lWiners.TryRemove(pid, out t);
        }

        public int PartCount()
        {
            int i = 0;
            foreach (var v in m_scenes)
            {
                i += v.Players.Count;
            }
            return i;
        }

        /// <summary>
        /// 获取排名..
        /// </summary>
        /// <param name="page"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public List<PlayerPKRank> GetRank(int page, int size)
        {
            List<Tuple<int, int>> players = new List<Tuple<int, int>>();
            foreach (var v in m_winers)
            {
                players.Add(new Tuple<int, int>(v.Value, v.Key));
            }
            players.Sort();
            players.Reverse();

            List<PlayerPKRank> results = new List<PlayerPKRank>();
            int start = page * size;
            int end = (page + 1) * size;
            for (int i = start; i < end && i < players.Count; i++)
            {
                Tuple<int, int> t = players[i];
                Player p = PlayersProxy.FindPlayerByID(t.Item2);
                results.Add(new PlayerPKRank(p, t.Item1));
            }
            return results;
        }

        /// <summary>
        /// 进入检查
        /// </summary>
        /// <param name="player"></param>
        public virtual bool IntoCheck(PlayerBusiness player)
        {
            if (m_over)
            {
                //活动已结束
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.PartBusiness1));
                
                return false;
            }
            if (m_start > DateTime.UtcNow || m_end <= DateTime.UtcNow)
            {
                //只有活动时间才能进入
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.PartBusiness2));
                return false;
            }
            if (player.Team != null)
            {
                //组队状态不能进入
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.PartBusiness3));
                return false;
            }
            if (m_part.MaxInto > 0)
            {
                int pid;
                if (m_playerInto.TryGetValue(player.PID, out pid))
                {
                    if (pid >= m_part.MaxInto)
                    {
                        //每次活动最多可进入{0}次
                        player.Call(ClientCommand.IntoSceneR, false, null, string.Format(TipManager.GetMessage(ClientReturn.PartBusiness4), m_part.MaxInto));
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 进入扣除
        /// </summary>
        /// <param name="player"></param>
        public virtual bool IntoPart(PlayerBusiness player)
        {
            bool pass = (m_part.Score == 0 && m_part.Coin == 0);

            if ((!pass) && m_part.Score > 0)
            {
                pass = player.AddScore(-m_part.Score, FinanceType.Part);
            }
            if ((!pass) && m_part.Coin > 0)
            {
                pass = player.AddCoin(-m_part.Coin, FinanceType.Part);
            }

            if (pass)
            {
                if (m_part.MaxInto > 0)
                {
                    m_playerInto.SetOrInc(player.PID, 1);
                }
            }
            else
            {
                player.Call(ClientCommand.IntoSceneR, false, null, m_part.CoinMsg);
            }
            return pass;
        }

        /// <summary>
        /// 玩家退出
        /// </summary>
        /// <param name="player"></param>
        public virtual void PlayerExit(PlayerBusiness player)
        {
            int pid;
            m_lWiners.TryRemove(player.PID, out pid);
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(m_part.ID);

            writer.WriteKey("Start");
            writer.WriteDateTime(m_start);

            writer.WriteKey("End");
            writer.WriteDateTime(m_end);
        }
    }
}
