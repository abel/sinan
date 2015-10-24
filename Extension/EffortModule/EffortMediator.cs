using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;

namespace Sinan.EffortModule
{
    /// <summary>
    /// 成就
    /// </summary>
    sealed public class EffortMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                EffortCommand.GetEffort,
                EffortCommand.GetEfforts,
                EffortCommand.ViewEffort,
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
                case EffortCommand.GetEfforts:
                    GetEfforts(note);
                    return;
                case EffortCommand.GetEffort:
                    GetEffort(note);
                    return;
                case EffortCommand.ViewEffort:
                    ViewEffort(note);
                    return;
                default:
                    return;
            }
        }

        #endregion

        #region 处理客户端的调用
        /// <summary>
        /// 获取所有成就信息
        /// </summary>
        /// <param name="note"></param>
        private void GetEfforts(UserNote note)
        {
            string subMain = note.GetString(0);
            var efforts = GameConfigAccess.Instance.Find(MainType.Effort, subMain);
            note.Call(EffortCommand.GetEffortsR, true, efforts.Select(x => new GameConfigUI(x)));
        }

        /// <summary>
        /// 获取单个成就的信息
        /// </summary>
        /// <param name="note"></param>
        private void GetEffort(UserNote note)
        {
            string id = note.GetString(0);
            var effort = GameConfigAccess.Instance.FindOneById(id);
            note.Call(EffortCommand.GetEffortR, true, new GameConfigUI(effort));
        }

        /// <summary>
        /// 查看其他用戶的成就
        /// </summary>
        /// <param name="note"></param>
        private void ViewEffort(UserNote note)
        {
            string pid = note.GetString(0);
            PlayerBusiness p;
            if (PlayersProxy.TryGetPlayerByID(pid, out p))
            {
                note.Call(EffortCommand.ViewEffortR, p.ID, p.Effort);
            }
        }
        #endregion
    }
}
