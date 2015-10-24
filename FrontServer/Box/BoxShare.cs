using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using MongoDB.Bson;
using Sinan.AMF3;
using Sinan.Extensions;
using Sinan.GameModule;
using Sinan.Command;
using Sinan.Entity;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 箱子(共享模式)
    /// </summary>
    public class BoxShare : BoxBusiness
    {
        ///<summary>
        ///用于保证同一时间只有1个用户开启;
        ///0:可以打开.1:已打开
        ///</summary>
        ///</summary>
        protected int m_open;
        protected DateTime m_bornTime;

        public override bool CanOpen
        {
            get { return m_open == 0 && m_bornTime < DateTime.UtcNow; }
        }

        public BoxShare(Box box)
            : base(box)
        {
            m_bornTime = DateTime.UtcNow;
        }


        protected override string CheckBox(PlayerBusiness player)
        {
            string msg = base.CheckBox(player);
            if (msg == null)
            {
                if (Interlocked.Exchange(ref m_open, 1) != 0)
                {
                    //return "已达最大开启数,不能开启";
                    return TipManager.GetMessage(ClientReturn.CheckBox5);
                }
            }
            return msg;
        }



        public override void Reset()
        {
            m_bornTime = DateTime.UtcNow.AddSeconds(m_box.GrowSecond);
            int index = NumberRandom.Next(m_bornPlace.Count);
            this.m_point = m_bornPlace[index];
            m_open = 0;
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
