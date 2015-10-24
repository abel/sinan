using System.Collections.Generic;
using Sinan.Core;
using Sinan.Observer;

namespace Sinan.RepairModule
{
    /// <summary>
    /// 用于修补数据错误..
    /// </summary>
    sealed public class RepairMediator : AysnSubscriber
    {

        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                Application.APPSTART
            };
        }
        public override void Execute(INotification notification)
        {
            Notification note = notification as Notification;
            if (note != null)
            {
                this.ExecuteNote(note);
            }
        }
        #endregion

        void ExecuteNote(Notification note)
        {
            if (note.Name == Application.APPSTART)
            {
                //ClearName cn = new ClearName();
                //cn.StartPlace();

                MinUserID mi = new MinUserID();
                mi.StartPlace();
            }
        }

    }
}
