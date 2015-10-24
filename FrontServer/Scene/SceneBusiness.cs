using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理单个场景的业务
    /// </summary>
    public abstract partial class SceneBusiness
    {
        /// <summary>
        /// 进入所需最小等级
        /// </summary>
        readonly protected int m_minLev;

        /// <summary>
        /// 最大等级限制
        /// </summary>
        readonly protected int m_maxLev;

        public int MinLev
        {
            get { return m_minLev; }
        }

        public int MaxLev
        {
            get { return m_maxLev; }
        }


        readonly protected static PlayerBusiness[] EmptyPlayerList = new PlayerBusiness[0];

        readonly protected string m_id;
        readonly protected string m_name;
        readonly protected string m_skin;
        readonly protected IntoLimit m_intoLimit;

        readonly int m_bornX;
        readonly int m_bornY;
        readonly protected Destination m_deadDest;
        readonly protected Destination m_propDest;

        /// <summary>
        /// 地图上的位置0:可行走.
        /// </summary>
        readonly protected List<Point> m_walk;

        /// <summary>
        /// 地图上可以行走的位置
        /// Key: 坐标;Value:(0:可行走,1:遮挡)
        /// </summary>
        readonly protected Dictionary<Point, int> m_map;

        /// <summary>
        /// 遇暗雷的时间间隔
        /// </summary>
        readonly protected int m_fightInterval;

        /// <summary>
        /// 所有在线的玩家.Key:玩家ID, Value:PlayerBusiness
        /// </summary>
        readonly protected ConcurrentDictionary<string, PlayerBusiness> m_players;

        /// <summary>
        /// 显示所有玩家()
        /// (城市/野外可见 副本/家园不可见)
        /// </summary>
        protected bool m_showAll;

        protected FightType m_fightType;

        /// <summary>
        /// 场景ID
        /// </summary>
        public string ID
        {
            get { return m_id; }
        }

        /// <summary>
        /// 场景名称
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }


        /// <summary>
        /// 场景中的所有玩家
        /// </summary>
        public ConcurrentDictionary<string, PlayerBusiness> Players
        {
            get { return m_players; }
        }

        /// <summary>
        /// 地图上可以行走并且无遮挡
        /// </summary>
        public List<Point> Walking
        {
            get { return m_walk; }
        }

        /// <summary>
        /// 死亡后的目的地
        /// </summary>
        public Destination DeadDestination
        {
            get { return m_deadDest; }
        }

        /// <summary>
        /// 使用回城道具(或副本超时)的目的地
        /// </summary>
        public Destination PropDestination
        {
            get { return m_propDest; }
        }

        /// <summary>
        /// 默认降生点X
        /// </summary>
        public int BornX
        {
            get { return m_bornX; }
        }

        /// <summary>
        /// 默认降生点Y
        /// </summary>
        public int BornY
        {
            get { return m_bornY; }
        }

        /// <summary>
        /// 同场景上的其它玩家是否可见
        /// (副本不可见)
        /// </summary>
        public bool ShowAll
        {
            get { return m_showAll; }
        }

        public abstract SceneType SceneType
        {
            get;
        }

        public SceneBusiness(GameConfig scene)
        {
            m_players = new ConcurrentDictionary<string, PlayerBusiness>();
            m_walk = new List<Point>(256);
            m_map = new Dictionary<Point, int>(256);

            m_id = scene.ID;
            m_name = scene.Name;
            Variant v = (scene.Value == null ? null : scene.Value.GetVariantOrDefault("Config"));
            if (v != null)
            {
                m_fightInterval = v.GetIntOrDefault("TimeLimit");
                if (v.GetBooleanOrDefault("canFight"))
                {
                    m_fightType = FightType.PK;
                }
                else
                {
                    m_fightType = FightType.NotPK;
                }

                m_minLev = v.GetIntOrDefault("MinLevel");
                m_maxLev = v.GetIntOrDefault("MaxLevel");

                IntoLimit limit = new IntoLimit(m_id, m_name, v);
                if (!limit.IsEmpty)
                {
                    m_intoLimit = limit;
                }

                //初始化降生点道具回城和死亡回城点
                Variant bornPoint = v.GetVariantOrDefault("BornPoint");
                if (bornPoint != null)
                {
                    m_bornX = bornPoint.GetIntOrDefault("X");
                    m_bornY = bornPoint.GetIntOrDefault("Y");
                }
                m_propDest = InitDestination(v.GetVariantOrDefault("BackPoint"));
                m_deadDest = InitDestination(v.GetVariantOrDefault("DeadPoint"));
                m_skin = v.GetStringOrDefault("ReUseSceneID");
            }
        }

        protected SceneBusiness(SceneBusiness scene)
        {
            m_players = new ConcurrentDictionary<string, PlayerBusiness>();
            m_walk = scene.m_walk;
            m_map = scene.m_map;
            m_id = scene.m_id;
            m_name = scene.m_name;
            m_skin = scene.m_skin;
            m_fightInterval = scene.m_fightInterval;
            m_fightType = scene.m_fightType;
            m_intoLimit = scene.m_intoLimit;
            m_bornX = scene.m_bornX;
            m_bornY = scene.m_bornY;
            m_propDest = scene.m_propDest;
            m_deadDest = scene.m_deadDest;

            m_showAll = scene.m_showAll;
            m_fightType = scene.m_fightType;
        }

        /// <summary>
        /// 获取目的地
        /// </summary>
        /// <param name="point"></param>
        static Destination InitDestination(Variant point)
        {
            if (point != null)
            {
                string sceneB = point.GetStringOrDefault("SceneB");
                if (!string.IsNullOrEmpty(sceneB))
                {
                    return new Destination(
                        sceneB,
                        point.GetIntOrDefault("X"),
                        point.GetIntOrDefault("Y"));
                }
            }
            return null;
        }


        /// <summary>
        /// 执行消息
        /// </summary>
        /// <param name="notification"></param>
        public void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null && note.Player != null)
            {
                ExecuteUserNote(note);
            }
        }


        protected virtual void ExecuteUserNote(UserNote note)
        {
            switch (note.Name)
            {
                case ClientCommand.WalkTo:
                    WalkTo(note);
                    return;
                case LoginCommand.PlayerLogin:
                    PlayerLogin(note);
                    return;
                case ClientCommand.IntoSceneSuccess:
                    IntoSceneSuccess(note);
                    return;
                case ClientCommand.ExitScene:
                    ExitScene(note.Player);
                    return;
                case ClientCommand.UserDisconnected:
                    Disconnected(note.Player);
                    return;
                case FightCommand.FightTaskApc:
                    FightTaskApc(note);
                    return;
                case FightCommand.FightPK:
                    FightPK(note);
                    return;
                case FightCommand.FightCC:
                    FightCC(note);
                    return;
                case FightCommand.FightReplyCC:
                    FightReplyCC(note);
                    return;
                default:
                    return;
            }
        }

        /// <summary>
        /// 玩家以登录方式进入场景
        /// </summary>
        /// <param name="note"></param>
        protected void PlayerLogin(UserNote note)
        {
            PlayerBusiness player = note.Player;
            Variant sceneinfo = CreateSceneInfo(player, true);
            player.Online = true;
            IntoSceneSuccess(player, sceneinfo);

            // 玩家登录成功,通知其它模块处理
            UserNote note2 = new UserNote(player, LoginCommand.PlayerLoginSuccess, null);
            Notifier.Instance.Publish(note2);
            if (ExperienceControl.ExpCoe != 1.0)
            {
                note.Call(PartCommand.PartStartR, new object[] { ExperienceControl.Instance });
            }
        }

        /// <summary>
        /// 成功进入场景
        /// </summary>
        /// <param name="note"></param>
        protected void IntoSceneSuccess(UserNote note)
        {
            PlayerBusiness player = note.Player;
            player.Point = 0;
            player.X = note.GetInt32(0);
            player.Y = note.GetInt32(1);
            Variant sceneinfo = CreateSceneInfo(player, false);
            IntoSceneSuccess(player, sceneinfo);
        }

        /// <summary>
        /// 创建场景副本
        /// </summary>
        /// <returns></returns>
        public abstract SceneBusiness CreateNew();

        /// <summary>
        /// 成功进入场景
        /// </summary>
        /// <param name="note"></param>
        protected virtual void IntoSceneSuccess(PlayerBusiness player, Variant sceneinfo)
        {
            // 发送用户列表并通知其它用户
            const int maxCount = 100;
            PlayerBusiness[] players;
            if (m_showAll)
            {
                players = m_players.Values.ToArray();
            }
            else
            {
                long showID = player.ShowID;
                players = m_players.Values.Where(x => { return x.ShowID == showID; }).ToArray();
            }

            if (players.Length <= maxCount)
            {
                player.CallBig(ClientCommand.IntoSceneR, true, sceneinfo, players);
            }
            else
            {
                PlayerBusiness[] first = new PlayerBusiness[maxCount];
                Array.Copy(players, first, maxCount);
                player.CallBig(ClientCommand.IntoSceneR, true, sceneinfo, first);
                //分割玩家列表
                for (int offset = maxCount; offset < players.Length; offset += maxCount)
                {
                    int count = Math.Min(players.Length - offset, maxCount);
                    PlayerBusiness[] other = new PlayerBusiness[count];
                    Array.Copy(players, offset, other, 0, count);
                    player.CallBig(ClientCommand.MorePlayer, this.ID, other);
                }
            }
            CallAllExcludeOne(player, ClientCommand.OtherIntoSceneR, new object[] { player });
            player.FightTime = DateTime.UtcNow.AddSeconds(15);
            player.Online = true;
            player.Save();

            if (this.HaveApc)
            {
                IList apcs = SceneApcProxy.GetSceneApc(this.ID);
                player.Call(ClientCommand.RefreshApcR, this.ID, apcs);
            }
            if (this.HaveBox)
            {
                IList boxs = BoxProxy.GetSceneBox(this.ID);
                player.Call(ClientCommand.RefreshBoxR, this.ID, boxs);
            }
        }

        /// <summary>
        /// 用户登录时创建场景信息
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected virtual Variant CreateSceneInfo(PlayerBusiness player, bool newlogin)
        {
            player.SceneID = ID;
            player.Scene = this;
            m_players[player.ID] = player;

            int x = player.X;
            int y = player.Y;
            if (((x | y) == 0) || (!m_map.ContainsKey(new Point(x, y))))
            {
                int index = NumberRandom.Next(m_walk.Count);
                Point p = m_walk[index];
                player.X = p.X;
                player.Y = p.Y;
            }

            Variant scene = new Variant(8);
            scene["ReUseSceneID"] = m_skin;
            scene["SceneID"] = ID;
            scene["SceneName"] = Name;
            scene["SceneType"] = (int)this.SceneType;
            scene["X"] = player.X;
            scene["Y"] = player.Y;
            return scene;
        }

        /// <summary>
        /// 行走
        /// </summary>
        /// <param name="note"></param>
        protected void WalkTo(UserNote note)
        {
            PlayerBusiness player = note.Player;
            //战斗状态或已加入队伍的队员不能自己行走(队长和离队的可以走)
            if (player.AState == ActionState.Fight || player.TeamJob == TeamJob.Member)
            {
                return;
            }
            double value = note.GetDouble(0);
            if (value == player.Point)
            {
                return;
            }
            //if (WalkEvent(player))
            //{
            //    CallAllExcludeOne(player, player.WalkTo(value));
            //}
            CallAllExcludeOne(player, player.WalkTo(value));
            WalkEvent(player);
        }
  

        /// <summary>
        /// 遇雷检查
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        protected virtual bool WalkEvent(PlayerBusiness player)
        {
            if (m_fightInterval > 0)
            {
                if ((DateTime.UtcNow - player.FightTime).TotalSeconds > m_fightInterval)
                {
                    HideApc hideApc = HideApcManager.Instance.Check(player.SceneID, player.X, player.Y);
                    if (hideApc != null)
                    {
                        player.FightTime = DateTime.UtcNow;
                        UserNote note2 = new UserNote(player, FightCommand.IntoBattle,
                            new object[] { FightType.HideAPC, hideApc, hideApc.ID });
                        Notifier.Instance.Publish(note2);
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 退出场景(玩家仍在线)
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool ExitScene(PlayerBusiness player)
        {
            if (player == null || player.AState == ActionState.Fight)
            {
                return false;
            }
            string playerID = player.ID;
            if (m_players.TryRemove(playerID, out player) && player != null)
            {
                //发送通知
                CallAll(player.ShowID, ClientCommand.ExitSceneR, new object[] { playerID });
                return true;
            }
            return false;
        }

        /// <summary>
        /// 玩家断线
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool Disconnected(PlayerBusiness player)
        {
            if (player == null)
            {
                return false;
            }
            string playerID = player.ID;
            if (m_players.TryRemove(playerID, out player) && player != null)
            {
                player.Online = false;
                player.Save();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="player"></param>
        /// <param name="inScene"></param>
        /// <param name="transmitType"></param>
        /// <returns></returns>
        public virtual bool TransferCheck(PlayerBusiness player, SceneBusiness inScene, TransmitType transmitType)
        {
            if (inScene == null) return false;
            return inScene.IntoCheck(player);
        }

        /// <summary>
        /// 进入检查
        /// </summary>
        /// <param name="note"></param>
        /// <returns></returns>
        protected virtual bool IntoCheck(PlayerBusiness player)
        {
            if (m_intoLimit == null)
            {
                return true;
            }
            string msg;
            if (m_intoLimit.IntoCheck(player, out msg))
            {
                return true;
            }
            player.Call(ClientCommand.IntoSceneR, false, null, msg);
            return false;
        }

        /// <summary>
        /// 检查玩家进入等级
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CheckLevel(PlayerBusiness player)
        {
            if (m_minLev > player.Level)
            {
                return false;
            }
            if (m_maxLev > 0 && m_maxLev < player.Level)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 获取玩家信息
        /// </summary>
        /// <param name="playerID"></param>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool TryGetPlayer(string playerID, out PlayerBusiness player)
        {
            return m_players.TryGetValue(playerID, out player);
        }

        /// <summary>
        /// 回城
        /// </summary>
        /// <param name="player"></param>
        /// <param name="tType">回城方式</param>
        public bool TownGate(PlayerBusiness player, TransmitType tType)
        {
            if (player.Scene != this)
            {
                return false;
            }
            Destination desc = null;
            if (tType == TransmitType.UseProp)
            {
                //不能使用道具回城
                desc = PropDestination;
                if (desc == null)
                {
                    return false;
                }
            }
            else if (tType == TransmitType.Dead)
            {
                desc = DeadDestination;
                if (desc != null)
                {
                    player.SetActionState(ActionState.Standing);
                }
            }
            else if (tType == TransmitType.OverTime)
            {
                string sceneid = player.Ectype.Value.GetStringOrDefault("SceneID");
                if (!string.IsNullOrEmpty(sceneid))
                {
                    int x = player.Ectype.Value.GetIntOrDefault("X");
                    int y = player.Ectype.Value.GetIntOrDefault("Y");
                    desc = new Destination(sceneid, x, y);
                }
                if (desc != null)
                {
                    player.SetActionState(ActionState.Standing);
                }
            }

            if (desc == null)
            {
                return false;
            }

            SceneBusiness inScene;
            ScenesProxy.TryGetScene(desc.SceneID, out inScene);

            //场景转移检查,除了死亡和超时.其它情况进行场景检查
            if (!(tType == TransmitType.Dead || tType == TransmitType.OverTime))
            {
                if ((!TransferCheck(player, inScene, tType)))
                {
                    return false;
                }
            }

            object[] point = new object[] { desc.X, desc.Y };
            if (ExitScene(player))
            {
                //执行进入
                inScene.Execute(new UserNote(player, ClientCommand.IntoSceneSuccess, point));
            }
            if (player.TeamJob == TeamJob.Captain)
            {
                PlayerTeam team = player.Team;
                if (team != null)
                {
                    for (int i = 1; i < team.Members.Length; i++)
                    {
                        PlayerBusiness member = team.Members[i];
                        if (member != null && member.TeamJob == TeamJob.Member)
                        {
                            if (ExitScene(member))
                            {   //执行进入
                                inScene.Execute(new UserNote(member, ClientCommand.IntoSceneSuccess, point));
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 检查是否可以行走
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public bool CheckWalk(int x, int y)
        {
            return m_map.ContainsKey(new Point(x, y));
        }

        /// <summary>
        /// 设置行走信息
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public void InitWalk(Variant v)
        {
            foreach (var item in v)
            {
                string[] p = item.Key.Split('_');
                if (p.Length == 2)
                {
                    int x, y;
                    if (int.TryParse(p[0], out x) && int.TryParse(p[1], out y))
                    {
                        //0:可行走,1:遮挡
                        int vi = v.GetIntOrDefault(item.Key);
                        if (vi == 0)
                        {
                            m_walk.Add(new Point(x, y));
                        }
                        m_map.Add(new Point(x, y), vi);
                    }
                }
            }
        }

    }
}
