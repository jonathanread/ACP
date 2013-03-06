using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using uBlogsy.Common.Helpers;
using umbraco.interfaces;
using System.Globalization;
using umbraco.MacroEngines;
using Examine;
using System.Collections;
using System.Text.RegularExpressions;
using Examine.SearchCriteria;
using Examine.LuceneEngine;
using Examine.LuceneEngine.SearchCriteria;
using umbraco.cms.businesslogic.web;
using umbraco;
using uBlogsy.BusinessLogic.Helpers;
using uBlogsy.BusinessLogic.Extensions;
using uHelpsy.Core;

namespace uBlogsy.BusinessLogic
{
    internal interface IPostService
    {
        Document CreatePost(int documentId);
        IEnumerable<DynamicNode> GetRelatedPosts(int postId, string itemAlias, int matchCount);
        IEnumerable<DynamicNode> GetPosts(int postId, string tag, string category, string author, string searchTerm, string commenter, int pageNo, int itemsPerPage);

        DynamicNode GetNextPost(DynamicNode current);
        DynamicNode GetPreviousPost(DynamicNode current);
        IEnumerable<string> GetAuthors(int postId);

        IEnumerable<DynamicNode> GetPosts(int nodeId);
    }


    /// <summary>
    /// This class is contains methods which generally take in a DynamicNode to do an operation/search on.
    /// </summary>
    public class PostService : IPostService
    {
        #region Singleton

        protected static volatile PostService m_Instance = new PostService();
        protected static object syncRoot = new Object();

        protected PostService() { }

        public static PostService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (syncRoot)
                    {
                        if (m_Instance == null)
                            m_Instance = new PostService();
                    }
                }

