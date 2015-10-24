using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Sinan.FastSocket;
using Sinan.GameModule;
using Sinan.Log;

namespace Sinan.ShipmentService
{
    /// <summary>
    /// 路由流程
    /// </summary>
    class ShipmentRouter : HttpServer
    {
        static long f = Stopwatch.Frequency / 1000;
        static Stopwatch watch = Stopwatch.StartNew();

        protected byte[] sendEnds;
        protected string m_path;
        protected bool m_checkSig;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="path">路径验证</param>
        /// <param name="checkSig">启用签名验证</param>
        public ShipmentRouter(string path, bool checkSig)
        {
            sendEnds = Encoding.UTF8.GetBytes(" HTTP/1.1\r\nHost: " + "sinan.com" + "\r\nConnection: Close\r\n\r\n");
            m_path = path;
            m_checkSig = checkSig;
        }

        public override CompactResponse Process(Socket client, CompactRequest request)
        {
            CompactResponse response = new CompactResponse();
            response.Ret = 4;
            try
            {
                if (request.Queries == null || (!request.Url.StartsWith(m_path)))
                {
                    response.Msg = "404";
                    return response;
                }
                Order order = Order.CreateShipment(request.Queries);
                order.Check = watch.ElapsedTicks;

                var host = FrontManager.Instance.GetValue(order.zoneid);
                if (host == null)
                {
                    response.Msg = "请求参数错误(zoneid)";
                    return response;
                }

                // 验证物品价格
                try
                {
                    int coin = 0;
                    ShipInfo[] shipInfos = order.GetShipInfos();
                    foreach (ShipInfo shipInfo in shipInfos)
                    {
                        int price = GoodsManager.Instance.GetValue(shipInfo.GoodID);
                        if (price <= 0 || price != shipInfo.Price)
                        {
                            response.Msg = "请求参数错误(payitem)";
                            return response;
                        }
                        coin += shipInfo.Price * shipInfo.Num;
                    }
                    order.Coin = coin;
                    //if (log.amt < amt * 6) //最低6折?
                    //{
                    //}
                }
                catch
                {
                    response.Msg = "请求参数错误(payitem)";
                    return response;
                }

                //记录日志,插入数据库.状态为1
                order.state = 1;
                order.Url = request.Url;

                bool write = false;
                try
                {
                    write = (OrderAccess.Instance.Insert(order)
                        || OrderAccess.Instance.Exists(order.token));
                }
                catch { }
                if (write)
                {
                    ResponseCompleted(client, order);
                    return null;
                }
            }
            catch (System.Exception ex)
            {
                response.Msg = ex.Message;
                LogWrapper.Error(ex);
            }
            return response;
        }

        /// <summary>
        /// 返回响应结果
        /// </summary>
        void ResponseCompleted(Socket client, Order order)
        {
            long sendStart = watch.ElapsedTicks;
            int sendByte = client.Send(CompactResponse.RetOK);
            try
            {
                client.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                sendByte = 0;
            }

            long sendEnd = watch.ElapsedTicks;

            if (m_checkSig)
            {
                string sig = order.GetSig(m_path);
                if (sig != order.sig)
                {
                    //(签名错误 1-->6)
                    OrderAccess.Instance.IncOrderState(order.token, 1, 5);
                    return;
                }
            }

            long ms = (sendEnd - order.Check) / f;
            if (ms > 1000 || sendByte != CompactResponse.RetOK.Length)
            {
                //处理时间大于1000毫秒,可能超时(1-->5)
                //需手动检查订单,如果腾讯扣款成功,可将5改为2.系统将自动补发.
                OrderAccess.Instance.IncOrderState(order.token, 1, 4);
            }
            else if (OrderAccess.Instance.IncOrderState(order.token, 1, 1))
            {
                //更新数据库状态,腾讯接收消息成功(扣款成功)(1-->2)
                Route(order);
            }
            if (ms > 100)
            {
                StringBuilder sb = new StringBuilder(100);
                sb.Append("P:");    //处理时间
                sb.Append((sendStart - order.Check) / f);
                sb.Append(",S:");  //发送时间
                sb.Append((sendEnd - sendStart) / f);
                sb.Append("T:");   //总时间
                sb.Append(ms);
                sb.Append(",token:");
                sb.Append(order.token);
                sb.Append(",billno:");
                sb.Append(order.billno);
                LogWrapper.Warn(sb.ToString());
            }
        }

        /// <summary>
        /// 路由到游戏(发货)服务器
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public virtual void Route(Order order)
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
                        if (result.Contains("\"ret\":0"))
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
    }

}
