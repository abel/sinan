using System;
using System.Collections;
using System.Collections.Generic;
using Sinan.Command;
using Sinan.Entity;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Observer;
using Sinan.Util;

namespace Sinan.TaskModule.Business
{
    /// <summary>
    /// 任务奖励业务
    /// </summary>
    class AwardBusiness
    {
        /// <summary>
        /// 奖励类型[经验,晶币,游戏币]
        /// </summary>
        /// <param name="note"></param>
        /// <param name="award">奖励</param>
        /// <param name="isdouble">该是否可以任务</param>
        public static Variant TaskAward20001(UserNote note, IList award, bool isdouble = false)
        {

            bool isp = false;//是否双倍
            PlayerEx assist = note.Player.Assist;
            Variant avs = assist.Value;
            //表示双倍日常任务经验
            if (isdouble)
            {
                if (avs.ContainsKey("TSP"))
                {
                    int tsp = avs.GetIntOrDefault("TSP");
                    if (tsp > 1)
                    {
                        avs["TSP"] = tsp - 1;
                    }
                    else
                    {
                        avs.Remove("TSP");
                    }
                    assist.Save();
                    isp = true;
                    note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(assist));
                }
            }

            Variant os = new Variant();
            bool isfamily = false;
            foreach (Variant d in award)
            {
                if (d.GetStringOrDefault("Type") == "20001")
                {
                    string name = d.GetStringOrDefault("Name");
                    int count = d.GetIntOrDefault("Count");
                    switch (name)
                    {
                        case "Experience":
            
                            count = isp ? count * 2 : count;
                            note.Player.AddExperience(count, FinanceType.Task);
           
                            break;
                        case "PetExperience":

                            count = isp ? count * 2 : count;
                            Pet p = note.Player.Pet;
                            if (p != null)
                            {
                                note.Player.AddPetExp(note.Player.Pet, count, true, (int)FinanceType.Task);
                            }

                            if (note.Player.Mounts != null) 
                            {
                                note.Player.AddMounts(count, GoodsSource.TaskAward);
                            }
                            
                            break;
                        case "Bond":
                            note.Player.AddBond(count, FinanceType.Task);
                            
                            break;
                        case "Score":
                            note.Player.AddScore(count, FinanceType.Task);
                            
                            break;
                        case "Dev":
                            isfamily = Dev(note.Player, count);                            
                            break;
                    }

                    if (name == "Dev")
                    {
                        if (isfamily)
                        {
                            os.SetOrInc(name, count);
                        }
                    }
                    else
                    {
                        os.SetOrInc(name, count);
                    }
                }
            }
            return os;
        }


        /// <summary>
        /// 获取家族成长度
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="count"></param>
        private static bool Dev(PlayerBusiness pb, int count)
        {
            PlayerEx fx = pb.Family;
            string id = fx.Value.GetStringOrDefault("FamilyID");
            if (!string.IsNullOrEmpty(id))
            {
                Variant v = new Variant();
                v.Add("FamilyID", id);
                v.Add("Experience", count);
                v.Add("PlayerID", pb.ID);
                v.Add("Type", "TaskServer");
                Notification note2 = new Notification(FamilyCommand.FamilyExperience, new object[] { v });
                Notifier.Instance.Publish(note2);
                return true;
            }
            return false;
        }

        /// <summary>
        /// 宠物奖励
        /// </summary>
        public static void TaskAward112009(UserNote note, Dictionary<string, int> pets)
        {
            foreach (string ps in pets.Keys)
            {
                for (int i = 0; i < pets[ps]; i++)
                {
                    PetAccess.Instance.CreatePet(note.Player.B3, ps, 0, 1);
                }
            }
            note.Call(ClientCommand.UpdateActorR, new PlayerExDetail(note.Player.B3));
        }

        /// <summary>
        /// 发送任务邮件
        /// </summary>
        /// <param name="note"></param>
        /// <param name="msg"></param>
        public static void TaskEmail(UserNote note, string[] msg)
        {
            if (msg.Length < 4)
                return;
            int reTime = Convert.ToInt32(TipManager.GetMessage(EmailReturn.HameDay));
            if (EmailAccess.Instance.SendEmail(msg[1], msg[2], note.PlayerID, note.Player.Name, msg[3], note.Player.Name,null,reTime))
            {
                note.Call(EmailCommand.NewEmailTotalR, EmailAccess.Instance.NewTotal(note.PlayerID));
            }
        }
    }
}
