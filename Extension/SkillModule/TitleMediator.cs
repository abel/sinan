using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;

namespace Sinan.TitleModule
{
    /// <summary>
    /// 称号...
    /// </summary>
    sealed public class TitleMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                TitleCommand.GetTitle,
                TitleCommand.GetTitles,
                TitleCommand.SetTitle,
                TitleCommand.MyTitle,
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
                case TitleCommand.GetTitles:
                    GetTitles(note);
                    return;
                case TitleCommand.GetTitle:
                    GetTitle(note);
                    return;
                case TitleCommand.SetTitle:
                    SetTitle(note);
                    return;
                case TitleCommand.MyTitle:
                    MyTitle(note);
                    return;
                default:
                    return;
            }
        }
        #endregion

        #region 处理客户端的调用

        /// <summary>
        /// 获取所有称号信息
        /// </summary>
        /// <param name="note"></param>
        private void GetTitles(UserNote note)
        {
            List<GameConfig> titles;
            if (note.Count > 0)
            {
                object[] ids = note[0] as object[];
                if (ids != null && ids.Length > 0)
                {
                    titles = GameConfigAccess.Instance.FindByIDList(ids);
                    note.Call(TitleCommand.GetTitlesR, true, titles.Select(x => new GameConfigUI(x)));
                    return;
                }
            }
            titles = GameConfigAccess.Instance.Find(MainType.Title, string.Empty);
            note.Call(TitleCommand.GetTitlesR, true, titles.Select(x => new GameConfigUI(x)));
        }

        /// <summary>
        /// 获取单个称号的信息
        /// </summary>
        /// <param name="note"></param>
        private void GetTitle(UserNote note)
        {
            string id = note.GetString(0);
            var skill = GameConfigAccess.Instance.FindOneById(id);
            note.Call(TitleCommand.GetTitleR, true, skill);
        }

        private void MyTitle(UserNote note)
        {
            var player = note.Player;
            player.Call(TitleCommand.MyTitleR, true, player.Value["Title"]);
        }

        private void SetTitle(UserNote note)
        {
            var player = note.Player;
            string prefix = note.GetString(0);
            string suffix = note.GetString(1);
            player.SetTitle(prefix, suffix);
            player.Call(TitleCommand.SetTitleR, true);
        }
        #endregion
    }
}
