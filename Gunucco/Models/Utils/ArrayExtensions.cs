using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Gunucco.Models.Utils
{
    public static class ArrayExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> items, Predicate<T> eq)
        {
            var index = 0;
            foreach (T item in items)
            {
                if (eq(item))
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public static T FindNext<T>(this IEnumerable<T> items, Predicate<T> eq)
        {
            var index = items.IndexOf(eq);
            return items.ElementAtOrDefault(index + 1);
        }

        public static T FindPrev<T>(this IEnumerable<T> items, Predicate<T> eq)
        {
            var index = items.IndexOf(eq);
            return items.ElementAtOrDefault(index - 1);
        }
    }
}
