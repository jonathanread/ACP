using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using uBlogsy.BusinessLogic.Extensions;
using uBlogsy.BusinessLogic.Models;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.member;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.web;

namespace uBlogsy.BusinessLogic
{
    internal interface ISubscriptionService
    {
        void SubscribeToPost(int pageId, CommentInfo commentInfo, bool subscribe);
        Result UnsubscribeFromPost(HttpRequest request, int pageId, int memberId, string guid);
        IEnumerable<SubscriptionMetaData> GetSubscriptionMetaData(int pageId);
        bool MetaDataContains(int pageId, string email);
    }

    public class SubscriptionService
    {

        #region Singleton

        protected static volatile SubscriptionService m_Instance = new SubscriptionService();
        protected static object syncRoot = new Object();

        protected SubscriptionService() { }

        public static SubscriptionService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new SubscriptionService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion





        #region SubscribeToPost
        /// <summary>
        /// If member does not already exist, creates a member using the users email address.
        /// Inserts post Id and unsubscribe code into member
        /// </summary>
        public void SubscribeToPost(int pageId, CommentInfo commentInfo, bool subscribe)
        {
            // find subscriber in commentContainer's subscriber items 
            if (MetaDataContains(pageId, commentInfo.EmailPrefixed))
            {
                // should we ensure member has subscription?
                // email has alredy subscribed so return
                return;
            }

            // get member if exists, otherwise create one...we are creating a member even if they do not subscribe!
            Member m = Member.GetMemberFromEmail(commentInfo.EmailPrefixed) ??
                       Member.MakeNew(commentInfo.EmailPrefixed, commentInfo.EmailPrefixed, MemberType.GetByAlias("uBlogsySubscriber"), new User(0));

            m.uBlogsyUpdateMember(commentInfo);

            // add subscription
            if (subscribe)
            {
                m.uBlogsyAddSubscription(commentInfo, pageId);
            }
        }



        #endregion




        #region UnsubscribeFromPost

        public Result UnsubscribeFromPost(HttpRequest request, int pageId, int memberId, string guid)
        {
            try
            {
                Member m = new Member(memberId);
                if (m.Id == 0)
                {
                    return Result.MemberNotFound;
                }

                // check if member is subscribed
                bool hasSubscription = m.uBlogsyGetSubscriptions().Where(x => x.PostId == pageId).Count() > 0;
                if (!hasSubscription)
                {
                    return Result.SubscriptionNotFound;
                }

                // now we can actually unsubscribe the member...

                // get subscriptions from member as array
                return m.uBlogsyUnsubscribe(pageId, guid);
            }
            catch (Exception ex)
            {
                Log.Add(LogTypes.Error, pageId, ex.Message + Environment.NewLine + ex.StackTrace);
                return Result.Error;
            }
        }

        #endregion






        #region GetSubscriptionMetaData

        /// <summary>
        /// Returns list of SubscriptionMetaData built from the strings of memberId|email|dateCreated int the comments container.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public IEnumerable<SubscriptionMetaData> GetSubscriptionMetaData(int pageId)
        {
            // get comments folder
            var comments = new Document(pageId).GetDescendants().Cast<Document>().Single(x => x.ContentType.Alias == "uBlogsyContainerComment");

            // get list of memberId|email|dateCreated from comments folder
            IEnumerable<SubscriptionMetaData> subscriberItems = comments
                .getProperty("uBlogsySubscriptionsMetaData")
                .Value
                .ToString()
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new SubscriptionMetaData(x));
            return subscriberItems;
        }

        #endregion



        #region MetaDataContains
        /// <summary>
        /// Returns true if the email exists in the meta data of the comments container under pageId.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool MetaDataContains(int pageId, string email)
        {
            bool found = GetSubscriptionMetaData(pageId)
                            .Where(x => x.MemberEmail == email)
                            .Count() > 0;
            return found;
        }

        #endregion




        #region GetSubscriptionMemberIds

        /// <summary>
        /// Gets the member ids which are in the meta data for the given post id.
        /// </summary>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public IEnumerable<int> GetSubscriptionMemberIds(int pageId)
        {
            return GetSubscriptionMetaData(pageId).Select(x => x.MemberId);
        }
        
        #endregion

    }
}
