using System.Collections.Generic;
using System.Linq;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Entity;
using Sinan.Command;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 抓捕
    /// </summary>
    partial class FightBase
    {
        /// <summary>
        /// 抓捕
        /// </summary>
        /// <param name="fighter"></param>
        private void ZhuaPu(FightPlayer fighter)
        {
            string targetID = fighter.Action.Target;
            FightBB bb = m_teamB.FirstOrDefault(x => x.ID == targetID) as FightBB;
            if (bb != null && bb.IsLive)
            {
                ActionResult result = new ActionResult(targetID);
                fighter.Action.Result = new List<ActionResult>() { result };
                fighter.Action.FightCount = FightAction.HasAction;
                m_actions.Add(fighter.Action);

                string petID = bb.APC.Value.GetStringOrDefault("PetID");
                if (string.IsNullOrEmpty(petID))
                {
                    //不可捕捉
                    result.Value["Msg"] = TipManager.GetMessage(ClientReturn.ZhuaPu1);
                    return;
                }

                var pet = GameConfigAccess.Instance.FindOneById(petID);
                if (pet == null) //|| pet.MainType != MainType.Pet
                {
                    //不可捕捉
                    result.Value["Msg"] = TipManager.GetMessage(ClientReturn.ZhuaPu1);
                    return;
                }

                //增加的捕捉机率
                double addP = 0;
                if (!string.IsNullOrEmpty(fighter.Action.Parameter))
                {
                    //扣除一个网..
                    const string clapnetID = "G_d000011";
                    if (BurdenManager.Remove(fighter.Player.B0, clapnetID))
                    {
                        fighter.Player.UseClapnet++;
                        addP = 0.5;
                    }
                    //else
                    //{
                    //    result.Value["Msg"] = "请使用捕兽网";
                    //    return;
                    //}
                }
                double lv = fighter.CP + bb.ClapP + addP;
                Variant mv = MemberAccess.MemberInfo(fighter.Player.MLevel);
                if (mv != null) 
                {
                    lv *=(1+ mv.GetDoubleOrDefault("ZhuaPuLv"));
                }
                if (Sinan.Extensions.NumberRandom.RandomHit(lv))
                {
                    //实行抓捕
                    if (!fighter.Player.AddGoodsNobingOne(petID, GoodsSource.Clap))
                    {
                        //包袱已满
                        result.Value["Msg"] = TipManager.GetMessage(ClientReturn.ZhuaPu2);
                        return;
                    }
                    result.Value["PetID"] = petID;
                    //捕捉成功
                    result.Value["Msg"] = TipManager.GetMessage(ClientReturn.ZhuaPu3);
                    bb.HP = 0;
                }
                else
                {
                    //捕捉失败
                    result.Value["Msg"] = TipManager.GetMessage(ClientReturn.ZhuaPu4);
                    return;
                }
            }
        }

    }
}
