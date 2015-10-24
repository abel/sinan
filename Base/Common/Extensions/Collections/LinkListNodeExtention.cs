using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sinan.Extensions
{
    /// <summary>
    ///  linklist 枚举器,可以指定从任意节点向前/向后枚举 不用每次从头来
    /// </summary>
    static public class LinkListNodeExtention
    {
        public enum EnumerateDirection
        {
            Forward = 0,
            Backward
        }

        static public IEnumerable<LinkedListNode<T>> GetMidwayEnumerable<T>(this LinkedListNode<T> currentNode, EnumerateDirection direction, bool selectSelf)
        {
            var tmpNode = currentNode;
            if (tmpNode == null) yield break;
            switch (direction)
            {
                case EnumerateDirection.Forward:

                    if (selectSelf) yield return tmpNode;

                    while (tmpNode.Next != null)
                    {
                        tmpNode = tmpNode.Next;
                        yield return tmpNode;
                    }
                    yield break;
                case EnumerateDirection.Backward:

                    if (selectSelf) yield return tmpNode;

                    while (tmpNode.Previous != null)
                    {
                        tmpNode = tmpNode.Previous;
                        yield return tmpNode;
                    }

                    yield break;
            }
        }
    }
}
