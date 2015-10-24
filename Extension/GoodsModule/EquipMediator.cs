using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;


namespace Sinan.GoodsModule
{
    sealed public class EquipMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                GoodsCommand.Ruin,
                GoodsCommand.GoodsWashing,
                GoodsCommand.LotteryAward
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
            // 需验证玩家登录的操作.....
            if (note.Player == null) return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            switch (note.Name)
            {
                case GoodsCommand.Ruin:
                    Ruin(note);
                    break;

                case GoodsCommand.GoodsWashing:
                    GoodsWashing(note);
                    break;

                case GoodsCommand.LotteryAward:
                    LotteryAward(note);
                    break;
            }
        }

        /// <summary>
        /// 道具及装备的销毁
        /// </summary>
        private void Ruin(UserNote note)
        {
            string key = note.GetString(0);
            int p = note.GetInt32(1);
            //得到普通包袱
            PlayerEx burden = note.Player.Value[key] as PlayerEx;

            if (burden == null)
            {
                note.Call(GoodsCommand.RuinR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }

            Variant con = BurdenManager.BurdenPlace(burden, p);
            if (con == null || string.IsNullOrEmpty(con.GetStringOrDefault("E")))
            {
                note.Call(GoodsCommand.RuinR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }

            string goodsid = con.GetStringOrDefault("E");

            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);

            if (g != null)
            {
                Variant v = g.Value;
                if (v.GetIntOrDefault("IsSmash") == 0)
                {
                    //表示该装备不能销毁
                    note.Call(GoodsCommand.RuinR, false, TipManager.GetMessage(GoodsReturn.NoRuin));
                    return;
                }
                //删除指定物品
                GoodsAccess.Instance.Remove(g.ID, note.PlayerID);
            }
            else
            {
                GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
                if (gc != null)
                {
                    Variant v = gc.Value;
                    if (v.GetIntOrDefault("IsSmash") == 0)
                    {
                        //表示该装备不能销毁
                        note.Call(GoodsCommand.RuinR, false, TipManager.GetMessage(GoodsReturn.NoRuin));
                        return;
                    }
                }
            }
            //道具
            if (!note.Player.RemoveGoods(p, GoodsSource.Ruin, true))
            {
                note.Call(GoodsCommand.RuinR, false, TipManager.GetMessage(GoodsReturn.NoRuin));
                return;
            }
            note.Call(GoodsCommand.RuinR, true, p);
            note.Player.UpdateBurden(key);
            //string gid = con.GetStringOrDefault("G");
            //int number = con.GetIntOrDefault("A");
            //BurdenManager.BurdenClear(con);
            //burden.Save();

            //note.Call(GoodsCommand.RuinR, true, p);
            //note.Player.UpdateBurden(key);
            //note.Player.UpdateTaskGoods(gid);

            //note.Player.AddLog(Log.Actiontype.GoodsUse, gid, number, GoodsSource.Ruin, "", 0);
        }

        /// <summary>
        /// 装被洗点
        /// </summary>
        /// <param name="note"></param>
        private void GoodsWashing(UserNote note)
        {
            int p = note.GetInt32(0);
            //那个不需要洗点
            IList list = note[1] as IList;
            if (list == null)
            {
                list = new List<string>();
            }

            string npcid = note.GetString(2);
            if (!note.Player.EffectActive(npcid, ""))
                return;

            PlayerEx b0 = note.Player.B0;
            //保存属性需要的道具
            string goodsid = "G_d000686";


            Variant v = BurdenManager.BurdenPlace(b0, p);
            if (v == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }

            Goods g = GoodsAccess.Instance.FindOneById(v.GetStringOrDefault("E"));
            if (g == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.NoGoodsInfo));
                return;
            }



            Variant msg = g.Value;
            Variant affix = msg.GetVariantOrDefault("Affix");
            if (affix == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing1));
                return;
            }

            if (list.Count >= affix.Count)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing7));
                return;
            }

            int num = 0;

            if (list.Count > 0)
            {
                num = Convert.ToInt32(0.5 * (Math.Pow(list.Count, 2) + list.Count));
                if (BurdenManager.GoodsCount(b0, goodsid) < num)
                {
                    note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing4));
                    return;
                }
                foreach (string item in list)
                {
                    if (!affix.ContainsKey(item))
                    {
                        note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing5));
                        return;
                    }
                }
            }
            GameConfig gc = GameConfigAccess.Instance.FindOneById(g.GoodsID);
            if (gc == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing1));
                return;
            }

            Variant tmp = gc.Value.GetVariantOrDefault("Affix");
            if (tmp == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing1));
                return;
            }

            Variant life = tmp.GetVariantOrDefault("Life");
            if (life == null)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing1));
                return;
            }

            int power = Convert.ToInt32(5 * Math.Pow(msg.GetIntOrDefault("Level"), 2) + 20);
            if (note.Player.StarPower < power)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing6));
                return;
            }

            int count = affix.Count;
            if (life.Count < count)
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing1));
                return;
            }

            int score = GameConfigAccess.Instance.GetPrice(gc.ID);
            if (list.Count > 0)
            {
                if (BurdenManager.GoodsCount(note.Player.B0, goodsid) == 0)
                {
                    note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing4));
                    return;
                }
            }

            if (!note.Player.AddStarPower(-power, FinanceType.GoodsWashing))
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing6));
                return;
            }

            if (!note.Player.AddScore(-score, FinanceType.GoodsWashing))
            {
                note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing3));
                return;
            }



            
            if (list.Count > 0)
            {
                if (num > 0)
                {
                    if (!note.Player.RemoveGoods(goodsid, num, GoodsSource.GoodsWashing))
                    {
                        note.Call(GoodsCommand.GoodsWashingR, false, TipManager.GetMessage(GoodsReturn.GoodsWashing4));
                        return;
                    }                    
                }

                note.Player.UpdateBurden();
            }
 
            if (list.Count > 0)
            {
                Variant gv = new Variant();
                foreach (string item in list)
                {
                    if (!affix.ContainsKey(item))
                        continue;
                    gv.Add(item, affix.GetIntOrDefault(item));
                }
                affix.Clear();
                //affix = gv;
                foreach (var item in gv)
                {
                    affix.Add(item.Key, item.Value);
                }
            }
            else
            {
                affix.Clear();
            }

            Washing(life, affix, count);
            g.Save();
            note.Call(GoodsCommand.GoodsWashingR, true, g.ID);
            note.Player.FinishNote(FinishCommand.Purification);
        }

        /// <summary>
        /// 回调
        /// </summary>
        /// <param name="life"></param>
        /// <param name="affix"></param>
        /// <param name="count"></param>
        private void Washing(Variant life, Variant affix, int count)
        {
            if (affix.Count >= count)
                return;
            int a = 0;
            foreach (var item in life)
            {
                Variant t = item.Value as Variant;
                double range = t.GetDoubleOrDefault("Range");
                if (range > 0)
                {
                    a++;
                }

                if (affix.ContainsKey(item.Key))
                    continue;               
                if (NumberRandom.RandomHit(range))
                {
                    //成功
                    int min = t.GetIntOrDefault("Min");
                    int max = t.GetIntOrDefault("Max") + 1;
                    affix.Add(item.Key, NumberRandom.Next(min, max));
                    if (affix.Count >= count)
                        return;
                }
            }
            //判断数据
            if (a < count)
                return;
            if (affix.Count < count)
            {
                Washing(life, affix, count);
            }
        }

        /// <summary>
        /// 取得抽取的奖励
        /// </summary>
        /// <param name="note"></param>
        private void LotteryAward(UserNote note)
        {
            PlayerEx ly = note.Player.Lottery;
            Variant v = ly.Value;
            if (v == null || v.Count == 0)
            {
                note.Call(GoodsCommand.LotteryAwardR, false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                return;
            }
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();

            Variant tmp = null;
            foreach (var item in v)
            {
                Variant t = item.Value as Variant;
                if (t.GetIntOrDefault("L") == 1)
                {
                    tmp = t;
                    break;
                }
            }

            if (tmp != null)
            {
                string gid = tmp.GetStringOrDefault("G");
                int h = tmp.GetIntOrDefault("H");
                int a = tmp.GetIntOrDefault("A");
                Variant goods = new Variant();
                goods.SetOrInc("Number" + h, a);
                dic.Add(gid, goods);
                if (BurdenManager.IsFullBurden(note.Player.B0, dic))
                {
                    note.Call(GoodsCommand.LotteryAwardR, false, TipManager.GetMessage(GoodsReturn.BurdenB0Full));
                    return;
                }
                note.Player.AddGoods(dic, GoodsSource.LotteryAward);
            }
            v.Clear();
            ly.Save();
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(ly));
            note.Call(GoodsCommand.LotteryAwardR, true, TipManager.GetMessage(GoodsReturn.Lottery2));
        }
    }
}
