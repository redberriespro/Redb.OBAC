using System;
using System.Linq;

namespace Redb.OBAC.Utils
{
    public class GuidUtils
    {
        public static bool ListsEquals(Guid[] list1, Guid[] list2)
        {
            if (!list1.Any() && !list2.Any()) return false; // edge case 
            // https://stackoverflow.com/questions/12795882/quickest-way-to-compare-two-generic-lists-for-differences
            var firstNotSecond = list1.Except(list2).ToList();
            var secondNotFirst = list2.Except(list1).ToList();
            return !firstNotSecond.Any() && !secondNotFirst.Any();
        }
    }
}