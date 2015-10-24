using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Extensions;

namespace Sinan.FrontServer
{
    public class BufferResult : ExternalizableBase
    {
        FightObject m_fighter;
        Dictionary<string, int> m_buffer = new Dictionary<string, int>();

        public bool HaveBuffer
        {
            get { return m_buffer.Count > 0; }
        }

        public BufferResult(FightObject fighter)
        {
            m_fighter = fighter;
        }

        public int HP
        { get; set; }

        public int MP
        { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="m"></param>
        /// <param name="minHP">保留的生命值</param>
        public void AddHP(string name, int m, int minHP = 0)
        {
            if (m != 0 && m_fighter.HP > 0)
            {
                m_fighter.AddHPAndMP(m, 0, minHP);
                m_buffer.SetOrInc("R" + name, m);
            }
        }

        public void AddMP(string name, int m)
        {
            if (m > 0 && m_fighter.HP > 0)
            {
                m_fighter.AddHPAndMP(0, m);
                m_buffer.SetOrInc("B" + name, m);
            }
        }

        internal void Clear()
        {
            m_buffer.Clear();
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            foreach (var v in m_buffer)
            {
                writer.WriteKey(v.Key);
                writer.WriteInt(v.Value);
            }
            writer.WriteKey("HP");
            writer.WriteInt(HP);

            writer.WriteKey("MP");
            writer.WriteInt(MP);

            writer.WriteKey("ID");
            writer.WriteUTF(m_fighter.ID);
        }
    }
}
