using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MongoDB.Bson;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;
using MongoDB.Driver.Builders;

namespace Sinan.Demo
{
    class MongoWriteTest
    {
        static public void Test()
        {
            int count = 20000;
            for (int i = 0; i < count; i++)
            {
                System.Threading.Thread t = new System.Threading.Thread(
                    new ThreadStart(Start));
                t.Start();
            }

        }

        static int sucess = 0;
        static Random random = new Random();
        static int id = 1;
        static string[] names = new string[] { "Skill", "Equips", "Social", "Family", "Title", "Effort", "Home", "PetBook", "Box", "B0", "B1", "B2", "B3", "EPro", "SPro" };
        static void Start()
        {
            try
            {
                int index = random.Next(names.Length);
                string name = names[index];
                PlayerEx client = new PlayerEx(id, name);
                client.Value = new Variant();
                for (int i = 0; i < index * 10; i++)
                {
                    client.Value.Add("sd" + i, i);
                }
                PlayerExAccess.Instance.Save(client);
                var q = Query.EQ("_id", id);
                //long count = PlayerExAccess.Instance.GetCount(q);
                //Variant x = PlayerExAccess.Instance.FindOneById<Variant>(id);
                System.Threading.Interlocked.Increment(ref sucess);
                Console.WriteLine("写成功:" + sucess);
            }
            catch (System.TimeoutException) { }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message, ex.TargetSite);
            }
        }
    }
}
