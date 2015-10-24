using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 跟明雷战斗
    /// </summary>
    public class FightSceneApc : FightBusiness
    {
        protected int m_subID;
        protected SceneApc m_apc;

        public FightSceneApc(FightObject[] teamA, FightObject[] teamB, List<PlayerBusiness> players,
            string npcID, bool isEctype, SceneApc apc, int subID) :
            base(teamA, teamB, players, npcID, isEctype)
        {
            m_apc = apc;
            m_subID = subID;
        }

        protected override void GameOver()
        {
            base.GameOver();
            m_apc.ExitFight(m_subID, playerWin);
            if (playerWin)
            {
                m_scene.CallAll(0, ClientCommand.KillApcR, new object[] { m_apc.ID, m_subID });
            }
        }
    }
}