                return m_Instance;
            }
        }

        #endregion



        #region CreatePost
        /// <summary>
        /// Creates a post and returns it.
        /// </summary>
        /// <param name="documentId"></param>
        /// <param name="useMonthFolder"></param>
        /// <returns></returns>
        public Document CreatePost(int documentId)
        {
            // create the node
            Document post = UmbracoAPIHelper.CreateContentNode("New Post", "uBlogsyPost", new Dictionary<string, object>(), documentId, false);

            return post;
        }

        #endregion




        #region GetRelatedPosts
        /// <summary>
        /// Gets posts which have a related tag or category
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="sorted"></param>
        /// <returns></returns>
        public IEnumerable<DynamicNode> GetRelatedPosts(int postId, string itemAlias, int matchCount)
        {
            // get all posts
            IEnumerable<DynamicNode> posts = GetPosts(postId);

            List<DynamicNode> nodes;
            if (!string.IsNullOrEmpty(itemAlias))
            {
                nodes = GetRelatedPosts(postId, itemAlias, posts, matchCount).ToList();
            }
            else
            {
                // get both tags and categories
                IEnumerable<DynamicNode> relatedByTags = GetRelatedPosts(postId, "uBlogsyPostTags", posts, matchCount);
                IEnumerable<DynamicNode> relatedByCategories = GetRelatedPosts(postId, "uBlogsyPostCategories", posts, matchCount);

                nodes = new List<DynamicNode>();
                nodes.AddRange(relatedByTags);
                nodes.AddRange(relatedByCategories);
            }

            // get distinct, and order by date
            return nodes.Distinct(new DynamicNodeEqualityComparer());

        }

        protected IEnumerable<DynamicNode> GetRelatedPosts(int postId, string itemAlias, IEnumerable<DynamicNode> posts, int matchCount)
        {
            DynamicNode current = new DynamicNode(postId);

            // get this page's items to compare
            List<string> currentItems = current.GetProperty(itemAlias).Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

            List<DynamicNode> nodes = new List<DynamicNode>();

            foreach (DynamicNode n in posts)
            {
                if (n.Id != current.Id)
                {
                    // get items as string array
                    List<string> items = n.GetProperty(itemAlias).Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).ToList();

                    // check if currentItems and items have at least matchCount in common
                    bool intersects = CollectionHelper.HashSetIntersects(currentItems, items, true, matchCount);

                    if (intersects)
                    {
                        nodes.Add(n);
                    }
                }
            }
            return nodes;
        }

        #endregion




        #region GetPosts

        /// <summary>
        /// Gets posts by tag, category, author, or all posts.
        /// </summary>
        /// <param name="nodeId"> </param>
        /// <param name="tag"></param>
        /// <param name="category"></param>
        /// <param name="author"></param>
        /// <param name="searchTerm"> </param>
        /// <param name="commenter"> </param>
        /// <param name="pageNo"> </param>
        /// <param name="itemsPerPage"> </param>
        /// <returns></returns>
        public IEnumerable<DynamicNode> GetPosts(int nodeId, string tag, string category, string author, string searchTerm, string commenter, int pageNo, int itemsPerPage)
        {
            // get entire list of posts
            IEnumerable<DynamicNode> postList = GetPosts(nodeId).Where(x => x.GetProperty("umbracoNaviHide").Value != "1");


            // TODO: the following is NOT how we write consise elegant code!!!!!!... another method??!!! 

            // filter by tag
            if (!string.IsNullOrEmpty(tag))
            {
                var tags = tag.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                foreach (var t in tags)
                {
                    postList = GetPosts(t, "uBlogsyPostTags", postList);
                }
            }

            // now filter by category
            if (!string.IsNullOrEmpty(category))
            {
                var categories = category.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                foreach (var c in categories)
                {
                    postList = GetPosts(c, "uBlogsyPostCategories", postList);
                }
            }

            // now filter by author
            if (!string.IsNullOrEmpty(author))
            {
                var authors = author.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
                foreach (var a in authors)
                {
                    postList = GetPosts(a, "uBlogsyPostAuthor", postList);
                }
            }

            // now filter by search term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // do search on everything!
                postList = DoSearch(nodeId, searchTerm, postList);
            }

            // now filter by commenter
            if (!string.IsNullOrEmpty(commenter))
            {
                postList = GetPostsByCommenter(commenter, postList).ToList();
            }

            var sorted = postList.Where(x => x.NodeTypeAlias == "uBlogsyPost").Distinct(new DynamicNodeEqualityComparer()).OrderByDescending(x => x.GetPropertyValue("uBlogsyPostDate"));

            var paged = GetContentForPage(sorted, pageNo, itemsPerPage);
            // sort and return
            return paged;
        }


        #endregion





        #region GetPostsByCommenter

        /// <summary>
        /// Gets posts where commenter == commenter.
        /// </summary>
        /// <param name="commenter"></param>
        /// <param name="postList"></param>
        /// <returns></returns>
        protected IEnumerable<DynamicNode> GetPostsByCommenter(string commenter, IEnumerable<DynamicNode> postList)
        {
            List<DynamicNode> posts = new List<DynamicNode>();
            foreach (var post in postList)
            {
                var foundCommenter = post.Descendants("uBlogsyComment")
                                         .Items.Where(x => x.Name.ToLower() == commenter.ToLower())
                                         .Count() > 0;

                if (foundCommenter)
                {
                    posts.Add(post);
                }
            }
            return posts;
        }

        #endregion





        #region GetPosts

        /// <summary>
        /// Returns posts which have a property with the given alias equal to the given item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="alias"></param>
        /// <param name="sorted"></param>
        /// <returns></returns>
        protected List<DynamicNode> GetPosts(string item, string alias, IEnumerable<DynamicNode> sorted)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            foreach (DynamicNode n in sorted)
            {
                // get items 
                List<string> itemList = new List<string>();
                var v = n.GetProperty(alias).Value
                            .ToLower()
                            .Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim());

                itemList.AddRange(v);

                if (itemList.Contains(item.ToLower()))
                {
                    nodes.Add(n);
                }
            }
            return nodes;
        }

        #endregion




        #region GetPosts

        /// <summary>
        /// Returns all the posts.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        public IEnumerable<DynamicNode> GetPosts(int nodeId)
        {
            string cacheKey = "GetPosts_uBlogsyPosts";

            var cached = CacheHelper.GetFromRequestCache(cacheKey) as IEnumerable<DynamicNode>;
            if (cached != null)
            {
                return cached;
            }

            var nodes = GetDescendentsOrSelf(nodeId, "uBlogsyPost", new string[] { "uBlogsyContainerPage", "uBlogsyPage", "uBlogsyContainerComment", "uBlogsyComment" }).OrderByDescending(x => x.GetProperty("uBlogsyPostDate").Value);

            // cache the result
            CacheHelper.AddToRequestCache(cacheKey, nodes);

            return nodes;
        }


        #endregion





        #region GetDescendentsOrSelf

        /// <summary>
        /// Uses a queue to traverse the tree and ignore nodes in stopAliases. This is a performance improvement!
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private IEnumerable<DynamicNode> GetDescendentsOrSelf(int nodeId, string targetAlias, IEnumerable<string> stopAliases)
        {
            var landing = DataService.Instance.GetLanding(nodeId);

            List<DynamicNode> nodes = new List<DynamicNode>();
            List<DynamicNode> q = new List<DynamicNode>();

            q.Add(landing);

            while (q.Count() > 0)
            {
                var child = q.FirstOrDefault();
                if (child != null)
                {
                    q.Remove(child);
                }
                else
                {
                    break;
                }

                if (child.NodeTypeAlias == targetAlias)
                {
                    nodes.Insert(0, child);
                }
                else
                {
                    if (stopAliases.Contains(child.NodeTypeAlias)) { continue; }

                    var grandChildren = child.GetChildrenAsList.Items;
                    grandChildren.Reverse();
                    foreach (var grandChild in grandChildren)
                    {
                        q.Insert(0, grandChild);
                    }
                }
            }

            return nodes;
        }


        #endregion




        #region GetNextPost
        /// <summary>
        /// Gets the post immediately following the current one.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public DynamicNode GetNextPost(DynamicNode current)
        {
            // get siblings
            IEnumerable<DynamicNode> siblings = GetPosts(current.Id);

            // get index of current
            var next = GetNext(siblings, current);

            return next;
        }


        #endregion




        #region GetPreviousPost

        /// <summary>
        /// Gets the post immediately preceding the current one.
        /// </summary>
        /// <param name="current"></param>
        /// <returns></returns>
        public DynamicNode GetPreviousPost(DynamicNode current)
        {
            // get siblings
            IEnumerable<DynamicNode> siblings = GetPosts(current.Id);

            // get index of current
            var prev = GetNext(siblings.Reverse(), current);

            // return previous
            return prev;
        }



        #endregion




        #region GetNext


        /// <summary>
        /// 
        /// </summary>
        /// <param name="siblings"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private static DynamicNode GetNext(IEnumerable<DynamicNode> siblings, DynamicNode current)
        {
            bool found = false;
            foreach (var s in siblings)
            {
                if (found)
                {
                    return s;
                }
                if (s.Id == current.Id)
                {
                    found = true;
                }

            }
            return null; // some crazy error!
        }

        #endregion





        #region GetIndexOf

        /// <summary>
        /// Gets the index of the current post in the list of siblings.
        /// </summary>
        /// <param name="siblings"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        protected int GetIndexOf(List<DynamicNode> siblings, DynamicNode current)
        {
            for (int i = 0; i < siblings.Count; i++)
            {
                if (siblings[i].Id == current.Id)
                {
                    return i;
                }
            }
            return -1; // some crazy error!
        }

        #endregion





        #region DoSearch
        /// <summary>
        /// Performs search on the given search term.
        /// </summary>
        /// <param name="searchTerm"></param>
        /// <returns></returns>
        protected IEnumerable<DynamicNode> DoSearch(int nodeId, string searchTerm, IEnumerable<DynamicNode> sorted)
        {
            List<DynamicNode> nodes = new List<DynamicNode>();
            // remove multiple spaces
            string cleanedSearchTerm = Regex.Replace(searchTerm, "\\s+", " ");

            // search using examine
            IEnumerable<SearchResult> results = ExamineManager.Instance.Search(cleanedSearchTerm, true); // DoSearch(searchTerm); //

            // add results to nodes list for returning
            foreach (var r in results)
            {
                nodes.Add(new DynamicNode(r.Id));
            }

            // split string when multiple words are typed
            IEnumerable<string> searchTerms = cleanedSearchTerm.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToList();

            // do examine search for each
            foreach (var term in searchTerms)
            {
                results = ExamineManager.Instance.Search(term, true);
                foreach (var r in results)
                {
                    if (nodes.Where(x => x.Id == r.Id).Count() == 0)
                    {
                        nodes.Add(new DynamicNode(r.Id));
                    }
                }

                // TODO: make custom indexes and use Examine!
                nodes.AddRange(GetPosts(term, "uBlogsyPostTags", sorted));
                nodes.AddRange(GetPosts(term, "uBlogsyPostCategories", sorted));
                nodes.AddRange(GetPosts(term, "uBlogsyPostAuthor", sorted));
            }

            // get distinct and filter by path!
            var landing = DataService.Instance.GetLanding(nodeId);

            return nodes
                    .Distinct(new DynamicNodeEqualityComparer())
                    .Where(x => x.Path.StartsWith(landing.Path));


            //ISearchCriteria sc = ExamineManager.Instance.CreateSearchCriteria(BooleanOperation.Or);

            //IBooleanOperation query = sc.NodeTypeAlias("uBlogsyPost")

            //                                .Field("uBlogsyContentTitle", LuceneSearchExtensions.Escape(searchTerm))
            //                              .Or().Field("uBlogsyContentBody", LuceneSearchExtensions.Escape(searchTerm))
            //                              .Or().Field("uBlogsyPostAuthor", LuceneSearchExtensions.Escape(searchTerm))
            //                              .Or().Field("uBlogsyPostCategories", LuceneSearchExtensions.Escape(searchTerm))
            //                              .Or().Field("uBlogsyPostTags", LuceneSearchExtensions.Escape(searchTerm))
            //                              .And();

            //IEnumerable<SearchResult> results = ExamineManager.Instance.Search(query.Compile()); // prepares the query to be handled by the searcher
            //return results;
        }

        #endregion





        #region GetAuthors
        /// <summary>
        /// Returns an IEnumberable of all authors
        /// </summary>
        /// <param name="postId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetAuthors(int postId)
        {
            // get all categories
            List<string> allAuthors = new List<string>();

            var posts = GetPosts(postId);
            foreach (var n in posts)
            {
                allAuthors.AddRange(n.GetProperty("uBlogsyPostAuthor").Value.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)); // take care of multiple author scenario
            }

            return allAuthors.Select(x => x.Trim()).Distinct();
        }

        #endregion




        #region GetContentForPage

        /// <summary>
        /// Used for paging.
        /// Gets a subset of the given nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <param name="pageNo"></param>
        /// <param name="itemsPerPage"></param>
        /// <returns></returns>
        protected IEnumerable<DynamicNode> GetContentForPage(IEnumerable<DynamicNode> nodes, int pageNo, int itemsPerPage)
        {
            if (pageNo == -2) // for testing!
            {
                return nodes;
            }

            int page = pageNo < 0 ? 0 : pageNo; // ensure pageNo is 0 or more

            // determine start and end indicies
            int startIndex = page * itemsPerPage;
            int itemsToGet = itemsPerPage < (nodes.Count() - startIndex) ? itemsPerPage : (nodes.Count() - startIndex);

            // start index is too high
            if (startIndex >= nodes.Count())
            {
                return new List<DynamicNode>();
            }

            // normal case
            return nodes.ToList().GetRange(startIndex, itemsToGet);
        }

        #endregion


    }
}