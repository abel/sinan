using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Sinan.Command;
using Sinan.Core;
using Sinan.Extensions;
using Sinan.FastJson;
using Sinan.FastSocket;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Log;
using Sinan.Observer;
using Sinan.Util;
using Tencent.OpenSns;

namespace Sinan.ShipmentModule
{
    /// <summary>
    /// 发货流程
    /// </summary>
    sealed public class ShipmentMediator : AysnSubscriber
    {
        static Encoding encoder = Encoding.GetEncoding(936);
        ShipmentServer server;

        #region ISubscriber 成员
        public override IList<string> Topics()
        {
            return new string[]
            {
                Application.APPSTART,
                Application.APPSTOP,
                MallCommand.CoinOrder,
                MallCommand.NewToken,
                MallCommand.Sign,
            };
        }

        public override void Execute(INotification notification)
        {
            UserNote note = notification as UserNote;

            if (note != null)
            {
                this.ExecuteNote(note);
            }
            else
            {
                if (notification.Name == Application.APPSTART)
                {
                    StartShipServer();
                }
                else if (notification.Name == Application.APPSTOP)
                {
                    if (server != null)
                    {
                        server.Stop();
                        server = null;
                    }
                }
            }
        }

        private void StartShipServer()
        {
            var add = ConfigLoader.Config.EpShip;
            if (add != null)
            {
                try
                {
                    if (server != null)
                    {
                        server.Stop();
                        server = null;
                    }
                }
                catch { }
                server = ConfigLoader.Config.Platform == "tencent" ? new ShipmentServer() : new ShipmentServerQY();
                server.Start(add);
            }
        }
        #endregion

        void ExecuteNote(UserNote note)
        {
            switch (note.Name)
            {
                case MallCommand.NewToken:
                    NewOrder(note);
                    return;
                case MallCommand.CoinOrder:
                    CreateOrder(note);
                    return;
                case MallCommand.Sign:
                    Sign(note);
                    return;
            }
        }

        private void NewOrder(UserNote note)
        {
            UserSession user = note.Session;
            Variant v = note.GetVariant(0);

            string vs = JsonConvert.SerializeObject(v);
            //LogWrapper.Warn("请求:" + vs);

            BuyGoodsRequest request = new BuyGoodsRequest();

            string meta = v.GetStringOrDefault("goodsmeta");
            request.goodsmeta = Convert.ToBase64String(Encoding.UTF8.GetBytes(meta));
            request.goodsurl = v.GetStringOrDefault("goodsurl");
            request.openid = user.UserID.PadLeft(32, '0');
            request.openkey = user.key;
            request.payitem = v.GetStringOrDefault("payitem");
            request.pf = v.GetStringOrDefault("platform") ?? v.GetStringOrDefault("pf");
            request.pfkey = v.GetStringOrDefault("pfkey");
            request.appmode = v.GetIntOrDefault("appmode", 2);
            request.device = v.GetIntOrDefault("device");
            request.zoneid = ServerLogger.zoneid;

            string host = ServerManager.BuyHost;
            string uri = ServerManager.BuyUri;

            StringBuilder sb = new StringBuilder(512);
            string quest = request.Build(sb, null).ToString();
            string sign = AppSign.Sign(uri, quest);

            sb.Clear();
            sb.Append(uri + "?");
            request.Build(sb, UrlUtility.UrlEncode);
            sb.Append("&sig=");
            sb.Append(sign);

            HttpArgs a = new HttpArgs();
            a.Host = host;
            a.Url = sb.ToString();

            //LogWrapper.Warn("请求Url:" + a.Url);

            IPEndPoint ipadd = IPHelper.CreateEndPoint(host, 443);
            byte[] bin = SslHttpClient.Get(ipadd, a, null);
            string result = Encoding.UTF8.GetString(bin);

            Variant re = JsonConvert.DeserializeObject<Variant>(result);
            if (re.GetIntOrDefault("ret", -1) == 0)
            {
                string token = re.GetStringOrDefault("token");
                if (!string.IsNullOrEmpty(token))
                {
                    if (CreateOrder(note.Player, token))
                    {
                        user.Call(MallCommand.NewTokenR, true, re);
                        return;
                    }
                }
            }
            user.Call(MallCommand.NewTokenR, false, re);
            //LogWrapper.Warn("请求结果:" + result);
        }

        /// <summary>
        /// 对用户数据签名
        /// </summary>
        /// <param name="note"></param>
        private void Sign(UserNote note)
        {
            if (note == null || note.Player == null)
            {
                return;
            }
            string par = note.GetString(0);
            string par2 = note.GetString(1);
            string sig = Tencent.OpenSns.AppSign.Sign(par);
            sig = UrlUtility.UrlEncode(sig);
            note.Call(MallCommand.SignR, sig, par, par2);
        }

        //保存订单到数据库
        private void CreateOrder(UserNote note)
        {
            if (note == null || note.Player == null)
            {
                return;
            }
            Variant v = note.GetVariant(0);
            if (v == null)
            {
                note.Call(MallCommand.CoinOrderR, false, v);
                return;
            }
            string token = v.GetStringOrDefault("token");
            if (string.IsNullOrEmpty(token))
            {
                note.Call(MallCommand.CoinOrderR, false, v);
                return;
            }
            bool result = CreateOrder(note.Player, token);
            note.Call(MallCommand.CoinOrderR, result, v);
        }

        private static bool CreateOrder(PlayerBusiness player, string token)
        {
            Order log = new Order();
            log.Created = DateTime.UtcNow;
            log.token = token;
            log.openid = player.UserID;
            log.pid = player.PID;
            log.zoneid = ServerLogger.zoneid;
            bool result = OrderAccess.Instance.Save(log);
            return result;
        }

    }
}
