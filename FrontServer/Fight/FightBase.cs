using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MongoDB.Bson;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗基类
    /// </summary>
    public abstract partial class FightBase
    {
        protected static readonly FightObject[] EmptyTeam = new FightObject[0];

        /// <summary>
        /// 最大回合数
        /// </summary>
        protected int m_maxRound = 200;

        /// <summary>
        /// 等待用户操作的时间40秒(毫秒).
        /// </summary>
        public const int WaitClintAction = 40000;

        /// <summary>
        /// 等待单次攻击播放的最长时间(毫秒)
        /// </summary>
        public const int PlayMaxTime = 10000;

        /// <summary>
        /// 进入战斗.等待用户加载资源的时间(毫秒)
        /// </summary>
        public const int LoadTime = 100000;

        private readonly string m_id;
        protected bool m_changeLife = true;
        protected Timer m_timer;

        /// <summary>
        /// A队/1队(必定为玩家队伍)
        /// </summary>
        protected FightObject[] m_teamA;

        /// <summary>
        /// B队/2队(APC队伍或被挑战的队伍)
        /// </summary>
        protected FightObject[] m_teamB;

        protected readonly List<FightAction> m_actions;
        protected readonly SceneBusiness m_scene;
        protected readonly int m_totalFighter;

        /// <summary>
        /// 播放完成时间
        /// </summary>
        protected DateTime m_showOver;

        /// <summary>
        /// 战斗状态.
        /// 0:等待客户端加载资源
        /// 1:可操作战斗指令
        /// 2:客户操作完成.
        /// 3:服务器开始计算
        /// 4:计算结束.等待客户端播放
        /// --5:客户播放完成..
        /// 10: 战斗结束.
        /// 11: 计算本次战斗的结果
        /// 12: 发送结果通知
        /// </summary>
        protected int m_fightState;

        /// <summary>
        /// 战斗回合数.
        /// </summary>
        protected int m_fightCount = 0;

        /// <summary>
        /// 客户播放完成的个数
        /// </summary>
        protected int m_playOverCount = 0;

        /// <summary>
        /// 所有参与战斗的玩家
        /// </summary>
        readonly protected PlayerBusiness[] m_players;

        /// <summary>
        /// 每回合开始时可以发招的战斗者
        /// </summary>
        readonly protected List<FightObject> m_fighter;

        /// <summary>
        /// 保护别人者
        /// </summary>
        readonly protected List<FightObject> m_protecter;

        /// <summary>
        /// 战斗标识ID
        /// </summary>
        public string FightID
        {
            get { return m_id; }
        }

        public FightBase(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players)
        {
            m_id = ObjectId.GenerateNewId().ToString();//Guid.NewGuid().ToString("N");
            m_players = players.ToArray();
            m_teamA = teamA;
            m_teamB = teamB;
            m_scene = players[0].Scene;
            foreach (var v in m_teamA)
            {
                v.Team = m_teamA;
            }
            foreach (var v in m_teamB)
            {
                v.Team = m_teamB;
            }
            m_protecter = new List<FightObject>();
            m_totalFighter = m_teamA.Length + m_teamB.Length;

            m_fighter = new List<FightObject>(m_totalFighter);
            m_actions = new List<FightAction>(m_totalFighter);
        }

        /// <summary>
        /// 执行战斗命令.
        /// </summary>
        /// <param name="note"></param>
        public void EnqueueUserNotification(UserNote note)
        {
            switch (note.Name)
            {
                case FightCommand.FightAction:
                    PlayerFightAction(note);
                    break;
                case FightCommand.FightPlayerOver:
                    FightPlayerOver(note);
                    break;
                case FightCommand.AutoFight:
                    AutoFight(note);
                    break;
                case FightCommand.ReadyFight:
                    ReadyFight(note);
                    break;
                case ClientCommand.UserDisconnected:
                    PlayerExit(note.Player);
                    break;
                default:
                    break;
            }
        }

        int m_ready = 0;
        private void ReadyFight(UserNote note)
        {
            if (Interlocked.Exchange(ref m_ready, 1) == 0)
            {
                m_timer.Change(Timeout.Infinite, Timeout.Infinite);
                CheckState(null);
            }
        }

        private void AutoFight(UserNote note)
        {
            PlayerBusiness player = note.Player;
            bool auto = note.GetBoolean(0);
            int count = 0;
            if (auto)
            {
                count = player.StartAutoFight();
            }
            else
            {
                player.EndAutoFight();
            }

            FightObject[] team;
            FightObject f = m_teamA.FirstOrDefault(x => x.ID == player.ID);
            if (f != null)
            {
                team = m_teamA;
            }
            else
            {
                f = m_teamB.FirstOrDefault(x => x.ID == player.ID);
                team = m_teamB;
            }

            if (f != null)
            {
                var buffer = AmfCodec.Encode(FightCommand.AutoFightR, new object[] { player.ID, count });
                foreach (var p in team)
                {
                    if (p.FType == FighterType.Player)
                    {
                        p.Player.Call(buffer);
                    }
                }
            }
        }

        /// <summary>
        /// 玩家掉线
        /// </summary>
        /// <param name="note"></param>
        protected virtual bool PlayerExit(PlayerBusiness player)
        {
            bool find = false;
            for (int i = 0; i < m_players.Length; i++)
            {
                if (m_players[i] == player)
                {
                    player.Fight = null;
                    find = true;
                    m_players[i] = null;
                    break;
                }
            }
            if (!find)
            {
                return false;
            }

            find = false;
            foreach (FightPlayer x in m_teamA)
            {
                if (x.Player == player)
                {
                    find = true;
                    x.ExitFight(m_changeLife);
                }
            }
            if (!find)
            {
                foreach (FightObject f in m_teamB)
                {
                    FightPlayer x = f as FightPlayer;
                    if (x != null && x.Player == player)
                    {
                        x.ExitFight(m_changeLife);
                    }
                }
            }

            //两队都有存活且在线的对象,则继续战斗
            if (m_teamA.Any(x => x.IsLive) && m_teamB.Any(x => x.IsLive))
            {
                return true;
            }
            GameOver();
            return true;
        }

        /// <summary>
        /// 玩家播放结束
        /// </summary>
        /// <param name="note"></param>
        protected void FightPlayerOver(UserNote note)
        {
            //if (m_showOver > DateTime.UtcNow)
            //{
            //    return;
            //}
            int x = note.GetInt32(0);
            if (x != m_fightCount) //检验结束参数
            {
                return;
            }
            if (m_fightState == 4 || m_fightState == 10)
            {
                //大于最大回合数
                if (m_fightCount >= m_maxRound)
                {
                    GameOver();
                    return;
                }

                // 收到第1个玩家播放结束的通知,等待500毫秒后开始新的回合
                if (Interlocked.Increment(ref m_playOverCount) == 1)
                {
                    if (m_players.Length == 1)
                    {
                        CheckState(null);
                    }
                    else
                    {
                        ChangeTimer(500);
                    }
                }
            }
        }

        /// <summary>
        /// 处理战斗中玩家发出的命令
        /// </summary>
        /// <param name="note"></param>
        protected virtual void PlayerFightAction(UserNote note)
        {
            IList actions = note[0] as IList;
            if (actions == null || m_timer == null || m_fightState != 1)
            {
                return;
            }

            foreach (object action in actions)
            {
                Variant v = action as Variant;
                if (v != null)
                {
                    string id = v.GetStringOrDefault("handler");
                    FightObject f = m_fighter.Find(x => x.ID == id);
                    if (f != null)
                    {
                        f.Action = new FightAction(v, m_fightCount);
                        if (AllPlayerReady())
                        {
                            if (ChangeState(1, 2))
                            {
                                ChangeTimer(Timeout.Infinite);
                                Fighting();
                            }
                        }
                    }
                }
            }
        }

        //所有玩家都发出了操作指令.
        protected virtual bool AllPlayerReady()
        {
            return m_fighter.All(x => x is FightApc || (x.Action != null && x.Action.FightCount == m_fightCount));
        }

        /// <summary>
        /// 开战战斗
        /// </summary>
        protected virtual void OnStart()
        { }

        /// <summary>
        /// 开始战斗计时.
        /// </summary>
        public void Start()
        {
            OnStart();
            if (m_timer == null)
            {
                m_timer = new Timer(CheckState);
                m_timer.Change(LoadTime, Timeout.Infinite);
            }
        }

        protected void CheckState(Object stateInfo)
        {
            try
            {
                //任何一方全部死亡
                if (m_fightState == 10 || CheckTeamOver())
                {
                    GameOver();
                    return;
                }
                //大于最大回合数
                if (m_fightCount >= m_maxRound)
                {
                    GameOver();
                    return;
                }
                m_ready = 1;
                if (ChangeState(0, 1) || ChangeState(4, 1))
                {
                    Interlocked.Increment(ref m_fightCount);
                    // 刷新活动的战斗者
                    if (!RefreshLiver())
                    {
                        GameOver();
                        return;
                    }
                    // 通知客户端开始新一回合的战斗
                    SendToClinet(FightCommand.FightPreparedR, m_fightCount, WaitClintAction / 1000);
                    ChangeTimer(WaitClintAction);
                    return;
                }
                if (m_fightState == 1 || m_fightState == 2)
                {
                    Fighting();
                }
            }
            catch (System.Exception ex)
            {
                try
                {
                    LogWrapper.Error(ex);
                }
                catch { }
            }
        }

        /// <summary>
        /// 刷新活动的战斗者
        /// </summary>
        private bool RefreshLiver()
        {
            lock (m_fighter)
            {
                m_fighter.Clear();
                foreach (var x in m_teamA)
                {
                    if (x.IsLive) m_fighter.Add(x);
                    if (x.Action != null) x.Action.FightCount = 0;
                }
                int count = m_fighter.Count;
                if (count == 0) return false;
                foreach (var x in m_teamB)
                {
                    if (x.IsLive) m_fighter.Add(x);
                    if (x.Action != null) x.Action.FightCount = 0;
                }
                return m_fighter.Count > count;
            }
        }

        /// <summary>
        /// 检查是否有一方全部死亡(或成功逃跑)
        /// </summary>
        /// <returns></returns>
        protected bool CheckTeamOver()
        {
            return m_taoPao || m_teamB.All(x => (x.Over)) || m_teamA.All(x => (x.Over));
        }

        private void Fighting()
        {
            try
            {
                if (ChangeState(1, 3) || ChangeState(2, 3))
                {
                    // 初始化战斗数据
                    InitAttack();
                    // 开始计算战斗结果...
                    bool over = StartAttack();
                    // 等待用户播放完成.如果用户无通知.则等待指定时间后就再次开始新的回合
                    Interlocked.Exchange(ref m_playOverCount, 0);
                    if (over)
                    {
                        ChangeState(3, 10);
                        ChangeTimer(PlayMaxTime * m_totalFighter);
                    }
                    else
                    {
                        if (ChangeState(3, 4))
                        {
                            ChangeTimer(PlayMaxTime * m_totalFighter);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(ex);
            }
        }

        /// <summary>
        /// 开始计算战斗
        /// </summary>
        /// <returns>是否结束</returns>
        private bool StartAttack()
        {
            StartBuffer();
            bool over = false;
            // 开始攻击..
            m_actions.Clear();
            for (int i = 0; i < m_fighter.Count; i++)
            {
                FightObject fighter = m_fighter[i];
                if (fighter.Action != null && fighter.IsLive && fighter.CanActive)
                {
                    over = SingleAttack(fighter);
                    if (over) break;
                }
            }
            var results = GetFightResults();
            foreach (var player in results)
            {
                if (player.Action.ActionType == ActionType.JiNeng && player is FightPlayer)
                {
                    Variant v = new Variant(2);
                    v["ID"] = player.ID;
                    v["MoFa"] = new MVPair(player.Life.MoFa, player.MP);
                    if (player is FightPet)
                    {
                        SendToClinet(PetsCommand.UpdatePetR, true, v);
                    }
                    else
                    {
                        SendToClinet(ClientCommand.UpdateActorR, v);
                    }
                }
            }
            DecrementAutoFight();
            SendToClinet(FightCommand.FightTurnEndR, m_buffer, m_actions);
            //m_showOver = DateTime.UtcNow.AddMilliseconds(m_actions.Count * 500);
            m_actions.Clear();
            return over;
        }

        /// <summary>
        /// 减掉自动回合数
        /// </summary>
        protected virtual void DecrementAutoFight()
        {
            foreach (var p in m_players)
            {
                if (p != null) p.DecrementAutoFight();
            }
        }

        private bool SingleAttack(FightObject fighter)
        {
            if (fighter.Action.ActionType == ActionType.JiNeng)
            {
                try
                {
                    GameConfig gc;
                    //检查技能限制
                    if (fighter.CheckJingNeng(m_fightCount, out gc))
                    {
                        int level = fighter.Action.SkillLev;
                        if (gc.SubType == SkillSub.Attack)
                        {
                            AttackJiNeng(fighter, level, gc);
                        }
                        //TODO: 已改为被动
                        //else if (gc.SubType == SkillSub.AddBuffer)
                        //{
                        //    SkillAddBuffer(fighter, level, gc);
                        //}
                        else if (gc.SubType == SkillSub.Revive)
                        {
                            SkillRevive(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.Cure)
                        {
                            SkillCure(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.CreateBuffer)
                        {
                            SkillCreateBuffer(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.IncreaseDefense)
                        {
                            SkillIncreaseDefense(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.NoDeath)
                        {
                            SkillNoDeath(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.XuLi)
                        {
                            SkillXuli(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.ShenTuo)
                        {
                            SkillShenTou(fighter, level, gc);
                        }
                        else if (gc.SubType == SkillSub.XiSheng)
                        {
                            SkillXiSheng(fighter, level, gc);
                        }
                        return CheckTeamOver();
                    }
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Error(ex);
                }
                //转换为默认的物理攻击
                fighter.Action.ActionType = ActionType.WuLi;
            }
            if (fighter.Action.ActionType == ActionType.WuLi)
            {
                AttackWuLi(fighter);
            }
            else if (fighter.Action.ActionType == ActionType.TaoPao)
            {
                TaoPao(fighter);
            }
            else if (fighter.Action.ActionType == ActionType.DaoJu)
            {
                DaoJu(fighter as FightPlayer);
            }
            else if (fighter.Action.ActionType == ActionType.ZhuaPu)
            {
                ZhuaPu(fighter as FightPlayer);
            }
            else if (fighter.Action.ActionType == ActionType.ChangePet)
            {
                ChangePet(fighter as FightPlayer);
            }
            return CheckTeamOver();
        }

        /// <summary>
        /// 初始化战斗数据.计算本回合的速度
        /// 为没有出招的生成默认招式,收集保护招式
        /// </summary>
        /// <param name="list"></param>
        private void InitAttack()
        {
            m_protecter.Clear();
            for (int i = 0; i < m_fighter.Count; i++)
            {
                FightObject fight = m_fighter[i];
                fight.Sudu = (fight.Life.SuDu * NumberRandom.Next(7000, 10001));

                if (fight.Team == m_teamA)
                {
                    fight.CreateAction(m_teamB, m_fightCount);
                }
                else
                {
                    fight.CreateAction(m_teamA, m_fightCount);
                }

                if (fight.Action.ActionType == ActionType.Protect)
                {
                    m_protecter.Add(fight);
                    m_fighter.RemoveAt(i--);
                }
                fight.Action.FightCount = m_fightCount;
                fight.Action.Result = null;
            }
            //使用速度排序..
            m_fighter.Sort((x, y) => { return y.Sudu - x.Sudu; });
        }

        /// <summary>
        /// 获取战斗结果..
        /// </summary>
        /// <returns></returns>
        protected IEnumerable<FightObject> GetFightResults()
        {
            return m_fighter.Where(v => v.Action != null && v.Action.FightCount == FightAction.HasAction);
        }

        /// <summary>
        /// 线程安全的方式更新战斗状态
        /// 0:等待客户端加载资源
        /// 1:可操作战斗指令
        /// 2:客户操作完成.
        /// 3:服务器开始计算
        /// 4:计算结束.等待客户端播放
        /// --5:客户播放完成..
        /// 10: 战斗结束.
        /// 11: 计算本次战斗的结果
        /// 12: 发送结果通知
        /// </summary>
        /// <param name="comp">旧值</param>
        /// <param name="value">新值</param>
        /// <returns>是否更新成功</returns>
        protected virtual bool ChangeState(int comp, int value)
        {
            return Interlocked.CompareExchange(ref m_fightState, value, comp) == comp;
        }

        /// <summary>
        /// 重量计时器
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        protected bool ChangeTimer(int m)
        {
            if (m_timer == null) return false;
            m_timer.Change(m, Timeout.Infinite);
            return true;
        }

        /// <summary>
        /// 发送消息给客户
        /// </summary>
        /// <param name="command"></param>
        /// <param name="msgs"></param>
        public void SendToClinet(string command, params object[] msgs)
        {
            var buffer = AmfCodec.Encode(command, msgs);
            foreach (var p in m_players)
            {
                if (p != null) p.Call(buffer);
            }
        }

        public void SendToTeam(FightObject[] team, string command, params object[] msgs)
        {
            var buffer = AmfCodec.Encode(command, msgs);
            foreach (var v in team)
            {
                if (v.FType == FighterType.Player && v.Online)
                {
                    PlayerBusiness player = (v as FightPlayer).Player;
                    player.Call(buffer);
                }
            }
        }

        /// <summary>
        /// 战斗结束,退出玩家
        /// </summary>
        protected abstract void GameOver();

        /// <summary>
        /// 强制结束.
        /// </summary>
        public void ForcedOver()
        {
            var buffer = AmfCodec.Encode(FightCommand.FightEndR, new object[] { (int)FightResult.Tie, null, string.Empty });
            foreach (var p in m_players)
            {
                if (p != null)
                {
                    p.Call(buffer);
                }
            }
            Close();
        }

        protected void Close()
        {
            var timer = m_timer;
            if (timer != null)
            {
                m_timer = null;
                timer.Dispose();
            }
            if (Interlocked.Exchange(ref m_fightState, 100) < 100)
            {
                foreach (var player in m_teamA)
                {
                    if (player.Online)
                    {
                        //player.Online = false;
                        player.ExitFight(m_changeLife);
                    }
                }
                m_teamA = EmptyTeam;
                foreach (var player in m_teamB)
                {
                    if (player.Online)
                    {
                        //player.Online = false;
                        player.ExitFight(m_changeLife);
                    }
                }
                m_teamB = EmptyTeam;
            }
        }
    }
}
