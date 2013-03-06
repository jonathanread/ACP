using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.MacroEngines;
using umbraco.cms.businesslogic.member;
using System.Web.Security;
using umbraco.cms.businesslogic.web;

namespace uBlogsy.BusinessLogic.Models
{
    public class CommentInfo
    {
        private const string EMAIL_PREFIX = "uBlogsySafePrefix_";

        public string Name { get; set; } // commenter's name

        public string Message { get; set; } // commenter's message
        public string Website { get; set; }  // commenter's website
        public string MemberNameForUrl { get; set; }  // generated commenter url name eg. anthony-1, anthony-2 etc 

        public bool IsAuthor { get; set; }

        public string Email { get { return GetEmail(); } } // commenter's email

        public string EmailPrefixed { get { return m_Email; } } // commenter's email

        public string PublicDomain { get; set; } // used for multi-domain sites

        public DateTime Created { get; set; }

        protected string m_Email;


        #region CommentInfo constructors

        /// <summary>
        /// Used when a content node does not already exist. When this is the case, this object is used for passing values neatly.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="email"></param>
        /// <param name="website"></param>
        /// <param name="message"></param>
        /// <param name="isAuthor"> </param>
        /// <param name="publicDomain"> </param>
        /// <param name="created"> </param>
        public CommentInfo(string name, string email, string website, string message, bool isAuthor, string publicDomain, DateTime created)
        {
            Name = name;
            m_Email = EMAIL_PREFIX + email; // prefix emails so our members do not interfere
            Message = message;
            Website = website;
            MemberNameForUrl = CreateNameForContentNode();
            IsAuthor = isAuthor;
            PublicDomain = publicDomain;
            Created = created;
        }


        /// <summary>
        /// Used when a content node exists. Gets property values from DynamicNode.
        /// </summary>
        /// <param name="commentId"></param>
        public CommentInfo(int commentId)
        {
            Document commentNode = new Document(commentId);
            Name = commentNode.getProperty("uBlogsyCommentName").Value.ToString();
            Message = commentNode.getProperty("uBlogsyCommentMessage").Value.ToString();
            m_Email = commentNode.getProperty("uBlogsyCommentEmail").Value.ToString();
            Website = commentNode.getProperty("uBlogsyCommentWebsite").Value.ToString();
            PublicDomain = commentNode.getProperty("uBlogsyCommentPublicDomain").Value.ToString();
            MemberNameForUrl = CreateNameForContentNode();
            Created = !string.IsNullOrEmpty(commentNode.getProperty("uBlogsyCommentDate").Value.ToString())
                        ? DateTime.Parse(commentNode.getProperty("uBlogsyCommentDate").Value.ToString())
                        : commentNode.CreateDateTime;
        }

        #endregion


        public string GetEmail()
        {
            return m_Email.Replace(EMAIL_PREFIX, string.Empty);
        }



        public static string GetEmailNonPrefix(string email)
        {
            return email.Replace(EMAIL_PREFIX, string.Empty);
        }



        #region GetDictionary

        /// <summary>
        /// Creates a dictionary from  properties.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, object> GetDictionary()
        {
            // create a dictionary from the posted properties
            Dictionary<string, object> properties = new Dictionary<string, object>() { 
                {"uBlogsyCommentName", Name},
                {"uBlogsyCommentEmail", Email},
                {"uBlogsyCommentWebsite", Website},
                {"uBlogsyCommentMessage", Message},
                {"uBlogsyCommentPublicDomain", PublicDomain},
                {"uBlogsyCommentDate", Created},

            };
            return properties;
        }

        #endregion




        #region CreateNameForContentNode
        /// <summary>
        /// When there are multiple members with the same name, we must distinguish them.
        /// Tries to find the member with the current email address. Return's their assigned url display name.
        /// Otherwise, creates a new url name. eg. anthony-1, anthony-2.
        /// </summary>
        /// <returns></returns>
        private string CreateNameForContentNode()
        {
            Member member = Member.GetMemberFromEmail(EmailPrefixed);

            if (member != null && member.ContentType.Alias == "uBlogsySubscriber")
            {
                // found member, so use it's stored url member name
                return member.getProperty("uBlogsyMemberCommentUrlName").Value.ToString();
            }
            // no member found, 
            // get all members with this name
            int count = Member.GetAllAsList().Where(x => x.ContentType.Alias == "uBlogsySubscriber").Where(x => x.getProperty("uBlogsyMemberName").Value.ToString() == Name).Count();

            if (count == 0)
            {
                return Name;
            }
            return Name + "-" + (count + 1);
        }

        #endregion



    }
}
