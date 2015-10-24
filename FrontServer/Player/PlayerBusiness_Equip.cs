using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.AMF3;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    partial class PlayerBusiness
    {
        public void ExitFight(bool autoFull, int hp, int mp)
        {
            bool add = false;
            //自动加满
            if (autoFull)
            {
                this.HP = Math.Max(1, hp);
                this.MP = mp;
                Abrade();
                add = AutoFull();
            }
            SetActionState(ActionState.Standing);
            FightTime = DateTime.UtcNow;
            Fight = null;
            if (!add)
            {
                Variant v = new Variant(3);
                v["ShengMing"] = new MVPair(Life.ShengMing, HP);
                v["MoFa"] = new MVPair(Life.MoFa, MP);
                UpdataActorR(v);
            }
        }

        /// <summary>
        /// 计算装备磨损
        /// </summary>
        private void Abrade()
        {
            int index = NumberRandom.Next(10);
            var p = m_equips.Value.GetVariantOrDefault("P" + index);

            if (p == null) 
                return;

            string goodsid = p.GetStringOrDefault("E");

            if (string.IsNullOrEmpty(goodsid)) 
                return;

            Goods g = GoodsAccess.Instance.FindOneById(goodsid);

            if (g == null) 
                return;

            Variant v = g.Value;
            if (v == null) 
                return;

            Variant stamina = v.GetVariantOrDefault("Stamina");
            if (stamina.GetIntOrDefault("V") <= 0)
                return;

            stamina.SetOrInc("V", -1);
            if (!g.Save()) 
                return;

            Variant t = new Variant();
            t.Add("ID", g.ID);
            t.Add("Stamina", stamina);
            //更新道具持久度
            Call(GoodsCommand.GetGoodsDetailR, true, t);

            //重新计算角色属性
            if (stamina.GetIntOrDefault("V") <= 0)
            {
                RefreshPlayer(string.Empty, null, ResetEquipsAdd);
            }
            Call(GoodsCommand.GetEquipPanelR, true, g.ID);
        }
    }
}