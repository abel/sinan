using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.Extensions;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Security.Cryptography;
using Sinan.Util;

namespace Sinan.ShipmentService
{
    /// <summary>
    /// 路由流程
    /// </summary>
    class ShipmentRouterQY : ShipmentRouter
    {
        string m_rechargeKey;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">路径验证</param>
        public ShipmentRouterQY(string path, string rkey)
            : base(path, false)
        {
            m_rechargeKey = rkey;
        }

        public override CompactResponse Process(Socket client, CompactRequest request)
        {
            CompactResponse response = new CompactResponse();
            response.Ret = 4;
            try
            {
                string auth, sign;
                if (request.Queries == null
                    || request.Queries.Count < 2
                    || (!request.Queries.TryGetValue("auth", out auth))
                    || (!request.Queries.TryGetValue("sign", out sign))
                    )
                {
                    LogWrapper.Warn("ShipErr:" + request.Url);
                    response.Msg = "404";
                    return response;
                }

                //检查签名.
                if (!string.IsNullOrEmpty(m_rechargeKey))
                {
                    string test = MD5Helper.MD5Encrypt(auth + m_rechargeKey);
                    if (test != sign.ToUpper())
                    {
                        return Send(client, OrderResult.SignError);
                    }
                }

                Order order = CreateOrder(auth, sign);
                OrderResult r = OrderTypeManager.CheckOrder(order, 1000);
                if (r != OrderResult.Success)
                {
                    return Send(client, r);
                }

                var host = FrontManager.Instance.GetValue(order.zoneid);
                if (host == null)
                {
                    return Send(client, OrderResult.InvalidSid);
                }
                //记录日志,插入数据库.状态为1
                order.state = 1;
                order.Url = request.Url;

                bool write = false;
                try
                {
                    write = OrderAccess.Instance.Insert(order);
                    if (!write)
                    {
                        return Send(client, OrderResult.OrderExists);
                    }
                }
                catch { }
                if (write && client.Connected)
                {
                    //成功
                    byte[] data = Encoding.UTF8.GetBytes("1");
                    client.Send(new ArraySegment<byte>[]{
                                    new ArraySegment<byte>(CompactResponse.HttpOK),
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.Length + "\r\n\r\n")),
                                    new ArraySegment<byte>(data)}
                           );
                    client.Shutdown(SocketShutdown.Both);
                    //更新数据库状态,趣游接收消息成功(扣款成功)(1-->2)
                    if (OrderAccess.Instance.IncOrderState(order.token, 1, 1))
                    {
                        Route(order);
                    }
                    return null;
                }
                return Send(client, OrderResult.OtherError);
            }
            catch (System.Exception ex)
            {
                response.Msg = ex.Message;
                LogWrapper.Error(ex);
            }
            return response;
        }

        /// <summary>
        /// 路由到游戏(发货)服务器
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public override void Route(Order order)
        {
            var host = FrontManager.Instance.GetValue(order.zoneid);
            if (host == null)
            {
                LogWrapper.Warn("找不到服务器:" + order.zoneid);
            }
            try
            {
                //转发到游戏服务器
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    s.Connect(host);
                    if (s.Connected)
                    {
                        string head = "GET " + order.Url;

                        LogWrapper.Warn("Send:" + order.Url);

                        byte[] bin = new byte[1024];
                        int headLen = Encoding.UTF8.GetBytes(head, 0, head.Length, bin, 0);
                        Buffer.BlockCopy(sendEnds, 0, bin, headLen, sendEnds.Length);

                        s.Send(bin, 0, headLen + sendEnds.Length, SocketFlags.None);
                        int len = 0;
                        while (true)
                        {
                            int bytes = s.Receive(bin, len, bin.Length - len, SocketFlags.None);
                            if (bytes == 0)
                            {
                                break;
                            }
                            len += bytes;
                        }
                        string result = (Encoding.UTF8.GetString(bin, 0, len));
                        LogWrapper.Warn(order.zoneid + ":" + result);
                        if (result == "1" || result.EndsWith("\r\n\r\n1"))
                        {
                            //发送成功(2-->3))
                            OrderAccess.Instance.IncOrderState(order.token, 2, 1);
                        }
                        else
                        {
                            //返回失败(2-->4)
                            OrderAccess.Instance.IncOrderState(order.token, 2, 2);
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(host.ToString() + ex);
            }
        }

        private Order CreateOrder(string authBase64, string sign)
        {
            string auth = StringEncoding.Base64StringDecode(authBase64);
            Variant token = new Variant();
            string[] kvs = auth.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kv in kvs)
            {
                int index = kv.IndexOf('=');
                if (index > -1)
                {
                    string key = kv.Substring(0, index);
                    string value = kv.Substring(index + 1);
                    token.Add(key, value);
                }
            }

            string oid = token.GetStringOrEmpty("oid");
            if (string.IsNullOrEmpty(oid))
            {
                return null;
            }

            Order order = new Order();
            order.billno = oid;
            order.token = oid;
            order.openid = token.GetStringOrEmpty("uid").TrimStart('0');
            order.money = token.GetIntOrDefault("money");
            order.Coin = token.GetIntOrDefault("gold");
            order.providetype = token.GetIntOrDefault("otype");
            order.ts = token.GetInt64OrDefault("time");
            order.zoneid = token.GetIntOrDefault("sid");
            order.payitem = auth;
            order.sig = sign;
            order.Created = DateTime.UtcNow;
            return order;
        }

        private static CompactResponse Send(Socket client, OrderResult ret)
        {
            try
            {
                if (client.Connected)
                {
                    //傳回Response
                    byte[] data = Encoding.UTF8.GetBytes(((int)ret).ToString());
                    //寫入資料本體
                    client.Send(new ArraySegment<byte>[]{
                                    new ArraySegment<byte>(CompactResponse.HttpOK),
                                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(data.Length + "\r\n\r\n")),
                                    new ArraySegment<byte>(data)}
                           );
                    client.Shutdown(SocketShutdown.Both);
                }
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(ex);
            }
            return null;
        }
    }

}
