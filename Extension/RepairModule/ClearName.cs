using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using Sinan.GameModule;
using Sinan.Log;

namespace Sinan.RepairModule
{
    /// <summary>
    /// 更改非法用户名.
    /// </summary>
    class ClearName
    {
        MongoDatabase database;


        public ClearName()
        {
        }


        /// <summary>
        /// 清理用户名
        /// </summary>
        /// <param name="note"></param>
        public void StartPlace()
        {
            string connectionString = ConfigLoader.Config.DbPlayer;

            database = MongoDatabase.Create(connectionString);
            MongoCollection player = database.GetCollection("Player");
            int count = NewMethod(player, "Name");
            Console.WriteLine("Player:" + count);

            MongoCollection pet = database.GetCollection("Pet");
            count = NewMethod(pet, "Name");
            Console.WriteLine("Pet:" + count);

            //MongoCollection family = database.GetCollection("Family");
            //count = NewMethod(family, "Name");
            //Console.WriteLine("Family:" + count);
        }

        private int NewMethod(MongoCollection coll, string fbname)
        {
            var v = coll.FindAs<BsonDocument>(Query.Null).SetFields(fbname);
            Dictionary<string, string> results = new Dictionary<string, string>();
            foreach (var item in v)
            {
                BsonValue value;
                if (item.TryGetValue(fbname, out value))
                {
                    string oldName = value.ToString();
                    string newName = ReplayceName(oldName);
                    if (oldName != newName)
                    {
                        item.TryGetValue("_id", out value);
                        string id = value.ToString();
                        results.Add(id, newName);
                    }
                }
            }
            foreach (var item in results)
            {
                string id = item.Key;
                string newName = item.Value;
                try
                {
                    Update.Set(fbname, newName);
                    coll.Update(Query.EQ("_id", id), Update.Set(fbname, newName), SafeMode.False);
                    //var v2 = coll.Update(Query.EQ("_id", id), Update.Set(fbname, newName), SafeMode.True);
                    //if (v2 != null && v2.Ok)
                    //{
                    //    Console.WriteLine(v2.DocumentsAffected);
                    //}
                }
                catch (System.Exception ex)
                {
                    LogWrapper.Error("renameErr:" + id + " newName" + newName, ex);
                }
            }
            return results.Count;
        }

        private string ReplayceName(string oldName)
        {
            //'\u0021' 到 '\u007e' 的字符
            //!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
            //const string s1 = "!\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
            const string replaceChars = "！“＃＄％＆’（）﹡＋，－·／0123456789：；〈﹦〉？﹫ABCDEFGHIJKLMNOPQRSTUVWXYZ［﹨］ˆ＿´abcdefghijklmnopqrstuvwxyz｛｜｝～";
            char[] newName = oldName.ToArray();
            for (int i = 0; i < oldName.Length; i++)
            {
                char c = oldName[i];
                if ((c >= '!' && c <= '~'))
                {
                    newName[i] = replaceChars[c - '!'];
                }
            }
            return new string(newName);
        }

    }
}
