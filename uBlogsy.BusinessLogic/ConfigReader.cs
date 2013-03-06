using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace uBlogsy.BusinessLogic
{
    public class ConfigReader
    {
        private const string m_ConfigPath = "~/config/uBlogsy.config";

         #region Singleton

        protected XDocument m_Doc;
        protected static volatile ConfigReader m_Instance = new ConfigReader();
        protected static object syncRoot = new Object();

        protected ConfigReader()
        {
            
        }

        public static ConfigReader Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new ConfigReader();
                    }
                }

                m_Instance.m_Doc = XDocument.Parse(File.ReadAllText(HttpContext.Current.Server.MapPath(m_ConfigPath)));

                return m_Instance;
            }
        }

        #endregion



        #region UseAutoDateFolders
        /// <summary>
        /// Returns true if auto sorting is selected.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public bool UseAutoDateFolders()
        {
            var dateFolders = Enumerable.Single(m_Doc.Descendants("dateFolders"));

            var enabled = dateFolders.Attribute("enabled").Value;

            return enabled == "true";
        }

        #endregion



        /// <summary>
        /// Gets month format
        /// </summary>
        /// <returns></returns>
        public string GetMonthFormat()
        {
            var dateFolders = m_Doc.Descendants("dateFolders").Single();

            var monthFormat = dateFolders.Descendants("monthFormat").Single().Value;

            return monthFormat;
        }



        /// <summary>
        /// Gets day format
        /// </summary>
        /// <returns></returns>
        public string GetDayFormat()
        {
            var dateFolders = m_Doc.Descendants("dateFolders").Single();

            var dayFormat = dateFolders.Descendants("dayFormat").Single().Value;

            return dayFormat;
        }


        /// <summary>
        /// Gets true or false from sendNotificationOnRepublish
        /// </summary>
        /// <returns></returns>
        public bool GetSendNotificationOnRepublish()
        {
            var comments = m_Doc.Descendants("comments").Single();

            var sendNotificationOnRepublish = comments.Descendants("sendNotificationOnRepublish").Single().Value;

            return bool.Parse(sendNotificationOnRepublish);
        }


    }
}
