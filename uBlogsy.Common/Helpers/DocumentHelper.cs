using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.web;

namespace uBlogsy.Common.Helpers
{
    public class DocumentHelper
    {
        public static void SortNodes(int parentId, IEnumerable<Document> documents, IComparer<Document> comparer)
        {
            var list = documents.ToList();
            list.Sort(comparer);

            var nodeSorter = new umbraco.presentation.webservices.nodeSorter();
            var sortOrder = string.Empty;
            foreach (var doc in list)
            {
                sortOrder += doc.Id + ",";
            }

            nodeSorter.UpdateSortOrder(parentId, sortOrder.TrimEnd(','));
        }

    }
}
