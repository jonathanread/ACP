using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using uBlogsy.BusinessLogic.Extensions;
using uBlogsy.BusinessLogic.Models;
using uBlogsy.Common.Helpers;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.MacroEngines;

namespace uBlogsy.BusinessLogic
{
    internal interface IEmailService
    {
        void SendNotificationEmails(string postUrl, int pageId, int commentId);
        void SendAdminNotificationEmail(string postUrl, int pageId, CommentInfo commentInfo, int commentId);
        void SendContactEmail(HttpRequest Request, int m_PageId, CommentInfo commentInfo);
    }


    public class EmailService : IEmailService
    {
        #region Singleton

        protected static volatile EmailService m_Instance = new EmailService();
        protected static object syncRoot = new Object();

        protected EmailService() { }

        public static EmailService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new EmailService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion


        #region SendCommentNotifications

        /// <summary>
        /// Sends notification emails to the subscribers of this post.
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="pageId"></param>
        /// <param name="commentId"></param>
        public void SendNotificationEmails(string postUrl, int pageId, int commentId)
        {
            // get title of post
            string postTitle = new DynamicNode(pageId).GetProperty("uBlogsyContentTitle").Value;

            // get reply to address from landing node
           // string replyToAddress = DataService.Instance.GetValueFromLanding(pageId, "uBlogsyContactReplyToEmail");

            // get commenter's name, message etc
            var commentInfo = new CommentInfo(commentId);

            // get subscriber items from comment folder
            IEnumerable<int> subscriptionMemberIds = SubscriptionService.Instance.GetSubscriptionMemberIds(pageId);

            // get email template node and values
            string emailBody, subject, replyToAddress, adminEmail;
            GetEmailProperties(pageId, "CommentNotification", out emailBody, out subject, out replyToAddress, out adminEmail);

            // iterate over meta data of subscribers to this post
            foreach (var memberId in subscriptionMemberIds)
            {
                if (!Member.IsNode(memberId))
                {
                    Log.Add(LogTypes.Error, memberId, string.Format("uBlogsy BusinessLogic SendNotificationEmails: member {0} does not exist", memberId));
                    continue;
                }

                // get member
                var m = new Member(memberId);
                

                // get this member's subscription to this post from string postId|unsubscribeGuid|dateCreated
                var subscription = m.uBlogsyGetSubscriptions().FirstOrDefault(x => x.PostId == pageId);

                if (subscription == null || commentInfo.Email == CommentInfo.GetEmailNonPrefix(m.Email))
                {
                    // do not send notification to the commenter
                    continue;
                }

                // create dictionary for email
                var dictionary = new Dictionary<string, string>() { 
                        {"##RecipientName##", subscription.MemberName},
                        {"##CommenterName##", commentInfo.Name},
                        {"##CommenterEmail##", commentInfo.Email},
                        {"##PostTitle##", postTitle},
                        {"##CommentBody##", commentInfo.Message},
                        {"##CommentUrl##", postUrl + CommentService.Instance.GetCommentUrl(new DynamicNode(commentId), false)},
                        {"##UnsubscribeUrl##", subscription.GetUnsubscribeUrl(postUrl)},
                    };

                // send email to subscriber
                //EmailHelper.SendTemplated(emailBody, subject, replyToAddress, subscription.MemberEmail, dictionary, true);
                //  EmailHelper.SendTemplated(EMAIL_TEMPLATE_PATH, "CommentNotification", replyToAddress, CommentInfo.GetEmailNonPrefix(subscription.MemberEmail), dictionary, true);
                
                EmailHelper.Send(emailBody, subject, replyToAddress, CommentInfo.GetEmailNonPrefix(subscription.MemberEmail), dictionary, true);
            }
        }


        #endregion




        #region GetEmailProperties

        /// <summary>
        /// Gets values from email template
        /// 
        /// I dont know if this is good or bad but it makes the other methods more concise :|
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="emailTemplateName"> </param>
        /// <param name="emailBody"></param>
        /// <param name="subject"></param>
        /// <param name="replyToAddress"></param>
        /// <param name="adminContactEmail"> </param>
        protected void GetEmailProperties(int pageId, string emailTemplateName, out string emailBody, out string subject, out string replyToAddress, out string adminContactEmail)
        {
            // get email tempalte node from Umbraco
            var templateNode = GetEmailTemplateNode(pageId, emailTemplateName);

            emailBody = templateNode.GetPropertyValue("uBlogsyEmailBody");
            subject = templateNode.GetPropertyValue("uBlogsyEmailSubject");

            replyToAddress = templateNode.GetPropertyValue("uBlogsyEmailReplyToOverride") != string.Empty
                                       ? templateNode.GetPropertyValue("uBlogsyEmailReplyToOverride")
                                       : DataService.Instance.GetValueFromLanding(pageId, "uBlogsyContactReplyToEmail"); // overrides replyToAddress if exists

            adminContactEmail = templateNode.GetPropertyValue("uBlogsyEmailContactEmailOverride") != string.Empty
                                       ? templateNode.GetPropertyValue("uBlogsyEmailContactEmailOverride")
                                       : DataService.Instance.GetValueFromLanding(pageId, "uBlogsyContactEmail"); // overrides replyToAddress if exists

        }
        #endregion



