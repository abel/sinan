using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 夺宝奇兵地图
    /// </summary>
    public sealed class SceneRob : ScenePart
    {
        public override SceneType SceneType
        {
            get { return SceneType.Rob; }
        }

        public SceneRob(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
            m_fightType = FightType.RobPK;
        }

        SceneRob(SceneRob scene)
            : base(scene)
        {
            m_showAll = true;
            m_fightType = FightType.RobPK;
        }

        public override SceneBusiness CreateNew()
        {
            return new SceneRob(this);
        }

        protected override void ExecuteUserNote(UserNote note)
        {
            switch (note.Name)
            {
                case ClientCommand.WalkTo:
                    WalkTo(note);
                    return;
                case LoginCommand.PlayerLogin:
                    PlayerLogin(note);
                    return;
                case ClientCommand.IntoSceneSuccess:
                    IntoSceneSuccess(note);
                    return;
                case ClientCommand.ExitScene:
                    ExitScene(note.Player);
                    return;
                case ClientCommand.UserDisconnected:
                    Disconnected(note.Player);
                    return;
                case FightCommand.FightTaskApc:
                    FightTaskApc(note);
                    return;
                case FightCommand.FightPK:
                    FightPK(note);
                    return;
                default:
                    return;
            }
        }

        protected override void IntoSceneSuccess(PlayerBusiness player, Variant sceneinfo)
        {
            base.IntoSceneSuccess(player, sceneinfo);
            var part = (m_part as RobBusiness);
            if (part != null)
            {
                if (player.ID == part.AuraOwner)
                {
                    //元素所有者进入
                    string msg = string.Format(TipManager.GetMessage((int)PartMsgType.AuraCross), player.Name, this.Name);
                    part.Call(PartCommand.AuraChangeR, new object[] { true, part.AuraOwner, part.OwnerName, msg });
                }
                else
                {
                    string msg = string.Format(TipManager.GetMessage((int)PartMsgType.PartCount), part.PartCount());
                    player.Call(PartCommand.AuraChangeR, new object[] { true, part.AuraOwner, part.OwnerName, msg });
                }
            }
        }

        public override bool ExitScene(PlayerBusiness player)
        {
            bool result = base.ExitScene(player);
            if (result)
            {
                m_part.PlayerExit(player);
            }
            return result;
        }

        public override bool Disconnected(PlayerBusiness player)
        {
            if (m_part != null)
            {
                (m_part as RobBusiness).LoseAuraOwner(player, false);
            }
            return base.Disconnected(player);
        }
    }
}
