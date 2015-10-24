using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace Sinan.FastSocket
{
    public class HttpArgs
    {
        /// <summary>
        /// 保存原始的请求
        /// </summary>
        public string Url { get; set; }
        public string Host { get; set; }
        //public string Accept { get; set; }
        //public string Referer { get; set; }
        //public string Cookie { get; set; }
        public string Body { get; set; }
    }

    /// <summary>
    /// 提交方法
    /// </summary>
    public enum HttpMethod
    {
        GET,
        POST
    }

    //Reuquest物件 
    public class CompactRequest : HttpArgs
    {
        public HttpMethod Method
        {
            get;
            set;
        }

        public string Protocol
        {
            get;
            set;
        }

        public Dictionary<string, string> Queries
        {
            get;
            set;
        }

        //傳入StreamReader，讀取Request傳入的內容
        public CompactRequest(StreamReader sr)
        {
            //第一列格式如: GET /index.html HTTP/1.1
            string firstLine = sr.ReadLine();
            string[] p = firstLine.Split(' ');
            Method = p[0] == "Get" ? HttpMethod.GET : HttpMethod.POST;

            Url = (p.Length > 1) ? p[1] : "NA";
            Protocol = (p.Length > 2) ? p[2] : "NA";

            //从Url中解码参数
            //string parm = HttpUtility.UrlDecode(Url);
            int index = Url.IndexOf('?');
            if (index > -1)
            {
                Queries = new Dictionary<string, string>();
                string[] kvs = Url.Substring(index + 1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kv in kvs)
                {
                    index = kv.IndexOf('=');
                    if (index > -1)
                    {
                        string key = HttpUtility.UrlDecode(kv.Substring(0, index));
                        string value = HttpUtility.UrlDecode(kv.Substring(index + 1));
                        Queries.Add(key, value);
                    }
                }
            }
        }
    }

    //Response物件 
    public class CompactResponse
    {
        public readonly static byte[] RetOK;
        public readonly static byte[] HttpOK;

        static CompactResponse()
        {
            HttpOK = Encoding.UTF8.GetBytes("HTTP/1.1 200 OK\r\nContent-type:text/html;charset=utf-8\r\nContent-Length:");

            byte[] dataOk = Encoding.UTF8.GetBytes("{\"ret\":0,\"msg\":\"OK\"}");
            byte[] lenInfo = Encoding.UTF8.GetBytes(dataOk.Length + "\r\n\r\n");

            int okLen = HttpOK.Length + lenInfo.Length + dataOk.Length;
            RetOK = new byte[okLen];
            Array.Copy(HttpOK, RetOK, HttpOK.Length);
            Array.Copy(lenInfo, 0, RetOK, HttpOK.Length, lenInfo.Length);
            Array.Copy(dataOk, 0, RetOK, HttpOK.Length + lenInfo.Length, dataOk.Length);
        }

        /// <summary>
        /// 0: 成功 
        /// 1: 系统繁忙 
        /// 2: token已过期 
        /// 3: token不存在 
        /// 4: 请求参数错误：（Msg这里填写错误的具体参数）
        /// 
        /// (趣游用)
        /// 1: 成功
        /// 10:无效服务器编号
        /// 11:无效玩家帐号
        /// 12:订单号已存在
        /// 13:无效订单类型
        /// 14:无效时间戳
        /// 15:充值金额错误
        /// 16:虚拟币数量错误
        /// 17:校验码错误
        /// 18:其它错误
        /// </summary>
        public int Ret;
        public string Msg;
        public string GetDate()
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("{\"ret\":");
            sb.Append(Ret.ToString());
            sb.Append(",\"msg\":\"");
            sb.Append(Msg);
            sb.Append("\"}");
            return sb.ToString();
        }

    }
}