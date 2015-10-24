
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;

#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.FrontServer
{
    /// <summary>
    /// 竞技场
    /// </summary>
    public abstract class SceneArena : SceneBusiness
    {
        /// <summary>
        /// 所有参战的宠物
        /// 参战的所有角色，宠物信息
        /// </summary>
        protected readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Pet>> m_pets;

        public SceneArena(GameConfig scene)
            : base(scene)
        {
            m_showAll = true;
            m_pets = new ConcurrentDictionary<string, ConcurrentDictionary<string, Pet>>();
        }

        protected SceneArena(SceneArena scene)
            : base(scene)
        {
            m_showAll = true;
            m_pets = new ConcurrentDictionary<string, ConcurrentDictionary<string, Pet>>();
        }

        protected override void ExecuteUserNote(UserNote note)
        {
            switch (note.Name)
            {
                //case ClientCommand.WalkTo:
                //    WalkTo(note);
                //    return;
                //case LoginCommand.PlayerLogin:
                //    PlayerLogin(note);
                //    return;
                case ClientCommand.IntoSceneSuccess:
                    IntoSceneSuccess(note);
                    return;
                case ClientCommand.ExitScene:
                    ExitScene(note.Player);
                    return;
                case ClientCommand.UserDisconnected:

                    Disconnected(note.Player);
                    return;

                default:
                    return;
            }
        }

    }
}
