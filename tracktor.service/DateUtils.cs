﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace tracktor.service
{
    public static class DateUtils
    {
        public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
        {
            int diff = dt.DayOfWeek - startOfWeek;
            if (diff < 0)
            {
                diff += 7;
            }

            return dt.AddDays(-1 * diff).Date;
        }
    }
}