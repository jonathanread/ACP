using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.MacroEngines;

namespace uBlogsy.BusinessLogic.Helpers
{


    public class DynamicNodeEqualityComparer : IEqualityComparer<DynamicNode>
    {

        public bool Equals(DynamicNode x, DynamicNode y)
        {
            return x.Id == y.Id;
        }

        public int GetHashCode(DynamicNode obj)
        {
            return obj.ToString().ToLower().GetHashCode();
        }
    }
}
