using System;
using System.Collections.Generic;
using System.Linq;
using uBlogsy.BusinessLogic.Comparers;
using uHelpsy.Core;
using uBlogsy.Common.Helpers;
using uBlogsy.Common.Extensions;
using umbraco.cms.businesslogic.web;


namespace uBlogsy.BusinessLogic
{

    interface IDateFolderService
    {
        Document EnsureCorrectDate(Document doc);
        Document EnsureCorrectParentForPost(Document doc);
    }



    public class DateFolderService : IDateFolderService
    {
        
        #region Singleton

        protected static volatile DateFolderService m_Instance = new DateFolderService();
        protected static object syncRoot = new Object();

        protected DateFolderService() { }

        public static DateFolderService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new DateFolderService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion




        #region EnsureCorrectDate

        /// <summary>
        /// Ensures that a post date is correct depending on it's parent.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Document EnsureCorrectDate(Document doc)
        {
            // get post date
            string dateString = doc.getProperty("uBlogsyPostDate").Value.ToString();
            DateTime postDate = DateTime.TryParse(dateString, out postDate) ? postDate : doc.CreateDateTime;

            if (string.IsNullOrEmpty(dateString))
            {
                doc.getProperty("uBlogsyPostDate").Value = postDate;
                return doc;
            }

            Document parent = new Document(doc.ParentId);

            if (parent.ContentType.Alias == "uBlogsyFolderYear")
            {
                // when parent is a year, create date as: year/1/1
                int year = int.Parse(parent.Text);
                if (year != postDate.Year)
                {
                    DateTime newDate = new DateTime(year, postDate.Month, postDate.Day, postDate.Hour, postDate.Minute, postDate.Second, postDate.Millisecond);
                    doc.getProperty("uBlogsyPostDate").Value = newDate;
                }
            }
            else if (parent.ContentType.Alias == "uBlogsyFolderMonth")
            {
                //when parent is a month, get year and create date as: year/month/1
                int year = int.Parse((new Document(parent.ParentId)).Text);
                int month = parent.Text.GetMonthNumberFromName();

                if (month != postDate.Month || year != postDate.Year)
                {
                    DateTime newDate = new DateTime(year, month, postDate.Day, postDate.Hour, postDate.Minute, postDate.Second, postDate.Millisecond);
                    doc.getProperty("uBlogsyPostDate").Value = newDate;
                }
            }
            else if (parent.ContentType.Alias == "uBlogsyFolderDay")
            {
                // when parent is a day, get year and month, create date as: year/month/day
                int year = int.Parse((new Document(parent.Parent.ParentId)).Text);
                int month = parent.Parent.Text.GetMonthNumberFromName(); // eg. month can be 1, 01, Jan, January
                int day = parent.Text.GetDayNumberFromString(); // eg. day can be in formats 1, 01

                if (day != postDate.Day || month != postDate.Month || year != postDate.Year)
                {
                    DateTime newDate = new DateTime(year, month, day, postDate.Hour, postDate.Minute, postDate.Second, postDate.Millisecond);
                    doc.getProperty("uBlogsyPostDate").Value = newDate;
                }
            }
            else
            {
                // case where node was created on the dashboard
                doc.getProperty("uBlogsyPostDate").Value = postDate;
            }

            return doc;
        }

        #endregion





        #region EnsureCorrectParentForPost

        /// <summary>
        /// Moves post to the correct parent, based on the post date.
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Document EnsureCorrectParentForPost(Document doc)
        {
            var dateString = doc.getProperty("uBlogsyPostDate").Value.ToString();
            if (string.IsNullOrEmpty(dateString))
            {
                // cannot find correct parent when date is not set!!!
                doc = EnsureCorrectDate(doc);
                //return doc;
            }

            Document newParent = GetCorrectParentForPost(doc);

            if (newParent.Id != doc.ParentId)
            {
                // move the node to the new parent
                doc.Move(newParent.Id);

                // sort
                List<Document> nodes = new List<Document>(newParent.Children);
                nodes.Sort(new PostDateComparer());

                // update cache!
                //umbraco.library.UpdateDocumentCache(newParent.Id);
                //umbraco.library.UpdateDocumentCache(doc.Id);

              //  doc.Save(); 
            }

            return doc;
        }

        #endregion





        #region protected GetCorrectParentForPost

