using System;
using System.Collections.Generic;
using MongoDB.Bson;
using Sinan.Command;
using Sinan.Data;
using Sinan.Entity;
using Sinan.GameModule;
using Sinan.Util;


namespace Sinan.AuctionModule.Business
{
    /// <summary>
    /// 拍卖相关邮件
    /// </summary>
    public class AuctionEmail
    {
        /// <summary>
        /// 竞价失败
        /// </summary>
        public const string BidFail = "Email_001";
        /// <summary>
        /// 竞价
        /// </summary>
        public const string Bid = "Email_002";
        /// <summary>
        /// 一口价
        /// </summary>
        public const string BidOne = "Email_003";
        /// <summary>
        /// 出售成功
        /// </summary>
        public const string Sell = "Email_004";
        /// <summary>
        /// 一口价出售成功
        /// </summary>
        public const string SellOne = "Email_005";
        /// <summary>
        /// 竞价过期
        /// </summary>
        public const string Expire = "Email_006";
        


        /// <summary>
        /// 系统邮件
        /// </summary>
        public static Email SendAuctionEmail(
            Dictionary<string, object> dic,
            string type, 
            string auctionid, 
            string receiveID, 
            string receiveName,
            int Score,
            int Coin,
            List<Variant> GoodsList
            )
        {
            Email email = new Email();
            email.ID = ObjectId.GenerateNewId().ToString();
            email.Status = 0;//0表示未读取过,1已读取
            email.Ver = 1;
            email.MainType = EmailCommand.System;
            email.Created = DateTime.UtcNow;
            GameConfig gc = GameConfigAccess.Instance.FindOneById(type);
            Variant d = new Variant(16);
            string Title = gc.Value.GetStringOrDefault("Title");
            string Source = gc.Value.GetStringOrDefault("Source");
            string Content = gc.Value.GetStringOrDefault("Content");
            foreach (string k in dic.Keys)
            {
                Title = Title.Replace(k, dic[k].ToString());
                Content = Content.Replace(k, dic[k].ToString());
            }
            email.Name = Title;
            d.Add("Title", Title);
            d.Add("Source", Source);
            d.Add("Content", Content);
            d.Add("SendID", EmailCommand.System);
            d.Add("SendName", Source);
            d.Add("ReceiveID", receiveID);
            d.Add("ReceiveName", receiveName);
            //d.Add("Take", string.Empty);
            DateTime dt = DateTime.UtcNow;
            d.Add("UpdateDate", dt);
            d.Add("EndDate", dt.AddDays(30));
            d.Add("AuctionID", auctionid);
            d.Add("Score", Score);
            d.Add("Coin", Coin);
            d.Add("GoodsList", GoodsList);
            if (d.GetIntOrDefault("Score") > 0 || d.GetIntOrDefault("Coin") > 0 || (GoodsList!=null && GoodsList.Count > 0))
            {                
                d.Add("IsHave", 1);
            }
            else
            {
                d.Add("IsHave", 0);                
            }

            email.Value = d;
            email.Save();
            return email;            
        }
    }
}
