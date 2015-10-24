using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;
using Sinan.Extensions;

namespace Sinan.GoodsModule
{

    /// <summary>
    /// 打孔操作
    /// </summary>
    sealed public class EquipPunchMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                PunchCommand.Punch,
                PunchCommand.PunchNeed
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
                case PunchCommand.Punch:
                    Punch(note);
                    break;
                case PunchCommand.PunchNeed:
                    PunchNeed(note);
                    break;
            }
        }

        /// <summary>
        /// 打孔需求[完成]
        /// </summary>
        /// <param name="note"></param>
        private void PunchNeed(UserNote note)
        {
            string goodsid = note.GetString(0);
            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);
            if (g != null)
            {
                if (!g.Value.ContainsKey("IsChange"))
                {
                    note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.PunchNoChange));
                    return;
                }
                if (g.Value.GetIntOrDefault("IsChange") == 0)
                {
                    note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.PunchNoChange));
                    return;
                }
                //if (!g.Value.ContainsKey("BaoShiInfo"))
                //{
                //    note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.NoPunch));
                //    return;
                //}
                Variant baoshiInfo = g.Value.GetValueOrDefault<Variant>("BaoShiInfo");
                if (baoshiInfo == null)
                {
                    note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.NoPunch));
                    return;
                }
                string key = string.Empty;
                for (int i = 0; i < 5; i++)
                {
                    string name = "P" + i;
                    if (!baoshiInfo.ContainsKey(name))
                    {
                        note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.PunchEquipError));
                        return;
                    }

                    if (baoshiInfo.GetStringOrDefault(name) == "-1")
                    {
                        //-1表示没有打孔的位置
                        key = name;
                        break;
                    }
                }
                if (key == string.Empty)
                {
                    //表示孔已经打满
                    note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.PunchFull));
                    return;
                }

                //打孔配置
                GameConfig gc = GameConfigAccess.Instance.FindOneById("Mix_00001");
                int few = Convert.ToInt32(key.Replace("P", string.Empty));
                //Variant Price = g.Value["Price"] as Variant;
                //Variant Buy = Price["Buy"] as Variant;
                //double Score = Convert.ToInt32(Buy["Score"]) * (few + 1) * 2;
                int Score = GetPrice(g.GoodsID, few);
                PlayerEx burden = note.Player.B0;
                IList c = burden.Value.GetValue<IList>("C");
                int number = BurdenManager.BurdenGoodsCount(c, gc.Value.GetStringOrDefault("UpID"));

                Variant v = new Variant();
                //打孔的位置
                v.Add("P", few);
                //打孔需要游戏币
                v.Add("Score", Score);
                //打孔需要相关材料及成功机率
                Variant pn = gc.Value.GetValueOrDefault<Variant>(key);// as Variant;
                v.Add("PunchNeed", pn["PunchNeed"]);
                v.Add("ChengGongLv", pn["ChengGongLv"]);
                v.Add("UpID", gc.Value.GetStringOrDefault("UpID"));
                v.Add("UpCount", number);
                note.Call(PunchCommand.PunchNeedR, true, v);
            }
            else
            {
                note.Call(PunchCommand.PunchNeedR, false, TipManager.GetMessage(PunchReturn.PunchEquipNo));
            }
        }


        /// <summary>
        /// 打孔,主要对装备进行开孔的操作,最多只能够开5个孔[完成]
        /// </summary>
        /// <param name="note"></param>
        private void Punch(UserNote note)
        {
            string goodsid = note.GetString(0);

            //提升成功率使用道具数量
            int number = note.GetInt32(1);


            string npcid = note.GetString(2);
            if (!note.Player.EffectActive(npcid, ""))
                return;
            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);
            if (g == null)
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchEquipNo));
                return;
            }
            if (g.Value.GetIntOrDefault("IsChange") == 0)
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchNoChange));
                return;
            }

            Variant baoShiInfo = g.Value.GetValueOrDefault<Variant>("BaoShiInfo");
            if (baoShiInfo == null)
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.NoPunch));
                return;
            }
            string key = string.Empty;
            for (int i = 0; i < 5; i++)
            {
                string name = "P" + i;
                if (!baoShiInfo.ContainsKey(name))
                {
                    note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchEquipError));
                    return;
                }

                if (baoShiInfo.GetStringOrDefault(name) == "-1")
                {
                    //-1表示没有打孔的位置
                    key = name;
                    break;
                }
            }

            if (string.IsNullOrEmpty(key))
            {
                //表示孔已经打满
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchFull));
                return;
            }

            int few = Convert.ToInt32(key.Replace("P", string.Empty));

            int score = GetPrice(g.GoodsID, few);



            PlayerEx burden = note.Player.B0;

            IList c = burden.Value.GetValue<IList>("C");

            //打孔配置
            GameConfig gc = GameConfigAccess.Instance.FindOneById("Mix_00001");
            Variant p = gc.Value.GetValueOrDefault<Variant>(key);//as Variant;
            if (p == null) return;
            //提升成功率物品
            string upID = gc.Value.GetStringOrDefault("UpID");
            //打孔需要的物品
            IList punchNeed = p.GetValue<IList>("PunchNeed");
            bool cha = true;

            //失去道具
            List<Variant> lost = new List<Variant>();
            Variant miss;
            foreach (Variant pn in punchNeed)
            {
                int n = 0, m = 0;

                string ngid = pn.GetStringOrDefault("GoodsID");
                foreach (Variant con in c)
                {
                    if (con.GetStringOrDefault("G") == ngid)
                    {
                        n += con.GetIntOrDefault("A");
                    }

                    //判断提高成功率的道具
                    if (cha && number > 0)
                    {
                        if (con.GetStringOrDefault("G") == upID)
                        {
                            m += con.GetIntOrDefault("A");
                        }
                    }
                }

                if (cha && number > m && number > 0)
                {
                    note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchNeedGoodsNo));
                    return;
                }

                cha = false;
                if (n < pn.GetIntOrDefault("Number"))
                {
                    note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchNeedGoodsNo));
                    return;
                }

                miss = new Variant();
                miss.Add("G", ngid);
                miss.Add("A", pn.GetIntOrDefault("Number"));
                miss.Add("IsSet", 0);
                lost.Add(miss);
            }
            if (number > 0)
            {
                miss = new Variant();
                miss.Add("G", upID);
                miss.Add("A", number);
                lost.Add(miss);
            }

            //原始成功率
            double chengGongLv = p.GetDoubleOrDefault("ChengGongLv");
            //当前成功率
            double curLv = chengGongLv;
            if (number > 0)
            {
                curLv += GoodsAccess.Instance.GetSuccess(number);
            }


            int num = Convert.ToInt32(curLv);

            Variant tmp = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == g.ID)
                {
                    tmp = v;
                    break;
                }
            }
            if (tmp == null)
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchEquipError));
                return;
            }

            if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.Punch)))
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchScoreNo));
                return;
            }


            bool isSuccess = NumberRandom.RandomHit(curLv);
            if (isSuccess)
            {
                //表示成功
                //isSuccess = true;
                g.Value.GetVariantOrDefault("BaoShiInfo")[key] = "0";
                g.Save();
            }

            #region 移除消除物品
            foreach (Variant item in lost)
            {
                string t = item.GetStringOrDefault("G");
                int r = item.GetIntOrDefault("A");
                if (!note.Player.RemoveGoods(t, r, GoodsSource.Punch))
                {
                    note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchNeedGoodsNo));
                    return;
                }
            }
            #endregion

            burden.Save();
            note.Player.UpdateBurden();
            if (isSuccess)
            {
                note.Call(PunchCommand.PunchR, true, goodsid);
            }
            else
            {
                note.Call(PunchCommand.PunchR, false, TipManager.GetMessage(PunchReturn.PunchFail));
            }

        }

        /// <summary>
        /// 得到物品当前价格
        /// </summary>
        /// <param name="goodsid">道具ID</param>
        /// <returns></returns>
        private int GetPrice(string goodsid, int few)
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
            if (gc != null)
            {
                Variant Price = gc.Value.GetValueOrDefault<Variant>("Price");
                if (Price != null)
                {
                    Variant buy = Price.GetValueOrDefault<Variant>("Buy");
                    if (buy != null)
                    {
                        return Convert.ToInt32(buy.GetIntOrDefault("Score") * Math.Pow((few + 1), 2) * 0.1);
                    }
                }
            }
            return 0;
        }
    }
}
