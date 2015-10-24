using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理玩家自身的行为(如换装.购物等..)
    /// </summary>
    sealed public class PlayersMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                LoginCommand.PlayerLogin,
                ClientCommand.UpdateHotKeys,
                ClientCommand.UserDisconnected,
                ClientCommand.GetPlayerDetail,
                ClientCommand.GetPlayerDetail2,
                ClientCommand.SendMsgToAllPlayer,
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
            if (note.Name == LoginCommand.PlayerLogin)
            {
                PlayerLogin(note);
                return;
            }
            // 需验证玩家登录的操作
            if (note.Player != null)
            {
                switch (note.Name)
                {
                    case ClientCommand.UpdateHotKeys:
                        UpdateHotKeys(note);
                        return;
                    case ClientCommand.UserDisconnected:
                        UserDisconnected(note);
                        return;
                    case ClientCommand.SendMsgToAllPlayer:
                        SendMsgToPlayers(note);
                        return;
                    case ClientCommand.GetPlayerDetail:
                        GetPlayerDetail(note);
                        return;
                    case ClientCommand.GetPlayerDetail2:
                        GetPlayerDetail2(note);
                        return;
                    default:
                        break;
                }
            }
        }
        #endregion

        /// <summary>
        /// 获取用户详细信息
        /// </summary>
        /// <param name="note"></param>
        private void GetPlayerDetail(UserNote note)
        {
            string playerID = note.GetString(0);
            PlayerBusiness player = PlayersProxy.FindPlayerByID(playerID);
            if (player != null)
            {
                note.Call(ClientCommand.GetPlayerDetailR, true, new PlayerDetail(player, 1));
                return;
            }
            note.Call(ClientCommand.GetPlayerDetailR, false, null);
        }

        /// <summary>
        /// 获取用户详细信息
        /// </summary>
        /// <param name="note"></param>
        private void GetPlayerDetail2(UserNote note)
        {
            string playerID = note.GetString(0);
            PlayerBusiness player = PlayersProxy.FindPlayerByID(playerID);
            if (player != null)
            {
                IList keys = note.GetValue<IList>(1);
                if (keys != null && keys.Count > 0)
                {
                    note.Call(ClientCommand.GetPlayerDetail2R, true, new PlayerDetail(player, keys));
                }
                else
                {
                    note.Call(ClientCommand.GetPlayerDetail2R, true, new PlayerDetail(player, 1));
                }
                return;
            }
            note.Call(ClientCommand.GetPlayerDetail2R, false, null);
        }

        /// <summary>
        /// 玩家首次请求进入场景(登录方式)
        /// </summary>
        /// <param name="note"></param>
        private void PlayerLogin(UserNote note)
        {
            // 取玩家信息
            string userID = note.Session.UserID;
            string playerID = note.GetString(0);
            UserSession session = note.Session;
            PlayerBusiness player;
            if (PlayersProxy.TryGetPlayerByID(playerID, out player))
            {
                if (player.UserID != userID) return;
                // 替换连接ID
                UserSession oldSession = player.Reconnection(session);
                if (oldSession != null && oldSession != session)
                {
                    //TODO: 通知旧连接
                }
            }
            else
            {
                player = PlayersProxy.FindPlayerByID(playerID);
                if (player == null || player.UserID != userID)
                {
                    //玩家角色不存在
                    note.Call(LoginCommand.PlayerLoginR, new object[] { false, playerID, TipManager.GetMessage(ClientReturn.PlayerLogin1) });
                    return;
                }
                if (!PlayersProxy.TryAddPlayer(player))
                {
                    LogWrapper.Debug(string.Format("Login conflict:{0}", playerID));
                    return;
                }
            }

            if (player.State != 1)
            {
                //角色已冻结
                note.Call(LoginCommand.PlayerLoginR, new object[] { false, playerID, TipManager.GetMessage(ClientReturn.PlayerLogin2) });
                return;
            }

            string sceneID = player.SceneID;
            SceneBusiness scene;
            if (!ScenesProxy.TryGetScene(sceneID, out scene, player.Line))
            {
                return;
            }

            player.InitPlayer(session);
            SetYellow(player, session.QQToken);

            TeamInstanceBusiness tib = null;

            //战场
            if (scene.SceneType == SceneType.Battle)
            {
                Destination des = scene.DeadDestination;
                sceneID = des.SceneID;
                if (!ScenesProxy.TryGetScene(sceneID, out scene))
                {
                    return;
                }
                player.X = des.X;
                player.Y = des.Y;
                player.SceneID = sceneID;
            }
            else if (scene is ScenePart)
            {
                sceneID = player.ResetScene();
                if (!ScenesProxy.TryGetScene(sceneID, out scene))
                {
                    return;
                }
            }
            else if (scene.SceneType == SceneType.Instance)
            {
                long eid = player.Ectype.Value.GetInt64OrDefault("EID");
                if (eid != 0)
                {
                    if (TeamInstanceProxy.TryGetTeamInstance(eid, out tib))
                    {
                        if (tib.OverTime > DateTime.UtcNow)
                        {
                            player.TeamInstance = tib;
                            scene = tib.Scene;
                            sceneID = scene.ID;
                            player.SceneID = sceneID;
                        }
                        else
                        {
                            player.Ectype.Value["EID"] = 0;
                            player.Ectype.Save();
                            tib = null;
                        }
                    }
                }
                if (tib == null)
                {
                    sceneID = player.ResetScene();
                    if (!ScenesProxy.TryGetScene(sceneID, out scene))
                    {
                        return;
                    }
                    //处理旧数据
                    if (scene.SceneType == SceneType.Instance)
                    {
                        sceneID = SceneCity.DefaultID;
                        ScenesProxy.TryGetScene(sceneID, out scene);
                        player.SceneID = sceneID;
                    }
                }
            }

            //副本或子副本,处理超时限制(小于10秒不能进入)
            else if ((scene is SceneEctype && ((SceneEctype)scene).MaxStay > 0)
                || (scene is SceneSubEctype && ((SceneSubEctype)scene).IsOverTime))
            {
                Destination des = null;
                DateTime overTime = player.Ectype.Value.GetDateTimeOrDefault("OverTime");
                if ((overTime - DateTime.UtcNow).TotalSeconds < 10)
                {
                    des = scene.PropDestination ?? scene.DeadDestination;
                }
                //else
                //{
                //    des = scene.DeadDestination;
                //}
                if (des != null)
                {
                    sceneID = des.SceneID;
                    if (!ScenesProxy.TryGetScene(sceneID, out scene))
                    {
                        return;
                    }
                    player.X = des.X;
                    player.Y = des.Y;
                    player.SceneID = sceneID;
                }
            }

            // 返回登录成功消息
            var buffer = AmfCodec.EncodeBig(LoginCommand.PlayerLoginR, new object[] { true, scene.ID, new PlayerDetail(player, 2) });
            note.Session.SendAsync(buffer);

            session.InitPlayer(player);
            player.Scene = scene;
            scene.Execute(new UserNote(player, note.Name, new object[] { player.X, player.Y }));

            if (tib != null)
            {
                tib.ReInto(player);
            }
            return;
        }

        /// <summary>
        /// 黄钻设置
        /// </summary>
        /// <param name="note"></param>
        /// <param name="token"></param>
        static private void SetYellow(PlayerBusiness player, Variant token)
        {
            if (token.GetIntOrDefault("is_yellow_vip") == 1)
            {
                int is_yellow_year = token.GetIntOrDefault("is_yellow_year_vip");
                int yellow_vip_level = token.GetIntOrDefault("yellow_vip_level");
                int t = yellow_vip_level * 10 + is_yellow_year;
                player.Yellow = t;
            }
            else
            {
                player.Yellow = 0;
            }
        }

        private void UserDisconnected(UserNote note)
        {
            PlayerBusiness player = note.Player;
            if (player == null) return;
            if (PlayersProxy.TryGetPlayerByID(player.PID, out player))
            {
                if (player.Session == note.Session)
                {
                    player.Reconnection(null);
                    PlayersProxy.TryRemovePlayer(player.PID, out player);
                    var buffer = AmfCodec.Encode(ClientCommand.PlayerExitR, new object[] { true, player.ID, string.Empty });
                    string sceneID = player.SceneID;
                    foreach (var p in PlayersProxy.Players)
                    {
                        if (p.SceneID == sceneID)
                        {
                            p.Call(buffer);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 更新热键
        /// </summary>
        /// <param name="note"></param>
        /// <param name="name"></param>
        private void UpdateHotKeys(UserNote note)
        {
            Variant v = note.GetVariant(0);
            PlayerEx hotKeys = note.Player.HotKeys;
            hotKeys.Value = v;
            hotKeys.Save();
        }

        private void SendMsgToPlayers(UserNote note)
        {
            DateTime now = DateTime.UtcNow;
            PlayerBusiness sender = note.Player;
            if (sender.Danned > now)
            {
                //禁止发言
                note.Call(ClientCommand.SendActivtyR, "T02", TipManager.GetMessage(ClientReturn.NoTalk));
                return;
            }

            Variant m = note.GetVariant(1);
            string msg = m.GetStringOrDefault("msg");
            if (msg.Length > 1000)
            {
                note.Call(ClientCommand.SendActivtyR, "T02", TipManager.GetMessage(ClientReturn.MsgToBig));
                return;
            }

            m["msg"] = StringFilter.Instance.ReplacetBadWord(msg);
            Sinan.Collections.BytesSegment buffer;
            if (msg.Length < 512)
            {
                buffer = AmfCodec.Encode(ClientCommand.SendMsgToAllPlayerR, note.Body as IList);
            }
            else
            {
                buffer = AmfCodec.EncodeBig(ClientCommand.SendMsgToAllPlayerR, note.Body as IList);
            }

            string range = note.GetString(0);
            if (range == "screne" || range == "city") //当前场景
            {
                note.Player.Scene.CallAllExcludeOne(note.Player, buffer);
            }
            else if (range == "world") //世界
            {
                if (note.Player.Level < 20)
                {
                    if (ConfigLoader.Config.Platform == Sinan.Entity.Platform.Tencent)
                    {
                        return;
                    }
                }
                foreach (var player in PlayersProxy.Players)
                {
                    player.Call(buffer);
                }
            }
            else if (range == "self") //私聊
            {
                string targetID = note.GetVariant(1).GetStringOrDefault("targetID");
                PlayerBusiness player;
                if (PlayersProxy.TryGetPlayerByID(targetID, out player))
                {
                    if (player.Online) player.Call(buffer);
                }
            }
            else if (range == "here") //近聊
            {
                object[] targetIDs = note.GetVariant(1).GetValueOrDefault<object[]>("targetIDs");
                if (targetIDs == null || targetIDs.Length == 0) return;
                foreach (var v in targetIDs)
                {
                    PlayerBusiness player;
                    if (note.Player.Scene.TryGetPlayer(v.ToString(), out player))
                    {
                        player.Call(buffer);
                    }
                }
            }
            else if (range == "team") //队伍
            {
                PlayerTeam tema = note.Player.Team;
                if (tema != null)
                {
                    tema.Call(buffer, true, null);
                }
            }
            else if (range == "job")   //职业
            {
                if (note.Player.Level < 20)
                {
                    return;
                }
                string roleID = note.Player.RoleID;
                foreach (var player in PlayersProxy.Players)
                {
                    if (player.RoleID == roleID)
                        player.Call(buffer);
                }
            }
            else if (range == "family") //家族
            {
                string family = note.Player.FamilyName;
                if (string.IsNullOrEmpty(family)) return;
                foreach (var player in PlayersProxy.Players)
                {
                    if (player.FamilyName == family)
                        player.Call(buffer);
                }
            }
        }
    }
}
