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
using uBlogsy.BusinessLogic.Helpers;
using System.Collections;
using System.Text.RegularExpressions;


namespace uBlogsy.BusinessLogic
{
    interface IDocumentService
    {
        Document EnsureCorrectPostNodeName(Document doc);
        Document EnsureNodeExists(int parentId, Document doc, string alias, string name, bool publish);
        Document GetDocumentByAlias(Document current, string rootAlias, string alias);
    }

    public class DocumentService : IDocumentService
    {
        #region Singleton

        protected static volatile DocumentService m_Instance = new DocumentService();
        protected static object syncRoot = new Object();

        protected DocumentService() { }

        public static DocumentService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new DocumentService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion




        #region EnsureCorrectPostNodeName

        /// <summary>
        /// Ensures that the node name is the same as the post title
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Document EnsureCorrectPostNodeName(Document doc)
        {
            string useTitleAsNodeName = doc.uBlogsyGetValueFromAncestor("uBlogsyLanding", "uBlogsyGeneralUseTitleAsNodeName");

            if (useTitleAsNodeName == "1")
            {
                string title = doc.getProperty("uBlogsyContentTitle").Value.ToString();
                if (!string.IsNullOrEmpty(title) && doc.Text != title)
                {
                    // ensure node name is same as title
                    doc.Text = title;
                }
            }

            // ensure that node name is not the same as other siblings
            //var siblingsWithSameName = new Document(doc.ParentId).Children.Where(x => x.Text == doc.Text).Where(x => x.Id != doc.Id);
            //if (siblingsWithSameName.Count() > 0)
            //{
            //    string suffix = string.Format("{0} ({1})", siblingsWithSameName.Count());
            //    string res = (new Regex("(\\d)")).Replace(doc.Text, suffix);

            //    doc.Text = res;
            //}

            return doc;
        }

        #endregion











        #region EnsureNodeExists

        /// <summary>
        /// Creates node if it does not exist.
        /// </summary>
        /// <param name="parentId"></param>
        /// <param name="doc"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="publish"></param>
        /// <returns></returns>
        public Document EnsureNodeExists(int parentId, Document doc, string alias, string name, bool publish)
        {
            return doc ?? UmbracoAPIHelper.CreateContentNode(name, alias, new Dictionary<string, object>(), parentId, publish);
        }

        #endregion





        #region GetDocumentByAlias

        /// <summary>
        ///  Searches up the tree until it hits rootAlias, then does a breadth first search to get a document by it's alias.
        /// </summary>
        /// <param name="current"></param>
        /// <param name="rootAlias"></param>
        /// <param name="alias"></param>
        /// <returns></returns>
        public Document GetDocumentByAlias(Document current, string rootAlias, string alias)
        {
            Document doc = current;

            // get uBlogsyLanding
            while (doc.ContentType.Alias != rootAlias)
            {
                if (doc.ParentId == -1)
                {
                    break;
                }
                doc = new Document(doc.ParentId);
            }

            // now do breathfirst search
            Queue q = new Queue();
            q.Enqueue(doc);
            foreach (Document d in doc.Children)
            {
                if (d.ContentType.Alias == alias)
                {
                    // required node is at the top level so return it!
                    return d;
                }

                // Queue the doc because the node with alias == alias is deeper
                q.Enqueue(d);
            }

            // process the queue    
            while (q.Count > 0)
            {
                Document d = (Document)q.Dequeue();
                if (d.ContentType.Alias == alias)
                {
                    // found the node so return!
                    return d;
                }

                foreach (Document child in d.Children)
                {
                    if (child.ContentType.Alias == alias)
                    {
                        // found the node so return!
                        return child;
                    }
                    q.Enqueue(child);
                }
            }

            // no node found :(
            return null;
        }


        #endregion







    }
}
