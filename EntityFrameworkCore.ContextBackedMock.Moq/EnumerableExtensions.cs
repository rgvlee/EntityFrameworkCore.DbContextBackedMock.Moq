using System.Collections.Generic;
using System.Linq;

namespace EntityFrameworkCore.ContextBackedMock.Moq {
    public static class EnumerableExtensions {
        public static bool IsEquivalentTo<T>(this IEnumerable<T> list1, IEnumerable<T> list2, IEqualityComparer<T> comparer) {
            var tmpList1 = list1.ToList();
            var tmpList2 = list2.ToList();
            var firstNotSecond = tmpList1.Except(tmpList2, comparer).ToList();
            var secondNotFirst = tmpList2.Except(tmpList1, comparer).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }
    }
}