using System.Collections.Generic;
using Sinan.ArenaModule.Business;
using Sinan.Command;
using Sinan.Core;
using Sinan.FrontServer;
using Sinan.Observer;

namespace Sinan.ArenaModule
{
    sealed public class ArenaMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[] 
            {
                ArenaCommand.CreateArena,
                ArenaCommand.GetArenaList,
                ArenaCommand.ArenaInto,
                ArenaCommand.PetListArena,
                ArenaCommand.PetInArena,
                ArenaCommand.PetOutArena,
                ArenaCommand.SelectSkill,
                ArenaCommand.ArenaGoods,
                ArenaCommand.ArenaWalk,
                ArenaCommand.ArenaGroupName,
                ArenaCommand.PlayerOutArena,
                ArenaCommand.SceneBase,
                ArenaCommand.ArenaUserCount,
                Application.APPSTART,
                LoginCommand.PlayerLoginSuccess,
                ClientCommand.UserDisconnected
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;
            if (note == null || note.Player == null)
            {
                if (notification.Name == Application.APPSTART)
                {
                    ArenaInfo.RunTime();
                }
                return;
            }
            Execute(note);
        }


        void Execute(UserNote note)
        {
            //表示角色正在战斗中，客户端提交信息不进行处理
            //if (note.Player.AState == ActionState.Fight)
            //    return;

            switch (note.Name)
            {
                case ArenaCommand.CreateArena:
                    ArenaInfo.CreateArena(note);
                    break;
                case ArenaCommand.GetArenaList:
                    ArenaInfo.GetArenaList(note);
                    break;
                case ArenaCommand.ArenaInto:
                    ArenaInfo.ArenaInto(note);
                    break;
                case ArenaCommand.PetListArena:
                    ArenaInfo.PetListArena(note);
                    break;
                case ArenaCommand.PetInArena:
                    ArenaInfo.PetInArena(note);
                    break;
                case ArenaCommand.PetOutArena:
                    ArenaInfo.PetOutArena(note);
                    break;
                case ArenaCommand.SelectSkill:
                    ArenaInfo.SelectSkill(note);
                    break;
                case ArenaCommand.ArenaGoods:
                    ArenaInfo.ArenaGoodsPet(note);
                    break;
                case ArenaCommand.ArenaWalk:
                    ArenaInfo.ArenaWalk(note);
                    break;
                case ArenaCommand.ArenaGroupName:
                    ArenaInfo.ArenaGroupName(note);
                    break;
                case ClientCommand.UserDisconnected:
                    ArenaInfo.PetOutArena(note);
                    break;

                case ArenaCommand.PlayerOutArena:
                    ArenaInfo.PlayerOutArena(note);
                    break;
                case ArenaCommand.SceneBase:
                    ArenaInfo.SceneBase(note);
                    break;
                case ArenaCommand.ArenaUserCount:
                    ArenaInfo.ArenaUserCount(note);
                    break;
            }
        }
    }
}
