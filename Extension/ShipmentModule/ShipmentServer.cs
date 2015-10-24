using System.Net.Sockets;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;

namespace Sinan.ShipmentModule
{
    public class ShipmentServer : HttpServer
    {
        /// <summary>
        /// 处理接收的分货请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public override CompactResponse Process(Socket client, CompactRequest request)
        {
            try
            {
                CompactResponse response = new CompactResponse();
                response.Ret = 4;
                if (request.Queries == null || request.Queries.Count < 10)
                {
                    LogWrapper.Warn("ShipErr:" + request.Url);
                    response.Msg = "404";
                    return response;
                }

                Order order = Order.CreateShipment(request.Queries);
                order.Url = request.Url;
                response.Msg = order.token;

                //验证签名
                if (ServerManager.CheckSig)
                {
                    int index = request.Url.IndexOf('?');
                    if (index > 0)
                    {
                        string path = request.Url.Substring(0, index);
                        string sig = order.GetSig(path);
                        if (sig != order.sig)
                        {
                            response.Msg = "请求参数错误(sig)";
                            return response;
                        }
                    }
                    else
                    {
                        response.Msg = "404";
                        return response;
                    }
                }

                //根据Token取角色ID
                int pid = OrderAccess.Instance.Replenish(order, 11);
                PlayerBusiness player = PlayersProxy.FindPlayerByID(pid);
                if (player == null)
                {
                    response.Ret = 3;
                    return response;
                }

                int totalCoin = 0;
                //取商品
                try
                {
                    ShipInfo[] shipInfos = order.GetShipInfos();
                    foreach (ShipInfo shipInfo in shipInfos)
                    {
                        GameConfig gs = GameConfigAccess.Instance.FindOneById(shipInfo.GoodID);
                        if (gs == null)
                        {
                            response.Ret = 2;
                            return response;
                        }
                        int num = shipInfo.Num;
                        int coin = gs.UI.GetIntOrDefault("Coin");
                        //int price = gs.UI.GetIntOrDefault("Price");
                        //if (price != shipInfo.Price)
                        //{
                        //    resp.Ret = 3;
                        //    return resp;
                        //}
                        if (coin > 0)
                        {
                            totalCoin += coin * num;
                        }
                    }
                }
                catch (System.Exception ex)
                {
                    response.Msg = ex.Message;
                    LogWrapper.Error("充值:" + request.Url, ex);
                    return response;
                }

                order.Coin = totalCoin;
                if (ResponseCompleted(client, order, player))
                {
                    return null;
                }
                response.Ret = 2;
                return response;
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
                        try
                        {
                            client.Send(CompactResponse.RetOK);
                            client.Shutdown(SocketShutdown.Both);
                        }
                        catch { }
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
    }
}
