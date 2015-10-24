using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Observer;
using Sinan.Log;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 开宝箱
    /// </summary>
    sealed public class BoxMediator : AysnSubscriber
    {
        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                ClientCommand.OpenBox,
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
            if (note.Player.Scene == null) return;
            switch (note.Name)
            {
                case ClientCommand.OpenBox:
                    OpenBox(note);
                    return;
                default:
                    return;
            }
        }
        #endregion

        void OpenBox(UserNote note)
        {
            BoxBusiness box;
            if (!BoxProxy.TryGetBox(note.GetString(0), out box))
            {
                return;
            }
            PlayerBusiness player = note.Player;
            bool open = box.OpenBox(player);
            if (open)
            {
                //更新
                box.Reset();
                List<BoxBusiness> boxs = BoxProxy.GetSceneBox(player.SceneID);
                player.Scene.CallAll(player.ShowID, ClientCommand.RefreshBoxR, new object[] { player.SceneID, boxs });

                //守护战争开启宝箱
                ScenePro sp = player.Scene as ScenePro;
                if (sp != null)
                {
                    PlayersProxy.CallAll(ActivityCommand.OpenChestR, new object[] { note.Player.Name });
                }
            }
        }

    }
}
