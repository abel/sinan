using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.Command;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.FrontServer
{
    //成就部分
    partial class PlayerBusiness
    {
        /// <summary>
        /// 完成通知(用于成就系统)
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objs"></param>
        /// <returns></returns>
        public bool FinishNote(string name, params object[] objs)
        {
            if (m_effort == null || m_effort.Value == null) 
                return false;

            switch (name)
            {
                case FinishCommand.RoleUpLev:
                    FinishRoleUpLev(objs);
                    break;
                case FinishCommand.PetUpLev:
                    FinishPetUpLev(objs);
                    break;
                case FinishCommand.MaxMoney:
                    FinishMoney(objs);
                    break;
                case FinishCommand.TotalTask:
                    FinishTask(objs);
                    break;

                case FinishCommand.XianQian:
                case FinishCommand.HeChen:
                case FinishCommand.Boss:
                case FinishCommand.Purification:
                case FinishCommand.ArenaWin:
                case FinishCommand.ExtendLib:
                case FinishCommand.StarBottle:
                case FinishCommand.WishFriends:
                case FinishCommand.ExtendStables:
                case FinishCommand.AnswerAward:
                    FinishCount(name, 1);
                    break;

                case FinishCommand.PKWin:
                    FinishCount(name, Convert.ToInt32(objs[0]));
                    break;

                case FinishCommand.PetZhiZi:
                case FinishCommand.PetJieJi:
                    FinishTotal(name, Convert.ToInt32(objs[0]));
                    break;

                case FinishCommand.PetSkill:
                    PetSkill();
                    break;
                case FinishCommand.PetJobFuHua:
                    PetJobFuHua(objs);
                    break;
                case FinishCommand.PetOut:
                    PetOut();
                    break;
                case FinishCommand.Friends:
                    Friends(Convert.ToInt32(objs[0]));
                    break;
                case FinishCommand.Star:
                    FinishStar();
                    break;
                case FinishCommand.Wardrobe:
                    FinishWardrobe(name, Convert.ToInt32(objs[0]));
                    break;
                default:
                    break;
            }
            return true;
        }

        #region  其它成就
        /// <summary>
        /// 达到指定数量
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objs"></param>
        private void FinishTotal(string name, int num)
        {
            var configs = GameConfigAccess.Instance.Find(MainType.Effort, name);
            foreach (var config in configs)
            {
                if (!m_effort.Value.ContainsKey(config.ID))
                {
                    //检查是否达成
                    int needTotal = config.Value.GetIntOrDefault("A");
                    if (needTotal > 0 && num >= needTotal)
                    {   //奖励
                        EffortAward(config);
                    }
                }
            }
        }


        /// <summary>
        /// 完成总次数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="objs"></param>
        private void FinishCount(string name, int count = 1)
        {
            int total = m_effort.Value.SetOrInc(name, count);
            var configs = GameConfigAccess.Instance.Find(MainType.Effort, name);
            bool issave = true;
            foreach (var config in configs)
            {
                if (!m_effort.Value.ContainsKey(config.ID))
                {   //检查是否达成
                    int needTotal = config.Value.GetIntOrDefault("A");
                    if (needTotal > 0 && total >= needTotal)
                    {   //奖励
                        EffortAward(config);
                        issave = false;
                    }
                }
            }
            if (issave) 
            {
                m_effort.Save();
                Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_effort));
            }
        }
        #endregion

        #region 任务成就

        /// <summary>
        /// 任务相关成就
        /// </summary>
        /// <param name="objs"></param>
        private void FinishTask(object[] objs)
        {
            //FinishCount(FinishCommand.TotalTask, 1);
            if (objs == null || objs.Length == 0)
                return;
            FinishTaskType(objs);
            //得到指定任务的成就
            var task = GameConfigAccess.Instance.Find(MainType.Effort, "TaskType");
            foreach (var v in task)
            {
                if (!m_effort.Value.ContainsKey(v.ID))
                {   //检查是否达成
                    string TaskID = v.Value.GetStringOrDefault("TaskID");
                    if (TaskID == objs[0].ToString())
                    {   //奖励
                        EffortAward(v);
                    }
                }
            }


        }

        /// <summary>
        /// 完成指定类型的任务数
        /// </summary>
        /// <param name="objs"></param>
        private void FinishTaskType(object[] objs)
        {
            Variant ev = m_effort.Value;
            if (ev == null)
                return;

            //当前完成的任务类型
            int n = Convert.ToInt32(objs[1]);
            Variant t;
            if (ev.TryGetValueT("TaskType", out t))
            {
                t.SetOrInc(n.ToString(), 1);
            }
            else
            {
                t = new Variant();
                t.SetOrInc(n.ToString(), 1);
                m_effort.Value.Add("TaskType", t);
            }
            ev.SetOrInc(FinishCommand.TotalTask, 1);

            List<GameConfig> list = GameConfigAccess.Instance.Find(MainType.Effort, FinishCommand.TotalTask);
            if (list == null)
                return;
            bool ischange = true;
            foreach (GameConfig gc in list)
            {
                if (ev.ContainsKey(gc.ID))
                    continue;

                int m = 0;
                //要求的任务类型
                if (gc.Value.TryGetValueT("TaskType", out m))
                {
                    //表示两类型不相同
                    if (m != n) continue;
                    //表示达成成就,将得到对应奖励
                    if (t.GetIntOrDefault(n.ToString()) >= gc.Value.GetIntOrDefault("A"))
                    {
                        ischange = false;
                        EffortAward(gc);
                    }
                }
                else
                {
                    //完成任务总数,
                    if (ev.GetIntOrDefault(FinishCommand.TotalTask) >= gc.Value.GetIntOrDefault("A"))
                    {
                        ischange = false;
                        EffortAward(gc);
                    }
                }
            }
            if (ischange)
            {
                m_effort.Save();
                Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_effort));
            }
        }

        #endregion

        #region 晶币成就

        /// <summary>
        /// 金钱达成类的成就
        /// </summary>
        /// <param name="objs"></param>
        private void FinishMoney(object[] objs)
        {
            var configs = GameConfigAccess.Instance.Find(MainType.Effort, "MaxMoney");
            foreach (var config in configs)
            {
                if (!m_effort.Value.ContainsKey(config.ID))
                {
                    double needCoin = config.Value.GetDoubleOrDefault("Coin");
                    double needScore = config.Value.GetDoubleOrDefault("Score");
                    //double needBond = config.Value.GetDoubleOrDefault("Bond");
                    if (this.Coin >= needCoin && this.Score >= needScore)
                    {
                        EffortAward(config);
                    }
                }
            }
        }
        #endregion

        #region 宠物相关成就
        /// <summary>
        /// 宠物升级类的成就.
        /// </summary>
        /// <param name="objs"></param>
        private void FinishPetUpLev(object[] objs)
        {
            int lev = (int)(objs[0]);
            var configs = GameConfigAccess.Instance.Find(MainType.Effort, "PetLev");
            foreach (var config in configs)
            {
                if (!m_effort.Value.ContainsKey(config.ID))
                {
                    int needLev = config.Value.GetIntOrDefault("A");
                    if (needLev > 0 && lev >= needLev)
                    {
                        EffortAward(config);
                    }
                }
            }
        }

        /// <summary>
        /// 角色升级类的成就
        /// </summary>
        /// <param name="objs"></param>
        private void FinishRoleUpLev(object[] objs)
        {
            var configs = GameConfigAccess.Instance.Find(MainType.Effort, "RoleLev");
            foreach (var config in configs)
            {
                if (!m_effort.Value.ContainsKey(config.ID))
                {
                    int needLev = config.Value.GetIntOrDefault("A");
                    if (needLev > 0 && this.m_level >= needLev)
                    {
                        EffortAward(config);
                    }
                }
            }
        }

        /// <summary>
        /// 完成宠物技能提取操作
        /// </summary>
        private void PetSkill()
        {
            //成就子类型  
            List<string> PetEffort = new List<string>()
            { 
                "PetSkillCount",// 宠物技能学习总数
                "PetJobSkillCount",// 统计宠物各职业技能个数
                "PetSkillIDLevel", // 宠物某技能达到等级
                "PetJobSkillLevel", // 宠物职业各等級技能学习个数
                "PetSkillsToLevel" //宠物不同技能达到某一等级
            };

            foreach (string name in PetEffort)
            {
                List<GameConfig> gc = GameConfigAccess.Instance.Find(MainType.Effort, name);
                if (gc.Count > 0)
                {
                    foreach (GameConfig config in gc)
                    {
                        if (m_effort.Value.ContainsKey(config.ID))
                            continue;

                        int a = 0, num = 0, level = 0;
                        string roleid = string.Empty, skillid = string.Empty;
                        Variant v = config.Value;
                        if (v == null) continue;
                        a = v.GetIntOrDefault("A");

                        bool isfinish = false;
                        switch (name)
                        {
                            case "PetSkillCount":

                                num = GameConfigAccess.Instance.PetSkillCount(m_petBook.Value);
                                isfinish = (a > 0 && num >= a);
                                break;

                            case "PetJobSkillCount":
                                roleid = config.Value.GetStringOrDefault("Job");
                                num = GameConfigAccess.Instance.PetJobSkillCount(m_petBook.Value, roleid);
                                isfinish = (a > 0 && num >= a);
                                break;

                            case "PetSkillIDLevel":
                                skillid = config.Value.GetStringOrDefault("SkillID");
                                isfinish = a > 0 && GameConfigAccess.Instance.PetSkillIDLevel(m_petBook.Value, skillid, a);
                                break;

                            case "PetJobSkillLevel":
                                level = config.Value.GetIntOrDefault("Level");
                                roleid = config.Value.GetStringOrDefault("Job");
                                isfinish = a > 0 && GameConfigAccess.Instance.PetJobSkillLevel(m_petBook.Value, roleid, level) >= a;
                                break;

                            case "PetSkillsToLevel":
                                IList Skills = v.GetValueOrDefault<IList>("Skills");
                                isfinish = GameConfigAccess.Instance.PetSkillsToLevel(m_petBook.Value, Skills);
                                break;
                        }
                        if (isfinish)
                        {
                            EffortAward(config);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 孵化不同职业宠物个数
        /// </summary>
        private void PetJobFuHua(object[] objs)
        {

            string role = objs[0].ToString();
            if (string.IsNullOrEmpty(role))
                return;
            m_effort.Value.SetOrInc("P" + role, 1);
            m_effort.Save();
            List<string> PetEffort = new List<string>()
            {
                "PetJobFuHua",//孵化不同職業宠物个数
                "PetFuHua"//"孵化宠物个数"
            };

            int total = 0;
            for (int i = 0; i < 4; i++)
            {
                total += m_effort.Value.GetIntOrDefault("P" + i);
            }

            foreach (string name in PetEffort)
            {
                List<GameConfig> gcs = GameConfigAccess.Instance.Find(MainType.Effort, name);
                foreach (GameConfig config in gcs)
                {
                    if (m_effort.Value.ContainsKey(config.ID))
                        continue;
                    Variant v = config.Value;
                    if (v == null) continue;
                    bool isfinish = false;
                    int a = v.GetIntOrDefault("A");//达到条件
                    switch (name)
                    {
                        case "PetFuHua":
                            isfinish = total >= a;
                            break;
                        case "PetJobFuHua":
                            string roleid = v.GetStringOrDefault("Job");
                            isfinish = m_effort.Value.GetIntOrDefault("P" + roleid) >= a;
                            break;
                    }
                    if (isfinish)
                    {
                        EffortAward(config);
                    }
                }
            }
        }

        /// <summary>
        /// 宠物放生
        /// </summary>
        private void PetOut()
        {
            int total = m_effort.Value.SetOrInc(FinishCommand.PetOut, 1);
            List<GameConfig> gcs = GameConfigAccess.Instance.Find(MainType.Effort, FinishCommand.PetOut);
            if (gcs == null) return;
            if (gcs.Count > 0)
            {
                foreach (GameConfig config in gcs)
                {
                    if (m_effort.Value.ContainsKey(config.ID))
                        continue;
                    Variant v = config.Value;
                    if (v == null) continue;
                    int A = v.GetIntOrDefault("A");
                    if (total >= A)
                    {
                        EffortAward(config);
                    }
                }
            }
            m_effort.Save();
        }

        /// <summary>
        /// 添加好友数量得到成就
        /// </summary>
        private void Friends(int total)
        {
            List<GameConfig> gcs = GameConfigAccess.Instance.Find(MainType.Effort, FinishCommand.Friends);
            if (gcs == null) return;
            if (gcs.Count > 0)
            {
                //当前好友数量
                foreach (GameConfig config in gcs)
                {
                    if (m_effort.Value.ContainsKey(config.ID))
                        continue;
                    Variant v = config.Value;
                    if (v == null) continue;
                    int A = v.GetIntOrDefault("A");
                    if (total >= A)
                    {
                        EffortAward(config);
                    }
                }
            }
        }
        #endregion

        #region 星座相关成就
        /// <summary>
        /// 星阵激活成功
        /// </summary>
        private void FinishStar()
        {
            Variant v = m_star.Value;
            if (v == null)
                return;
            //得到星阵成就
            List<GameConfig> list = GameConfigAccess.Instance.Find(MainType.Effort, FinishCommand.Star);

            Variant r = new Variant();
            for (int i = 1; i < 13; i++)
            {
                IList item = v.GetValue<IList>(i.ToString());
                int num = item == null ? 0 : item.Count;
                r.SetOrInc(i.ToString(), num);
                r.SetOrInc("0", num);
            }

            foreach (GameConfig gc in list)
            {
                if (m_effort.Value.ContainsKey(gc.ID))
                    continue;

                Variant t = gc.Value;
                string starType = t.GetStringOrDefault("StarType");
                int m = 0;
                if (!r.TryGetValueT(starType, out m))
                    continue;
                //达成成就条件
                int a = t.GetIntOrDefault("A");
                if (m >= a)
                {
                    EffortAward(gc);
                }
            }

        }
        #endregion

        #region 时装成就
        /// <summary>
        /// 时装成就
        /// </summary>
        /// <param name="name"></param>
        /// <param name="count">购买总数量</param>
        private void FinishWardrobe(string name,int count)
        {
            List<GameConfig> gcs = GameConfigAccess.Instance.Find(MainType.Effort, name);
            if (gcs == null)
                return;
            foreach (GameConfig gc in gcs)
            {
                //表示成就不存在
                if (!m_effort.Value.ContainsKey(gc.ID))
                {
                    Variant v = gc.Value;
                    if (count >= v.GetIntOrDefault("A"))
                    {
                        EffortAward(gc);
                    }
                }
            }
        }
        #endregion

        #region 成就奖励
        /// <summary>
        /// 成就奖励
        /// </summary>
        /// <param name="config"></param>
        private void EffortAward(GameConfig config)
        {
            //设置为已领取
            m_effort.Value[config.ID] = DateTime.UtcNow;
            Variant award;
            int dian = 0;
            if (config.Value.TryGetValueT<Variant>("Award", out award) && award != null)
            {
                dian = award.GetIntOrDefault("Dian");
                if (dian > 0)
                {
                    //成就点奖励
                    AddDian(dian);
                }
                AddTitle(award.GetStringOrDefault("Title"));
                AddExperience(award.GetIntOrDefault("Exp"), FinanceType.Effort);
                AddPetBook(award);
                GetOwe(award, config.Name);
                AddShiZhuang(award.GetStringOrDefault("ShiZhuang" + Sex), config.ID);
            }
            m_effort.Save();
            //通知客户端更新成就
            Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_effort));
            //通知客户端
            CallAll(EffortCommand.DaCheng, config.ID, config.Name, dian, ID, Name);            
        }

        /// <summary>
        /// 设置成就点
        /// </summary>
        /// <param name="dian"></param>
        /// <returns></returns>
        bool AddDian(int dian)
        {
            dian += m_effort.Value.GetIntOrDefault("Dian");
            m_effort.Value["Dian"] = dian;
            m_effort.Save();
            this.Dian = dian;
            return PlayerAccess.Instance.SaveValue(_id, "Dian", dian);
        }

        /// <summary>
        /// 添加称号
        /// </summary>
        /// <param name="titleID"></param>
        private void AddTitle(string titleID)
        {
            if (!string.IsNullOrEmpty(titleID))
            {
                if (m_title != null && (!m_title.Value.ContainsKey(titleID)))
                {
                    var gc = GameConfigAccess.Instance.FindOneById(titleID);
                    if (gc != null)
                    {
                        m_title.Value[titleID] = gc.Name;
                        m_title.Save();
                        PlayersProxy.CallAll(EffortCommand.GetActTitleR, new object[] { ID, Name, titleID });
                        Call(ClientCommand.UpdateActorR, new PlayerExDetail(m_title));
                    }
                }
            }
        }

        /// <summary>
        /// 设置师傅感恩值
        /// </summary>
        /// <returns></returns>
        private void GetOwe(Variant award, string name)
        {
            int owe = award.GetIntOrDefault("Owe");
            if (owe <= 0)
                return;

            Variant m = Social.Value.GetValueOrDefault<Variant>("Mentor");
            if (m == null)
                return;
            IList master = m.GetValue<IList>("Master");

            if (master == null || master.Count <= 0)
                return;
            Variant v = master[0] as Variant;
            if (v == null)
                return;
            //得到师傅基本信息
            PlayerBusiness pb = PlayersProxy.FindPlayerByID(v.GetStringOrDefault("PlayerID"));
            if (pb == null) return;
            if (pb.AddOwe(owe, FinanceType.AppFinish, this.ID))
            {
                //"你的徒弟【" + this.Name + "】达成【" + name + "】成就，贡献了【" + owe + "】点感恩值给你";
                string msg = string.Format(TipManager.GetMessage(ClientReturn.GetOwe1), this.Name, name, owe);

                if (pb.Online)
                {
                    pb.Call(ClientCommand.SendActivtyR, new object[] { "T02", msg });
                }
                else
                {
                    //不在线
                    int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
                    EmailAccess.Instance.SendEmail(TipManager.GetMessage(ClientReturn.GetOwe2), TipManager.GetMessage(ClientReturn.GetOwe3), pb.ID, pb.Name, msg, null, null, reTime);
                }
            }
        }

        /// <summary>
        /// 成就时装奖励
        /// </summary>
        /// <param name="id"></param>
        private void AddShiZhuang(string id,string gid)
        {
            if (string.IsNullOrEmpty(id))
                return;
            PlayerEx wx = this.m_wardrobe;
            Variant wv = wx.Value;
            IList wl = wv.GetValue<IList>("WardrobeList");
            if (wl != null)
            {
                if (wl.Contains(id))
                    return;
                wl.Add(id);
            }
            else
            {
                wv["WardrobeList"] = new List<string>() { id };
            }
            wx.Save();
            Call(ClientCommand.UpdateActorR, new PlayerExDetail(wx));

            //成就得到时间
            Variant os = new Variant();
            os["IsMode"] = 1;
            os["ShiZhuang"] = id;
            os["ChengJiu"] = gid;
            AddLogVariant(Actiontype.MallShiZhuang, null, null, os);
        }
        #endregion

    }
}
