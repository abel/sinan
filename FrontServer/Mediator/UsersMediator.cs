using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 用户管理自己角色(创建/删除/恢复)
    /// </summary>
    sealed public class UsersMediator : AysnSubscriber
    {
        static PlayerAccess m_access = PlayerAccess.Instance;

        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                LoginCommand.UserLogin,
                UserPlayerCommand.DeletePlayer,
                UserPlayerCommand.RecoveryPlayer,
                UserPlayerCommand.CreatePlayer,
                UserPlayerCommand.CreatePlayerName,

                ClientCommand.UserDisconnected,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            // 已登录的用户才能执行的操作..
            if (string.IsNullOrEmpty(note.Session.UserID))
            {
                if (note.Name == LoginCommand.UserLogin)
                {
                    Login(note);
                }
                return;
            }
            switch (note.Name)
            {
                case UserPlayerCommand.DeletePlayer:
                    DeletePlayer(note);
                    return;
                case UserPlayerCommand.CreatePlayer:
                    CreatePlayer(note);
                    return;
                case UserPlayerCommand.RecoveryPlayer:
                    RecoveryPlayer(note);
                    return;
                case UserPlayerCommand.CreatePlayerName:
                    CreatePlayerName(note);
                    return;
                case ClientCommand.UserDisconnected:
                    UserDisconnected(note);
                    return;
                default:
                    break;
            }
        }
        #endregion

        /// <summary>
        /// 断开连接.
        /// </summary>
        /// <param name="note"></param>
        private void UserDisconnected(UserNote note)
        {
            UserSession session = note.Session;
            string userID = session.UserID;
            session.UserID = null;
            if (userID != null)
            {
                UsersProxy.RemoveUser(userID, session.ConnectID);
                int onlineTime = (int)(DateTime.UtcNow.Subtract(session.Created).TotalSeconds);
                UserLogAccess.Instance.UserExit(userID, onlineTime);

                UserLogout log = new UserLogout();
                log.opopenid = userID;
                log.onlinetime = onlineTime;
                var lastCommand = session.LastCommand;
                if (lastCommand != null)
                {
                    log.reserve_1 = lastCommand.Item1;
                    PlayerBusiness player = session.Player;
                    if (player != null)
                    {
                        log.reserve_2 = player.Level;
                        log.reserve_5 = player.SceneID;
                        if (player.Level <= 20)
                        {
                            if (lastCommand.Item1 != 2009 && lastCommand.Item1 != 1703
                                && lastCommand.Item2 != null && lastCommand.Item2.Count > 0)
                            {
                                log.reserve_6 = JsonConvert.SerializeObject(lastCommand.Item2);
                            }
                        }
                    }
                }
                ServerLogger.WriteLog(session, log);
            }
        }

        /// <summary>
        /// 用户登录命令
        /// </summary>
        void Login(UserNote note)
        {
            //LogWrapper.Warn("开始登录:" + note.Session.IP);
            UserSession user = note.Session;
            if (note.Count < 1)
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.ParaErr, null);
                return;
            }
            string par = note.GetString(0);
            Variant token = UserLogAccess.Instance.LoginCheck(par, ServerManager.FastLogin);
            if (token == null)
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.Fail, null);
                return;
            }

            string userID = token.GetStringOrDefault("uid");
            if (string.IsNullOrEmpty(userID))
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.Fail, null);
                return;
            }

            if (BlackListManager.Instance.Contains(userID))
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.BlackList, null);
                return;
            }

            // 时间检查,24小时过期
            long t = token.GetInt64OrDefault("time");
            if (t > 0 && (UtcTimeExtention.NowTotalSeconds() - t > (3600 * 24)))
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.OverTime, null);
                return;
            }

            int zoneid = token.GetIntOrDefault("sid");
            if (zoneid == 0 && (!int.TryParse(note.GetString(1), out zoneid)))
            {
                zoneid = ServerLogger.zoneid;
            }

            // 成功.获取用户列表.设置用户ID.
            List<PlayerSimple> players = FilterOver(userID, zoneid);
            if (players.Count == 0 && (!ServerManager.OpenUser))
            {
                user.Call(LoginCommand.UserLoginR, (int)LoginResult.FreezeCreate, null);
                return;
            }

            Variant result = UserLogAccess.Instance.WriteLog(userID, user.IP.ToString(), zoneid);
            result["Players"] = players;
            result["ZoneEpoch"] = ConfigLoader.Config.ZoneEpoch;

            user.Call(LoginCommand.UserLoginR, (int)(zoneid == ServerLogger.zoneid ? LoginResult.Success : LoginResult.Success2), result);

            user.zoneid = zoneid;
            user.UserID = userID;
            user.key = token.GetStringOrDefault("key");
            Domain domain;
            if (!token.TryGetValue("domain", out domain))
            {
                domain = Domain.Qzone;
            }
            user.domain = domain;
            user.QQToken = token;

            UserSession oldUser = UsersProxy.AddAndGetOld(user);
            if (oldUser != null && oldUser != user)
            {
                ProcessOldUser(oldUser);
            }

            //写用户登录日志
            UserLogin log = new UserLogin();
            int uid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(userID, out uid))
            {
                log.opuid = uid;
            }
            ServerLogger.WriteLog(user, log);
            //LogWrapper.Warn("登录成功:" + note.Session.IP);
        }

        /// <summary>
        /// 过滤掉已过期的数据
        /// </summary>
        /// <param name="players"></param>
        /// <returns></returns>
        private static List<PlayerSimple> FilterOver(string userID, int zoneid)
        {
            List<Player> players = m_access.GetPlayers(userID, zoneid);
            List<PlayerSimple> ps = new List<PlayerSimple>(players.Count);
            foreach (var player in players)
            {
                int state = player.State;
                if (state == 2)
                {
                    int passSecond = (int)(DateTime.UtcNow - player.Modified).TotalSeconds;
                    int keepSecond = PlayerSimple.GetRetainSecond(player.Level);
                    if (passSecond >= keepSecond)
                    {
                        PlayersProxy.DeletePlayer(player.PID, true);
                        continue;
                    }
                }

                if (state <= 2)
                {
                    ps.Add(new PlayerSimple(player));
                }
                else if (state == 3)
                {
                    PlayersProxy.DeletePlayer(player.PID, false);
                }
            }
            return ps;
        }

        /// <summary>
        /// 处理重复登录用户
        /// </summary>
        /// <param name="oldUser"></param>
        private static void ProcessOldUser(UserSession oldUser)
        {
            try
            {
                //TODO: 需添加发送重复登录信息 
                //Socket socket = oldUser.Socket;
                //if (socket != null)
                //{
                //    oldUser.Call(LoginCommand.UserLoginR, (int)LoginResult.OtherInto, null);
                //    oldUser.Socket.Shutdown(SocketShutdown.Send);
                //}
                oldUser.Close();
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(ex);
            }
        }

        /// <summary>
        /// 删除角色,先冻结
        /// </summary>
        /// <param name="user"></param>
        /// <param name="playerID"></param>
        void DeletePlayer(UserNote note)
        {
            string playerID = note.GetString(0);
            int pid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(playerID, out pid))
            {
                int lev = m_access.GetPlayerLevel(pid, note.Session.UserID);
                if (lev > 0)
                {
                    int second = PlayerSimple.GetRetainSecond(lev);
                    if (second <= 0)
                    {
                        //直接删除
                        bool result = PlayersProxy.DeletePlayer(pid, true);
                        note.Call(UserPlayerCommand.DeletePlayerR, result, playerID, UtcTimeExtention.UnixEpoch);
                    }
                    else
                    {
                        bool result = m_access.ChangeState(pid, 2);
                        note.Call(UserPlayerCommand.DeletePlayerR, result, playerID, DateTime.UtcNow.AddSeconds(second));
                    }
                }
            }
        }

        /// <summary>
        /// 恢复被删除角色
        /// </summary>
        /// <param name="note"></param>
        /// <param name="playerID"></param>
        void RecoveryPlayer(UserNote note)
        {
            string playerID = note.GetString(0);
            int pid;
            if (Sinan.Extensions.StringFormat.TryHexNumber(playerID, out pid))
            {
                bool result = m_access.ChangeState(pid, 1);
                note.Call(UserPlayerCommand.RecoveryPlayerR, result, playerID);
            }
        }

        /// <summary>
        /// 创建角色.
        /// </summary>
        /// <param name="note"></param>
        /// <param name="roleID"></param>
        /// <param name="playerName"></param>
        /// <param name="other"></param>
        void CreatePlayer(UserNote note)
        {
            if (!ServerManager.OpenRole)
            {
                note.Call(UserPlayerCommand.CreatePlayerR, false,
                    TipManager.GetMessage(UserPlayerReturn.CreateLimit2));
                return;
            }

            string roleID = note.GetString(0);
            string name = note.GetString(1);
            int sex = note.GetInt32(2);
            string msg = NameManager.Instance.CheckName(name);
            if (!string.IsNullOrEmpty(msg))
            {
                note.Call(UserPlayerCommand.CreatePlayerR, false, msg);
                return;
            }

            //检查是否达到最大允许创建角色数
            int maxPlyaer = ConfigLoader.Config.Platform == Sinan.Entity.Platform.Gamewave ? 1 : 3;

            UserSession session = note.Session;
            int count = m_access.Count(session.zoneid, session.UserID);
            if (count >= maxPlyaer)
            {
                msg = TipManager.GetMessage(UserPlayerReturn.CreateLimit1) + maxPlyaer;
                session.Call(UserPlayerCommand.CreatePlayerR, false, msg);
                return;
            }

            if (m_access.ExistsName(name))
            {
                msg = TipManager.GetMessage(UserPlayerReturn.NameLimit4);
                session.Call(UserPlayerCommand.CreatePlayerR, false, msg);
                WordAccess.Instance.SetUsed(name);
                return;
            }

            //创建新的角色ID,返回小于0的数,则表示创建失败
            Variant role = RoleManager.RoleConfig.GetVariantOrDefault(roleID);
            //合服后创建的玩家都使用当前区ID为后缀
            int id = m_access.CreatePlayerID(ServerLogger.zoneid);
            if (id <= 0 || role == null)
            {
                msg = TipManager.GetMessage(UserPlayerReturn.CreateLimit2);
                session.Call(UserPlayerCommand.CreatePlayerR, false, msg);
                return;
            }

            Player player = Player.Create();
            player.PID = id;
            player.UserID = session.UserID;
            player.RoleID = roleID;
            player.Name = name;
            player.Sex = sex;
            player.SetBirthInfo(role);

            if (m_access.CreatePlayer(session.zoneid, player))
            {
                PlayerBusiness pb = PlayersProxy.FindPlayerByID(id);
                if (pb != null)
                {
                    NewRoleLog log = new NewRoleLog(session.zoneid);
                    log.source = session.domain.ToString();
                    pb.WriteLog(log);
                    //发送注册成功通知..
                    UserNote note2 = new UserNote(note, UserPlayerCommand.CreatePlayerSuccess, new object[] { pb });
                    Sinan.Observer.Notifier.Instance.Publish(note2);
                    session.Call(UserPlayerCommand.CreatePlayerR, true, new PlayerSimple(player));
                }
                return;
            }
            msg = TipManager.GetMessage(UserPlayerReturn.NameLimit4);
            session.Call(UserPlayerCommand.CreatePlayerR, false, msg);
        }

        /// <summary>
        /// 自动创建玩家名
        /// </summary>
        /// <param name="user"></param>
        private void CreatePlayerName(UserNote note)
        {
            for (int i = 0; i < 10; i++)
            {
                string name = WordAccess.Instance.AutomaticName();
                if (!PlayerAccess.Instance.ExistsName(name))
                {
                    note.Call(UserPlayerCommand.CreatePlayerNameR, true, name);
                    return;
                }
                else
                {
                    WordAccess.Instance.SetUsed(name);
                }
            }
            note.Call(UserPlayerCommand.CreatePlayerNameR, true, string.Empty);
        }

    }
}
