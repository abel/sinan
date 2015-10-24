using MongoDB.Driver;
using Sinan.GameModule;

namespace Sinan.RepairModule
{
    class FixSkill
    {
        MongoDatabase database;


        public FixSkill()
        {
        }


        /// <summary>
        /// 清理技能问题..
        /// </summary>
        /// <param name="note"></param>
        public void StartPlace()
        {
            string connectionString = ConfigLoader.Config.DbPlayer;
            database = MongoDatabase.Create(connectionString);

            //var x = GameConfigManager
            //MongoCollection player = database.GetCollection("Player");
            //int count = NewMethod(player, "UserID");
            //Console.WriteLine("Player:" + count);

            //MongoCollection user = database.GetCollection("UserLog");
            //count = NewMethod2(user);
            //Console.WriteLine("UserLog" + count);
        }
    }
}
