using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Examine.LuceneEngine;
using Joel.Net;
using uBlogsy.BusinessLogic.Models;
using uBlogsy.Common.Extensions;
using uHelpsy.Core;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.web;
using umbraco.MacroEngines;

namespace uBlogsy.BusinessLogic
{
    internal interface ICommentService
    {
        IEnumerable<DynamicNode> GetComments(int postId, bool allComments);
        Result SubmitComment(HttpRequest request, int pageId, CommentInfo commentInfo, bool subscribe);

        Document EnsureCommentsFolder(Document doc);

        string GetCommentUrl(DynamicNode d, bool complete);
    }

    public class CommentService : ICommentService
    {
        
        #region Singleton

        protected static volatile CommentService m_Instance = new CommentService();
        protected static object syncRoot = new Object();

        protected CommentService() { }

        public static CommentService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new CommentService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion


        #region GetComments

        /// <summary>
        /// Gets comments for a post, or all comments.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="allComments"></param>
        /// <returns></returns>
        public IEnumerable<DynamicNode> GetComments(int nodeId, bool allComments)
        {
            List<DynamicNode> comments;

            if (allComments)
            {
                // get posts
                var posts = PostService.Instance.GetPosts(nodeId);
                comments = new List<DynamicNode>();
                
                // get comments in posts
                foreach (var post in posts)
                {
                    var postComments = post.DescendantsOrSelf("uBlogsyComment").Items;
                    comments.AddRange(postComments);
                }
            }
            else
            {
                comments = new DynamicNode(nodeId).DescendantsOrSelf("uBlogsyComment").Items;
            }

            return comments.OrderByDescending(x => x.GetPropertyValue("uBlogsyCommentDate"));
        }
        
        #endregion




        #region SubmitComment
        /// <summary>
        /// Creates a content node, subscribes commenter to post, and sends notifications.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pageId"></param>
        /// <param name="commentInfo"></param>
        /// <param name="subscribe"></param>
        /// <returns></returns>
        public Result SubmitComment(HttpRequest request, int pageId, CommentInfo commentInfo, bool subscribe)
        {
            try
            {
                // create the comment node
                Document comment = CreateComment(request, pageId, commentInfo);

                if (comment == null)
                {
                    // returned null because of auto delete of spam
                    return Result.Spam;
                }

                // comment was not spam and was auto approved so now handle subscriptions
                // add user to members if they don't already exist
                SubscriptionService.Instance.SubscribeToPost(pageId, commentInfo, subscribe);

                //if (comment.Published)
                //{
                //    // comment was auto approved, so send out notifications
                //    EmailService.Instance.SendNotificationEmails(request.Url.AbsoluteUri, pageId, comment.Id);
                //}

                // now notify admin
                EmailService.Instance.SendAdminNotificationEmail(request.Url.AbsoluteUri, pageId, commentInfo, comment.Id);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, pageId, ex.Message + Environment.NewLine + ex.StackTrace);

                return Result.Error;
            }

            return Result.Success;
        }


        #endregion




        #region protected CreateComment
        /// <summary>
        /// Creates an umbraco node under the current page
        /// </summary>
        protected Document CreateComment(HttpRequest request, int pageId, CommentInfo commentInfo)
        {
            // get current page
            var d = new DynamicNode(pageId);

            // get comments folder
            var commentsFolder = EnsureCommentsFolder(new Document(pageId)); 

            // run spam detection
            bool spam = IsSpam(request, pageId, commentInfo.Name, commentInfo.Email, commentInfo.Message, commentInfo.Website);
            if (spam)
            {
                if (!AutoDeleteSpam(pageId))
                {
                    // not auto delete spam, so create node but do not publish
                    return UmbracoAPIHelper.CreateContentNode(commentInfo.MemberNameForUrl, "uBlogsyComment", commentInfo.GetDictionary(), commentsFolder.Id, false);
                }
                return null;
            }

            // create content node
            var comment = UmbracoAPIHelper.CreateContentNode(commentInfo.MemberNameForUrl, "uBlogsyComment", commentInfo.GetDictionary(), commentsFolder.Id, GetAutoApprove(pageId));
           
            return comment;
        }

        #endregion





        #region EnsureCommentsFolder
        /// <summary>
        /// Ensures that a comment container exists under the current post
        /// </summary>
        /// <param name="doc"></param>
        public Document EnsureCommentsFolder(Document doc)
        {
            var commentsFolder = doc.Children.FirstOrDefault(x => x.ContentType.Alias == "uBlogsyContainerComment");
            if (commentsFolder == null)
            {
                // create and publish the comments folder
                umbraco.library.UpdateDocumentCache(doc.Id);
                commentsFolder = UmbracoAPIHelper.CreateContentNode("Comments", "uBlogsyContainerComment", new Dictionary<string, object>(), doc.Id, true);
                umbraco.library.UpdateDocumentCache(commentsFolder.Id);
            }

            // publish comments folder when not published
            if (!commentsFolder.Published)
            {
                umbraco.library.UpdateDocumentCache(doc.Id);
                commentsFolder.Publish(new User(0));
                umbraco.library.UpdateDocumentCache(commentsFolder.Id);
            }

            return new Document(commentsFolder.Id);
        }

        #endregion





        #region protected AutoDeleteSpam
        /// <summary>
        /// Returns true if we are to delete spam automatically.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected bool AutoDeleteSpam(int pageId)
        {
            return DataService.Instance.GetValueFromLanding(pageId, "uBlogsySpamAutoDeleteSpam") == "1" ? true : false;
        }
        #endregion




        #region protected GetAutoApprove
        /// <summary>
        /// Returns true if we are to auto approve comments
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        protected bool GetAutoApprove(int pageId)
        {
            return DataService.Instance.GetValueFromLanding(pageId, "uBlogsySpamAutoApproveComments") == "1" ? true : false;
        }

        #endregion




        #region protected isSpam
        /// <summary>
        /// Uses Akismet service to detect spam
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pageId"></param>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="message"></param>
        /// <param name="commentAuthorUrl"></param>
        /// <returns></returns>
        protected bool IsSpam(HttpRequest request, int pageId, string name, string email, string message, string commentAuthorUrl)
        {
            var root = new DynamicNode(pageId).uBlogsyAncestor("uBlogsyLanding");

            string apiKey = root.GetProperty("uBlogsySpamAkismetAPIKey").Value;

            if (string.IsNullOrEmpty(apiKey.Trim()))
            {
                return false; // return not spam by default when no API key 
            }

            string blogUrl = request.Url.Host;
            Akismet api = new Akismet(apiKey, blogUrl, request.UserAgent);
            AkismetComment comment = new AkismetComment();
            comment.Blog = blogUrl;
            comment.UserIp = request.UserHostAddress;
            comment.UserAgent = request.UserAgent;
            comment.CommentContent = message;
            comment.CommentType = "comment";
            comment.CommentAuthorUrl = name;
            comment.CommentAuthorEmail = email;
            comment.CommentAuthorUrl = commentAuthorUrl == "http://" ? string.Empty : commentAuthorUrl;

            return api.CommentCheck(comment);
        }




        #endregion




        #region GetCommentUrl
        /// <summary>
        /// Creates a #! link for the given comment node.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="complete"></param>
        /// <returns></returns>
        public string GetCommentUrl(DynamicNode d, bool complete)
        {
            string hash = d.Url.Replace(d.Parent.Parent.Url, string.Empty);
            if (complete)
            {
                return d.Parent.Parent.Url + "#!/" + hash;
            }

            return "#!/" + hash;
        }

        #endregion

    }
}
