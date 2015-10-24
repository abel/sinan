using System;
using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Observer;
using log4net;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 每个连接都可以调用的操作
    /// (包括 获取职业列表/ 获取单个物品详细信息/ 获取所有物品信息)
    /// </summary>
    sealed public class EveryoneMediator : AysnSubscriber
    {
        private static ILog logger = LogManager.GetLogger("ClientReport");
        private static ILog logger2 = LogManager.GetLogger("ClientError");

        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                ClientCommand.GetRoles,
                ClientCommand.GetGoods,
                GMCommand.ClientError,
                GMCommand.ClinetReport,
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
            //已连接的用户可以执行的操作.
            switch (note.Name)
            {
                case ClientCommand.GetGoods:
                    GetGoods(note);
                    return;
                case ClientCommand.GetRoles:
                    GetRoles(note);
                    return;
                case GMCommand.ClientError:
                    ClientError(note);
                    break;
                case GMCommand.ClinetReport:
                    ClinetReport(note);
                    break;
                default:
                    return;
            }
        }
        #endregion

        #region 处理客户端的调用
        /// <summary>
        /// 获取物品信息
        /// </summary>
        /// <param name="user"></param>
        /// <param name="goodsType">物品分类</param>
        private void GetGoods(UserNote note)
        {
            string goodsType = note.GetString(0);
            var goods = GoodsBusiness.GetAllGoods(goodsType);
            note.Call(ClientCommand.GetGoodsR, goodsType, goods);
        }

        /// <summary>
        /// 获取所有职业信息..
        /// </summary>
        /// <param name="user"></param>
        private void GetRoles(UserNote note)
        {
            var roles = GameConfigAccess.Instance.Find(MainType.Role, string.Empty);
            note.Call(ClientCommand.GetRolesR, true, roles.Select(x => new GameConfigUI(x)));
        }

        /// <summary>
        /// 客户端异常日志
        /// </summary>
        /// <param name="note"></param>
        private void ClientError(UserNote note)
        {
            if (note.Player == null) return;
            string msg = note.GetString(0);
            logger2.Warn(string.Format("ID:{0},Name:{1},Msg:{2}", note.PlayerID, note.Player.Name, msg));
        }

        private void ClinetReport(UserNote note)
        {
            if (note.Player == null) return;
            string msg = note.GetString(0);
            logger.Info(string.Format("ID:{0},Name:{1},Msg:{2}", note.PlayerID, note.Player.Name, msg));
        }
        #endregion
    }
}
