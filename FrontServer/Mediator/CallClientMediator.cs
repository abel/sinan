using System.Collections;
using System.Collections.Generic;
using Sinan.GameModule.Command;
using Sinan.Observer;
using Sinan.Util;
using Sinan.NetModule;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 调用客户端方法
    /// </summary>
    sealed public class CallClientMediator : ISubscriber
    {
        #region ISubscriber 成员
        public IList<string> Topics()
        {
            return new string[]
            {
                MessageCommand.InvokeClientByConnectID,
                MessageCommand.InvokeClientByUserID,
                MessageCommand.InvokeClientByPlayerID,
            };
        }
        void ISubscriber.Execute(INotification notification)
        {
            InvokeClientNote note = notification as InvokeClientNote;
            if (note != null) this.ExecuteNote(note);
        }

        void ExecuteNote(InvokeClientNote note)
        {
            if (note.Name == MessageCommand.InvokeClientByConnectID)
            {
                InvokeClientByConnectID(note);
            }
            else if (note.Name == MessageCommand.InvokeClientByPlayerID)
            {
                InvokeClientByPlayerID(note);
            }
            else if (note.Name == MessageCommand.InvokeClientByUserID)
            {
                InvokeClientByUserID(note);
            }
        }
        #endregion

        /// <summary>
        /// 通过连接ID调用客户端的方法...
        /// (此类通知一般由服务端发起)
        /// </summary>
        /// <param name="note"></param>
        private void InvokeClientByConnectID(InvokeClientNote note)
        {
            //if (note == null || note.IDList == null) return;
            //switch (note.IDList.Count)
            //{
            //    case 0: return;
            //    case 1:
            //        SessionsProxy.Call(note.IDList[0], note.Type, note.Body as IList);
            //        return;
            //    default:
            //        var buffer = AmfCodec.Encode(note.Type, note.Body as IList);
            //        foreach (var connectID in note.IDList)
            //        {
            //            SessionsProxy.Call(connectID, buffer);
            //        }
            //        return;
            //}
        }

        /// <summary>
        /// 通过玩家ID调用客户端的方法...
        /// </summary>
        /// <param name="note"></param>
        private void InvokeClientByPlayerID(InvokeClientNote note)
        {
            //if (note == null || note.IDList == null) return;
            //switch (note.IDList.Count)
            //{
            //    case 0: return;
            //    case 1: PlayerBusiness one;
            //        if (PlayersProxy.TryGetPlayerByID(note.IDList[0], out one))
            //        {
            //            one.Call(note.Type, note.Body as IList);
            //        }
            //        return;
            //    default:
            //        var buffer = AmfCodec.Encode(note.Type, note.Body as IList);
            //        foreach (var playerID in note.IDList)
            //        {
            //            PlayerBusiness player;
            //            if (PlayersProxy.TryGetPlayerByID(playerID, out player))
            //            {
            //                player.Call(buffer);
            //            }
            //        }
            //        return;
            //}
        }



        /// <summary>
        /// 通过用户ID调用客户端的方法...
        /// </summary>
        /// <param name="note"></param>
        private void InvokeClientByUserID(InvokeClientNote note)
        {
            //if (note == null || note.IDList == null) return;
            //switch (note.IDList.Count)
            //{
            //    case 0: return;
            //    case 1: UserSession one;
            //        if (UsersProxy.TryGetUser(note.IDList[0], out one))
            //        {
            //            one.CallArray(note.Type, note.Body as IList);
            //        }
            //        return;
            //    default:
            //        var buffer = AmfCodec.Encode(note.Type, note.Body as IList);
            //        foreach (var user in note.IDList)
            //        {
            //            UserSession session;
            //            if (UsersProxy.TryGetUser(user, out session))
            //            {
            //                session.SendAsync(buffer);
            //            }
            //        }
            //        return;
            //}
        }


        //void ExecuteNote(InvokeScenePlayerNote note)
        //{
        //    if (note.Name == MessageCommand.InvokeClientBySceneID)
        //    {
        //        InvokeClientBySceneID(note);
        //    }
        //}
        ///// <summary>
        ///// 调用场景中玩家的方法
        ///// </summary>
        ///// <param name="note"></param>
        //private void InvokeClientBySceneID(InvokeScenePlayerNote note)
        //{
        //    if (note == null || string.IsNullOrEmpty(note.SceneID))
        //    {
        //        return;
        //    }
        //    SceneBusiness scene;
        //    if (!ScenesProxy.TryGetScene(note.SceneID, out scene))
        //    {
        //        return;
        //    }

        //    string playerID = note.PlayerID;
        //    //只发给指定用户.
        //    if (note.TargetType == InvokeTargetType.OnePlayer)
        //    {
        //        PlayerBusiness pb;
        //        if (scene.TryGetPlayer(playerID, out pb))
        //        {
        //            pb.Call(note.Type, note.Body as IList);
        //        }
        //        return;
        //    }

        //    var buffer = EncodeHelper2.EncodeArray(note.Type, note.Body);
        //    //场景中的所有用户
        //    if (note.TargetType == InvokeTargetType.AllPlayer)
        //    {
        //        foreach (var player in scene.Players)
        //        {
        //            //SessionsProxy.Call(player.ConnectID, buffer);
        //        }
        //    }
        //    //除自己的其它用户(InvokeTargetType.ButOne)
        //    else
        //    {
        //        foreach (var player in scene.Players)
        //        {
        //            if (player.ID != playerID)
        //            {
        //                player.Call(buffer);
        //            }
        //        }
        //    }
        //}

    }
}