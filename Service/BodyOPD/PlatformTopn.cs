using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace Sinan.BabyOPD
{
    /// <summary>
    /// 排名
    /// </summary>
    public class PlatformTopn : PlatformBase
    {
        MongoDatabase m_gameLog;
        public PlatformTopn(string gameLogString, string operationString)
            : base(operationString)
        {
            m_gameLog = MongoDatabase.Create(gameLogString);
        }
    }
}
