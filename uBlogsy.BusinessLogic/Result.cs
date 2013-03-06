using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace uBlogsy.BusinessLogic
{
    public enum Result
    {
        Success, 
        GuidNotFound, 
        MemberNotFound,
        SubscriptionNotFound,
        Spam,
        Error
    }
}
