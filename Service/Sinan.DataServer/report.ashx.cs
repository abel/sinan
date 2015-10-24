using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;

namespace Sinan.DataServer
{
    /// <summary>
    /// report 的摘要说明
    /// </summary>
    public class report : IHttpHandler
    {
        MongoDatabase database;
        MongoCollection collection;

        public report()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["gamebase"].ConnectionString;
            database = MongoDatabase.Create(connectionString);
            collection = database.GetCollection("report");
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            string msg = context.Request.Url.Query;
            if (!string.IsNullOrEmpty(msg))
            {
                var query = Query.EQ("_id", ObjectId.GenerateNewId());
                var update = MongoDB.Driver.Builders.Update.Set("Msg", msg);
                collection.Update(query, update, UpdateFlags.Upsert, SafeMode.False);
            }
            context.Response.Write("{\"ret\":0,\"msg\":\"成功\"}");
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}