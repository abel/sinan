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
using Sinan.Log;

namespace Sinan.GoodsModule
{
    /// <summary>
    /// 装备合成
    /// </summary>
    sealed public class EquipFuseMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                FuseCommand.FuseList,
                FuseCommand.Fuse
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
                case FuseCommand.FuseList:
                    FuseList(note);
                    break;
                case FuseCommand.Fuse:
                    Fuse(note);
                    break;
            }
        }

        /// <summary>
        /// 合成配置列表信息[完成]
        /// </summary>
        /// <param name="note"></param>
        private void FuseList(UserNote note)
        {
            List<Variant> list = new List<Variant>();
            //合成配置
            List<GameConfig> fuseList = GameConfigAccess.Instance.Find("Mix", "Mix");
            if (fuseList != null)
            {
                PlayerEx burden = note.Player.Value["B0"] as PlayerEx;
                IList c = burden.Value.GetValue<IList>("C");

                foreach (GameConfig gc in fuseList)
                {
                    Variant v = new Variant();
                    foreach (var itme in gc.Value)
                    {
                        v[itme.Key] = itme.Value;
                    }
                    //目标物品ID
                    GameConfig goods = GameConfigAccess.Instance.FindOneById(gc.Value["GoodsID"].ToString());
                    if (goods == null)
                        continue;

                    int fuseCount = 0;
                    IList mn = v["MixNeed"] as IList;
                    //提升成功率的物品ID
                    string upid = v["UpID"].ToString();
                    int upCount = 0;
                    //是否存在提升合成成功率的道具
                    bool IsUpGoods = true;
                    foreach (Variant mix in mn)
                    {
                        string goodsid = mix.GetStringOrDefault("GoodsID");//需要道具
                        int number = mix.GetIntOrDefault("Number");//需要数量
                        int m = 0;
                        foreach (Variant con in c)
                        {
                            string g = con.GetStringOrDefault("G");
                            int a = con.GetIntOrDefault("A");
                            if (g == goodsid)
                            {
                                m += a;
                            }
                            if (g == upid && IsUpGoods)
                            {
                                upCount += a;
                            }
                        }
                        if (fuseCount == 0)
                        {
                            //得到初始值
                            fuseCount = m;
                        }
                        //表示已经计算了,不许要重复计算
                        IsUpGoods = false;
                        int s = 0;
                        //可合成数量
                        int r = Math.DivRem(m, number, out s);
                        fuseCount = (fuseCount > r) ? r : fuseCount;
                        if (fuseCount == 0)
                            break;
                    }
                    Variant d = goods.Value["Price"] as Variant;
                    Variant buy = d["Buy"] as Variant;
                    v.Add("ID", gc.ID);
                    v.Add("Name", gc.Name);
                    v.Add("Count", fuseCount);
                    //if (goods.Value["GoodsType"].ToString().IndexOf("111") == 0)
                    //{
                    v.Add("Score", Convert.ToInt32(buy["Score"]));
                    //}
                    v.Add("UpCount", upCount);
                    list.Add(v);
                }
            }
            //得到当前可合成的数量
            note.Call(FuseCommand.FuseListR, list);
        }

        /// <summary>
        /// 合成操作,将多个道具或装备合成新的道具[完成]
        /// </summary>
        /// <param name="note"></param>
        private void Fuse(UserNote note)
        {
            //合成配置的ID
            string mixid = note.GetString(0);
            //合成数量
            int number = note.GetInt32(1);
            //提升成功率的数量
            int upCount = note.GetInt32(2);

            string npcid = note.GetString(3);
            if (!note.Player.EffectActive(npcid, ""))
                return;

            GameConfig gc = GameConfigAccess.Instance.FindOneById(mixid);
            if (gc == null)
            {
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseEquipNo));
                return;
            }

            #region 目标道具基本信息,判断包袱是否满和角色游戏币是否满足
            string upid = gc.Value.GetStringOrDefault("UpID");

            string fuseGoodsID = gc.Value.GetStringOrDefault("GoodsID");
            GameConfig fuseGC = GameConfigAccess.Instance.FindOneById(fuseGoodsID);

            int m = BurdenManager.StactCount(fuseGC);

            if (m == 0)
            {
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseEquipError));
                return;
            }

            int y = 0;
            int s = Math.DivRem(number, m, out y);
            //需要空格子的数量
            int n = (y == 0) ? s : s + 1;
            PlayerEx burden = note.Player.B0;
            IList c = burden.Value.GetValue<IList>("C");
            int grid = 0;
            int upNumber = 0;
            foreach (Variant d in c)
            {
                if (string.IsNullOrEmpty(d.GetStringOrDefault("E")))
                    grid++;
                //提升成功率的道具
                if (upid == d.GetStringOrDefault("G"))
                {
                    upNumber += d.GetIntOrDefault("A");
                }
            }

            if (grid < n)
            {
                //表示包袱格子数不足
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseBurdenFull));
                return;
            }

            if (upNumber < upCount)
            {
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseGoodsLv));
                return;
            }

            int score = 0;//合成所需游戏币

            Variant price = fuseGC.Value.GetValueOrDefault<Variant>("Price");
            Variant buy = price.GetValueOrDefault<Variant>("Buy");

            score = buy.GetIntOrDefault("Score") * number;


            #endregion

            #region 判断合成所需材料是否够
            IList mixNeed = gc.Value.GetValue<IList>("MixNeed");
            int isBinding = 0;
            foreach (Variant d in mixNeed)
            {
                int total = 0;
                int numCount = d.GetIntOrDefault("Number") * number;
                foreach (Variant con in c)
                {
                    if (con.GetStringOrDefault("G") == d.GetStringOrDefault("GoodsID"))
                    {
                        if (total < numCount && con.GetIntOrDefault("H") == 1)
                        {
                            //判断目标道具是否为绑定状态
                            isBinding = 1;
                        }
                        total += con.GetIntOrDefault("A");
                    }
                }

                if (total < numCount)
                {
                    note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseLessNo));
                    return;
                }
            }
            #endregion

            if (note.Player.Score < score || (!note.Player.AddScore(-score, FinanceType.Fuse)))
            {
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseScore));
                return;
            }

            #region 得到当前成功率
            double lv = gc.Value.GetDoubleOrDefault("ChengGongLv");
            if (upCount != 0)
            {
                lv = lv + GoodsAccess.Instance.GetSuccess(upCount);
            }

            
            int ml = note.Player.MLevel;
            Variant mv = MemberAccess.MemberInfo(ml);
            if (mv != null)
            {
                lv *= (1 + mv.GetDoubleOrDefault("FuseLv"));
            }
           

            //表示成功
            bool isSuccess = NumberRandom.RandomHit(lv);


            #endregion



            //消耗道具数量
            Variant us = new Variant();

            #region 移除道具           
            foreach (Variant d in mixNeed)
            {
                int count = d.GetIntOrDefault("Number") * number;
                string gid = d.GetStringOrDefault("GoodsID");
                if (!note.Player.RemoveGoods(gid, count,GoodsSource.Fuse))
                {
                    note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseLessNo));
                    return;
                }

                us.SetOrInc(gid, count);
            }
            #endregion


            if (upCount > 0)
            {
                if (!note.Player.RemoveGoods(upid, upCount,GoodsSource.Fuse))
                {
                    note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseGoodsLv));
                    return;
                }
                us.SetOrInc(upid, upCount);
            }

            //取得的物品
            Variant gs = new Variant();
            if (isSuccess)
            {
                Variant doc = new Variant();
                doc.Add("Number" + isBinding, number);

                Dictionary<string, Variant> goods = new Dictionary<string, Variant>();
                goods.Add(fuseGC.ID, doc);
                note.Player.AddGoods(goods, GoodsSource.Fuse);

                gs.SetOrInc(fuseGC.ID, number);
            }

            note.Player.UpdateBurden();
            if (isSuccess)
            {
                note.Player.FinishNote(FinishCommand.HeChen, 1);
                string str = string.Format(TipManager.GetMessage(FuseReturn.FuseSuccess), fuseGC.Name, number);
                note.Call(FuseCommand.FuseR, true, str);
            }
            else
            {
                note.Call(FuseCommand.FuseR, false, TipManager.GetMessage(FuseReturn.FuseFail));
            }

            Variant os = new Variant();
            os["IsSuccess"] = isSuccess;
            os["Lv"] = lv;
            os["Score"] = -score;
            note.Player.AddLogVariant(Actiontype.Fuse, us, gs, os);
        }
    }
}
