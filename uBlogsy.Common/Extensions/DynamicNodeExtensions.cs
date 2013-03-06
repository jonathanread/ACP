using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.MacroEngines;

namespace uBlogsy.Common.Extensions
{
    public static class DynamicNodeExtensions
    {

        #region Ancestor
        /// <summary>
        /// Gets the ancestor with the given nodeTypeAlias
        /// </summary>
        /// <param name="d"></param>
        /// <param name="nodeTypeAlias"></param>
        /// <returns></returns>
        public static DynamicNode uBlogsyAncestor(this DynamicNode d, string nodeTypeAlias)
        {
            DynamicNode node = d;

            while (node.NodeTypeAlias != nodeTypeAlias)
            {
                node = node.Parent;
            }
            return node;
        }
        
        #endregion



        #region uBlogsyGetValueFirstOf

        /// <summary>
        /// Returns the value of the first property which has a value.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="aliases"></param>
        /// <returns></returns>
        public static string uBlogsyGetValueFirstOf(this DynamicNode d, string[] aliases)
        {
            foreach (string s in aliases)
            {
                if (d.GetProperty(s) != null)
                {
                    string title = d.GetProperty(s).Value;
                    if (!string.IsNullOrEmpty(title))
                    {
                        return title;
                    }
                }
            }

            return string.Empty;
        }
        
        #endregion
    }
}