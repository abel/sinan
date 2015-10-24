using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FastSocket
{
    /// <summary>
    /// 安全沙箱检查
    /// </summary>
    public class FlashSandBox
    {
        /// <summary>
        /// 安全沙箱请求(<policy- file-request/>)
        /// 3C 70 6F 6C 69 63 79 2D 66 69 6C 65 2D 72 65 71 75 65 73 74 2F 3E 00
        /// </summary>
        static readonly byte[] policyBytes = Encoding.ASCII.GetBytes("<policy-file-request/>\0");

        /// <summary>
        /// 默认沙箱应答(任意域,任意端口)
        /// </summary>
        public static readonly string DefaultPolicy =
            "<cross-domain-policy><allow-access-from domain=\"*\" to-ports=\"*\"/></cross-domain-policy>";

        // 安全沙箱应答
        byte[] m_crossDomain;

        /// <summary>
        /// 跨域策略
        /// </summary>
        public byte[] CrossDomainPolicy
        {
            get { return m_crossDomain; }
        }

        public FlashSandBox(string crossDomainPolicy)
        {
            if (string.IsNullOrWhiteSpace(crossDomainPolicy))
            {
                crossDomainPolicy = DefaultPolicy;
            }
            m_crossDomain = Encoding.ASCII.GetBytes(crossDomainPolicy + "\0");
        }


        /// <summary>
        /// 检查是否是PolicyRequestFile
        /// </summary>
        /// <param name="bin"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns>成功:安全沙箱请求的长度,失败:0</returns>
        public static int IsPolicyRequest(byte[] bin, int offset, int count)
        {
            if (count < policyBytes.Length)
            {
                return 0;
            }
            for (int i = 0; i < policyBytes.Length; i++)
            {
                if (policyBytes[i] != bin[i + offset])
                {
                    return 0;
                }
            }
            return policyBytes.Length;
        }

     
        /// <summary>
        /// 检查是否是PolicyRequestFile
        /// </summary>
        /// <param name="bin"></param>
        /// <returns>成功:安全沙箱请求的长度,失败:0</returns>
        public static int IsPolicyRequest(byte[] bin)
        {
            return IsPolicyRequest(bin, 0, bin.Length);
        }
    }
}
