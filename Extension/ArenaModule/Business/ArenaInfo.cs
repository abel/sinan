using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Sinan.ArenaModule.Detail;
using Sinan.ArenaModule.Fight;
using Sinan.Data;
using Sinan.Extensions;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;
#if mono
using Sinan.Collections;
#else
using System.Collections.Concurrent;
#endif

namespace Sinan.ArenaModule.Business
{
    public class ArenaInfo
    {
        static ConcurrentDictionary<string, string> m_dic = new ConcurrentDictionary<string, string>();
        static Timer timer = new Timer(timer_Elapsed);

        /// <summary>
        /// 创建竞技场
        /// </summary>
        /// <param name="note"></param>
        public static void CreateArena(UserNote note)
        {
            string soleid = note.PlayerID + "CreateArena";
            if (!m_dic.TryAdd(soleid, soleid))
                return;
            try
            {
                Variant m = note.GetVariant(0);
                if (m == null)
                {
                    //参数为不能为空
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena1));
                    return;
                }
                ArenaBase model = new ArenaBase();
                model.PlayerID = note.PlayerID;
                model.ArenaID = m.GetStringOrDefault("ID");


                GameConfig gc = GameConfigAccess.Instance.FindOneById(model.ArenaID);
                if (gc == null)
                {
                    //竞技场基本配置不正确
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena2));
                    return;
                }
                Variant v = gc.Value;
                if (v == null)
                {
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena2));
                    return;
                }
                //ArenaConfig.ArenaInfo(gc);

                model.Group = m.GetIntOrDefault("Group");
                //if (!ArenaConfig.IsGroup(model.Group))
                //{
                //    note.Call(ArenaCommand.CreateArenaR, false, "分组数量不正确");
                //    return;
                //}

                //Variant group = v.GetValueOrDefault<Variant>("Group").GetValueOrDefault<Variant>(model.Group.ToString());

                string petlevel = m.GetStringOrDefault("PetLevel");
                //if (!ArenaConfig.IsPetLevel(petlevel))
                //{
                //    note.Call(ArenaCommand.CreateArenaR, false, "请选择宠物等级限制");
                //    return;
                //}

                int petmix = 0;
                int petmax = 0;

                string[] strs = petlevel.Split('-');

                if (!int.TryParse(strs[0], out petmix))
                {
                    //请选择宠物最底等级限制
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena3));
                    return;
                }
                if (!int.TryParse(strs[1], out petmax))
                {
                    //请选择宠物最高等级限制
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena4));
                    return;
                }

                model.PetMin = petmix;
                model.PetMax = petmax;

                model.WarType = m.GetIntOrDefault("WinType");
                if (model.WarType == 0)
                {
                    for (int i = 0; i < model.Group; i++)
                    {
                        model.GroupName.Add(i.ToString());
                    }
                }

                model.IsOtherInto = m.GetBooleanOrDefault("IsOtherInto");
                model.IsWatch = m.GetBooleanOrDefault("IsWatch");

                if (model.IsOtherInto && (!model.IsWatch))
                {
                    //允许中途参战必须可以观战
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena5));
                    return;
                }

                model.IsGoods = m.GetBooleanOrDefault("IsGoods");

                model.WinType = m.GetIntOrDefault("WinType");

                model.Scene = m.GetStringOrDefault("Scene");

                GameConfig sc = GameConfigAccess.Instance.FindOneById(model.Scene);
                if (sc == null)
                {
                    //场景不存在
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena6));
                    return;
                }
                //得到场景信息
                model.SceneSession = new ArenaScene(sc);

                int PrepareTime = m.GetIntOrDefault("PrepareTime");
                model.PrepareTime = PrepareTime;

                int GameTime = m.GetIntOrDefault("GameTime");
                model.GameTime = GameTime;

                DateTime dt = DateTime.UtcNow;

                model.StartTime = dt.AddMinutes(PrepareTime);

                model.EndTime = model.StartTime.AddMinutes(GameTime);
                model.PetNumber = m.GetIntOrDefault("PetNumber");
                model.Name = note.Player.Name;
                //密码设置
                model.PassWord = m.GetStringOrDefault("PassWord");
                model.SoleID = GetAreneID();// Guid.NewGuid().ToString("N");
                model.UserPets = m.GetIntOrDefault("UserPets");
                model.FightPoor = m.GetIntOrDefault("FightPoor");
                if (!ArenaLimit(note, model, v))
                    return;

                ArenaBusiness dal = new ArenaBusiness();
                int a = dal.CreateArenaBase(note.PlayerID, model);
                if (a == 0)
                {
                    PlayersProxy.CallAll(ArenaCommand.CreateArenaR, new object[] { true, note.Player.Name });
                    return;
                }
                if (a == 1)
                {
                    //该竞技场已经存在
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena7));
                    return;
                }
                if (a == 2)
                {
                    //你已经创建有竞技场,不能再创建
                    note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.CreateArena8));
                    return;
                }
            }
            finally
            {
                m_dic.TryRemove(soleid, out soleid);
            }
        }

        /// <summary>
        /// 得到竞技场列表
        /// </summary>
        /// <param name="note"></param>
        public static void GetArenaList(UserNote note)
        {
            List<ArenaDetail> list = new List<ArenaDetail>();
            foreach (ArenaBase v in ArenaBusiness.ArenaList.Values)
            {
                ArenaDetail model = new ArenaDetail(v);
                list.Add(model);
            }
            note.Call(ArenaCommand.GetArenaListR, list);
        }

        /// <summary>
        /// 进入竞技场
        /// </summary>
        /// <param name="note"></param>
        public static void ArenaInto(UserNote note)
        {
            string soleid = note.GetString(0);
            SceneBusiness sb = note.Player.Scene;
            if (sb.SceneType == SceneType.City || sb.SceneType == SceneType.Home || sb.SceneType == SceneType.Outdoor)
            {
                string password = note.GetString(1);
                ArenaBase model;
                if (!ArenaBusiness.ArenaList.TryGetValue(soleid, out model))
                {
                    //竞技场不存在
                    note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto1), soleid);
                    return;
                }
                if (note.Player.Team != null)
                {
                    //组队状态不能参加
                    note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto2), soleid);
                    return;
                }


                if (!string.IsNullOrEmpty(model.PassWord))
                {
                    if (model.PassWord != password)
                    {
                        //进入密码不正确
                        note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto3), soleid);
                        return;
                    }
                }

                if (!model.Players.TryAdd(note.PlayerID, note.Player))
                {
                    //已经进入竞技场,不能重复进入
                    note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto4), soleid);
                    return;
                }
                if (string.IsNullOrEmpty(soleid))
                {
                    //该竞技场不存在
                    note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto5), soleid);
                    return;
                }

                //通知当前场景所有角色，角色属性发生变化
                note.Player.SetActionState(ActionState.Arena);
                note.Player.SoleID = soleid;
                note.Player.GroupName = "";
                List<PetDetail> list = new List<PetDetail>();
                foreach (Pet p in model.Pets.Values)
                {
                    PlayerBusiness user;
                    if (model.Players.TryGetValue(p.PlayerID, out user))
                    {
                        PetDetail detail = new PetDetail(p, user.Name);
                        list.Add(detail);
                    }
                }
                note.Call(ArenaCommand.ArenaIntoR, true, list, soleid);
                model.ArenaUserCount();
            }
            else
            {
                //当前场景不能进入竞技场
                note.Call(ArenaCommand.ArenaIntoR, false, TipManager.GetMessage(ArenaReturn.ArenaInto6), soleid);
                return;

            }
        }

        /// <summary>
        /// 得到宠物列表
        /// </summary>
        /// <param name="note"></param>
        public static void PetListArena(UserNote note)
        {
            PlayerEx b3 = note.Player.B3;
            IList c = b3.Value.GetValue<IList>("C");

            List<PetDetail> list = new List<PetDetail>();

            ArenaBase model;
            if (ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
            {
                foreach (Variant v in c)
                {
                    string petid = v.GetStringOrDefault("E");
                    if (string.IsNullOrEmpty(petid))
                        continue;
                    Pet p = PetAccess.Instance.FindOneById(petid);
                    if (p != null)
                    {
                        p.IsWar = model.Pets.ContainsKey(p.ID);
                        PlayerBusiness user;
                        if (model.Players.TryGetValue(p.PlayerID, out user))
                        {
                            PetDetail detail = new PetDetail(p, user.Name);
                            list.Add(detail);
                        }
                    }
                }
            }
            note.Call(ArenaCommand.PetListArenaR, list);
        }

        /// <summary>
        /// 得到竞技场分组列表
        /// </summary>
        /// <param name="note"></param>
        public static void ArenaGroupName(UserNote note)
        {
            ArenaBase model;
            if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
            {
                //请先选择竞技场
                note.Call(ArenaCommand.ArenaGroupNameR, false, TipManager.GetMessage(ArenaReturn.ArenaGroupName1), string.Empty);
                return;
            }
            note.Call(ArenaCommand.ArenaGroupNameR, true, model.GroupName, note.Player.GroupName);
        }

        /// <summary>
        /// 宠物进入参战
        /// </summary>
        /// <param name="note"></param>
        public static void PetInArena(UserNote note)
        {
            string petid = note.GetString(0);
            string groupname = note.GetString(1);

            int x = note.GetInt32(2);
            int y = note.GetInt32(3);

            if (string.IsNullOrEmpty(note.Player.SoleID))
            {
                //请选择要参战的竞技场
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaGroupName1));
                return;
            }

            ArenaBase model;
            if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
            {
                //请选择要参战的竞技场
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena1));
                return;
            }

            if (model.StartTime < DateTime.UtcNow)
            {
                //竞技场已经开始,不能再参战
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena2));
                return;
            }

            if (!model.GroupName.Contains(groupname))
            {
                //请选择分组
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena3));
                return;
            }

            if (!string.IsNullOrEmpty(note.Player.GroupName))
            {
                if (note.Player.GroupName != groupname)
                {
                    //你已经加入【" + note.Player.GroupName + "】组,不能再选择其它组
                    note.Call(ArenaCommand.PetInArenaR, false, string.Format(TipManager.GetMessage(ArenaReturn.PetInArena4), note.Player.GroupName));
                    return;
                }
            }

            PlayerEx b3 = note.Player.B3;
            IList c = b3.Value.GetValue<IList>("C");
            Variant tmp = null;
            foreach (Variant v in c)
            {
                if (v.GetStringOrDefault("E") == petid)
                {
                    tmp = v;
                    break;
                }
            }

            if (tmp == null)
            {
                //宠物不存在
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena5));
                return;
            }

            Pet p = PetAccess.Instance.FindOneById(petid);
            if (p == null || p.PlayerID != note.PlayerID)
            {
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena5));
                return;
            }

            if (PetAccess.Instance.IsFatigue(p))
            {
                //宠物过度疲劳,不能参加竞技场
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena6));
                return;
            }


            if (Math.Abs(note.Player.FightValue - model.FightValue) > model.FightPoor)
            {
                //战绩差距太大,不能参加
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena7));
                return;
            }

            int count = 0;//已经放入的宠物数量
            int groupcount = 0;//得到选组宠物总数
            foreach (Pet pk in model.Pets.Values)
            {
                if (pk.PlayerID == note.PlayerID)
                {
                    count++;
                }

                if (pk.GroupName == groupname)
                {
                    groupcount++;
                }
            }

            if (model.UserPets <= count)
            {
                //每人只能【" + model.UserPets + "】只宠物参战
                note.Call(ArenaCommand.PetInArenaR, false, string.Format(TipManager.GetMessage(ArenaReturn.PetInArena8), model.UserPets));
                return;
            }

            if (model.PetNumber <= groupcount)
            {
                //每组只能【" + model.PetNumber + "】只宠物参战
                note.Call(ArenaCommand.PetInArenaR, false, string.Format(TipManager.GetMessage(ArenaReturn.PetInArena9), model.PetNumber));
                return;
            }


            if (!model.Pets.TryAdd(p.ID, p))
            {
                //宠物已经参战
                note.Call(ArenaCommand.PetInArenaR, false, TipManager.GetMessage(ArenaReturn.PetInArena10));
                return;
            }
            note.Player.GroupName = groupname;

            p.GroupName = groupname;
            p.X = x;
            p.Y = y;

            p.BeginPoint = new Point(x, y);
            p.EndPoint = new Point(x, y);
            p.CurPoint = new Point(x, y);

            p.RangePet = string.Empty;
            p.PetStatus = 0;
            p.Range = 100;

            List<PetDetail> list = new List<PetDetail>();
            PetDetail detail = new PetDetail(p, note.Player.Name);
            list.Add(detail);

            model.CallAll(ArenaCommand.PetInArenaR, true, list);
            model.ArenaUserCount();
        }

        /// <summary>
        /// 宠物取回
        /// </summary>
        /// <param name="note"></param>
        public static void PetOutArena(UserNote note)
        {
            if (note.Name == ClientCommand.UserDisconnected)
            {
                if (note.Player.AState == ActionState.Arena)
                {
                    ArenaBase model;
                    if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
                    {
                        return;
                    }
                    model.UserDisconnected(note.PlayerID);
                    model.ArenaUserCount();
                }
                return;
            }

            if (note.Name == ArenaCommand.PetOutArena)
            {
                string petid = note.GetString(0);
                ArenaBase model;
                if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
                {
                    //角色不在该竞技场上
                    note.Call(ArenaCommand.PetOutArenaR, false, TipManager.GetMessage(ArenaReturn.PetOutArena1));
                    return;
                }
                if (!model.RemovePet(note.PlayerID, petid))
                {
                    //宠物不存在
                    note.Call(ArenaCommand.PetOutArenaR, false, TipManager.GetMessage(ArenaReturn.PetOutArena2));
                }
                int count = 0;
                foreach (Pet p in model.Pets.Values)
                {
                    if (p.PlayerID == note.PlayerID)
                    {
                        count++;
                        break;
                    }
                }
                if (count <= 0)
                {
                    //表示没有宠物参战
                    note.Player.GroupName = "";
                }
                if (model != null)
                {
                    model.ArenaUserCount();
                }
            }

        }

        /// <summary>
        /// 宠物技能选择
        /// </summary>
        /// <param name="note"></param>
        public static void SelectSkill(UserNote note)
        {
            string petid = note.GetString(0);//宠物ID
            string skillid = note.GetString(1);//技能ID

            ArenaBase model;
            if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
            {
                //请先选择竞技场
                note.Call(ArenaCommand.SelectSkillR, false, TipManager.GetMessage(ArenaReturn.SelectSkill1), petid);
                return;
            }

            Pet p;
            if (model.Pets.TryGetValue(petid, out p))
            {
                if (!string.IsNullOrEmpty(skillid))
                {
                    Variant skill = p.Value.GetValueOrDefault<Variant>("Skill");
                    if (skill == null || (!skill.ContainsKey(skillid)))
                    {
                        //宠物不存在该技能
                        note.Call(ArenaCommand.SelectSkillR, false, TipManager.GetMessage(ArenaReturn.SelectSkill2), petid);
                        return;
                    }

                    GameConfig gc = GameConfigAccess.Instance.FindOneById(skillid);
                    if (gc == null)
                    {
                        //技能不存在
                        note.Call(ArenaCommand.SelectSkillR, false, TipManager.GetMessage(ArenaReturn.SelectSkill3), petid);
                        return;
                    }
                    Variant ui = gc.UI;
                    if (ui.GetIntOrDefault("TypeNoFight") != 1)
                    {
                        //非主动技能
                        note.Call(ArenaCommand.SelectSkillR, false, TipManager.GetMessage(ArenaReturn.SelectSkill4), petid);
                        return;
                    }
                    //技能领取时间
                    p.CoolingTime = gc.Value.GetIntOrDefault("CoolingTime");
                    //得到技能攻击范围
                    p.Range = gc.Value.GetIntOrDefault("Range");
                    p.CurLevel = skill.GetIntOrDefault(skillid);
                }
                else
                {
                    p.Range = 100;//默认攻击的范围
                    p.CurLevel = 0;
                }
                p.CurSkill = skillid;//当前宠物使用的技能
                note.Call(ArenaCommand.SelectSkillR, true, skillid, petid);
                return;
            }
            //宠物不存在
            note.Call(ArenaCommand.SelectSkillR, false, TipManager.GetMessage(ArenaReturn.SelectSkill3));
        }

        /// <summary>
        /// 物品使用
        /// </summary>
        /// <param name="note"></param>
        public static void ArenaGoodsPet(UserNote note)
        {
            string sole = note.PlayerID + "ArenaGoodsPet";
            if (!m_dic.TryAdd(sole, sole))
                return;
            try
            {
                string petid = note.GetString(0);
                //使用格子物品的位置
                int position = note.GetInt32(1);

                ArenaBase model;
                if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
                {
                    //请先选择竞技场
                    note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet1));
                    return;
                }

                Variant v = BurdenManager.BurdenPlace(note.Player.B0, position);
                if (v == null)
                {
                    //输入参数不对
                    note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet2));
                    return;
                }

                string goodsid = v.GetStringOrDefault("G");

                if (string.IsNullOrEmpty(goodsid))
                {
                    //物品不存在
                    note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet3));
                    return;
                }

                GameConfig gc = GameConfigAccess.Instance.FindOneById(goodsid);
                if (gc == null)
                {
                    //物品不存在
                    note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet3));
                    return;
                }

                if (gc.SubType != GoodsSub.Supply)
                {
                    //该物品不能在战斗中使用
                    note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet4));
                    return;
                }

                if (!ArenaGoods.SupplyLimit(note, gc))
                    return;

                Pet p;
                if (model.Pets.TryGetValue(petid, out p))
                {
                    if (!SupplyPet(gc.Value, p, model, note.Player.Name))
                    {
                        //不需要补充
                        note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(ArenaReturn.ArenaGoodsPet5));
                        return;
                    }

                    //作用成功移除物品
                    if (note.Player.RemoveGoods(position, GoodsSource.ArenaGoodsPet))
                    {
                        //Variant tmp = new Variant();
                        //tmp.Add("B0", note.Player.B0);
                        //note.Call(BurdenCommand.BurdenListR, tmp);
                        note.Player.UpdateBurden();
                    }
                }
            }
            finally
            {
                m_dic.TryRemove(sole, out sole);
            }
        }

        /// <summary>
        /// 行走坐标中转
        /// </summary>
        /// <param name="note"></param>
        public static void ArenaWalk(UserNote note)
        {
            string petid = note.GetString(0);
            Int64 point = note.GetInt64(1);

            ArenaBase model;
            if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
                return;
            //开始坐标
            Point begin = new Point();
            begin.X = Convert.ToInt32(point >> 24);
            begin.Y = Convert.ToInt32(point >> 16 & 0xff);

            //结束坐标
            Point end = new Point();
            end.X = Convert.ToInt32(point >> 8 & 0xff);
            end.Y = Convert.ToInt32(point & 0xff);
            Pet p;
            if (model.Pets.TryGetValue(petid, out p))
            {
                p.StartTime = DateTime.UtcNow;
                p.BeginPoint = model.SceneSession.ToScreen(begin);
                p.EndPoint = model.SceneSession.ToScreen(end);
                p.RangePet = note.GetString(2);
                p.PetStatus = note.GetInt32(3);
            }
            if (p != null)
            {
                if (p.PetStatus != 0)
                {
                    model.CallAll(ArenaCommand.ArenaWalkR, true, petid, point);
                }
            }
        }

        /// <summary>
        /// 角色退出竞技场
        /// </summary>
        /// <param name="note"></param>
        public static void PlayerOutArena(UserNote note)
        {
            if (string.IsNullOrEmpty(note.Player.SoleID))
                return;
            ArenaBase model;
            if (ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
            {
                model.UserDisconnected(note.PlayerID);
                if (model != null)
                {
                    model.ArenaUserCount();
                }
            }
            note.Player.SoleID = "";
            note.Player.GroupName = "";
            note.Call(ArenaCommand.PlayerOutArenaR, true);
        }

        /// <summary>
        /// 得到竞技场进入人数与参战人数
        /// </summary>
        /// <param name="note"></param>
        public static void ArenaUserCount(UserNote note)
        {
            ArenaBase model;
            if (!ArenaBusiness.ArenaList.TryGetValue(note.Player.SoleID, out model))
                return;
            model.ArenaUserCount();
        }

        /// <summary>
        /// 得到当前场景信息
        /// </summary>
        /// <param name="note"></param>
        public static void SceneBase(UserNote note)
        {
            UserNote note2 = new UserNote(note.Player, ClientCommand.IntoSceneSuccess, new object[] { note.Player.X, note.Player.Y });
            note.Player.Scene.Execute(note2);
        }


        /// <summary>
        /// 定时
        /// </summary>
        public static void RunTime()
        {
            timer.Change(500, 500);
            //timer.Elapsed += new ElapsedEventHandler(timer_Elapsed);
            //timer.Start();
        }


        /// <summary>
        /// 定时业务
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void timer_Elapsed(object sender)
        {
            ArenaFight.FightInfo();
        }

        /// <summary>
        /// 补充宠物的HP/MP
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private static bool SupplyPet(Variant v, Pet m_pet, ArenaBase model, string name)
        {
            bool use = false;
            //庞物
            if (m_pet == null) return use;
            Variant pet = m_pet.Value;
            if (pet == null) return use;

            Variant moFa = pet.GetVariantOrDefault("MoFa");
            Variant shengMing = pet.GetVariantOrDefault("ShengMing");

            double dhp = v.GetDoubleOrDefault("HP");
            double dmp = v.GetDoubleOrDefault("MP");
            int hp, mp;
            if (dhp <= 1)
            {
                hp = (int)(dhp * shengMing.GetIntOrDefault("M")); //百分比方式
            }
            else
            {
                hp = (int)(dhp);
            }
            if (dmp <= 1)
            {
                mp = (int)(dmp * moFa.GetIntOrDefault("M")); //百分比方式
            }
            else
            {
                mp = (int)(dmp);
            }

            if (hp > 0)
            {
                int sv = shengMing.GetIntOrDefault("V");
                int need = shengMing.GetIntOrDefault("M") - sv;
                if (need > 0)
                {
                    m_pet.HP = Math.Min(need, hp) + sv;
                    shengMing["V"] = m_pet.HP;
                    use = true;
                }
            }

            if (mp > 0)
            {
                int mv = moFa.GetIntOrDefault("V");
                int need = moFa.GetIntOrDefault("M") - mv;
                if (need > 0)
                {
                    m_pet.MP = Math.Min(need, mp) + mv;
                    moFa["V"] = m_pet.MP;
                    use = true;
                }
            }

            if (hp > 0 || mp > 0)
            {
                List<PetDetail> list = new List<PetDetail>();
                PetDetail detail = new PetDetail(m_pet, name);
                list.Add(detail);
                model.CallAll(ArenaCommand.ArenaGoodsR, true, list);
                m_pet.Save();
            }
            return use;
        }

        /// <summary>
        /// 竞技场创建限制
        /// </summary>
        /// <param name="note"></param>
        /// <param name="model">选择条件</param>
        /// <param name="v">限制条件</param>
        /// <returns></returns>
        private static bool ArenaLimit(UserNote note, ArenaBase model, Variant v)
        {
            int score = 0;
            int level = note.Player.Level;
            int fightvalue = 0;
            Variant gametime = v.GetValueOrDefault<Variant>("GameTime").GetValueOrDefault<Variant>(model.GameTime.ToString());
            if (gametime == null)
            {
                //请选择战斗时长
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit1));
                return false;
            }
            score += gametime.GetIntOrDefault("Score");
            fightvalue += gametime.GetIntOrDefault("FightValue");

            Variant group = v.GetValueOrDefault<Variant>("Group").GetValueOrDefault<Variant>(model.Group.ToString());
            if (group == null)
            {
                //请选择分组
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit2));
                return false;
            }
            score += group.GetIntOrDefault("Score");
            fightvalue += group.GetIntOrDefault("FightValue");

            if (!string.IsNullOrEmpty(model.PassWord))
            {
                Variant password = v.GetValueOrDefault<Variant>("PassWord").GetValueOrDefault<Variant>("1");
                if (password != null)
                {
                    score += password.GetIntOrDefault("Score");
                    fightvalue += password.GetIntOrDefault("FightValue");
                }
            }

            Variant petlevel = v.GetValueOrDefault<Variant>("PetLevel").GetValueOrDefault<Variant>(model.PetMin + "-" + model.PetMax);
            if (petlevel == null)
            {
                //请选择战宠等级
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit3));
                return false;
            }
            score += petlevel.GetIntOrDefault("Score");
            fightvalue += petlevel.GetIntOrDefault("FightValue");


            Variant petnumber = v.GetValueOrDefault<Variant>("PetNumber").GetValueOrDefault<Variant>(model.PetNumber.ToString());
            if (petnumber == null)
            {
                //请选择各组产战宠物数量
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit4));
                return false;
            }
            score += petnumber.GetIntOrDefault("Score");
            fightvalue += petnumber.GetIntOrDefault("FightValue");


            Variant preparetime = v.GetValueOrDefault<Variant>("PrepareTime").GetValueOrDefault<Variant>(model.PrepareTime.ToString());
            if (preparetime == null)
            {
                //请选择准备时间
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit5));
                return false;
            }
            score += preparetime.GetIntOrDefault("Score");
            fightvalue += preparetime.GetIntOrDefault("FightValue");


            Variant scene = v.GetValueOrDefault<Variant>("Scene").GetValueOrDefault<Variant>(model.Scene.ToString());
            if (scene == null)
            {
                //请选择竞技场
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit6));
                return false;
            }
            Variant userpets = v.GetValueOrDefault<Variant>("UserPets").GetValueOrDefault<Variant>(model.UserPets.ToString());
            if (userpets == null)
            {
                //请选择角色参战宠物数量
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit7));
                return false;
            }
            score += userpets.GetIntOrDefault("Score");
            fightvalue += userpets.GetIntOrDefault("FightValue");

            Variant fightpoor = v.GetValueOrDefault<Variant>("FightPoor").GetValueOrDefault<Variant>(model.FightPoor.ToString());
            if (fightpoor == null)
            {
                //请选择战绩差
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit8));
                return false;
            }
            score += fightpoor.GetIntOrDefault("Score");
            fightvalue += fightpoor.GetIntOrDefault("FightValue");

            //if (note.Player.FightValue< fightvalue)
            //{
            //    note.Call(ArenaCommand.CreateArenaR, false, "战绩值不足");
            //    return false;
            //}

            if (score < 0)
            {
                //配置有问题
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit9));
                return false;
            }

            if (note.Player.Score < score)
            {
                //石币不足,不能创建
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit10));
                return false;
            }

            if (!note.Player.AddScore(-score, FinanceType.CreateArena))
            {
                //石币不足,不能创建
                note.Call(ArenaCommand.CreateArenaR, false, TipManager.GetMessage(ArenaReturn.ArenaLimit10));
                return false;
            }

            //if (!note.Player.AddFightValue(-fightvalue, true, FinanceType.CreateArena))
            //{
            //    note.Call(ArenaCommand.CreateArenaR, false, "战绩值不足");
            //    return false;
            //}
            //战绩设置
            model.FightValue = note.Player.FightValue;
            return true;
        }

        /// <summary>
        /// 得到竞技场唯一
        /// </summary>
        /// <returns></returns>
        private static string GetAreneID()
        {
            string str = NumberRandom.Next(10000, 99999).ToString();
            if (ArenaBusiness.ArenaList.ContainsKey(str))
            {
                GetAreneID();
            }
            return str;
        }

    }
}
