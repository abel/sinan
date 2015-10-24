using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 战斗信息
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 换宠
        /// </summary>
        /// <param name="fighter"></param>
        protected void ChangePet(FightPlayer fighter)
        {
            string targetID = fighter.Action.Target;
            FightPet pet = fighter.Team.FirstOrDefault(x => x.ID == targetID) as FightPet;

            if (pet == null || pet.Player != fighter.Player)
            {
                return;
            }

            string newPetID = fighter.Action.Parameter;

     
            if (fighter.Player.ChangePet(newPetID, fighter.Player.Mounts))
            {
                //产生新的战斗宠.
                pet.ChangePet(m_changeLife);
            }
        }
    }
}
