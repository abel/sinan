using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 组队副本
    /// </summary>
    public class TeamInstanceBusiness
    {
        static readonly PlayerBusiness[] Empty = new PlayerBusiness[0];

        static int __staticIncrement;
        private static int GetTimestampUtcNow()
        {
            return (int)Math.Floor((DateTime.UtcNow - BsonConstants.UnixEpoch).TotalSeconds);
        }

        /// <summary>
        /// 唯一ID
        /// </summary>
        public long ID
        {
            get { return m_id; }
        }

        public ActionState Astate
        {
            get;
            set;
        }

        protected int level = 0;
        protected readonly long m_id;
        protected readonly string m_difficulty;
        protected readonly GameConfig m_gc;
        protected readonly List<Variant> m_movie;
        protected readonly List<EctypeApc> m_currentApcs;
        protected readonly IntoLimit m_intoLimit;
        protected int oldx, oldy;
        protected SceneBusiness m_oldScene;
        protected SceneBusiness m_currentScene;

        protected PlayerBusiness[] m_members;
        protected PlayerTeam m_team;

        /// <summary>
        /// 难度
        /// </summary>
        public string Difficulty
        {
            get { return m_difficulty; }
        }

        /// <summary>
        /// 当前APCS
        /// </summary>
        public List<EctypeApc> CurrentApcs
        {
            get { return m_currentApcs; }
        }

        /// <summary>
        /// 过期时间
        /// </summary>
        public DateTime? OverTime
        {
            get;
            set;
        }

        /// <summary>
        /// 当前所在场景
        /// </summary>
        public SceneBusiness Scene
        {
            get { return m_currentScene; }
        }

        public TeamInstanceBusiness(GameConfig gc, string difficulty)
        {
            m_members = Empty;
            int increment = Interlocked.Increment(ref __staticIncrement) & 0x00ffffff;
            int t = GetTimestampUtcNow();
            m_id = (((long)t) << 32) | ((UInt32)increment);
            m_gc = gc;
            m_difficulty = difficulty;
            m_movie = new List<Variant>();
            m_currentApcs = new List<EctypeApc>();
            foreach (var item in gc.Value.GetValueOrDefault<IList>("Movie"))
            {
                if (item is Variant)
                {
                    m_movie.Add((Variant)item);
                }
            }
            Variant v = gc.Value.GetVariantOrDefault("Limit");
            if (v != null)
            {
                m_intoLimit = new IntoLimit(m_gc.ID, m_gc.Name, v);
                int maxStay = v.GetIntOrDefault("MaxStay");
                if (maxStay > 0)
                {
                    OverTime = DateTime.UtcNow.AddSeconds(maxStay);
                }
            }
        }

        public bool TryInto(PlayerBusiness player)
        {
            PlayerTeam team = player.Team;
            //设置玩家列表.
            string msg;
            if (!FillPlayers(team, player, out msg))
            {
                player.Call(InstanceCommand.NewInstanceR, new object[] { false, msg, string.Empty });
                return false;
            }

            player.TeamInstance = this;
            // 检查副本进入限制
            if (m_intoLimit != null)
            {
                PlayerBusiness member;
                if ((!m_intoLimit.IntoCheck(m_members, out msg, out member))
                    || (!m_intoLimit.IntoDeduct(m_members, out msg, out member)))
                {
                    player.TeamInstance = null;
                    this.CallAll(InstanceCommand.NewInstanceR, new object[] { false, msg, member == null ? string.Empty : member.Name });
                    m_members = Empty;
                    return false;
                }
            }
            m_team = team;
            TeamInstanceProxy.TryAddInstance(this);

            //设置进入次数
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member == null) continue;
                member.TeamInstance = this;
                member.WriteDaily(PlayerBusiness.DailyMap, m_gc.ID);

                //进入秘境通知..
                member.AddAcivity(ActivityType.FuBenCount, 1);

                // 记录副本进入日志
                PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.EctypeIn);
                log.itemtype = m_difficulty;
                log.itemid = m_gc.ID;
                log.remark = m_gc.Name;
                member.WriteLog(log);
            }

            oldx = player.X;
            oldy = player.Y;
            m_oldScene = player.Scene;
            m_currentScene = m_oldScene;
            Astate = ActionState.Standing;
            return true;
        }

        protected virtual bool FillPlayers(PlayerTeam team, PlayerBusiness player, out string msg)
        {
            if (team == null || player.TeamJob != TeamJob.Captain)
            {
                msg = TipManager.GetMessage(ClientReturn.EctypeLimitTeamMsg1);
                return false;
            }
            for (int i = 0; i < team.Members.Length; i++)
            {
                PlayerBusiness member = team.Members[i];
                if (member != null)
                {
                    if (member.TeamJob == TeamJob.Away)
                    {
                        msg = TipManager.GetMessage(ClientReturn.EctypeLimitTeamMsg);
                        return false;
                    }
                }
            }
            m_members = team.Members;
            msg = null;
            return true;
        }

        /// <summary>
        /// 进入下一场景
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool NextDrame()
        {
            if (level >= m_movie.Count)
            {
                return false;
            }
            m_currentApcs.Clear();

            Variant movieItem = m_movie[level++];
            string sceneID = movieItem.GetStringOrDefault("SceneID");
            SceneBusiness inScene;
            if (!ScenesProxy.TryGetScene(sceneID, out inScene) || inScene.SceneType != SceneType.Instance)
            {
                return NextDrame();
            }

            InitDrame(inScene, movieItem.GetValueOrDefault<IList>("Drame"));
            int x = movieItem.GetIntOrDefault("X");
            int y = movieItem.GetIntOrDefault("Y");

            //切换场景
            TransScene(inScene, new object[] { x, y }, false);
            m_currentScene = inScene;

            //发送怪物信息
            var apcs = m_currentApcs.FindAll(k => k.State == 0);
            CallAll(InstanceCommand.NewInstanceR, new object[] { true, apcs, m_currentScene.ID });
            return true;
        }

        /// <summary>
        /// 初始化战斗怪
        /// </summary>
        /// <param name="inScene"></param>
        /// <param name="drame"></param>
        private void InitDrame(SceneBusiness inScene, IList drame)
        {
            int total = 0;
            foreach (object item in drame)
            {
                Variant config = item as Variant;
                if (config != null)
                {
                    Variant apcs = config.GetVariantOrDefault("VisibleAPC");
                    if (apcs != null)
                    {
                        int count = config.GetIntOrDefault("Count", 1);
                        string apcid = apcs.GetStringOrDefault(Difficulty);
                        if (!string.IsNullOrEmpty(apcid))
                        {
                            Rectangle range = RangeHelper.NewRectangle(config.GetVariantOrDefault("Range"), true);
                            for (int i = 0; i < count; i++)
                            {
                                EctypeApc ecApc = new EctypeApc(apcid);
                                if (ecApc.Apc != null)
                                {
                                    ecApc.ID = (total++);
                                    ecApc.Say = config.GetIntOrDefault("Say");
                                    ecApc.Batch = config.GetIntOrDefault("Batch");
                                    Point point = inScene.RandomBronPoint(range);
                                    if (point.X == 0)
                                    {
                                        Console.WriteLine("");
                                    }
                                    ecApc.X = point.X;
                                    ecApc.Y = point.Y;
                                    m_currentApcs.Add(ecApc);
                                }
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// 检查战斗批次
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool CheckBatch(int index)
        {
            if (index >= m_currentApcs.Count)
            {
                return false;
            }
            EctypeApc apc = m_currentApcs[index];
            if (apc == null || apc.State != 0)
            {
                return false;
            }
            int batch = apc.Batch;
            if (batch > 1)
            {
                if (m_currentApcs.Exists(x => (x.State == 0 && x.Batch > 0 && x.Batch < batch)))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// 检查怪是否清除
        /// </summary>
        /// <returns></returns>
        public bool ClearApc()
        {
            if (m_currentApcs.Exists(x => x.State == 0))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 检查场景是否结束
        /// </summary>
        public bool TryOver(bool checkApc)
        {
            if (Astate == ActionState.Fight)
            {
                return false;
            }
            if (OverTime.HasValue && OverTime < DateTime.UtcNow)
            {
                //超时结束副本
                Over();
                return true;
            }
            if (checkApc && ClearApc())
            {
                //转场景
                if (!NextDrame())
                {
                    //结束副本
                    Over();
                    return true;
                }
            }
            return false;
        }

        public void Over()
        {
            TeamInstanceProxy.TryRemove(m_id);
            TransScene(m_oldScene, new object[] { oldx, oldy }, true);
            m_team = null;
        }

        /// <summary>
        /// 切换场景
        /// </summary>
        /// <param name="inScene">要进入的新场景</param>
        /// <param name="point">位置</param>
        /// <param name="exit">是否退出副本</param>
        void TransScene(SceneBusiness inScene, object[] point, bool exit)
        {
            lock (m_members)
            {
                for (int i = 0; i < m_members.Length; i++)
                {
                    PlayerBusiness member = m_members[i];
                    if (member != null)
                    {
                        if (m_currentScene.ExitScene(member) || exit)
                        {
                            UserNote note = new UserNote(member, ClientCommand.IntoSceneSuccess, point);
                            inScene.Execute(note);
                            point[0] = member.X;
                            point[1] = member.Y;
                            if (exit)
                            {
                                member.TeamInstance = null;
                            }
                        }
                    }
                }
            }
            if (exit)
            {
                m_members = Empty;
            }
        }

        public void CallAll(string command, IList objs)
        {
            var buffer = AmfCodec.Encode(command, objs);
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member != null)
                {
                    member.Call(buffer);
                }
            }
        }

        /// <summary>
        /// 重新进入
        /// </summary>
        /// <param name="member"></param>
        internal void ReInto(PlayerBusiness member)
        {
            if (m_team != null && m_team.TryAddMember(member))
            {
                var members = m_team.AllPlayerDetail;
                member.Call(TeamCommand.IntoTeamR, true, new object[] { m_team, members });
                member.CallAllExcludeOne(member, TeamCommand.NewMemberR, m_team.TeamID, new PlayerDetail(member));

                var apcs = m_currentApcs.FindAll(k => k.State <= 1);
                member.Call(InstanceCommand.NewInstanceR, new object[] { true, apcs, string.Empty });

                // 记录副本进入日志
                PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.EctypeIn);
                log.itemtype = m_gc.SubType; //副本类型
                log.itemid = m_gc.ID;        //副本ID
                log.remark = m_gc.Name;      //副本名称
                
                member.WriteLog(log);
            }
        }

        /// <summary>
        /// 玩家退出
        /// </summary>
        /// <param name="player"></param>
        /// <returns>剩余玩家数</returns>
        public int Exit(PlayerBusiness player, bool online = false)
        {
            int count = 0;
            for (int i = 0; i < m_members.Length; i++)
            {
                PlayerBusiness member = m_members[i];
                if (member != null)
                {
                    if (member != player)
                    {
                        count++;
                    }
                }
            }
            if (count == 0)
            {
                TeamInstanceProxy.TryRemove(m_id);
                m_members = Empty;
            }

            // 记录副本退出日志
            PlayerLog log = new PlayerLog(ServerLogger.zoneid, Actiontype.EctypeOut);
            log.itemtype = m_difficulty;
            log.itemid = m_gc.ID;
            log.remark = m_gc.Name;
            player.WriteLog(log);

            if (online)
            {
                //转回主场景..
                if (m_currentScene.ExitScene(player))
                {
                    player.TeamInstance = null;
                    UserNote note = new UserNote(player, ClientCommand.IntoSceneSuccess, new object[] { oldx, oldy });
                    m_oldScene.Execute(note);
                }
            }
            return count;
        }

    }
}
