using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.FastJson;
using Sinan.Log;
using Sinan.Util;

namespace Sinan.GameModule
{
    /// <summary>
    /// 创建索引
    /// </summary>
    public class IndexCreater
    {
        MongoDatabase db;
        public IndexCreater(string connectionString)
        {
            db = MongoDatabase.Create(connectionString);
        }


        void CreateIndexs(string collName, Variant keys)
        {
            if (!string.IsNullOrEmpty(collName))
            {
                MongoCollection collection;
                if (!db.CollectionExists(collName))
                {
                    //创建集合
                    db.CreateCollection(collName);
                }
                collection = db.GetCollection(collName);
                //创建索引
                if (keys != null)
                {
                    CreateIndex(collection, keys);
                }
            }
        }

        /// <summary>
        /// 创建索引
        /// </summary>
        /// <param name="collection"></param>
        /// <param name="v"></param>
        private void CreateIndex(MongoCollection collection, Variant v)
        {
            var indexs = collection.GetIndexes();
            HashSet<string> hs = new HashSet<string>();
            foreach (IndexInfo index in indexs)
            {
                hs.Add(index.Name);
            }
            foreach (var item in v)
            {
                string indexName = item.Key;
                Variant keys = item.Value as Variant;
                if (keys != null)
                {
                    IndexOptionsBuilder indexOpt = new IndexOptionsBuilder();
                    indexOpt.SetName(indexName);
                    if (keys.ContainsKey("dropDups"))
                    {
                        if (keys["dropDups"] is bool)
                        {
                            indexOpt.SetDropDups((bool)keys["dropDups"]);
                        }
                        keys.Remove("dropDups");
                    }

                    if (keys.ContainsKey("background"))
                    {
                        if (keys["background"] is bool)
                        {
                            indexOpt.SetBackground((bool)keys["background"]);
                        }
                        keys.Remove("background");
                    }

                    if (keys.ContainsKey("unique"))
                    {
                        if (keys["unique"] is bool)
                        {
                            indexOpt.SetUnique((bool)keys["unique"]);
                        }
                        keys.Remove("unique");
                    }

                    if (keys.ContainsKey("sparse"))
                    {
                        if (keys["sparse"] is bool)
                        {
                            indexOpt.SetSparse((bool)keys["sparse"]);
                        }
                        keys.Remove("sparse");
                    }

                    bool find = false;
                    IndexKeysBuilder indexKey = new IndexKeysBuilder();
                    foreach (var dsa in keys)
                    {
                        string ckey = dsa.Key;
                        if (ckey != string.Empty && dsa.Value is int)
                        {
                            int dec = (int)dsa.Value;
                            if (dec == 1)
                            {
                                indexKey.Ascending(ckey);
                                find = true;
                            }
                            else if (dec == -1)
                            {
                                indexKey.Descending(ckey);
                                find = true;
                            }
                        }
                    }
                    if (find)
                    {
                        if (!hs.Contains(indexName))
                            collection.CreateIndex(indexKey, indexOpt);
                    }
                    else
                    {
                        if (hs.Contains(indexName))
                            collection.DropIndexByName(indexName);
                    }
                }
            }
        }


        public void LoadIndex(string path)
        {
            using (FileStream fs = File.OpenRead(path))
            using (StreamReader sr = new StreamReader(fs, Encoding.UTF8, false))
            {
                string name = Path.GetFileNameWithoutExtension(path);
                string x = sr.ReadToEnd();
                try
                {
                    Variant v = JsonConvert.DeserializeObject<Variant>(x);
                    if (v != null)
                    {
                        CreateIndexs(name, v);
                    }
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Warn(ex);
                }
            }
        }
    }
}
