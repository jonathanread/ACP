﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text.RegularExpressions;
using umbraco.MacroEngines;
using System.IO;
using System.Globalization;
namespace uBlogsy.Common.Extensions
{
    public static class StringExtensions
    {
        #region StripNonAphaNumeric
        /// <summary>
        /// Strips Non-apha-numeric characters from the string.
        /// Replaces them with single spaces.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StripNonAphaNumeric(this string s)
        {
            return Regex.Replace(s, "[^A-Za-z0-9]", " ").Replace("  ", " ");
        }

        #endregion



        #region Flatten
        /// <summary>
        /// Converts to lowercase, and removes spaces.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Flatten(this string s)
        {
            return s.ToLower().Replace(" ", "");
        }

        #endregion



        #region FormatDateTime
        /// <summary>
        /// Formats date time.
        /// </summary>
        /// <param name="dateString"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string FormatDateTime(this string dateString, string format)
        {
            DateTime date;
            return DateTime.TryParse(dateString, out date) ? date.ToString(format) : string.Empty;
        }

        #endregion



        #region FormatDateTimeOrdinal
        /// <summary>
        /// Formats date time and then adds suffix for day.
        /// </summary>
        /// <param name="dateString"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string FormatDateTimeOrdinal(this string dateString, string format)
        {
            if (string.IsNullOrEmpty(dateString)) { return string.Empty; }
            DateTime d = DateTime.Parse(dateString);

            string newFormat = format.Replace("d", "d$$$$$");
            string formatedDate = dateString.FormatDateTime(newFormat);

            return formatedDate.Replace("$$$$$", GetOrdinal(d.Day));
        }

        #endregion



        /// <summary>
        /// Gets the number of the month.
        /// Month name can be in the form: January, Jan, 01, 1
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetMonthNumberFromName(this string s)
        {
            DateTime date;

            // note "M " is intentional due to some strange thing in c#
            bool success = DateTime.TryParseExact(s, new string[] { "MMMM", "MMM", "MM", "M " }, CultureInfo.CurrentCulture, DateTimeStyles.AllowInnerWhite, out date);
            if (success)
            {
                return date.Month;
            }

            return -1;
        }


        /// <summary>
        /// Gets the number of the day.
        /// Day name can be in the form: 01, 1
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static int GetDayNumberFromString(this string s)
        {
            DateTime date;

            // note "D " is intentional due to some strange thing in c#
            bool success = DateTime.TryParseExact(s, new string[] { "dd", "d " }, CultureInfo.CurrentCulture, DateTimeStyles.AllowInnerWhite, out date);
            if (success)
            {
                return date.Day;
            }

            return -1;
        }


        #region GetOrdinal
        /// <summary>
        /// Gets suffix for day number of month. eg. 1st, 2nd 3rd.
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static string GetOrdinal(int number)
        {
            switch (number % 100)
            {
                case 11:
                case 12:
                case 13:
                    return "th";
            }
            switch (number % 10)
            {
                case 1:
                    return "st";
                case 2:
                    return "nd";
                case 3:
                    return "rd";
                default:
                    return "th";
            }
        }

        #endregion




        #region StripHtml

        /// <summary>
        /// Removes HTML tags from a string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string StripHtml(this string s)
        {
            //Replace all nastiness with space the remove double spaces
            return Regex.Replace(s, @"<(.|\n)*?>", " ").Replace(Environment.NewLine, " ").Replace("\n", " ").Replace("  ", " ");
        }

        #endregion




        #region GetSummary

        /// <summary>
        /// Gets maxCharacters characters from the given string
        /// </summary>
        /// <param name="n"></param>
        /// <param name="s"></param>
        /// <param name="maxCharacters"></param>
        /// <returns></returns>
        public static string GetSummary(this string s, int maxCharacters)
        {
            if (s.Length > maxCharacters + 1)
            {
                var endIndex = s.Substring(0, maxCharacters + 1).LastIndexOf(" ");

                // take care of case where string is small
                if (endIndex == -1)
                {
                    return s.Substring(0, maxCharacters) + "...";
                }

                return s.Substring(0, endIndex) + "...";
            }
            return s;
        }

        #endregion




        #region GetUrl

        /// <summary>
        /// Adds a suffix to the given file.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="s"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string GetUrl(this string s, string suffix)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;

            return Path.GetDirectoryName(s).Replace("\\", "/") + "/" + Path.GetFileNameWithoutExtension(s) + suffix + Path.GetExtension(s);
        }

        #endregion





        public static string ReplaceQueryStringItem(this string url, string key, string value)
        {
            if (!url.Contains("?"))
            {
                return string.Format("{0}?{1}={2}", url, key,value);
            }

            if (!url.Contains(key + "="))
            {
                return string.Format("{0}&{1}={2}", url, key, value);
            }

            return Regex.Replace(url, @"([?&]" + key + ")=[^?&]+", "$1=" + value);
        }


    }
}