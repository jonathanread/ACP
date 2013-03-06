using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.web;
using uBlogsy.BusinessLogic.Helpers;

namespace uBlogsy.BusinessLogic.Extensions
{
    public static class DocumentExtensions
    {

        #region GetValueFromLanding
       /// <summary>
        ///  Returns a value from the ancestor specified by ancestorAlias.
       /// </summary>
       /// <param name="doc"></param>
       /// <param name="propertyName"></param>
       /// <returns></returns>
        public static string uBlogsyGetValueFromAncestor(this Document doc, string ancestorAlias, string propertyAlias)
        {
            Document root = doc;

            while (root.ContentType.Alias != ancestorAlias)
            {
                if (root.ParentId == -1)
                {
                    return string.Empty;
                }
                root = new Document(root.ParentId);
            }

            return root.getProperty(propertyAlias).Value.ToString();
        }
        #endregion







    }
}
