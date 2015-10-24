using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.FrontServer
{
    public interface IAttack
    {
        /// <summary>
        /// 攻击
        /// </summary>
        /// <param name="target">接受攻击者</param>
        /// <returns></returns>
        ActionResult Attack(FightObject target);
    }
}
