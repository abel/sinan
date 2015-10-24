using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Schedule;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    public sealed class PlayerReference : WeakReference
    {
        public PlayerReference()
            : base(null, false)
        { }
        /// <summary>
        /// 引用的对象状态
        /// 1.准备初始化
        /// 2.初始化完成
        /// 3.初始化完成,没有对象
        /// </summary>
        public volatile int state = 0;
    }

    public sealed class PlayersProxy : SchedulerBase
    {
        /// <summary>
        /// 所有在线的玩家(Key:玩家ID,Value: PlayerBusiness) 
        /// </summary>
        readonly static ConcurrentDictionary<int, PlayerBusiness> m_online =
            new ConcurrentDictionary<int, PlayerBusiness>(4 * Environment.ProcessorCount, 2000);

        /// <summary>
        /// 弱引用玩家,用于缓存
        /// </summary>
        readonly static Dictionary<int, PlayerReference> m_players = new Dictionary<int, PlayerReference>(4000);


        static public PlayerBusiness[] Players
        {
            get { return m_online.Values.ToArray(); }
        }

        /// <summary>
        /// 当前在线人数
        /// </summary>
        static public int OnlineCount
        {
            get { return m_online.Count; }
        }

        /// <summary>
        /// 缓存人数,弱引用,不准确
        /// </summary>
        static public int PlayerCount
        {
            get { return m_players.Count; }
        }

        PlayersProxy()
            : base(120 * 1000, 120 * 1000)
        {
            Sinan.Common.GCNotification.GCDone += this.GCNotification;
        }

        int gc_count;
        int lastClear;
        List<int> ids = new List<int>(256);
        protected override void Exec()
        {
            if (gc_count - lastClear < 64)
            {
                return;
            }
            lastClear = gc_count;
            int count = m_players.Count;
            //缓存大于128,并且大于2倍在线人数时回收
            if (count > 128 && count > (m_online.Count << 1))
            {
                lock (m_players)
                {
                    foreach (var item in m_players)
                    {
                        PlayerReference playerRef = item.Value;
                        if ((!item.Value.IsAlive) && playerRef.state > 1)
                        {
                            ids.Add(item.Key);
                        }
                    }
                    if (ids.Count > 0)
                    {
                        for (int i = 0; i < ids.Count; i++)
                        {
                            m_players.Remove(ids[i]);
                        }
                        ids.Clear();
                    }
                }
            }
        }

        void GCNotification(int i)
        {
            gc_count += ((i << 6) + 1);
            //LogWrapper.Warn(string.Format("GC{0}:{1}:{2}MB", i, GC.CollectionCount(i), (GC.GetTotalMemory(false) >> 20)));
        }

        /// <summary>
        /// 通过玩家ID获取玩家
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        static public bool TryGetPlayerByID(string playerID, out PlayerBusiness player)
        {
            int pid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(playerID, out pid))
            {
                return m_online.TryGetValue(pid, out player);
            }
            player = null;
            return false;
        }

        /// <summary>
        /// 通过玩家ID获取玩家
        /// </summary>
        /// <param name="id"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        static public bool TryGetPlayerByID(int pid, out PlayerBusiness player)
        {
            return m_online.TryGetValue(pid, out player);
        }

        /// <summary>
        /// 添加在线玩家
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        static public bool TryAddPlayer(PlayerBusiness player)
        {
            return m_online.TryAdd(player.PID, player);
        }

        /// <summary>
        /// 移出玩家
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        static public bool TryRemovePlayer(int pid, out PlayerBusiness player)
        {
            return m_online.TryRemove(pid, out player);
        }

        /// <summary>
        /// 查找用户.
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        static public PlayerBusiness FindPlayerByID(int pid)
        {
            if (pid <= 0) return null;

            PlayerReference playerRef;
            lock (m_players)
            {
                if (!m_players.TryGetValue(pid, out playerRef))
                {
                    playerRef = new PlayerReference();
                    m_players.Add(pid, playerRef);
                }
                playerRef.state = 1;
            }

            lock (playerRef)
            {
                try
                {
                    PlayerBusiness player = playerRef.Target as PlayerBusiness;
                    if (player == null)
                    {
                        player = PlayerAccess.Instance.GetPlayer<PlayerBusiness>(pid);
                        if (player != null)
                        {
                            player.InitPlayer(null);
                            playerRef.Target = player;
                        }
                    }
                    return player;
                }
                finally
                {
                    playerRef.state = 2;
                }
            }
        }

        static public PlayerBusiness FindPlayerByID(string playerID)
        {
            int pid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(playerID, out pid))
            {
                return FindPlayerByID(pid);
            }
            return null;
        }

        /// <summary>
        /// 根据名字查找用户
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        static public PlayerBusiness FindPlayerByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }
            int pid = PlayerAccess.Instance.GetPlayerIdByName(name);
            return FindPlayerByID(pid);
        }

        /// <summary>
        /// 调用所有玩家的方法
        /// </summary>
        /// <param name="commond"></param>
        /// <param name="pars"></param>
        static public void CallAll(string commond, IList pars)
        {
            CallAll(AmfCodec.Encode(commond, pars));
        }

        /// <summary>
        /// 调用所有玩家的方法
        /// </summary>
        /// <param name="buffer"></param>
        static public void CallAll(Sinan.Collections.BytesSegment buffer)
        {
            foreach (var player in m_online.Values)
            {
                if (player.Online)
                {
                    player.Call(buffer);
                }
            }
        }

        /// <summary>
        /// 调用某个家族所有在线玩家的方法
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="commond"></param>
        /// <param name="pars"></param>
        static public void CallFamily(string familyName, string commond, IList pars)
        {
            CallFamily(familyName, AmfCodec.Encode(commond, pars));
        }

        /// <summary>
        /// 调用某个家族所有在线玩家的方法
        /// </summary>
        /// <param name="familyName"></param>
        /// <param name="buffer"></param>
        static public void CallFamily(string familyName, Sinan.Collections.BytesSegment buffer)
        {
            foreach (var player in m_online.Values)
            {
                if (player.Online && player.FamilyName == familyName)
                {
                    player.Call(buffer);
                }
            }
        }

        /// <summary>
        /// 删除玩家
        /// </summary>
        /// <param name="pid">角色ID</param>
        /// <param name="freeName">是否释放角色名</param>
        /// <returns></returns>
        static public bool DeletePlayer(int pid, bool freeName)
        {
            //趣游不能删除角色.
            if (ConfigLoader.Config.Platform == Sinan.Entity.Platform.Gamewave)
                return false;

            PlayerBusiness pb = PlayersProxy.FindPlayerByID(pid);
            if (pb != null)
            {
                Player del = PlayerAccess.Instance.DeletePlayer(pid, freeName);
                if (del != null)
                {
                    UserNote note = new UserNote(pb, UserPlayerCommand.DeletePlayerSuccess, new object[] { del });
                    Sinan.Observer.Notifier.Instance.Publish(note);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 检查玩家是否在线
        /// </summary>
        /// <param name="pid"></param>
        /// <returns></returns>
        static public bool Online(int pid)
        {
            return m_online.ContainsKey(pid);
        }
    }
}
