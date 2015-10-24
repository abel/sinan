using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.AMF3;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 副本明雷
    /// </summary>
    public class EctypeApc : ActiveApc
    {
        protected VisibleApc m_apc;

        /// <summary>
        /// 剧情
        /// </summary>
        public int Say
        {
            get;
            set;
        }

        /// <summary>
        /// 击杀顺序(大于0为必杀)
        /// </summary>
        public int Batch
        {
            get;
            set;
        }

        /// <summary>
        /// 进行战斗的怪
        /// </summary>
        public VisibleApc Apc
        {
            get { return m_apc; }
        }

        public EctypeApc(string apcid)
        {
            m_apc = VisibleAPCManager.Instance.FindOne(apcid);
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("X");
            writer.WriteInt(X);
            writer.WriteKey("Y");
            writer.WriteInt(Y);

            writer.WriteKey("Say");
            writer.WriteInt(Say);

            writer.WriteKey("Batch");
            writer.WriteInt(Batch);

            writer.WriteKey("Name");
            writer.WriteUTF(m_apc.Name);

            writer.WriteKey("Skin");
            writer.WriteUTF(m_apc.Skin);

            writer.WriteKey("ID");
            writer.WriteInt(ID);
        }
    }
}