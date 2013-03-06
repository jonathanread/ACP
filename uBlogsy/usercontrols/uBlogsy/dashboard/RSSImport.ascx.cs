using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using uBlogsy.BusinessLogic.Helpers;
using uBlogsy.Common.Helpers;
using uHelpsy.Core;
//using umbraco.MacroEngines;
using umbraco.cms.businesslogic.web;
using System.Globalization;
using uBlogsy.Common.Extensions;
using System.Transactions;
using uBlogsy.BusinessLogic;

namespace uBlogsy.Web.usercontrols.uBlogsy.dashboard
{
    public partial class RSSImport : System.Web.UI.UserControl
    {
        private const string DEFAULT_BODY = "The body and title are included in case you want to use this page as a landing page.";



        #region btnRssImport_Click
        protected void btnRssImport_Click(object sender, EventArgs e)
        {
            // make import a transaction
            try
            {
                Import();
                mv.ActiveViewIndex = 1;
            }
            catch (Exception ex)
            {
                // display errors
                lblError.Text = ex.Message + "<br/><br/>StackTrace:<br/>" + ex.StackTrace;
                mv.ActiveViewIndex = 2;
            }
        }

        #endregion




        #region Import
        /// <summary>
        /// Performs import.
        /// Does call to get rss.
        /// Creates content nodes.
        /// </summary>
        protected void Import()
        {
            RssReader reader = RssReader.CreateAndCache(txtRssUrl.Text, new TimeSpan(0, 1, 0));

            var root = Document.GetRootDocuments().Where(x => x.ContentType.Alias == "uBlogsyLanding").FirstOrDefault();

            // get landing
            Document landing = DocumentService.Instance.GetDocumentByAlias(root, "uBlogsyLanding", "uBlogsyLanding");
            landing = DocumentService.Instance.EnsureNodeExists(-1, landing, "uBlogsyLanding", "My Site", false);

            // make landing title == reader.Title
            landing.getProperty("uBlogsyContentTitle").Value = reader.Title;

            // get blog folder
            Document blogFolder = landing.Children.Where(x => x.ContentType.Alias == "uBlogsyContainerBlog").FirstOrDefault();
            blogFolder = DocumentService.Instance.EnsureNodeExists(landing.Id, blogFolder, "uBlogsyContainerBlog", "Blog", false);

            int year = -1;
            int month = -1;
            Document yearFolder = null;
            Document monthFolder = null;

            var items = reader.Items.OrderBy(x => x.Date);
            foreach (RssItem item in items)
            {
                // create year folder
                if (item.Date.Year != year)
                {
                    year = item.Date.Year;

                    // get year folder if already exists
                    yearFolder = blogFolder.Children.Where(x => x.Text.Trim() == year.ToString()).FirstOrDefault();

                    if (yearFolder == null)
                    {
                        // create new year folder
                        yearFolder = CreateYear(blogFolder.Id, year);
                    }
                }

                // we are using month fodlers
                if (cbxUseMonth.Checked)
                {
                    if (item.Date.Month != month)
                    {
                        month = item.Date.Month;

                        // get month folder if already exists
                        monthFolder = GetMonth(item, yearFolder);
                        if (monthFolder == null)
                        {
                            // create new month folder
                            monthFolder = CreateMonthFolder(item, yearFolder.Id);
                        }
                    }

                    if (!PostExists(item, monthFolder))
                    {
                        // create post item
                        CreatePost(item, monthFolder.Id);
                    }
                }
                else
                {
                    if (!PostExists(item, yearFolder))
                    {
                        // create post item
                        CreatePost(item, yearFolder.Id);
                    }
                }
            }
        }

        #endregion



        #region GetMonth
        /// <summary>
        /// Returns month if exists.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentFolder"></param>
        /// <returns></returns>
        private Document GetMonth(RssItem item, Document parentFolder)
        {
            foreach (var child in parentFolder.Children)
            {
                if (child.ContentType.Alias == "uBlogsyFolderMonth")
                {
                    if (child.Text.Trim().Contains(item.Date.ToString("MMMM")))
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        #endregion




        #region PostExists

        /// <summary>
        /// Returns true of a post exists with the same name and publish date
        /// </summary>
        /// <param name="item"></param>
        /// <param name="monthFolder"></param>
        /// <returns></returns>
        private bool PostExists(RssItem item, Document parentFolder)
        {
            foreach (var post in parentFolder.Children)
            {
                if (post.getProperty("uBlogsyContentTitle").Value.ToString().Flatten() == item.Title.Flatten()
                    && (DateTime)post.getProperty("uBlogsyPostDate").Value == item.Date)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion



        #region CreateMonthFolder
        /// <summary>
        /// Creates a month folder
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private Document CreateMonthFolder(RssItem item, int parentId)
        {
            string monthName = item.Date.ToString("MMMM");

            return UmbracoAPIHelper.CreateContentNode(monthName, "uBlogsyFolderMonth", new Dictionary<string, object>(), parentId, false);
        }

        #endregion










        #region CreatePost
        /// <summary>
        /// Creates a uBlogsyPost Document 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private Document CreatePost(RssItem item, int parentId)
        {
            // create post item
            Dictionary<string, object> postDic = new Dictionary<string, object>()
            {
                {"uBlogsyPostDate", item.Date},
                {"uBlogsyPostAuthor",txtAuthor.Text},
                {"uBlogsyContentBody",item.Description},
                {"uBlogsyContentTitle",item.Title}
            };

            Document post = UmbracoAPIHelper.CreateContentNode(item.Title, "uBlogsyPost", postDic, parentId, false);

            return post;
        }

        #endregion




        #region CreateYear
        /// <summary>
        /// Creates a uBlogsyFolderYear document
        /// </summary>
        /// <param name="blogFolderId"></param>
        /// <param name="year"></param>
        /// <returns></returns>
        private Document CreateYear(int blogFolderId, int year)
        {
            return UmbracoAPIHelper.CreateContentNode(year.ToString(), "uBlogsyFolderYear", new Dictionary<string, object>(), blogFolderId, false);
        }

        #endregion






    }
}