        /// <summary>
        /// Gets the correct parent for a post based on post date, creates it if it does not exist.
        /// </summary>
        /// <returns></returns>
        protected Document GetCorrectParentForPost(Document doc)
        {
            // get post date 
            DateTime postDate = DateTime.Parse(doc.getProperty("uBlogsyPostDate").Value.ToString());

            // get landing
            Document landing = DocumentService.Instance.GetDocumentByAlias(doc, "uBlogsyLanding", "uBlogsyLanding");
            
            // check for posts container
            Document postsContainer = landing.Children.Where(x => x.ContentType.Alias == "uBlogsyContainerBlog").FirstOrDefault();

            Document yearFolder = GetYearFolder(postsContainer ?? landing, postDate);

            if (yearFolder.Id == doc.ParentId)
            {
                // case where a move has occured to a year or when node was created using dashboard/live writer
                return yearFolder;
            }

            // get or create month based on date
            Document monthFolder = GetMonthFolder(yearFolder, postDate);

            if (monthFolder.Id == doc.ParentId)
            {
                // case where a move has occured to a month or when node was created using dashboard/live writer
                return monthFolder;
            }

            Document dayFolder = GetDayFolder(monthFolder, postDate);

            return dayFolder;
        }


        #endregion





        #region protected GetYearFolder

        /// <summary>
        /// Gets year folder. Creates if doesnt exist.
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        protected Document GetYearFolder(Document root, DateTime postDate)
        {
            var yearFolders = root.Children.Where(x => x.ContentType.Alias == "uBlogsyFolderYear");

            var yearFolder = yearFolders.Where(x => int.Parse(x.Text) == postDate.Year).FirstOrDefault();

            if (yearFolder != null)
            {
                return yearFolder;
            }

            // year not found so create it!
            yearFolder = UmbracoAPIHelper.CreateContentNode(postDate.Year.ToString(), "uBlogsyFolderYear", new Dictionary<string, object>(), root.Id, root.Published);

            // add new year folder to list and sort
            var list = yearFolders.ToList();
            list.Add(yearFolder);
            uBlogsy.Common.Helpers.DocumentHelper.SortNodes(yearFolder.ParentId, list, new YearComparer());

            return yearFolder;
        }

        #endregion




        #region protected GetMonthFolder

        /// <summary>
        /// Gets month folder from year folder. Creates month folder if it does not exist.
        /// </summary>
        /// <param name="yearFolder"></param>
        /// <param name="postDate"></param>
        /// <returns></returns>
        protected Document GetMonthFolder(Document yearFolder, DateTime postDate)
        {
            // parent should be a month folder so lets get the months
            var monthFolders = yearFolder.Children.Where(x => x.ContentType.Alias == "uBlogsyFolderMonth");

            // search for correct month
            Document monthFolder = monthFolders.FirstOrDefault(x => x.Text.GetMonthNumberFromName() == postDate.Month);
            if (monthFolder != null)
            {
                return monthFolder;
            }

            // month not found so create it
            string monthName = DateHelper.GetMonthNameWithFormat(postDate.Month, ConfigReader.Instance.GetMonthFormat());
            monthFolder = UmbracoAPIHelper.CreateContentNode(monthName, "uBlogsyFolderMonth", new Dictionary<string, object>(), yearFolder.Id, yearFolder.Published);

            // add new month folder to list and sort
            var list = monthFolders.ToList();
            list.Add(monthFolder);
            uBlogsy.Common.Helpers.DocumentHelper.SortNodes(yearFolder.Id, list, new MonthComparer());

            return monthFolder;
        }
        
        #endregion




        #region protected GetDayFolder

        /// <summary>
        /// Gets month folder from year folder. Creates month folder if it does not exist.
        /// </summary>
        /// <param name="monthFolder"></param>
        /// <param name="postDate"></param>
        /// <returns></returns>
        protected Document GetDayFolder(Document monthFolder, DateTime postDate)
        {
            // parent should be a month folder so lets get the months
            var dayFolders = monthFolder.Children.Where(x => x.ContentType.Alias == "uBlogsyFolderDay");

            // search for correct day
            Document dayFolder = dayFolders.FirstOrDefault(x => x.Text.GetDayNumberFromString() == postDate.Day);
            if (dayFolder != null)
            {
                return dayFolder;
            }

            // day not found so create it
            string dayName = DateHelper.GetDayNameWithFormat(postDate.Day, ConfigReader.Instance.GetDayFormat());
            dayFolder = DocumentService.Instance.EnsureNodeExists(monthFolder.Id, dayFolder, "uBlogsyFolderDay", dayName, monthFolder.Published);

            // add new day folder to list and sort
            var list = dayFolders.ToList();
            list.Add(dayFolder);
            uBlogsy.Common.Helpers.DocumentHelper.SortNodes(monthFolder.Id, list, new DayComparer());

            return dayFolder;
        }

        #endregion

    }
}
