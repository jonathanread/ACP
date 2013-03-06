using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using uBlogsy.BusinessLogic.Models;
using uHelpsy.Core;
using umbraco.cms.businesslogic.member;
using umbraco.MacroEngines;

namespace uBlogsy.BusinessLogic.Extensions
{
    using umbraco.cms.businesslogic.web;

    public static class  MemberExtensions
    {
        #region uBlogsyGetSubscriptions

        /// <summary>
        /// Gets subscriptions for this member
        /// </summary>
        /// <param name="member"></param>
        /// <param name="pageId"></param>
        /// <param name="postUrl"></param>
        /// <param name="commentId"></param>
        /// <returns></returns>
        public static IEnumerable<Subscription> uBlogsyGetSubscriptions(this Member member)
        {
            IEnumerable<Subscription> subscriptions = member.getProperty("uBlogsyMemberSubscriptions")
                                                        .Value
                                                        .ToString()
                                                        .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(x => x.Split("|".ToCharArray(), StringSplitOptions.RemoveEmptyEntries))
                                                        .Select(x => new Subscription(member, x));
            return subscriptions;
        }
        
        #endregion




        #region uBlogsyUpdateMember
        /// <summary>
        /// Updates website, member name, and url for member
        /// </summary>
        /// <param name="member"></param>
        /// <param name="commentInfo"></param>
        /// <returns></returns>
        public static Member uBlogsyUpdateMember(this Member member, CommentInfo commentInfo)
        {
            var properties = new Dictionary<string, string>()
                                                        {
                                                            {"uBlogsyMemberWebsite", commentInfo.Website}, 
                                                            {"uBlogsyMemberName", commentInfo.Name}, 
                                                            {"uBlogsyMemberCommentUrlName", commentInfo.MemberNameForUrl}, 
                                                        };

            return member.uBlogsyUpdateMember(properties);
        }
        
        #endregion




        #region uBlogsyAddSubscription

        /// <summary>
        /// Adds a subscription string to the member.
        /// Updates the comment folder with meta data.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="commentInfo"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static Member uBlogsyAddSubscription(this Member member, CommentInfo commentInfo, int pageId)
        {
            // add subscription string to member
            string subscriptionString = member.getProperty("uBlogsyMemberSubscriptions").Value + new Subscription(member, pageId).GetSubscriptionStringForMember() + Environment.NewLine;
            member.uBlogsyUpdateMember(new Dictionary<string, string>() {{"uBlogsyMemberSubscriptions", subscriptionString}});

            // now update comment folder with new meta data...

            // get comment folder
            var commentContainer = new Document(pageId).Children.Single(x => x.ContentType.Alias == "uBlogsyContainerComment");

            // add member id|email|date created to the 
            string subscriberMetaData = uBlogsyGetSubscriptionMetaData(commentContainer, member) + Environment.NewLine;

            // now update the node
            UmbracoAPIHelper.UpdateContentNode(commentContainer.Id, new Dictionary<string, object>() { { "uBlogsySubscriptionsMetaData", subscriberMetaData } }, true);

            return member;
        }

        #endregion







        #region uBlogsyUnsubscribe
        /// <summary>
        /// Unsubscribes a user from the given post.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="pageId"></param>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static Result uBlogsyUnsubscribe(this Member member, int pageId, string guid)
        {
            // get subscription list, remove, item, then create new string
            var subscriptionsAfterUnsubscribe = member.uBlogsyGetSubscriptions(pageId);

            // create new subscription string
            string newSubscriptionString = string.Join(Environment.NewLine, subscriptionsAfterUnsubscribe.Select(x => x.GetSubscriptionStringForMember()).ToArray());

            // save string to member
            //member.getProperty("uBlogsyMemberSubscriptions").Value = newSubscriptionString;
            member.uBlogsyUpdateMember(new Dictionary<string, string>() { { "uBlogsyMemberSubscriptions", newSubscriptionString } });

            // remove meta data from comment node
            var commentContainer = new Document(pageId).Children.Single(x => x.ContentType.Alias == "uBlogsyContainerComment");

            // get subscriptionMetaDatas
            var subscriptionMetaDatas = commentContainer
                                                .getProperty("uBlogsySubscriptionsMetaData")
                                                .Value.ToString()
                                                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                                                .Select(x => new SubscriptionMetaData(x))
                                                .Where(x => x.MemberId != member.Id);

            // create new meta data string with new line at end to ensure next subscription starts on new line
            var newMetaDataString = string.Join(Environment.NewLine,
                                                   subscriptionMetaDatas.Select(x => x.GetSubscriptionMetaDataForComment()).ToArray()) + Environment.NewLine;

            // now update the node
            UmbracoAPIHelper.UpdateContentNode(commentContainer.Id, new Dictionary<string, object>()
                {
                    { "uBlogsySubscriptionsMetaData", newMetaDataString.TrimStart(Environment.NewLine.ToCharArray()) }
                }, true);

            return Result.Success;
        }
        
        #endregion







        #region uBlogsyGetSubscriptions

        /// <summary>
        /// Creates Subscription objects from this member's uBlogsyMemberSubscriptions property string.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="pageId"></param>
        /// <returns></returns>
        public static IEnumerable<Subscription> uBlogsyGetSubscriptions(this Member member, int pageId)
        {
            return member.getProperty("uBlogsyMemberSubscriptions").Value
                .ToString()
                .Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(x => new Subscription(member, x))
                .Where(x => x.PostId != pageId);
        }
        
        #endregion





        #region uBlogsyUpdateMember

        /// <summary>
        /// Takes a dictionary of properties to update.
        /// </summary>
        /// <param name="member"></param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public static Member uBlogsyUpdateMember(this Member member, Dictionary<string, string> properties)
        {
            foreach (string property in properties.Keys)
            {
                member.getProperty(property).Value = properties[property];
            }
            return member;
        }
        
        #endregion







        #region uBlogsyGetSubscriptionMetaData

        private static string uBlogsyGetSubscriptionMetaData(Document commentContainer, Member member)
        {
            return commentContainer.getProperty("uBlogsySubscriptionsMetaData").Value +
                   new SubscriptionMetaData(member.Id, member.Email).GetSubscriptionMetaDataForComment();
        }

        #endregion
    }
}
