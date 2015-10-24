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
    /// <summary>
    /// 装备熔炼
    /// </summary>
    sealed public class EquipMeltMediator : AysnSubscriber
    {
        public override IList<string> Topics()
        {
            return new string[]
            {
                //MeltCommand.MeltInfo,
                //MeltCommand.Melt,
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
            if (note.Player == null)
                return;
            //表示角色正在战斗中，客户端提交信息不进行处理
            if (note.Player.AState == ActionState.Fight)
                return;
            switch (note.Name)
            {
                case MeltCommand.MeltInfo:
                    MeltInfo(note);
                    break;
                case MeltCommand.Melt:
                    Melt(note);
                    break;
            }
        }

        /// <summary>
        /// 得到道具熔炼配置信息[完成]
        /// </summary>
        /// <param name="note"></param>
        private void MeltInfo(UserNote note)
        {
            string goodsid = note.GetString(0);
            Goods g = GoodsAccess.Instance.FindOneById(goodsid);
            if (g == null)
            {
                note.Call(MeltCommand.MeltInfoR, false, TipManager.GetMessage(MeltReturn.MeltEquipNo));
                return;
            }
            //不能改造
            if (g.Value.GetIntOrDefault("IsChange") == 0)
            {
                note.Call(MeltCommand.MeltInfoR, false, TipManager.GetMessage(MeltReturn.MeltNoChange));
                return;
            }
            //类型不正确
            if (g.Value["GoodsType"].ToString().IndexOf("111") != 0)
            {
                note.Call(MeltCommand.MeltInfoR, false, TipManager.GetMessage(MeltReturn.MeltEquipNo));
                return;
            }


            List<GameConfig> meltList = GameConfigAccess.Instance.Find("Split");

            foreach (GameConfig gc in meltList)
            {
                if ((int)gc.Value["Level"] == (int)g.Value["Level"])
                {
                    //可以熔炼类型列表
                    IList ts = gc.Value["Type"] as IList;
                    foreach (Variant m in ts)
                    {
                        if (m["GoodsType"].ToString() == g.Value["GoodsType"].ToString())
                        {
                            Variant v = new Variant();
                            v.Add("ID", gc.ID);
                            //得到当前装备宝石的信息-1表示没有打孔,0表示打孔没有镶嵌宝石,其它表示宝石ID
                            v.Add("BaoShiInfo", g.Value.GetVariantOrDefault("BaoShiInfo"));
                            //得到的可选道具列表   
                            v.Add("GetGoods", gc.Value["GetGoods"]);
                            //熔炼需要币=买价*0.5,绑定游戏币
                            Variant Price = g.Value["Price"] as Variant;
                            Variant Buy = Price["Buy"] as Variant;

                            v.Add("Score", Math.Round(Convert.ToInt32(Buy["Score"]) * 0.5));
                            note.Call(MeltCommand.MeltInfoR, true, v);
                            return;
                        }
                    }
                }
            }

            note.Call(MeltCommand.MeltInfoR, false, TipManager.GetMessage(MeltReturn.MeltConfigError));
        }

        /// <summary>
        /// 熔炼,主要是将装备销除得到新的道具和取下镶嵌的宝石，只能够选择一颗宝石
        /// </summary>
        /// <param name="note"></param>
        private void Melt(UserNote note)
        {
            //熔练配置信息
            //string id = note.GetString(0);
            //熔炼的道具
            string goodsid = note.GetString(0);
            //选择宝石,-1表示没有选择
            string baoshi = note.GetString(1);

            //熔炼的道具信息
            Goods g = GoodsAccess.Instance.GetGoodsByID(goodsid, note.PlayerID);

            if (g == null)
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltEquipNo));
                return;
            }
            //熔炼物品的绑定状态
            int isBinding = g.Value.GetIntOrDefault("IsBinding");
            if (g.Value.GetIntOrDefault("IsChange") == 0)
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltNoChange));
                return;
            }

            if (g.Value.GetStringOrDefault("GoodsType").IndexOf("111") != 0)
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltEquipNo));
                return;
            }
            Variant Price = g.Value.GetValueOrDefault<Variant>("Price");
            Variant Buy = Price.GetValueOrDefault<Variant>("Buy");
            double Score = Math.Round(Buy.GetIntOrDefault("Score") * 0.5);
            if (Score > note.Player.Score)
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltNoScoreB));
                return;
            }
            string id = string.Empty;
            List<GameConfig> meltList = GameConfigAccess.Instance.Find("Split");
            foreach (GameConfig gcs in meltList)
            {
                if ((int)gcs.Value["Level"] == g.Value.GetIntOrDefault("Level"))
                {
                    //可以熔炼类型列表
                    IList ts = gcs.Value["Type"] as IList;
                    foreach (Variant m in ts)
                    {
                        if (m.GetStringOrDefault("GoodsType") == g.Value.GetStringOrDefault("GoodsType"))
                        {
                            id = gcs.ID;
                            break;
                        }
                    }
                }
            }

            //得到熔炼配置信息
            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltConfigError));
                return;
            }
            //得到的道具
            IList v = gc.Value.GetValue<IList>("GetGoods");

            PlayerEx burden = note.Player.B0;
            IList c = burden.Value.GetValue<IList>("C");
            //包袱空格数
            int space = BurdenManager.BurdenSpace(c);
            //需要空格数 
            int count = baoshi == "-1" ? v.Count : v.Count + 1;

            if (count > space)
            {
                //表示包袱不能存放可得到的物品
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltBurdenFull));
                return;
            }
            List<Variant> list = new List<Variant>();

            Dictionary<string, PlayerEx> bus = new Dictionary<string, PlayerEx>();
            Dictionary<string, int> numberList = new Dictionary<string, int>();
            Dictionary<string, int> binds = new Dictionary<string, int>();
            Variant tmp;
            foreach (Variant m in v)
            {
                //得到的物品
                GameConfig getGoods = GameConfigAccess.Instance.FindOneById(m.GetStringOrDefault("GoodsID"));

                int a = BurdenManager.StactCount(getGoods);
                string name = "B0";
                if (a == 0)
                {
                    note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltEquipError));
                    return;
                }
                PlayerEx b = note.Player.Value[name] as PlayerEx;
                if (!bus.ContainsKey(name)) bus.Add(name, b);

                if (!numberList.ContainsKey(getGoods.ID))
                    numberList.Add(getGoods.ID, m.GetIntOrDefault("Number"));
                else
                    numberList[getGoods.ID] += m.GetIntOrDefault("Number");

                if (!binds.ContainsKey(getGoods.ID)) binds.Add(getGoods.ID, isBinding);


                //TODO:得到的物品检查
                tmp = new Variant();
                tmp.Add("GoodsID", getGoods.ID);
                tmp.Add("Name", getGoods.Name);
                tmp.Add("Count", m.GetIntOrDefault("Number"));
                tmp.Add("IsGet", 1);
                list.Add(tmp);
            }
            if (baoshi != string.Empty)
            {
                GameConfig getBaoShi = GameConfigAccess.Instance.FindOneById(g.Value.GetVariantOrDefault("BaoShiInfo")[baoshi].ToString());
                if (getBaoShi != null)
                {
                    if (!bus.ContainsKey("B0"))
                    {
                        bus.Add("B0", note.Player.B0);
                    }
                    numberList.SetOrInc(getBaoShi.ID, 1);
                    if (!binds.ContainsKey(getBaoShi.ID))
                    {
                        binds.Add(getBaoShi.ID, isBinding);
                    }
                }
            }

            if (BurdenManager.BurdenIsFull(bus, numberList, binds))
            {
                note.Call(MeltCommand.MeltR, false, TipManager.GetMessage(MeltReturn.MeltBurdenFull));
                return;
            }

            BurdenManager.TaskGoodsInsert(bus, numberList, binds, note.PlayerID);


            #region 移除熔炼的装备
            foreach (Variant con in c)
            {
                if (con.GetStringOrDefault("E") == goodsid)
                {
                    tmp = new Variant();
                    tmp.Add("GoodsID", g.GoodsID);
                    tmp.Add("Name", g.Name);
                    tmp.Add("Count", con.GetIntOrDefault("A"));
                    tmp.Add("IsGet", 0);//0表示失去，1表示得到
                    list.Add(tmp);

                    //g.Mode = 1;
                    g.Save();

                    BurdenManager.BurdenClear(con);
                    break;
                }
            }
            #endregion

            burden.Save();
            note.Call(MeltCommand.MeltR, true, list);
            note.Player.UpdateBurden();
        }
    }
}
