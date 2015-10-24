using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Util;
using Sinan.FastJson;
using Sinan.Extensions;
using Sinan.Log;

namespace Sinan.DataServer
{
    /// <summary>
    /// GameConfigService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消对下行的注释。
    [System.Web.Script.Services.ScriptService]
    public class GameConfigService : System.Web.Services.WebService
    {
        MongoDatabase database;
        MongoCollection<GameConfig> collection;

        public GameConfigService()
        {
            string connectionString = System.Configuration.ConfigurationManager.ConnectionStrings["gamebase"].ConnectionString;
            database = MongoDatabase.Create(connectionString);
            collection = database.GetCollection<GameConfig>("GameConfig");
        }

        //[WebMethod]
        //public string HelloWorld(string name)
        //{
        //    //var query = Query.EQ("_id", name);
        //    //var update = MongoDB.Driver.Builders.Update.Rename("Version","Ver");
        //    //var v = collection.Update(query, update, UpdateFlags.None, SafeMode.False);
        //    //var update2 = MongoDB.Driver.Builders.Update.Rename("UpdateDate", "Modified");
        //    //var v2 = collection.Update(query, update2, UpdateFlags.None, SafeMode.False);
        //    return "Hello World:" + name;
        //}

        /// <summary>
        /// 验证JSON字符串
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        [WebMethod]
        public object ValidateJson(string json)
        {
            var v = JsonConvert.DeserializeObject<Variant>(json);
            return v.ToString();
        }

        /// <summary>
        /// 保存配置..
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public bool Save(string ID, string Name, string MainType, string SubType, string UI, string Value, int ver)
        {
            GameConfig config = new GameConfig();

            config.ID = ID;
            config.Name = Name;
            config.MainType = MainType;
            config.SubType = SubType;
            config.Ver = ver;

            if (!DictionaryExtension.CheckKey(ID)) return false;
            if (!DictionaryExtension.CheckKey(Name)) return false;
            if (!DictionaryExtension.CheckKey(MainType)) return false;
            if (!DictionaryExtension.CheckKey(SubType)) return false;

            var ui = JsonConvert.DeserializeObject<Variant>(UI);
            if (ui != null)
            {
                if (!ui.CheckKey())
                {
                    string msg = "Value格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return false;
                }
                config.UI = ui;
            }

            var value = JsonConvert.DeserializeObject<Variant>(Value);
            if (value != null)
            {
                if (!value.CheckKey())
                {
                    string msg = "Value格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return false;
                }
                config.Value = value;
            }

            config.Author = HttpContext.Current.Request.UserHostAddress;
            config.Modified = DateTime.UtcNow;
            var v = collection.Save(config, SafeMode.True);

            return v.DocumentsAffected > 0;
        }

        /// <summary>
        /// 插入新的对象.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [WebMethod]
        public ConfigEntity Insert(string ID, string Name, string MainType, string SubType, string UI, string Value, int ver)
        {
            GameConfig config = new GameConfig();

            config.ID = ID;
            config.Name = Name;
            config.MainType = MainType;
            config.SubType = SubType;
            config.Ver = ver;
            var ui = JsonConvert.DeserializeObject<Variant>(UI);
            if (ui != null)
            {
                if (!ui.CheckKey())
                {
                    string msg = "UI格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return null;
                }
                config.UI = ui;
            }

            var value = JsonConvert.DeserializeObject<Variant>(Value);
            if (value != null)
            {
                if (!value.CheckKey())
                {
                    string msg = "Value格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return null;
                }
                config.Value = value;
            }

            config.Author = HttpContext.Current.Request.UserHostAddress;
            config.Modified = DateTime.UtcNow;

            var v = collection.Insert(config, VariantBuilder.DefaultInsertOptions);
            if (v.Ok)
            {
                GameConfig n2 = collection.FindOneByIdAs<GameConfig>(ID);
                return n2.CovertToConfigEntity();
            }
            return null;
        }

        [WebMethod]
        public ConfigEntity Update(string ID, string Name, string MainType, string SubType, string UI, string Value, int ver)
        {
            GameConfig config = new GameConfig();

            config.ID = ID;
            config.Name = Name;
            config.MainType = MainType;
            config.SubType = SubType;
            config.Ver = ver;

            var ui = JsonConvert.DeserializeObject<Variant>(UI);
            if (ui != null)
            {
                if (!ui.CheckKey())
                {
                    string msg = "UI格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return null;
                }
                config.UI = ui;
            }

            var value = JsonConvert.DeserializeObject<Variant>(Value);
            if (value != null)
            {
                if (!value.CheckKey())
                {
                    string msg = "Value格式不正确:" + ID + Name;
                    LogWrapper.Error(msg);
                    return null;
                }
                config.Value = value;
            }

            config.Author = HttpContext.Current.Request.UserHostAddress;
            config.Modified = DateTime.UtcNow;

            var query = Query.And(Query.EQ("_id", ID), Query.LT("Ver", ver));
            var update = MongoDB.Driver.Builders.Update.Set("Name", Name).Set("MainType", MainType).Set("SubType", SubType)
                .SetWrapped<Variant>("UI", config.UI).SetWrapped<Variant>("Value", config.Value)
                .Set("Author", config.Author).Set("Modified", DateTime.UtcNow).Set("Ver", ver);
            var v = collection.Update(query, update, UpdateFlags.None, SafeMode.False);

            GameConfig n2 = collection.FindOneAs<GameConfig>(Query.EQ("_id", ID));
            return n2.CovertToConfigEntity();
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        /// <returns></returns>
        [WebMethod]
        public List<ConfigEntity> GetAll()
        {
            var c = collection.FindAllAs<GameConfig>();
            return c.Select(x => x.CovertToConfigEntity()).ToList();
        }

        /// <summary>
        /// 获取所有配置
        /// </summary>
        /// <param name="MainType">主要类型</param>
        /// <param name="SubType">次要类型</param>
        /// <returns></returns>
        [WebMethod]
        public List<ConfigEntity> GetList(string MainType, string SubType)
        {
            IMongoQuery qc;
            if (string.IsNullOrEmpty(SubType))
            {
                qc = Query.EQ("MainType", MainType);
            }
            else
            {
                qc = Query.And(Query.EQ("MainType", MainType), Query.EQ("SubType", SubType));
            }
            var c = collection.Find(qc);
            return c.Select(x => x.CovertToConfigEntity()).ToList();
        }

        /// <summary>
        /// 根据ID获取配置
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [WebMethod]
        public ConfigEntity GetConfigByID(string ID)
        {
            var v = collection.FindOneByIdAs<GameConfig>(ID);
            return v == null ? null : v.CovertToConfigEntity();
        }

        /// <summary>
        /// 根据名称获取配置
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        [WebMethod]
        public ConfigEntity GetConfigByName(string Name)
        {
            var v = collection.FindOneByIdAs<GameConfig>(Name);
            return v == null ? null : v.CovertToConfigEntity();
        }

        /// <summary>
        /// 删除指定ID的配置
        /// </summary>
        /// <param name="ID"></param>
        /// <returns></returns>
        [WebMethod]
        public bool Delete(string ID)
        {
            IMongoQuery qc = Query.EQ("_id", ID);
            var v = collection.Remove(qc, SafeMode.True);
            return v == null ? false : v.DocumentsAffected > 0;
        }
    }
}
