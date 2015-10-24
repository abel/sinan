using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.FrontServer;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.Command;
using Sinan.Data;
using System.Collections;
using Sinan.Extensions;

namespace Sinan.MountsModule.Bussiness
{
    class MountsBussiness
    {
        static Random random = new Random();
        /// <summary>
        /// 骑乘或召回坐骑
        /// </summary>
        /// <param name="note"></param>
        public static void InOutMounts(UserNote note)
        {
            Mounts m = note.Player.Mounts;
            if (m == null)
            {
                note.Call(MountsCommand.InOutMountsR, false, TipManager.GetMessage(MountsReturn.Mounts2));
                return;
            }
            m.Status = m.Status == 0 ? 1 : 0;
            if (m.Save())
            {      
                PetAccess.PetReset(note.Player.Pet, note.Player.Skill, false, m);
                note.Call(PetsCommand.UpdatePetR, true, note.Player.Pet);
                note.Player.MountsUpdate(m, new List<string>() { "Status" });
                note.Player.MountsInfo();
                note.Call(MountsCommand.InOutMountsR, true, "");
            }
            else
            {
                //操作失败
                note.Call(MountsCommand.InOutMountsR, false,TipManager.GetMessage(MountsReturn.Mounts1));
            }
        }

        /// <summary>
        /// 坐骑技能更换
        /// </summary>
        /// <param name="note"></param>
        public static void MountsSkillChange(UserNote note)
        {
            string oldid = note.GetString(0);
            string newid = note.GetString(1);
            bool ischange = note.GetBoolean(2);
            Mounts m = note.Player.Mounts;
            if (m == null)
            {
                //没有坐骑
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts2));
                return;
            }
            Variant mv = m.Value;
            if (mv == null)
            {
                //被更换的技能不存在
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts3));
                return;
            }
            Variant skill = mv.GetVariantOrDefault("Skill");
            if (skill == null)
            {
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts3));
                return;
            }
            Variant o = skill.GetVariantOrDefault(oldid);
            if (o == null)
            {
                //被更换的技能不存在
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts3));
                return;
            }

            if (skill.ContainsKey(newid))
            {
                //你想更换的技能已经存在
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts4));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(m.MountsID);
            if (gc == null)
            {
                //坐骑配置问题
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts5));
                return;
            }
            Variant v = gc.Value;
            if (v == null)
            {
                //坐骑配置问题
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts5));
                return;
            }

            Variant skills = v.GetVariantOrDefault("Skills");
            if (skills == null)
            {
                //技能不存在
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts6));
                return;
            }

            int level = 0;//新技能等级
            if (!skills.TryGetValueT(newid, out level))
            {
                //技能不存在
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts6));
                return;
            }

            //选择的新技能类型
            GameConfig gck = GameConfigAccess.Instance.FindOneById(newid);
            if (gck == null)
            {
                //技能配置有问题
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts7));
                return;
            }

            if (!gck.Value.ContainsKey(level.ToString()))
            {
                //坐骑技能等级配置有问题
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts8));
                return;
            }

            if (gck.SubType != "MountAddition")
            {
                //坐骑类型不正确
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts9));
                return;
            }

            int score = v.GetIntOrDefault("ChangeScore");
            if (score < 0)
            {
                //石币不足
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts10));
                return;
            }

            if (note.Player.Score < score)
            {
                //石币不足
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts10));
                return;
            }

            if (ischange) 
            {
                string goodsid = "G_d000689";
                if (!note.Player.RemoveGoods(goodsid, GoodsSource.MountsSkillChange))
                {
                    note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts15));
                    return;
                }
            }

            if (!note.Player.AddScore(-score, FinanceType.MountsSkillChange))
            {
                //石币不足
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts10));
                return;
            }

            Variant info = new Variant();
            info.Add("P", o.GetIntOrDefault("P"));
            if (ischange)
            {
                info.Add("Cur", o.GetIntOrDefault("Cur"));
                info.Add("Level", o.GetIntOrDefault("Level"));
            }
            else 
            {
                info.Add("Cur", 0);
                info.Add("Level", 1);
            }
            //添加新技能
            skill.Add(newid, info);
            //移除旧的技能
            skill.Remove(oldid);
          
            if (m.Save())
            {
                PetAccess.PetReset(note.Player.Pet, note.Player.Skill, false, m);
                note.Call(PetsCommand.UpdatePetR, true, note.Player.Pet);
                note.Player.MountsUpdate(m, new List<string>() { "Skill" });
                note.Call(MountsCommand.MountsSkillChangeR, true, "");
            }
            else
            {
                //操作失败
                note.Call(MountsCommand.MountsSkillChangeR, false, TipManager.GetMessage(MountsReturn.Mounts1));
            }
        }


        /// <summary>
        /// 坐骑技能升级
        /// </summary>
        /// <param name="note"></param>
        public static void MountsSkillUp(UserNote note)
        {
            string id = note.GetString(0);
            Mounts m = note.Player.Mounts;
            if (m == null) 
            {
                //没有坐骑
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts2));
                return;
            }
            Variant mv = m.Value;
            if (mv == null)
            {
                //技能不存在
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts6));
                return;
            }
            Variant skill = mv.GetVariantOrDefault("Skill");
            if (skill == null)
            {
                //技能不存在
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts6));
                return;
            }
            if (!skill.ContainsKey(id))
            {
                //技能不存在
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts6));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(id);
            if (gc == null)
            {
                //技能配置问题
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts7));
                return;
            }
            Variant info = skill.GetVariantOrDefault(id);
            //当前熟练度
            int cur = info.GetIntOrDefault("Cur");
            //当前技能等级
            int level = info.GetIntOrDefault("Level");
            //技能最大等级
            int max = gc.UI.GetIntOrDefault("level");

            if (level >= max)
            {
                //技能已经达到最高级
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts11));
                return;
            }

            IList sn = gc.UI.GetValue<IList>("StudyNeeds");
            if (sn == null)
            {
                //技能配置问题
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts7));
                return;
            }

            if (level >= sn.Count)
            {
                ////技能已经达到最高级
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts11));
                return;
            }
            if (cur < Convert.ToInt32(sn[level]))
            {
                //技能熟练度不足
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts12));
                return;
            }
            info["Level"] = level + 1;
            info["Cur"] = 0;
            if (m.Save())
            {
                PetAccess.PetReset(note.Player.Pet, note.Player.Skill, false, m);
                note.Call(PetsCommand.UpdatePetR, true, note.Player.Pet);

                note.Player.MountsUpdate(m, new List<string>() { "Skill" });
                note.Call(MountsCommand.MountsSkillUpR, true,"");
            }
            else 
            {
                //操作失败
                note.Call(MountsCommand.MountsSkillUpR, false, TipManager.GetMessage(MountsReturn.Mounts1));
            }
        }


        /// <summary>
        /// 坐骑进化
        /// </summary>
        /// <param name="note"></param>
        public static void MountsUp(UserNote note)
        {
            Mounts m = note.Player.Mounts;
            if (m == null)
            {
                //没有坐骑
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts2));
                return;
            }

            GameConfig gc = GameConfigAccess.Instance.FindOneById(m.MountsID);
            if (gc == null)
            {
                //坐骑配置有问题
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts5));
                return;
            }

            Variant mv = gc.Value;
            if (mv == null)
            {
                //坐骑配置有问题
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts5));
                return;
            }

            int rank = m.Rank;
            Variant jh = mv.GetVariantOrDefault("JinHua");
            if (jh == null)
            {
                return;
            }

            Variant info = jh.GetVariantOrDefault((rank + 1).ToString());
            if (info == null)
            {
                //坐骑已经进化到最高级
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts13));
                return;
            }

            //技能列表
            Variant ss = mv.GetVariantOrDefault("Skills");
            
            Variant v = m.Value;

            List<string> list = new List<string>();
            foreach (var item in ss)
            {
                //取得技能列表
                list.Add(item.Key);
            }
            if (m.Value == null) 
            {
                m.Value = new Variant();
                v = m.Value;
            }
            Variant sk = v.GetVariantOrDefault("Skill");
            if (sk != null)
            {
                foreach (var k in sk)
                {
                    if (list.Contains(k.Key))
                    {
                        list.Remove(k.Key);
                    }
                }
            }
            else 
            {
                v["Skill"] = new Variant();
                sk = v.GetVariantOrDefault("Skill");
            }

            if (list == null || list.Count <= 0)
            {
                //坐骑技能配置有问题
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts8));
                return;
            }
            Variant demand = info.GetVariantOrDefault("Demand");
            if (demand == null)
            {
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts8));
                return;
            }
            string goodsid = demand.GetStringOrDefault("GoodsID");
            int number = demand.GetIntOrDefault("Number");
            PlayerEx b0 = note.Player.B0;
            if (number > 0)
            {
                if (!note.Player.RemoveGoods(goodsid, number, GoodsSource.MountsUp))
                {
                    //坐骑进化需要的道具数量不足
                    note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts14));
                    return;
                }
                //更新服务端
                note.Player.UpdateBurden();
            }

            //机率
            double lv = demand.GetDoubleOrDefault("Rand");
            Variant rv = MemberAccess.MemberInfo(note.Player.MLevel);
            if (rv != null) 
            {
                lv *= (1 + rv.GetDoubleOrDefault("ZuoQiLv"));
            }

            int zf = 0;
            DateTime dt = DateTime.UtcNow;            
            Variant zhufu = ZhuFuAccess.ZhuFuInfo("Mounts", m.Rank);
            //祝福值增加成功率
            if (m.FailTime.ToLocalTime().Date == dt.ToLocalTime().Date && zhufu != null)
            {                
                Variant fc = zhufu.GetVariantOrDefault("FailCount");
                if (fc != null)
                {
                    double addlv = 0;
                    foreach (var item in fc)
                    {
                        if (m.FailCount >= Convert.ToInt32(item.Key) && addlv < Convert.ToDouble(item.Value))
                        {
                            addlv = Convert.ToDouble(item.Value);
                        }
                    }
                    lv += addlv;
                }
            }
            else
            {
                m.FailCount = 0;
                m.ZhuFu = 0;
            }
            if (zhufu != null)
            {
                //计算祝福值
                string[] strs = zhufu.GetStringOrDefault("Rand").Split('-');
                int min = Convert.ToInt32(strs[0]);
                int max = Convert.ToInt32(strs[1]) + 1;
                zf = random.Next(min, max);
            }

            if (NumberRandom.RandomHit(lv))
            {
                string skillid = list[random.Next(0, list.Count)];
                Variant vc = new Variant();
                vc.Add("P", sk.Count);
                vc.Add("Level", ss[skillid]);
                vc.Add("Cur", 0);
                sk[skillid] = vc;
                m.Rank = rank + 1;

                m.FailCount = 0;
                m.FailTime = dt;
                m.ZhuFu = 0;
                m.Save();

                PetAccess.PetReset(note.Player.Pet, note.Player.Skill, false, m);
                note.Call(PetsCommand.UpdatePetR, true, note.Player.Pet);

                note.Player.MountsUpdate(m, new List<string>() { "Skill", "Rank", "FailCount", "FailTime", "ZhuFu" });
                note.Player.MountsInfo();
                note.Call(MountsCommand.MountsUpR, true, "");
            }
            else
            {
                m.FailCount++;
                m.FailTime = dt;
                m.ZhuFu += zf;
                m.Save();
                note.Player.MountsUpdate(m, new List<string>() { "FailCount", "FailTime", "ZhuFu" });
                //进化失败
                note.Call(MountsCommand.MountsUpR, false, TipManager.GetMessage(MountsReturn.Mounts16));
            }
        }

        /// <summary>
        /// 增加坐骑熟练度
        /// </summary>
        /// <param name="note"></param>
        public static void MountSkilling(UserNote note) 
        {
            Mounts m = note.Player.Mounts;
            if (m == null)            
                return;
            Variant mv = m.Value;
            if (mv == null)
                return;
            Variant sk = mv.GetVariantOrDefault("Skill");
            if (sk == null)
                return;
            bool ischange = false;
            foreach (var item in sk) 
            {                
                Variant info = item.Value as Variant;
                if (info == null)
                    continue;

                GameConfig gc = GameConfigAccess.Instance.FindOneById(item.Key);
                if (gc == null)
                    continue;

                Variant ui = gc.UI;
                if (ui == null)
                    continue;

                IList sn = ui.GetValue<IList>("StudyNeeds");
                if(sn==null)
                    continue;

                //当前技能等级
                int level= info.GetIntOrDefault("Level");
                //升级需要的最大熟练度
                int max = Convert.ToInt32(sn[level]);
                int cur = info.GetIntOrDefault("Cur");
                if (cur >= max)
                    continue;

                info["Cur"] = cur + 1;
                if (!ischange)
                {
                    ischange = true;
                }
            }

            if (ischange)
            {
                m.Save();                
                note.Player.MountsUpdate(m, new List<string>() { "Skill" });             
            }
        }
    }
}
