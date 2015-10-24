using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FamilyModule
{
    public class FamilyBossBusiness
    {
        /// <summary>
        /// 查看家族Boss
        /// </summary>
        /// <param name="note"></param>
        public static void BossList(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string fid = player.Family.Value.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(fid))
            {
                player.Call(FamilyCommand.BossListR, false, TipManager.GetMessage(FamilyReturn.NoAddFamily));
                return;
            }

            var list = FamilyBossAccess.Instance.FindBoss(fid, player.ID);
            player.Call(FamilyCommand.BossListR, true, list);
        }

        /// <summary>
        /// 招唤Boss
        /// </summary>
        /// <param name="note"></param>
        public static void SummonBoss(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string fid = player.Family.Value.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(fid))
            {
                player.Call(FamilyCommand.BossListR, false, TipManager.GetMessage(FamilyReturn.NoPowerSummonBoss));
                return;
            }

            int roleID = player.Family.Value.GetIntOrDefault("FamilyRoleID", -1);
            if (roleID == 0 || roleID == 1)
            {
                //0族长/1副族长
            }
            else
            {
                player.Call(FamilyCommand.BossListR, false, TipManager.GetMessage(FamilyReturn.NoPowerSummonBoss));
                return;
            }

            string bid = note.GetString(0);
            var p = GameConfigAccess.Instance.FindOneById(bid);
            if (p == null)
            {
                player.Call(FamilyCommand.SummonBossR, true, bid);
                return;
            }

            //检查条件
            bool result = FamilyBossAccess.Instance.SummonBoss(fid, bid);
            if (result)
            {
                //通知所有家族成员
                PlayersProxy.CallFamily(player.FamilyName, FamilyCommand.SummonBossR, new object[] { true, bid });
            }
            else
            {
                player.Call(FamilyCommand.SummonBossR, true, bid);
            }
        }

        /// <summary>
        /// 跟Boss战斗
        /// </summary>
        /// <param name="note"></param>
        public static void FightBoss(UserNote note)
        {
            PlayerBusiness player = note.Player;
            string fid = player.Family.Value.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(fid))
            {
                player.Call(FamilyCommand.BossListR, false, TipManager.GetMessage(FamilyReturn.NoPowerFightBoss));
                return;
            }

            int roleID = player.Family.Value.GetIntOrDefault("FamilyRoleID", -1);
            if (roleID == 0 || roleID == 1)
            {
                //0族长/1副族长
            }
            else
            {
                player.Call(FamilyCommand.BossListR, false, TipManager.GetMessage(FamilyReturn.NoPowerFightBoss));
                return;
            }

            string bid = note.GetString(0);
            var p = GameConfigAccess.Instance.FindOneById(bid);
            if (p == null)
            {
                return;
            }

            bool result = FamilyBossAccess.Instance.TryFight(fid + bid);
            if (result)
            {
                List<PlayerBusiness> players = FightBase.GetPlayers(note.Player);
                FightObject[] teamA = FightBase.CreateFightPlayers(players);
                FightType fType = FightType.FamilyBoss;

                FightObject[] teamB = FightBase.CreateApcTeam(players, fType, p.Value.GetVariantOrDefault("APC"));

                if (players.Count == 0 || teamB.Length == 0)
                {
                    foreach (var v in players)
                    {
                        v.SetActionState(ActionState.Standing);
                    }
                    return;
                }

                FightBusiness fb = new FightBusinessFamilyBoss(teamA, teamB, players, bid, fid);

                foreach (var member in players)
                {
                    member.Fight = fb;
                }
                fb.SendToClinet(FightCommand.StartFight, (int)fType, teamA, teamB);
                fb.Start();
                return;
            }
            note.Call(FamilyCommand.FightBossR, false, result);
            return;
        }

        /// <summary>
        /// 领取奖励
        /// </summary>
        /// <param name="note"></param>
        public static void BossAward(UserNote note)
        {
            string fid = note.Player.Family.Value.GetStringOrDefault("FamilyID");
            if (string.IsNullOrEmpty(fid))
            {
                note.Call(FamilyCommand.BossAwardR, false, TipManager.GetMessage(FamilyReturn.NoAddFamily));
                return;
            }

            string bid = note.GetString(0);
            var p = GameConfigAccess.Instance.FindOneById(bid);
            if (p == null)
            {
                return;
            }

            string gid = note.GetString(1); //FamilyBossAccess.Instance.ViewAward(fid + bid, note.Player.ID);
            if (string.IsNullOrEmpty(gid))
            {
                return;
            }

            string[] tt = gid.Split('$');
            int number = 1;
            if (tt.Length != 2 || (!int.TryParse(tt[1], out number)))
            {
                number = 1;
            }
            string goodsid = tt[0];

            //TODO:检查包袱
            Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
            Variant tmp = new Variant();
            tmp.Add("Number1", number);
            goods.Add(goodsid, tmp);
            if (BurdenManager.GetBurdenSpace(note.Player.B0.Value.GetValue<IList>("C")) == null)
            {
                note.Call(FamilyCommand.BossAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward4));
                return;
            }

            //if (BurdenManager.IsFullBurden(note.Player.B0, goods))
            //{
            //    note.Call(FamilyCommand.BossAwardR, false, TipManager.GetMessage(ActivityReturn.SignAward4));
            //    return;
            //}

            if (FamilyBossAccess.Instance.GetAward(fid + bid, note.Player.ID, gid))
            {
                //发放物品
                note.Player.AddGoods(goods, GoodsSource.FamilyBoss);
                note.Call(FamilyCommand.BossAwardR, true, gid);
            }
            else
            {
                //已领取..
                note.Call(FamilyCommand.BossAwardR, false, TipManager.GetMessage(FamilyReturn.CannotBossAward));
            }
        }
    }
}
