using System;
using System.Collections;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.PetsModule.Business;
using Sinan.Util;

namespace Sinan.PetsModule
{
    partial class PetMediator
    {
        // 吸星技能ID
        const string AbsorbSkillID = "S_xx001";

        /// <summary>
        /// 吸星魔法
        /// </summary>
        /// <param name="note"></param>
        public void Absorb(UserNote note)
        {
            string playerid = note.GetString(0); //被吸人ID
            string soleid = note.GetString(1);   //被吸宠,正在放养的宠物

            PlayerBusiness player = note.Player;

            //吸星魔法等级
            IList levels = player.PetBook.Value.GetValueOrDefault<IList>(AbsorbSkillID);
            if (levels == null || levels.Count == 0)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.Absorb1));
                return;
            }
            int level = Convert.ToInt32(levels[levels.Count - 1]);

            //正在放养的宠物
            Pet p = PetAccess.Instance.FindOneById(soleid);
            if (p == null)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            ////用于吸星的宠物不能高于角色5级
            //if (p.Value.GetIntOrDefault("PetsLevel") - 5 > note.Player.Level)
            //{
            //    note.Call(PetsCommand.PetAbsorbR, false, string.Empty, "用于吸星的宠物不能高于角色5级");
            //    return;
            //}

            //被吸宠当前生命值
            int hp = p.Value.GetValueOrDefault<Variant>("ShengMing").GetIntOrDefault("V");
            if (hp < 1)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.StockingNo));
                return;
            }

            //放养宠主人
            PlayerBusiness loster = PlayersProxy.FindPlayerByID(playerid);
            if (loster == null)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            PlayerEx b2 = loster.B2;
            IList c = b2.Value.GetValue<IList>("C");
            Variant m = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == soleid)
                {
                    m = v;
                    break;
                }
            }
            if (m == null)
            {
                //宠物不在家园中
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            //可以得到的奖励
            Variant award = m.GetValueOrDefault<Variant>("T");
            if (award == null)
            {
                //奖励不存在 
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }

            //是否到领奖时间
            DateTime endTime = award.GetDateTimeOrDefault("EndTime");
            if (DateTime.UtcNow < endTime)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoCareTime));
                return;
            }

            //收益比
            double xishoulv = PetXiShou(note, p, level);
            if (xishoulv <= 0) return;

            int score = (int)(award.GetIntOrDefault("Score") * xishoulv);
            int exp1 = (int)(award.GetIntOrDefault("P1exp") * xishoulv);
            int exp2 = (int)(award.GetIntOrDefault("P2exp") * xishoulv);

            award["Score"] = score;
            award["P1exp"] = exp1;
            award["P2exp"] = exp2;

            //获取奖励
            if (score > 0)
            {
                player.AddScore(score, FinanceType.StockingAward0);
            }
            if (exp1 > 0)
            {
                player.AddExperience(exp1, FinanceType.StockingAward0);
            }
            if (exp2 > 0)
            {
                player.AddPetExp(player.Pet, exp2, true, (int)FinanceType.StockingAward0);
            }

            award.Remove("EndTime");
            b2.Save();
            player.Call(PetsCommand.PetAbsorbR, true, PetAccess.Instance.GetPetModel(m), award);

            string msg = string.Format(TipManager.GetMessage(PetsReturn.Absorb2),
                DateTime.Now, p.Name, note.Player.Name);
            loster.AddBoard(msg);

            //添加吸星日志
            LogVariant log = new LogVariant(ServerLogger.zoneid, Actiontype.Absorb);
            log.Value["Loster"] = loster.PID; //被吸者
            log.Value["Score"] = score;       //石币
            log.Value["P1exp"] = exp1;        //角色经验
            log.Value["P2exp"] = exp2;        //战宠经验
            log.Value["Lv"] = xishoulv;       //吸收率
            player.WriteLog(log);
        }

        /// <summary>
        /// 吸星魔法
        /// </summary>
        /// <param name="note"></param>
        /// <param name="petid">进攻宠物的ID</param>
        /// <param name="p">放养的宠物</param>
        private double PetXiShou(UserNote note, Pet p, int level)
        {
            //取技能配置
            GameConfig gc = GameConfigAccess.Instance.FindOneById(AbsorbSkillID);
            if (gc == null)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoSkill));
                return 0;
            }
            Variant config = gc.Value.GetVariantOrDefault(level.ToString());
            if (config == null)
            {
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, TipManager.GetMessage(PetsReturn.NoSkill));
                return 0;
            }

            //每天可以使用吸星魔法上限
            int maxcount = gc.UI.GetIntOrDefault("MaxUse");
            if (maxcount <= 0)
            {
                maxcount = 30;
            }
            if (note.Player.ReadDaily(PlayerBusiness.DailyOther, "Xi") >= maxcount)
            {
                //每天可以使用吸星魔法上限为"+maxcount+"次
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, string.Format(TipManager.GetMessage(PetsReturn.Absorb3), maxcount));
                return 0;
            }
            int count = maxcount - note.Player.WriteDaily(PlayerBusiness.DailyOther, "Xi");

            //成功率
            double su = config.GetDoubleOrDefault("P");
            //double su = 0.15 + 0.05 * (level);
            if (!Sinan.Extensions.NumberRandom.RandomHit(su))
            {
                //"吸星失败！无收益。今日还可以使用吸星魔法" + count + "次。"
                note.Call(PetsCommand.PetAbsorbR, false, string.Empty, string.Format(TipManager.GetMessage(PetsReturn.Absorb4), count));
                return 0;
            }
            //收益比
            double xishoulv = config.GetDoubleOrDefault("A");
            return xishoulv;
        }

        /// <summary>
        /// 护理
        /// </summary>
        /// <param name="note"></param>
        private void PetNurse(UserNote note)
        {
            string petID = note.GetString(0);   //正在放养的宠物
            PlayerEx b2 = note.Player.B2;
            IList c = b2.Value.GetValue<IList>("C");
            //放养格
            Variant m = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == petID)
                {
                    m = v;
                    break;
                }
            }
            if (m == null)
            {
                //宠物不在家园中
                note.Call(PetsCommand.PetNurseR, false, TipManager.GetMessage(PetsReturn.NoExists));
                return;
            }
            Variant ct = PetAccess.Instance.CreateAward(note.Player.Level, petID, note.PlayerID, note.Player.Pet);
            //PetBusiness.CreateAward(note.Player, award, petID, note.PlayerID);
            
            Variant t = m.GetValueOrDefault<Variant>("T");
            if (t.ContainsKey("ProtectionTime"))
            {
                if (ct == null) 
                    ct = new Variant();
                ct.Add("ProtectionTime", t.GetDateTimeOrDefault("ProtectionTime"));
            }
            m["T"] = ct;
            b2.Save();

            note.Call(PetsCommand.PetNurseR, true, PetAccess.Instance.GetPetModel(m));
        }
    }
}
