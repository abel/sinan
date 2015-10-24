using Sinan.Command;
using Sinan.Data;
using Sinan.FrontServer;
using Sinan.GameModule;
using Sinan.Util;

namespace Sinan.ArenaModule.Business
{
    /// <summary>
    /// 战斗中物品使用限制
    /// </summary>
    public class ArenaGoods
    {
        /// <summary>
        /// 道具使用限制
        /// </summary>
        /// <param name="note"></param>
        /// <param name="gc"></param>
        /// <returns></returns>
        public static bool SupplyLimit(UserNote note, GameConfig gc)
        {
            Variant limit = gc.Value.GetVariantOrDefault("UserLimit");
            if (limit == null) return true;
            //补充类使用限制
            return CheckLevel(note, limit);
        }

        /// <summary>
        /// 使用者等级限制
        /// </summary>
        /// <param name="note"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        private static bool CheckLevel(UserNote note, Variant limit)
        {
            int needLev = limit.GetIntOrDefault("LevelRequire");
            if (needLev > note.Player.Level)
            {
                note.Call(ArenaCommand.ArenaGoodsR, false, TipManager.GetMessage(GoodsReturn.NoLevel));
                return false;
            }
            return true;
        }
    }
}
