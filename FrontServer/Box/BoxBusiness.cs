using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Sinan.AMF3;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;
using Sinan.Util;
using System;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 箱子(副本模式)
    /// </summary>
    public class BoxBusiness : ExternalizableBase
    {
        protected Box m_box;
        protected Point m_point;
        protected List<Point> m_bornPlace;

        public virtual bool CanOpen
        {
            get { return true; }
        }

        public BoxBusiness(Box box)
        {
            m_box = box;
        }

        public string SceneID
        {
            get { return m_box.SceneID; }
        }

        /// <summary>
        /// 初始化降生点
        /// </summary>
        /// <param name="points">可行走区域</param>
        /// <param name="pins">排除传送阵区域</param>
        public void InitBron(IList<Point> points, IList<Rectangle> pins)
        {
            m_bornPlace = new List<Point>();
            if (m_box.Range != Rectangle.Empty)
            {
                foreach (var p in points)
                {
                    if (m_box.Range.Contains(p))
                    {
                        if (!pins.Any(x => x.Contains(p)))
                        {
                            m_bornPlace.Add(p);
                        }
                    }
                }
            }
            if (m_bornPlace.Count == 0)
            {
                foreach (var p in points)
                {
                    if (!pins.Any(x => x.Contains(p)))
                    {
                        m_bornPlace.Add(p);
                    }
                }
            }
            int index = NumberRandom.Next(m_bornPlace.Count);
            this.m_point = m_bornPlace[index];
        }

        public bool OpenBox(PlayerBusiness player)
        {
            string msg = CheckBox(player);
            if (msg == null)
            {
                if (!string.IsNullOrEmpty(m_box.GoodsID) && (!BurdenManager.Remove(player.B0, m_box.GoodsID, 1)))
                {
                    //缺少钥匙;
                    msg = TipManager.GetMessage(ClientReturn.CheckBox3);
                }
            }
            if (msg != null)
            {
                //开箱失败
                player.Call(ClientCommand.OpenBoxR, false, msg);
                return false;
            }
            //开箱奖励
            GetAward(player);
            return true;
        }

        protected virtual string CheckBox(PlayerBusiness player)
        {
            if (player.SceneID != m_box.SceneID)
            {
                //你已跨场景;
                return TipManager.GetMessage(ClientReturn.CheckBox1);
            }
            int x = m_point.X - player.X;
            int y = m_point.Y - player.Y;
            if (x * x + y * y > 50)
            {
                return TipManager.GetMessage(ClientReturn.CheckBox1);
            }

            int count = player.ReadDaily(PlayerBusiness.DailyBox, m_box.ID);
            if (count >= m_box.MaxOpen)
            {
                //当天开启数达到限制数;
                return TipManager.GetMessage(ClientReturn.CheckBox2);
            }
            bool needKey = (!string.IsNullOrEmpty(m_box.GoodsID));
            if (needKey && BurdenManager.GoodsCount(player.B0, m_box.GoodsID) == 0)
            {
                //缺少钥匙;
                return TipManager.GetMessage(ClientReturn.CheckBox3);
            }
            if (player.AState == ActionState.Fight)
            {
                //你已进入战斗中,无法打开宝箱;
                return TipManager.GetMessage(ClientReturn.CheckBox4);
            }
            return null;
        }

        protected void GetAward(PlayerBusiness player)
        {
            player.WriteDaily(PlayerBusiness.DailyBox, m_box.ID);
            AwardBox award = m_box.GetAward();
            if (award.Score > 0)
            {
                player.AddScore(award.Score, FinanceType.OpenBox);
            }
            if (award.Bond > 0)
            {
                player.AddBond(award.Bond, FinanceType.OpenBox);
            }
            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            foreach (var k in award.Goods)
            {
                Variant v = new Variant(1);
                v.Add("Number0", k.Value);
                dic.Add(k.Key, v);
            }
            player.AddGoods(dic, GoodsSource.OpenBox);
            //发送结果
            player.Call(ClientCommand.OpenBoxR, true, award);
        }

        public virtual void Reset()
        {
            int index = NumberRandom.Next(m_bornPlace.Count);
            this.m_point = m_bornPlace[index];
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(m_box.ID);
            writer.WriteKey("Name");
            writer.WriteUTF(m_box.Name);

            writer.WriteKey("GoodsID");
            writer.WriteUTF(m_box.GoodsID);
            writer.WriteKey("OpenMS");
            writer.WriteInt(m_box.OpenMS);
            writer.WriteKey("Skin");
            writer.WriteUTF(m_box.Skin);

            writer.WriteKey("X");
            writer.WriteInt(m_point.X);
            writer.WriteKey("Y");
            writer.WriteInt(m_point.Y);
        }
    }
}
