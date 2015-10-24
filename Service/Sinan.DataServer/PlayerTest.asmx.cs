using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.GameModule;
using Sinan.Entity;

namespace Sinan.DataServer
{
    /// <summary>
    /// PlayerTest 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    //[System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    [System.Web.Script.Services.ScriptService]
    public class PlayerTest : System.Web.Services.WebService
    {
        MongoDatabase database;
        MongoCollection<Player> collection;

        public PlayerTest()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["gamebase"].ConnectionString;
            database = MongoDatabase.Create(connectionString);
            collection = database.GetCollection<Player>("Player");
        }

        [WebMethod]
        public string HelloWorld()
        {
            return "Hello World";
        }

        //[WebMethod]
        //public string CreatePlayer(int serverID, string userID, string roleID, string name, int sex)
        //{
        //    PlayerAccess a = PlayerAccess.Instance;
        //    int id = a.CreatePlayerID();
        //    if (a.CreatePlayer(id, serverID, userID, roleID, name, sex))
        //    {
        //    }
        //    return v == null ? "错误" : v.ID;
        //}

        /// <summary>
        /// 删除指定ID的配置
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [WebMethod]
        public bool Delete(string table, string ID)
        {
            var c = database.GetCollection(table);
            var v = c.Remove(Query.EQ("_id", ID), RemoveFlags.Single, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }
    }
}
