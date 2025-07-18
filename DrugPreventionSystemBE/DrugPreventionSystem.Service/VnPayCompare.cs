﻿using System.Collections.Generic;
using System.Globalization;

namespace DrugPreventionSystemBE.DrugPreventionSystem.Service
{
    public class VnPayCompare : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == y) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var vnpCompare = CompareInfo.GetCompareInfo("en-US");
            return vnpCompare.Compare(x, y, CompareOptions.Ordinal); // Phân biệt chữ hoa thường
        }
    }
}
