using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uBlogsy.BusinessLogic.Extensions;
using uHelpsy.Core;
using Joel.Net;
using umbraco.MacroEngines;
using uBlogsy.Common.Helpers;
using uBlogsy.Common.Extensions;
using umbraco.cms.businesslogic.member;
using umbraco.BusinessLogic;
using uBlogsy.BusinessLogic.Models;
using umbraco.cms.businesslogic.web;


namespace uBlogsy.BusinessLogic
{
    interface IuBlogsyService
    {
        string GetValueFromLanding(int pageId, string propertyAlias);
        string GetValueFromAncestor(int pageId, string ancestorAlias, string propertyAlias);
        DynamicNode GetLanding(int nodeId);
    }

    public class DataService : IuBlogsyService
    {
        #region Singleton

        protected static volatile DataService m_Instance = new DataService();
        protected static object syncRoot = new Object();

        protected DataService() { }

        public static DataService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new DataService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion




        #region GetValueFromLanding
        /// <summary>
        /// Returns a value from the landing node.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        public string GetValueFromLanding(int pageId, string propertyAlias)
        {
            return GetLanding(pageId).GetProperty(propertyAlias).Value;
        }
        #endregion




        #region GetValueFromAncestor
      

        /// <summary>
        /// Returns a value from the ancestor specified by ancestorAlias.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="ancestorAlias"></param>
        /// <param name="propertyAlias"></param>
        /// <returns></returns>
        public string GetValueFromAncestor(int pageId, string ancestorAlias, string propertyAlias)
        {
            string cacheKey = "uBlogsy_GetValueFromAncestor_" + ancestorAlias;

            DynamicNode root = CacheHelper.GetFromRequestCache(cacheKey) as DynamicNode;
            if (root == null)
            {
                DynamicNode d = new DynamicNode(pageId);
                root = d.AncestorOrSelf(ancestorAlias);
                CacheHelper.AddToRequestCache(cacheKey, root);
            }
            
            return root.GetProperty(propertyAlias).Value;
        }
        #endregion




        /// <summary>
        /// Gets landing node, caches result.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public DynamicNode GetLanding(int nodeId)
        {
            string cacheKey = "GetLanding_uBlogsyLanding";

            DynamicNode cached = CacheHelper.GetFromRequestCache(cacheKey) as DynamicNode;
            if (cached != null)
            {
                return cached;
            }

            var node = new DynamicNode(nodeId).AncestorOrSelf("uBlogsyLanding");

            // cache the result
            CacheHelper.AddToRequestCache(cacheKey, node);

            return node;
        }
    }
}
