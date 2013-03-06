using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uBlogsy.Common.Extensions;
using umbraco.cms.businesslogic.web;

namespace uBlogsy.BusinessLogic.Comparers
{
    public class PostDateComparer : IComparer<Document>
    {
        public int Compare(Document x, Document y)
        {
            DateTime d1 = DateTime.Parse(x.getProperty("uBlogsyPostDate").Value.ToString());
            DateTime d2 = DateTime.Parse(y.getProperty("uBlogsyPostDate").Value.ToString());

            if (d1 < d2)
                return -1;

            if (d1 == d2)
                return 0;

            return 1;
        }
    }


    public class YearComparer : IComparer<Document>
    {
        public int Compare(Document x, Document y)
        {
            int year1 = int.Parse(x.Text);
            int year2 = int.Parse(y.Text);

            if (year1 < year2)
                return -1;

            if (year1 == year2)
                return 0;

            return 1;
        }
    }



    public class MonthComparer : IComparer<Document>
    {
        public int Compare(Document x, Document y)
        {
            int month1 = x.Text.GetMonthNumberFromName();
            int month2 = y.Text.GetMonthNumberFromName();

            if (month1 < month2)
                return -1;

            if (month1 == month2)
                return 0;

            return 1;
        }
    }


    public class DayComparer : IComparer<Document>
    {
        public int Compare(Document x, Document y)
        {
            int day1 = x.Text.GetDayNumberFromString();
            int day2 = y.Text.GetDayNumberFromString();

            if (day1 < day2)
                return -1;

            if (day1 == day2)
                return 0;

            return 1;
        }
    }
}
