using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.TaskModule.Business;
using Sinan.Util;
using Sinan.Core;



namespace Sinan.TaskModeule
{
    sealed public class TaskMediator : AysnSubscriber
    {
        
        public override IList<string> Topics()
        {
            return new string[] { 
                UserPlayerCommand.CreatePlayerSuccess,
                LoginCommand.PlayerLoginSuccess,

                TaskCommand.PlayerActivation,
                TaskCommand.TaskFinish,

                TaskCommand.PlayerTaskList,
                TaskCommand.UpdateTask,
                TaskCommand.FightingTask,
                
                TaskCommand.Award,
                TaskCommand.Giveup,
                TaskCommand.IsShow,
                TaskCommand.TaskCollect,
                
                TaskCommand.TaskGoods,
                ClientCommand.UserDisconnected,
                SocialCommand.InvitedFriends,

                FamilyCommand.AddFamily,
                Application.APPSTART
            };
        }
        public override void Execute(INotification notification)
        {                  
            if (notification.Name == Application.APPSTART)
            {
                //加载任务
                GameConfigAccess.Instance.FindTaskInfo();
                return;
            }
            UserNote note = notification as UserNote;
            if (note != null)
            {
 
                this.ExecuteNote(note);
            }
        }

        void ExecuteNote(UserNote note)
        {
            if (note.Name == UserPlayerCommand.CreatePlayerSuccess)
            {
                TaskBusinessBase.TaskActivation(note);
                return;
            }
            // 需验证玩家登录的操作.....
            if (note.Player == null)
                return;

            switch (note.Name)
            {
                case TaskCommand.PlayerActivation:
                    TaskBusinessBase.TaskActivation(note);
                    TaskBusinessBase.UpdateTask(note);
                    break;

                case TaskCommand.PlayerTaskList:
                case LoginCommand.PlayerLoginSuccess:
                    TaskBusinessBase.TaskList(note);
                    break;
                case TaskCommand.UpdateTask:
                    TaskBusinessBase.UpdateTask_1(note);
                    break;
                case TaskCommand.Award:
                    TaskBusinessBase.Award(note);
                    break;
                case TaskCommand.Giveup:
                    TaskBusinessBase.Giveup(note, note.GetString(0));
                    break;
                case TaskCommand.FightingTask:
                    TaskBusinessBase.FightingTask(note);
                    break;
                case TaskCommand.IsShow:
                    TaskBusinessBase.IsShow(note, note.GetString(0));
                    break;
                case TaskCommand.TaskCollect:
                    TaskBusinessBase.TaskCollect(note);
                    break;
                case ClientCommand.UserDisconnected:
                    TaskAccess.Instance.Remove(note.PlayerID);
                    break;
                case TaskCommand.TaskGoods:
                    TaskBusinessBase.GoodsTask(note);
                    break;
                case SocialCommand.InvitedFriends:
                    FinishBusiness.TaskFriends(note);
                    break;
                case FamilyCommand.AddFamily:
                    TaskBusinessBase.ActFamilyTask(note);
                    break;
            }
        } 
    }
}
