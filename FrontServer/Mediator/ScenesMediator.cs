using System.Collections.Generic;
using System.Drawing;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 处理场景上的一般行为
    /// </summary>
    sealed public class ScenesMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                ClientCommand.IntoScene,
                ClientCommand.FindPinPath,

                ClientCommand.WalkTo,
                ClientCommand.UseSkill,
                ClientCommand.DiscardGoods,
                ClientCommand.ExitScene,
                ClientCommand.MapTrans,
                ClientCommand.UserDisconnected,

                FightCommand.FightTaskApc,
                FightCommand.FightPK,
                FightCommand.FightCC,
                FightCommand.FightReplyCC,
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
            if (note.Player == null) return;

            switch (note.Name)
            {
                case ClientCommand.IntoScene:
                    IntoScene(note);
                    return;
                case ClientCommand.MapTrans:
                    MapTrans(note);
                    return;
                case ClientCommand.FindPinPath:
                    FindPinPath(note);
                    return;
                default:
                    // 已登录的玩家才能执行的操作..
                    SceneBusiness scene = note.Player.Scene;
                    if (scene != null)
                    {
                        scene.Execute(note);
                    }
                    return;
            }
        }
        #endregion

        /// <summary>
        /// 查找传送阵
        /// </summary>
        /// <param name="note"></param>
        private void FindPinPath(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string a = note.GetString(0);
            string b = note.GetString(1);
            var result = ScenesProxy.FindPath(a, b, player.Level);
            if (result == null || result.Count == 0)
            {
                if (player.Scene.SceneType == SceneType.Home)
                {
                    //取下一个场景.
                    string sceneID = player.Ectype.Value.GetStringOrDefault("SceneID");
                    if (string.IsNullOrEmpty(sceneID) || sceneID == SceneHome.DefaultID)
                    {
                        sceneID = SceneCity.DefaultID;
                    }
                    var result2 = ScenesProxy.FindPath(sceneID, b, player.Level);
                }
            }
            note.Call(ClientCommand.FindPinPathR, new object[] { result });
        }

        /// <summary>
        /// 请求进入场景
        /// </summary>
        /// <param name="note"></param>
        void IntoScene(UserNote note)
        {
            PlayerBusiness player = note.Player;
            //进入方式
            int enterMode = note.GetInt32(0);

            //其它方式..要求玩家已在场景中
            SceneBusiness scene = player.Scene;
            if (player.AState == ActionState.Fight || scene == null)
            {
                return;
            }

            if (player.Team != null && player.TeamJob == TeamJob.Member)
            {
                //只有队长可以执行此操作
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.IntoScene1));
                return;
            }

            SceneBusiness inScene;
            //1:传送阵传送
            if (enterMode == 1)
            {
                string pinID = note.GetString(1);
                ScenePin pin = ScenePinManager.Instance.GetPin(pinID, scene.ID);
                if (pin == null) return;
                object[] point = null;
                string sceneB = pin.SceneB;
                if (string.IsNullOrEmpty(sceneB) && scene.SceneType == SceneType.Home)
                {
                    // 家园传送阵
                    sceneB = player.ResetScene();
                    if (sceneB == SceneHome.DefaultID)
                    {
                        sceneB = SceneCity.DefaultID;
                    }
                    point = new object[] { player.X, player.Y };
                }

                if (ScenesProxy.TryGetScene(sceneB, out inScene))
                {
                    //场景转移检查
                    if (scene.TransferCheck(player, inScene, TransmitType.Pin))
                    {
                        TransScene(player, scene, inScene, point ?? new object[] { pin.ScenebX, pin.ScenebY });
                        return;
                    }
                    if (player.Team == null && !scene.CheckLevel(player))
                    {
                        //玩家等级不符合当前场景要求.转回出生点..
                        GoHome(player);
                    }
                }
                return;
            }

            //NPC方式进入
            if (enterMode == 2)
            {
                if (ScenesProxy.TryGetScene(note.GetString(1), out inScene))
                {
                    if (inScene.SceneType == SceneType.Pro || inScene.SceneType == SceneType.Rob)
                    {
                        return;
                    }
                    if (scene.TransferCheck(player, inScene, TransmitType.Npc))
                    {
                        object[] point = new object[] { inScene.BornX, inScene.BornY };
                        TransScene(player, scene, inScene, point);
                    }
                }
                return;
            }
            //回家的方式,直接传送
            if (enterMode == 3)
            {
                GoHome(player);
                return;
            }
            //参加活动
            if (enterMode == 4)
            {
                IntoPart(note);
                return;
            }
        }

        /// <summary>
        /// 参数活动
        /// </summary>
        /// <param name="note"></param>
        private static void IntoPart(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string partID = note.GetString(1);
            PartBusiness part = PartProxy.TryGetPart(partID);
            if (part == null)
            {
                //活动还未开始
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.IntoPart1));
                return;
            }
            if (!part.IntoCheck(player))
            {
                return;
            }
            SceneBusiness inScene = part.MainScene;
            if (inScene != null)
            {
                if (player.Scene.TransferCheck(player, inScene, TransmitType.Part))
                {
                    if (!part.IntoPart(player))
                    {
                        return;
                    }
                    object[] point = new object[] { inScene.BornX, inScene.BornY };
                    TransScene(player, player.Scene, inScene, point);
                    return;
                }
            }
        }

        /// <summary>
        /// 回家的方式,直接传送
        /// </summary>
        /// <param name="note"></param>
        private static void GoHome(PlayerBusiness player)
        {
            //回基地的配置
            Variant hq = RoleManager.Instance.GetRoleConfig<Variant>(player.RoleID, "HQ");
            string sceneid = hq.GetStringOrDefault("Map");
            if (sceneid != null)
            {
                SceneBusiness inScene;
                if (ScenesProxy.TryGetScene(sceneid, out inScene))
                {
                    object[] point = new object[] { hq.GetIntOrDefault("X"), hq.GetIntOrDefault("Y") };
                    TransScene(player, player.Scene, inScene, point);
                    return;
                }
            }
            LogWrapper.Warn("GoHome err:" + player.Name);
        }

        static PlayerTeam TransScene(PlayerBusiness player, SceneBusiness scene, SceneBusiness inScene, object[] point)
        {
            if (scene.ExitScene(player))
            {
                UserNote note = new UserNote(player, ClientCommand.IntoSceneSuccess, point);
                inScene.Execute(note);

                //转移队伍成员
                PlayerTeam team = player.Team;
                if (team != null && player.TeamJob == TeamJob.Captain)
                {
                    point = new object[] { player.X, player.Y };
                    for (int i = 1; i < team.Members.Length; i++)
                    {
                        PlayerBusiness member = team.Members[i];
                        if (member != null && member.TeamJob == TeamJob.Member)
                        {
                            if (scene.ExitScene(member))
                            {   //执行进入
                                UserNote note2 = new UserNote(member, ClientCommand.IntoSceneSuccess, point);
                                inScene.Execute(note2);
                            }
                        }
                    }
                }
                return team;
            }
            return null;
        }


        private void MapTrans(UserNote note)
        {
            PlayerBusiness player = note.Player;

            //要求玩家已在场景中
            SceneBusiness scene = player.Scene;
            if (player.AState == ActionState.Fight || scene == null)
            {
                return;
            }

            if (player.Team != null && player.TeamJob == TeamJob.Member)
            {
                //只有队长可以执行此操作
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.IntoScene1));
                return;
            }

            string sceneB = note.GetString(0);
            if (sceneB == scene.ID)
            {
                //同场景传送...
                if (CheckMapTrans(player))
                {
                    SceneMovie(note, player, scene);
                }
                return;
            }

            switch (scene.SceneType)
            {
                case SceneType.Battle:
                case SceneType.Home:
                case SceneType.Outdoor:
                case SceneType.City:
                    break;
                default:
                    player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.MapTrans1));
                    return;
            }

            SceneBusiness inScene;
            if (!ScenesProxy.TryGetScene(sceneB, out inScene))
            {
                return;
            }

            switch (inScene.SceneType)
            {
                case SceneType.Battle:
                case SceneType.Outdoor:
                case SceneType.City:
                    break;
                default:
                    player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.MapTrans2));
                    return;
            }

            //检查个人等级要求
            if (!inScene.CheckLevel(player))
            {
                string msg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit2), inScene.Name, inScene.MinLev);
                player.Call(ClientCommand.IntoSceneR, false, null, msg);
                return;
            }
            PlayerTeam team = player.Team;
            //检查等级要求.
            if (team != null && player.TeamJob == TeamJob.Captain)
            {
                for (int i = 1; i < team.Members.Length; i++)
                {
                    PlayerBusiness member = team.Members[i];
                    if (member != null && member.TeamJob == TeamJob.Member)
                    {
                        if (!inScene.CheckLevel(member))
                        {
                            string msg = string.Format(TipManager.GetMessage(ClientReturn.IntoLimit7), member.Name);
                            player.Call(ClientCommand.IntoSceneR, false, null, msg);
                            return;
                        }
                    }
                }
            }

            //检查传送次数
            if (CheckMapTrans(player))
            {
                //场景转移
                TransScene(player, scene, inScene, new object[] { note.GetInt32(1), note.GetInt32(2) });
            }
        }

        /// <summary>
        /// 同场景瞬间移动
        /// </summary>
        /// <param name="note"></param>
        /// <param name="player"></param>
        private static void SceneMovie(UserNote note, PlayerBusiness player, SceneBusiness scene)
        {
            List<string> pids = new List<string>();
            int x = note.GetInt32(1);
            int y = note.GetInt32(2);
            if (!scene.CheckWalk(x, y))
            {
                //随机重置x,y
                int index = NumberRandom.Next(scene.Walking.Count);
                Point p = scene.Walking[index];
                x = p.X;
                y = p.Y;
            }
            player.X = x;
            player.Y = y;
            pids.Add(player.ID);

            PlayerTeam team = player.Team;
            if (team != null && player.TeamJob == TeamJob.Captain)
            {
                for (int i = 1; i < team.Members.Length; i++)
                {
                    PlayerBusiness member = team.Members[i];
                    if (member != null && member.TeamJob == TeamJob.Member)
                    {
                        member.X = x;
                        member.Y = y;
                        pids.Add(member.ID);
                    }
                }
            }
            player.CallAll(ClientCommand.MapTransR, true, new object[] { note.GetString(0), x, y }, pids);
        }

        /// <summary>
        /// 检查传送次数
        /// </summary>
        /// <param name="player"></param>
        private static bool CheckMapTrans(PlayerBusiness player)
        {
            Variant v = MemberAccess.MemberInfo(player.MLevel);
            if (v == null)
            {
                player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.MapTrans3));
                return false;
            }
            int maxTranr = v.GetIntOrDefault("MapTrans");
            int cur = player.ReadDaily(PlayerBusiness.DailyOther, "MapTrans");
            if (cur >= maxTranr)
            {
                //检查道具...传送羽毛(G_d003050)
                const string transGoodsid = "G_d003050";
                //GameConfigAccess.Instance.Find(MainType.Goods, "MapTrans");
                var g = GameConfigAccess.Instance.FindOneById(transGoodsid);
                if (player.RemoveGoods(transGoodsid, 1, GoodsSource.MapTrans))
                {
                    return true;
                }

                player.Call(ClientCommand.MapTransR, false, null, null);
                //player.Call(ClientCommand.IntoSceneR, false, null, TipManager.GetMessage(ClientReturn.MapTrans4));
                return false;
            }
            else
            {
                //记录免费传送次数
                player.WriteDaily(PlayerBusiness.DailyOther, "MapTrans");
            }
            return true;
        }
    }
}