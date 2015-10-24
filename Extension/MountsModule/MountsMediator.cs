using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Observer;
using Sinan.FrontServer;
using Sinan.Core;
using Sinan.Command;
using Sinan.MountsModule.Bussiness;

namespace Sinan.MountsModule
{
    public class MountsMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                MountsCommand.InOutMounts,
                MountsCommand.MountsSkillChange,
                MountsCommand.MountsUp,
                MountsCommand.MountsSkillUp,
                TaskCommand.FightingTask
            };
        }
        public override void Execute(INotification notification)
        {
            if (notification.Name == Application.APPSTART)
            {
                //加载任务
                return;
            }
            UserNote note = notification as UserNote;
            if (note != null)
            {
                ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            switch (note.Name)
            {
                case MountsCommand.InOutMounts:
                    MountsBussiness.InOutMounts(note);
                    break;
                case MountsCommand.MountsSkillChange:
                    MountsBussiness.MountsSkillChange(note);
                    break;
                case MountsCommand.MountsUp:
                    MountsBussiness.MountsUp(note);
                    break;
                case MountsCommand.MountsSkillUp:
                    MountsBussiness.MountsSkillUp(note);
                    break;     
                case TaskCommand.FightingTask:
                    MountsBussiness.MountSkilling(note);
                    break;
            }
        }
    }
}
