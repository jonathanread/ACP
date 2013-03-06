using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.member;
using uBlogsy.Common.Helpers;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.web;

namespace uBlogsy.BusinessLogic.Models
{

    public class Subscription
    {

        public int MemberId { get; set; }
        public string MemberName { get; set; }
        public string MemberEmail { get; set; }

        public int PostId { get; set; }
        
        
        public string UnsubscribeGuid { get; set; }
        public DateTime Created { get; set; }

        public string UnsubscribeUrl { get; set; }
        public string CommentUrl { get; set; }


 




        public Subscription(Member member, string subscriptionString)
        {
            string[] values = subscriptionString.Split('|');
            MemberId = member.Id;
            
            PostId = int.Parse(values[0]);
            MemberEmail = member.Email;
            UnsubscribeGuid = values[1];
            Created = DateTime.Parse(values[2]);
        }



        public Subscription(Member member, int pageId)
        {
            MemberId = member.Id;
            PostId = pageId;
            MemberEmail = member.Email;
            UnsubscribeGuid = Guid.NewGuid().ToString();
            Created = DateTime.UtcNow;
        }


        //public Subscription(Member member, int pageId, string AbsoluteUri, int commentId) : this(member, pageId)
        //{
        //    UnsubscribeUrl = GetUnsubscribeUrl(AbsoluteUri);
        //    CommentUrl = commentId != -1 ? AbsoluteUri + CommentService.Instance.GetCommentUrl(new DynamicNode(commentId), false) : string.Empty;
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="m">The member</param>
        /// <param name="items">a string in the form postId|unsubscribeGuid|dateCreated</param>
        
        public Subscription(Member m, string[] items)
        {
            //string[] items = subscriptionItem.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            MemberId = m.Id;
            MemberName = m.getProperty("uBlogsyMemberName").Value.ToString();
            MemberEmail = m.Email;

            PostId = int.Parse(items[0]);
            UnsubscribeGuid = items[1];
            Created = DateTime.Parse(items[2]);

            
        }

        //#region CreateSubscriptionStringForMember

        ///// <summary>
        ///// Creates a "|" delimited subscription string which is stored in the member
        ///// String is in the form postId|unsubscribeGuid|date created
        ///// </summary>
        ///// <returns></returns>
        //public static string  CreateSubscriptionStringForMember(int pageId)
        //{
        //    return string.Format("{0}|{1}|{2}", pageId, Guid.NewGuid(), DateTime.UtcNow.ToString()) + Environment.NewLine;
        //}

        //#endregion




        public string GetSubscriptionStringForMember()
        {
            return string.Format("{0}|{1}|{2}", PostId, UnsubscribeGuid, Created);
        }


        //public static string GetCommentUrl(string postUrl, int commentId)
        //{
        //    return commentId != -1 ? postUrl + CommentService.Instance.GetCommentUrl(new DynamicNode(commentId), false) : string.Empty;
        //}



        #region GetUnsubscribeUrl

        public string GetUnsubscribeUrl(string absoluteUri)
        {
            return absoluteUri + "?action=unsubscribe&mid=" + MemberId + "&guid=" + UnsubscribeGuid;
        }
        
        #endregion



    }
}
