using System;
using System.Linq;
using System.Collections.Generic;
using Sinan.Data;
using Sinan.GameModule;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.FrontServer
{
    /// <summary>
    /// 计算战斗Buffer
    /// </summary>
    partial class FightBase
    {
        List<BufferResult> m_buffer = new List<BufferResult>();

        private void StartBuffer()
        {
            m_buffer.Clear();
            lock (m_fighter)
            {
                foreach (var v in m_fighter)
                {
                    StartBuffer(v);
                    if (v.BufferResult.HaveBuffer)
                    {
                        m_buffer.Add(v.BufferResult);
                    }
                }
                foreach (var v in m_protecter)
                {
                    StartBuffer(v);
                    if (v.BufferResult.HaveBuffer)
                    {
                        m_buffer.Add(v.BufferResult);
                    }
                }
            }
        }

        private static void StartBuffer(FightObject fighter)
        {
            fighter.BufferResult.Clear();
            for (int i = 0; i < fighter.Buffers.Count; i++)
            {
                SkillBuffer buff = fighter.Buffers[i];
                buff.RemainingNumber--;
                if (buff.RemainingNumber <= 0)
                {
                    fighter.Buffers.RemoveAt(i);
                    i--;
                }
                string name = buff.ID;
                // 流血/中毒/灼烧
                if (name == BufferType.LiuXue || name == BufferType.ZhongDu || name == BufferType.ZuoShao)
                {
                    fighter.BufferResult.AddHP(name, -(int)(buff.V), 1);
                }
                // 母育/神谕/再生
                else if (name == BufferType.MuYu || name == BufferType.ShengYu || name == BufferType.ZaiSheng)
                {
                    int hp = fighter.TryHP(buff.V);
                    fighter.BufferResult.AddHP(name, hp);
                }
                // 冥想 
                else if (name == BufferType.MingXiang)
                {
                    int mp = fighter.TryMP(buff.V);
                    fighter.BufferResult.AddMP(name, mp);
                }
            }
            fighter.BufferResult.HP = fighter.HP;
            fighter.BufferResult.MP = fighter.MP;
        }
    }
}
