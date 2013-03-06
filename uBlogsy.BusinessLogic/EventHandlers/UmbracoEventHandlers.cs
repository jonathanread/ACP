using System;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using System.Web;


namespace uBlogsy.BusinessLogic.EventHandlers
{
    public class UmbracoExtensions : ApplicationBase
    {
        // used to check if emails should be sent after a publish
        private const string SkipEmailsKey = "uBlogsy.BusinessLogic.EventHandlers_SkipEmailsKey_";


        public UmbracoExtensions()
        {
            Document.AfterSave += new Document.SaveEventHandler(Document_AfterSave);
            Document.AfterMove += new EventHandler<MoveEventArgs>(Document_AfterMove);
            Document.AfterPublish += new Document.PublishEventHandler(Document_AfterPublish);
            Document.BeforePublish += new Document.PublishEventHandler(Document_BeforePublish);
        }

        void Document_BeforePublish(Document sender, PublishEventArgs e)
        {
            if (sender.ContentType.Alias == "uBlogsyComment")
            {
                // flag skip emails
                HttpContext.Current.Session[SkipEmailsKey + sender.Id] = sender.HasPublishedVersion();

                //if (sender.Published) { return; } // no need to send notifications because publish already occured

                // this is purely because of upgrading from 2.1 to 2.1.1 introduced uBlogsyCommentDate. 
                if (sender.getProperty("uBlogsyCommentDate").Value == string.Empty)
                {
                    sender.getProperty("uBlogsyCommentDate").Value = sender.CreateDateTime;
                }
            }
        }




        void Document_AfterPublish(Document sender, PublishEventArgs e)
        {
            if (sender.ContentType.Alias == "uBlogsyComment")
            {
                if (!ConfigReader.Instance.GetSendNotificationOnRepublish() && (bool)HttpContext.Current.Session[SkipEmailsKey + sender.Id])
                {
                    // do not send notifications if this not has already been published
                    HttpContext.Current.Session.Remove(SkipEmailsKey + sender.Id);
                    return;
                }

                try
                {
                    // ensure that we can use DynamicNode
                    umbraco.library.UpdateDocumentCache(sender.Parent.Id);
                    umbraco.library.UpdateDocumentCache(sender.Id);
                    umbraco.library.RefreshContent();

                    // send out notifications
                    EmailService.Instance.SendNotificationEmails(sender.getProperty("uBlogsyCommentPublicDomain").Value.ToString(), sender.Parent.ParentId, sender.Id);
                }
                catch (Exception ex)
                {
                    //string message = string.Format("Document_AfterPublish - Post with Id{0} and it's children were not in cache. Could not call SendNotificationEmails", sender.Parent.ParentId);
                    Log.Add(LogTypes.Error, sender.Parent.ParentId, ex.Message + ex.StackTrace);
                }
            }

        }



        #region Document_AfterSave

        /// <summary>
        /// Ensures that a post exists under the correct year/month node determined by its date.
        /// Ensures that node name is the same as post title.
        /// Assigns values to meta keywords and descriptions for Posts.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterSave(Document sender, SaveEventArgs e)
        {
            if (sender.ContentType.Alias == "uBlogsyPost")
            {
                if (ConfigReader.Instance.UseAutoDateFolders())
                {
                    // put node under correct parent
                    DateFolderService.Instance.EnsureCorrectParentForPost(sender);
                }

                // make node name same as title, and ensure uniqueness under parent
                DocumentService.Instance.EnsureCorrectPostNodeName(sender);
            }
        }

        #endregion






        #region Document_AfterMove

        /// <summary>
        /// After a uBlogsyPost has been moved, we must change the date.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Document_AfterMove(object sender, MoveEventArgs e)
        {
            Document doc;

            try
            {
                doc = new Document(((CMSNode)sender).Id);
            }
            catch
            {
                // crashed because it was a media node
                return;
            }

            if (doc.ContentType.Alias == "uBlogsyPost")
            {
                // this may have been a move to the recycle bin!
                bool isMoveToBin = doc.ParentId == -20;

                // check up the .parent path for recycle bin
                Document current = doc;
                while (current.ParentId != -1)
                {
                    if (current.ParentId == -20)
                    {
                        isMoveToBin = true;
                        break;
                    }
                    current = new Document(current.ParentId);
                }

                if (!isMoveToBin)
                {
                    if (ConfigReader.Instance.UseAutoDateFolders())
                    {
                        DateFolderService.Instance.EnsureCorrectDate(doc);
                    }
                }
            }
        }

        #endregion



    }
}