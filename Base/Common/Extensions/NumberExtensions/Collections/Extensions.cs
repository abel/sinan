﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Extensions.Collections.SortStrategies;

namespace Sinan.Extensions.Collections
{
    public static class Extensions
    {
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sortMethod">ALgorithm to use for sorting</param>
        /// <returns>Sorted List</returns>
        public static IEnumerable<int> Sort(this IEnumerable<int> val, SortMethod sortMethod)
        {
            IList<int> list = val.ToList<int>();
            switch (sortMethod)
            {
                case SortMethod.Bubble:
                    BubbleSort.Sort(ref list);
                    break;
                case SortMethod.Insertion:
                    InsertionSort.Sort(ref list);
                    break;
                case SortMethod.Selection:
                    SelectionSort.Sort(ref list);
                    break;
            }
            return list;
        }
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sortMethod">ALgorithm to use for sorting</param>
        /// <returns>Sorted List</returns>
        public static IEnumerable<long> Sort(this IEnumerable<long> val, SortMethod sortMethod)
        {
            IList<long> list = val.ToList<long>();
            switch (sortMethod)
            {
                case SortMethod.Bubble:
                    BubbleSort.Sort(ref list);
                    break;
                case SortMethod.Insertion:
                    InsertionSort.Sort(ref list);
                    break;
                case SortMethod.Selection:
                    SelectionSort.Sort(ref list);
                    break;
            }
            return list;
        }
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sortMethod">ALgorithm to use for sorting</param>
        /// <returns>Sorted List</returns>
        public static IEnumerable<decimal> Sort(this IEnumerable<decimal> val, SortMethod sortMethod)
        {
            IList<decimal> list = val.ToList<decimal>();
            switch (sortMethod)
            {
                case SortMethod.Bubble:
                    BubbleSort.Sort(ref list);
                    break;
                case SortMethod.Insertion:
                    InsertionSort.Sort(ref list);
                    break;
                case SortMethod.Selection:
                    SelectionSort.Sort(ref list);
                    break;
            }
            return list;
        }
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sortMethod">ALgorithm to use for sorting</param>
        /// <returns>Sorted List</returns>
        public static IEnumerable<float> Sort(this IEnumerable<float> val, SortMethod sortMethod)
        {
            IList<float> list = val.ToList<float>();
            switch (sortMethod)
            {
                case SortMethod.Bubble:
                    BubbleSort.Sort(ref list);
                    break;
                case SortMethod.Insertion:
                    InsertionSort.Sort(ref list);
                    break;
                case SortMethod.Selection:
                    SelectionSort.Sort(ref list);
                    break;
            }
            return list;
        }
        /// <summary>
        /// Test Coverage: Included
        /// </summary>
        /// <param name="val"></param>
        /// <param name="sortMethod">ALgorithm to use for sorting</param>
        /// <returns>Sorted List</returns>
        public static IEnumerable<double> Sort(this IEnumerable<double> val, SortMethod sortMethod)
        {
            IList<double> list = val.ToList<double>();
            switch (sortMethod)
            {
                case SortMethod.Bubble:
                    BubbleSort.Sort(ref list);
                    break;
                case SortMethod.Insertion:
                    InsertionSort.Sort(ref list);
                    break;
                case SortMethod.Selection:
                    SelectionSort.Sort(ref list);
                    break;
            }
            return list;
        }
    }
}
