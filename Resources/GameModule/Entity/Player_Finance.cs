using System;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Sinan.AMF3;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.Entity
{
    /// <summary>
    /// Player的财务
    /// </summary>
    partial class Player
    {
        /// <summary>
        /// 晶币支出总额(Gross Coin Expediture)
        /// </summary>
        public Int64 GCE
        {
            get;
            set;
        }

        /// <summary>
        /// 晶币收入总额(Gross Coin Income)
        /// </summary>
        public Int64 GCI
        {
            get;
            set;
        }

        /// <summary>
        /// 点券
        /// </summary>
        public Int64 Bond
        {
            get;
            set;
        }

        /// <summary>
        /// 晶币
        /// </summary>
        public Int64 Coin
        {
            get;
            set;
        }

        /// <summary>
        /// 石币
        /// </summary>
        public Int64 Score
        {
            get;
            set;
        }

        /// <summary>
        /// 战绩
        /// </summary>
        public Int32 FightValue
        {
            get;
            set;
        }

        /// <summary>
        /// 感恩值
        /// </summary>
        public Int32 Owe
        {
            get;
            set;
        }

        /// <summary>
        /// 星力
        /// </summary>
        public Int32 StarPower
        {
            get;
            set;
        }

        /// <summary>
        /// 写金钱(Coin/Score)
        /// </summary>
        /// <param name="writer"></param>
        public void WriteFinance(IExternalWriter writer)
        {
            writer.WriteKey("Coin");
            writer.WriteDouble((double)Coin);

            writer.WriteKey("Score");
            writer.WriteDouble((double)Score);

            writer.WriteKey("Bond");
            writer.WriteDouble((double)Bond);
        }
    }
}