using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Entity;
using Sinan.Data;
using MongoDB.Driver.Builders;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class MountsAccess : VariantBuilder<Mounts>
    {
        readonly static MountsAccess m_instance = new MountsAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static MountsAccess Instance
        {
            get { return m_instance; }
        }

        MountsAccess()
            : base("Mounts")
        {


        }


        /// <summary>
        /// 到得坐骑列表信息
        /// </summary>
        /// <param name="playerid">角色</param>
        /// <returns></returns>
        public List<Mounts> GetMounts(string playerid)
        {
            var query = Query.EQ("PlayerID", playerid);
            var n = m_collection.FindAs<Mounts>(query);
            return n.ToList();
        }

        /// <summary>
        /// 坐骑属性值
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        public Variant MountsLife(Mounts model)
        {
            Variant life = new Variant();
            double mag = Mag(model);
            life.Add("LiLiang", Math.Ceiling((6 + model.Level * 4) * 0.6 * mag));
            life.Add("ZhiLi", Math.Ceiling((6 + model.Level * 5) * 0.6 * mag));
            life.Add("TiZhi", Math.Ceiling((6 + model.Level * 5) * 0.4 * mag));
            life.Add("JingShen", Math.Ceiling((6 + model.Level * 4) * 0.5 * mag));
            return life;
        }

        /// <summary>
        /// 坐骑对宠物属性加成
        /// </summary>
        /// <param name="level">坐骑等级</param>
        /// <param name="roleid">宠物职业</param>
        /// <returns></returns>
        public Variant MountsPetLife(Mounts model, string roleid)
        {
            if (model == null || model.Status == 0)
                return null;
            double mag = Mag(model);
            Variant life = new Variant();
            if (roleid == "1")
            {
                //狂战士
                life.Add("LiLiang", Math.Ceiling((6 + model.Level * 4) * 0.6 * mag));
                life.Add("ZhiLi", 0);
                life.Add("TiZhi", Math.Ceiling((6 + model.Level * 5) * 0.4 * mag));
                life.Add("JingShen", Math.Ceiling(((6 + model.Level * 4) * 0.5 * mag) * 0.5));
            }
            else if (roleid == "2")
            {
                //魔弓手
                life.Add("LiLiang", 0);
                life.Add("ZhiLi", Math.Ceiling((6 + model.Level * 5) * 0.6 * mag));
                life.Add("TiZhi", Math.Ceiling(((6 + model.Level * 5) * 0.4 * mag) * 0.5));
                life.Add("JingShen", Math.Ceiling((6 + model.Level * 4) * 0.5 * mag));
            }
            else
            {
                //神祭师
                life.Add("LiLiang", Math.Ceiling(((6 + model.Level * 4) * 0.6 * mag) * 0.67));
                life.Add("ZhiLi", Math.Ceiling(((6 + model.Level * 5) * 0.6 * mag) * 0.67));
                life.Add("TiZhi", Math.Ceiling((6 + model.Level * 5) * 0.4 * mag));
                life.Add("JingShen", Math.Ceiling(((6 + model.Level * 4) * 0.5 * mag) * 0.5));
            }


            Variant v = model.Value;
            if (v != null)
            {
                //坐骑技能对宠物加成
                Variant skill = v.GetVariantOrDefault("Skill");
                foreach (var item in skill)
                {
                    Variant msg = item.Value as Variant;
                    if (msg == null)
                        continue;
                    GameConfig gc = GameConfigAccess.Instance.FindOneById(item.Key);
                    if (gc == null)
                        continue;
                    Variant sv = gc.Value;
                    //技能当前等级
                    int level = msg.GetIntOrDefault("Level");
                    //得到技能加成
                    Variant sk = sv.GetVariantOrDefault(level.ToString());
                    if (sk == null)
                        continue;

                    foreach (var t in sk)
                    {
                        life.SetOrInc(t.Key, Convert.ToDouble(t.Value));
                    }

                }
            }
            return life;
        }

        /// <summary>
        /// 得到陪伴兽信息
        /// </summary>
        /// <param name="model"></param>
        /// <param name="update">更新内容</param>
        /// <returns></returns>
        public Variant MountsInfo(Mounts model, List<string> update)
        {
            if (model == null)
                return null;
            Variant info = new Variant();
            info.Add("ID", model.ID);
            if (update == null)
            {
                info.Add("MountsID", model.MountsID);
                info.Add("Level", model.Level);
                info.Add("Experience", model.Experience);
                info.Add("Status", model.Status);
                if (model.Value == null)
                {
                    info.Add("Skill", null);
                }
                else
                {
                    info.Add("Skill", model.Value.GetVariantOrDefault("Skill"));
                }
                info.Add("Rank", model.Rank);
                info.Add("FailCount", model.FailCount);//失败次数
                info.Add("FailTime", model.FailTime);//失败时间
                info.Add("ZhuFu", model.ZhuFu);//祝福值
            }
            else
            {
                if (update.Contains("MountsID"))
                {
                    info.Add("MountsID", model.MountsID);
                }
                if (update.Contains("Level"))
                {
                    info.Add("Level", model.Level);
                }
                if (update.Contains("Experience"))
                {
                    info.Add("Experience", model.Experience);
                }
                if (update.Contains("Status"))
                {
                    info.Add("Status", model.Status);
                }
                if (update.Contains("Skill"))
                {
                    if (model.Value == null)
                    {
                        info.Add("Skill", null);
                    }
                    else
                    {
                        info.Add("Skill", model.Value.GetVariantOrDefault("Skill"));
                    }
                }
                if (update.Contains("Rank"))
                {
                    info.Add("Rank", model.Rank);
                }

                if (update.Contains("FailCount"))
                {
                    info.Add("FailCount", model.FailCount);
                }

                if (update.Contains("FailTime"))
                {
                    info.Add("FailTime", model.FailTime);
                }
                if (update.Contains("ZhuFu"))
                {
                    info.Add("ZhuFu", model.ZhuFu);
                }
            }
            return info;
        }


        /// <summary>
        /// 取得阶级倍数
        /// </summary>
        /// <param name="model">坐骑</param>
        /// <returns></returns>
        public double Mag(Mounts model)
        {
            double lv = 1;
            GameConfig gc = GameConfigAccess.Instance.FindOneById(model.MountsID);
            if (gc == null)
                return lv;

            Variant v = gc.Value;
            if (v == null)
                return lv;
            Variant jinhua = v.GetVariantOrDefault("JinHua");
            if (jinhua == null)
                return lv;
            Variant info = jinhua.GetVariantOrDefault(model.Rank.ToString());
            if (info == null)
                return lv;
            return info.GetDoubleOrDefault("Mag");
        }
    }
}
