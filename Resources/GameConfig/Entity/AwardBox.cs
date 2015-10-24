using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Entity
{
    /// <summary>
    /// 开箱奖励
    /// </summary>
    public class AwardBox : Award
    {
        public AwardBox(string boxID)
        {
            m_boxID = boxID;
        }

        string m_boxID;
        public string BoxID
        {
            get { return m_boxID; }
        }

        protected override void WriteAmf3(AMF3.IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF(BoxID);

            base.WriteAmf3(writer);
        }
    }
}
