using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.MacroEngines;

namespace uBlogsy.BusinessLogic.Models
{
    public class SubscriptionMetaData
    {

        public int MemberId { get; set; }
        public string MemberEmail { get; set; }
        public DateTime Created { get; set; }


        public SubscriptionMetaData(string subscriptionMetaDataString)
        {
            // split string memberId|email|dateCreated
            string[] s = subscriptionMetaDataString.Split('|');
            MemberId = int.Parse(s[0]);
            MemberEmail = s[1];
            Created = DateTime.Parse(s[2]);
        }

        public SubscriptionMetaData(int memberId, string email)
        {
            // split string memberId|email|dateCreated
            MemberId = memberId;
            MemberEmail = email;
            Created = DateTime.UtcNow;
        }
       

        /// <summary>
        /// Creates a string in the from memberId|email|createdDate, 
        /// </summary>
        /// <returns></returns>
        public string GetSubscriptionMetaDataForComment()
        {
            return string.Format("{0}|{1}|{2}", MemberId, MemberEmail, Created);
        }

    }
}
