using System;
using System.Net.Sockets;
using System.Text;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Extensions;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Security.Cryptography;
using Sinan.Util;

namespace Sinan.ShipmentModule
{
    public class ShipmentServerQY : ShipmentServer
    {
        LoginVerification verification = new LoginVerification(ConfigLoader.Config.RechargeKey, ConfigLoader.Config.DesKey);

        /// <summary>
        /// 处理接收的发货请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override CompactResponse Process(Socket client, CompactRequest request)
        {
            try
            {
                CompactResponse response = new CompactResponse();
                response.Ret = 4;

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
                string test = MD5Helper.MD5Encrypt(auth + ConfigLoader.Config.RechargeKey);
                if (test != sign.ToUpper())
                {
                    return Send(client, OrderResult.SignError);
                }

                Order order = CreateOrder(auth, sign);
                //检查时间(7天)
                OrderResult r = OrderTypeManager.CheckOrder(order, 3600 * 24 * 7);
                if (r != OrderResult.Success)
                {
                    return Send(client, r);
                }

                //根据Token取角色ID
                int pid = PlayerAccess.Instance.GetPlayerId(order.zoneid, order.openid);
                PlayerBusiness player = PlayersProxy.FindPlayerByID(pid);
                if (player == null)
                {
                    return Send(client, OrderResult.InvalidUid);
                }

                order.Url = request.Url;
                if (!OrderAccess.Instance.NewOrder(order))
                {
                    return Send(client, OrderResult.OrderExists);
                }

                // 角色正确,状态由0变到11
                OrderAccess.Instance.IncOrderState(order.token, 0, 11);

                if (ResponseCompleted(client, order, player))
                {
                    return null;
                }
                return Send(client, OrderResult.OrderExists);
            }
            catch (System.Exception ex)
            {
                LogWrapper.Error(ex);
                return null;
            }
        }

        private static bool ResponseCompleted(Socket client, Order order, PlayerBusiness player)
        {
            int coin = order.Coin;
            //检查tokenID(状态由11变到12,准备充值,用于锁定)
            if (OrderAccess.Instance.IncOrderState(order.token, 11, 1, coin))
            {
                if (player.AddCoin(coin, FinanceType.BuyCoin))
                {
                    //(状态由12变到13,充值成功)
                    if (OrderAccess.Instance.IncOrderState(order.token, 12, 1))
                    {
                        //成功
                        Send(client, OrderResult.Success);
                        try
                        {
                            //发送充值成功通知.
                            player.Call(MallCommand.CoinSuccess, order.token, coin, player.Coin);
                            UserNote note = new UserNote(player, PartCommand.Recharge, new object[] { coin });
                            Notifier.Instance.Publish(note);
                        }
                        catch { }
                        return true;
                    }
                }
            }
            return false;
        }

        private Order CreateOrder(string authBase64, string sign)
        {
            Variant token = verification.DecryptGamewaveToken(authBase64, sign);
            if (token == null)
            {
                return null;
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
            order.money = token.GetDoubleOrDefault("money");
            order.Coin = token.GetIntOrDefault("gold");
            order.providetype = token.GetIntOrDefault("otype");
            order.ts = token.GetInt64OrDefault("time");
            order.zoneid = token.GetIntOrDefault("sid");
            order.payitem = token.GetStringOrDefault("auth");
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
