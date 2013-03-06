using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.cms.businesslogic.datatype;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.propertytype;


namespace uBlogsy.Common.Extensions
{
    public static class MemberTypeExtensions
    {
        #region uBlogsyCreateTab
        /// <summary>
        /// Creates a tab in the member type
        /// </summary>
        /// <param name="memberType"></param>
        /// <param name="tabName"></param>
        /// <returns></returns>
        public static int uBlogsyCreateTab(this MemberType memberType, string tabName)
        {
            int id = memberType.AddVirtualTab(tabName);
            memberType.Save();

            return id;
        }
        
        #endregion



        #region uBlogsyCreateProperty
        /// <summary>
        /// Creates a property for the member type
        /// </summary>
        /// <param name="memberType"></param>
        /// <param name="tabId"></param>
        /// <param name="dataTypeName"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="desc"></param>
        /// <param name="mandatory"></param>
        /// <returns></returns>
        public static PropertyType uBlogsyCreateProperty(this MemberType memberType, int tabId, string dataTypeName, string alias, string name, string desc, bool mandatory)
        {
            DataTypeDefinition dataType = DataTypeDefinition.GetAll().Where(x => x.Text == dataTypeName).Single();
            memberType.AddPropertyType(dataType, alias, name);
            PropertyType prop = memberType.getPropertyType(alias);
            prop.Description = desc;
            prop.Mandatory = mandatory;
            prop.TabId = tabId;
            prop.Save();

            memberType.SetTabOnPropertyType(prop, tabId);
            memberType.Save();

            return prop;
        }
        
        #endregion
    }
}