        #region SendAdminNotificationEmail

        /// <summary>
        /// Sends notification email to blog admin.
        /// </summary>
        /// <param name="postUrl"></param>
        /// <param name="pageId"></param>
        /// <param name="commentInfo"></param>
        /// <param name="commentId"></param>
        public void SendAdminNotificationEmail(string postUrl, int pageId, CommentInfo commentInfo, int commentId)
        {
            var notify = DataService.Instance.GetValueFromLanding(pageId, "uBlogsyContactCommentNotification");
            if (notify != "1")
            {
                return;
            }

            // get title of post
            string postTitle = new DynamicNode(pageId).GetProperty("uBlogsyContentTitle").Value;

            // get reply to address from landing node
            //string replyToAddress = DataService.Instance.GetValueFromLanding(pageId, "uBlogsyContactReplyToEmail");

            // get email template node and values
            string emailBody, subject, replyToAddress, adminEmail;
            GetEmailProperties(pageId, "AdminCommentNotification", out emailBody, out subject, out replyToAddress, out adminEmail);

            // create dictionary for email tokens
            var dictionary = new Dictionary<string, string>() { 
                        {"##CommenterName##", commentInfo.Name},
                        {"##PostTitle##", postTitle},
                        {"##CommentBody##", commentInfo.Message},
                        {"##PostUrl##", postUrl},
                        {"##CommenterEmail##", commentInfo.Email}

                        //{"##CommentUrl##", postUrl + CommentService.Instance.GetCommentUrl(new DynamicNode(commentId), false)},
                    };

            // send email to subscriber
            //EmailHelper.SendTemplated(EMAIL_TEMPLATE_PATH, "AdminCommentNotification", replyToAddress, adminEmail, dictionary, true);

            EmailHelper.Send(emailBody, subject, replyToAddress, adminEmail, dictionary, true);
        }

        #endregion




        #region SendContactEmail
        /// <summary>
        /// Sends contact email to admin
        /// </summary>
        /// <param name="request"></param>
        /// <param name="pageId"></param>
        /// <param name="commentInfo"></param>
        public void SendContactEmail(HttpRequest request, int pageId, CommentInfo commentInfo)
        {
            // get email template node and values
            string emailBody, subject, replyToAddress, adminEmail;
            GetEmailProperties(pageId, "AdminContact", out emailBody, out subject, out replyToAddress, out adminEmail);

            // make dictionary for tokens
            var dictionary = new Dictionary<string, string>() { 
                    {"##CommenterName##", commentInfo.Name},
                    {"##CommenterEmail##", commentInfo.Email},
                    {"##CommenterWebsite##", commentInfo.Website},
                    {"##CommentBody##", commentInfo.Message}
                };

            // send the email
            //EmailHelper.SendTemplated(EMAIL_TEMPLATE_PATH, "AdminContact", commentInfo.Email, adminEmail, dictionary, true);

            // send email to admin - reply-to address is admin?
            EmailHelper.Send(emailBody, subject, replyToAddress, adminEmail, dictionary, true);
        }

        #endregion




        #region GetEmailTemplateNode

        /// <summary>
        /// Gets email template node in current tree with uBlogsyLanding as root.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="templateName"> </param>
        /// <returns></returns>
        protected DynamicNode GetEmailTemplateNode(int pageId, string templateName)
        {
            // get root
            var landing = DataService.Instance.GetLanding(pageId);

            // get container
            var templateContainer = landing.GetChildrenAsList.Items.Single(x => x.NodeTypeAlias == "uBlogsyContainerEmailTemplate");

            // get node
            var templateNode = templateContainer
                                .Descendants()
                                .Items
                                .Single(x => x.GetPropertyValue("uBlogsyEmailTemplateName") == templateName);

            return templateNode;
        }
        
        #endregion
    }
}
