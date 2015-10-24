using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Timers;
using Sinan.FrontServer;
using Sinan.Schedule;
using Sinan.Command;
using Sinan.AMF3;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 经验控制
    /// </summary>
    public class ExperienceControl : ExternalizableBase
    {
        static string m_msg;

        /// <summary>
        /// 全局经验系数(手动开活动用)
        /// </summary>
        static double m_expCoe = 1;

        /// <summary>
        /// 全局经验系数(自动开活动用)
        /// </summary>
        static double m_expCoePart;

        static public double ExpCoe
        {
            get { return Math.Max(m_expCoe, m_expCoePart); }
        }

        static public void SetExpCoePart(double exp)
        {
            m_expCoePart = exp;
            //if (exp != 1.0)
            //{
            //    //发送消息
            //    PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { m_instance });
            //}
        }

        static public void SetExpCoe(double exp)
        {
            m_expCoe = exp;
            if (exp != 1.0)
            {
                //发送消息
                PlayersProxy.CallAll(PartCommand.PartStartR, new object[] { m_instance });
            }
        }
        readonly System.Threading.Timer m_timer;
        readonly static ExperienceControl m_instance = new ExperienceControl();

        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static ExperienceControl Instance
        {
            get { return m_instance; }
        }

        ExperienceControl()
        {
            m_timer = new System.Threading.Timer(CheckState);
        }


        /// <summary>
        /// 起始时间
        /// </summary>
        DateTime m_start;

        /// <summary>
        /// 结束时间
        /// </summary>
        DateTime m_end;

        /// <summary>
        /// 倍数
        /// </summary>
        double m_expC; //倍数

        public void SetExp(DateTime startTime, DateTime endTime, double exp, string msg)
        {
            m_start = startTime;
            m_end = endTime;
            m_expC = exp;
            m_msg = msg;
            DateTime now = DateTime.UtcNow;
            int s = (int)(m_start - now).TotalMilliseconds;
            if (s > 0)
            {
                m_timer.Change(s, Timeout.Infinite);
            }
            else
            {
                s = (int)(m_end - now).TotalMilliseconds;
                if (s > 0)
                {
                    SetExpCoe(m_expC);
                    m_timer.Change(s, Timeout.Infinite);
                }
                else
                {
                    SetExpCoe(1.0);
                }
            }
        }

        void CheckState(Object stateInfo)
        {
            DateTime now = DateTime.UtcNow;
            if (now > m_start && now < m_end)
            {
                int s = (int)(m_end - now).TotalMilliseconds;
                SetExpCoe(m_expC);
                m_timer.Change(s, Timeout.Infinite);
            }
            else
            {
                SetExpCoe(1.0);
            }
        }

        protected override void WriteAmf3(IExternalWriter writer)
        {
            writer.WriteKey("ID");
            writer.WriteUTF("PA_ManyTimesExperience");

            writer.WriteKey("Start");
            writer.WriteDateTime(m_start);

            writer.WriteKey("End");
            writer.WriteDateTime(m_end);

            writer.WriteKey("Exp");
            writer.WriteDouble(ExpCoe);

            writer.WriteKey("Msg");
            writer.WriteUTF(m_msg);
        }
    }
}
