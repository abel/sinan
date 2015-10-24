using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Threading;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Log;

namespace Sinan.ReportService
{
    [System.ComponentModel.DesignerCategory("Class")]
    public partial class DCService : ServiceBase
    {
        public DCService()
        {
            this.ServiceName = "DCService";
        }

        Thread loggerThread;
        protected override void OnStart(string[] args)
        {
            loggerThread = new Thread(new ThreadStart(Exec));
            loggerThread.Start();
        }

        protected override void OnStop()
        {
            loggerThread.Abort();
            loggerThread = null;
            LogWrapper.Warn("服务结束");
        }

        IMongoQuery QS0 = Query.EQ("S", 0);
        UpdateBuilder US1 = Update.Set("S", 1);

        /// <summary>
        /// 数据操作基类
        /// </summary>
        public void Exec()
        {
            LogWrapper.Warn("初始化");
            try
            {
                string page = ConfigurationManager.AppSettings["ReportPage"];
                string host = ConfigurationManager.AppSettings["ReportHost"];
                int onceRead = int.Parse(ConfigurationManager.AppSettings["onceRead"]);
                string gameLog = ConfigurationManager.AppSettings["gameLog"];

                ServerReport instance = new ServerReport(host, page);

                MongoDatabase db = MongoDatabase.Create(gameLog);
                if (!db.CollectionExists("Log"))
                {
                    LogWrapper.Warn("配置错误,日志数据不存在");
                }
                LogWrapper.Warn("服务开始...");

                MongoCollection collection = db.GetCollection("Log");
                Queue<ObjectId> ids = new Queue<ObjectId>(onceRead);
                while (true)
                {
                    try
                    {
                        //得到最小的ID.
                        BsonDocument doc = collection.FindAs<BsonDocument>(QS0).SetLimit(1).SetSortOrder("_id").SetFields("_id").FirstOrDefault();
                        if (doc != null)
                        {
                            BsonValue min = doc["_id"];
                            while (true)
                            {
                                int count = 0;
                                IMongoQuery query = Query.And(Query.GTE("_id", min), QS0);
                                var logs = collection.FindAs<QQLog>(query).SetLimit(onceRead).SetSortOrder("_id");
                                foreach (QQLog log in logs)
                                {
                                    count++;
                                    if (instance.Request(log))
                                    {
                                        min = log.ID;
                                        ids.Enqueue(log.ID);
                                    }
                                }
                                if (count == 0)
                                {
                                    break;
                                }
                                UpdateS(collection, ids);
                                System.Threading.Thread.Sleep(0);
                            }
                        }
                        else
                        {
                            LogWrapper.Warn("无数据");
                        }
                        System.Threading.Thread.Sleep(2000);
                    }
                    catch (Exception err)
                    {
                        LogWrapper.Error(err);
                        System.Threading.Thread.Sleep(10000);
                    }
                }
            }
            catch (Exception err)
            {
                LogWrapper.Error(err);
            }
        }

        MongoUpdateOptions upOption = new MongoUpdateOptions()
        {
            CheckElementNames = false,
            Flags = UpdateFlags.None
        };
        private void UpdateS(MongoCollection collection, Queue<ObjectId> ids)
        {
            int count = ids.Count;
            if (count > 0)
            {
                ObjectId id;
                for (int i = 0; i < count; i++)
                {
                    id = ids.Dequeue();
                    if (id != null)
                    {
                        try
                        {
                            collection.Update(Query.EQ("_id", id), US1, upOption);
                        }
                        catch
                        {
                            ids.Enqueue(id);
                            System.Threading.Thread.Sleep(10000);
                        }
                    }
                }
            }
        }

    }
}
