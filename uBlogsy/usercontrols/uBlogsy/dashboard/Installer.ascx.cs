using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml.Linq;
using uBlogsy.Common.Extensions;
using uHelpsy.Core;
using System.IO;
using System.Text;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.member;
using umbraco.cms.businesslogic.datatype;
using uBlogsy.Common;


namespace uBlogsy.Web.usercontrols.uBlogsy.dashboard
{
    public partial class Installer : System.Web.UI.UserControl
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // WriteToDashboard("Blog Comments", "~/usercontrols/uBlogsy/dashboard/Admin.ascx");
            //WriteToDashboard("Blog RSS Import Tool", "~/usercontrols/uBlogsy/dashboard/RSSImport.ascx");

            CreateMemberTypeAll("uBlogsySubscriber", "uBlogsy Subscriber");
        }



        #region WriteToDashboard

        private void WriteToDashboard(string tabName, string controlPath)
        {
            XElement root = XDocument.Load(Server.MapPath("~/config/Dashboard.config")).Root;

            // first determine if tab is already added
            if (root.Descendants("control").Where(x => x.Value.Contains(controlPath)).Count() > 0)
            {
                return;
            }


            // get sections
            IEnumerable<XElement> sections = root.Descendants("section");

            XElement section = null;
            bool found = false;

            // find content section
            foreach (var s in sections)
            {
                var area = s.Descendants("area").FirstOrDefault();
                if (area.Value.ToLower() == "content")
                {
                    section = s;
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                // dashboard does not contain a content section so create it!
                // <section alias="StartupSettingsDashboardSection">
                //    <areas>
                //      <area>content</area>
                //    </areas>
                //<section>
                section = new XElement("section");
                section.SetAttributeValue("alias", "uBlogsySection");
                XElement areas = new XElement("areas");
                XElement area = new XElement("area");
                area.Value = "content";
                areas.Add(area);
                section.Add(areas);
            }

            // create tab for section
            //<tab caption="uBlogsy">
            //  <control addPanel="true" panelCaption="">
            //    /usercontrols/uBlogsy/dashboard/Admin.ascx
            //  </control>
            //</tab>
            XElement tab = new XElement("tab");
            tab.SetAttributeValue("caption", tabName);
            XElement control = new XElement("control");
            control.SetAttributeValue("addPanel", "true");
            control.Value = controlPath;
            tab.Add(control);
            section.Add(tab);

            if (found == false)
            {
                root.Add(section);
            }

            var sb = new StringBuilder();
            root.Save(new StringWriter(sb));

            File.WriteAllText(Server.MapPath("~/config/Dashboard.config"), sb.ToString(), Encoding.Unicode);
        }

        #endregion



        public MemberType CreateMemberType(string alias, string typeName)
        {
            return CreateMemberTypeAll(alias, typeName);
        }



        private MemberType CreateMemberTypeAll(string alias, string typeName)
        {
            MemberType memberType = MemberType.GetByAlias(alias);
            if (memberType != null)
            {
                return memberType;
            }

            // create MemberType
            memberType = MemberType.MakeNew(new User(0), alias);

            memberType.Text = typeName;


            
            // create subscriptions tab
            int subscriptionsTabId = memberType.uBlogsyCreateTab("Subscriptions");

            // add property to subscriptions tab
            memberType.uBlogsyCreateProperty(subscriptionsTabId, "Textbox multiple", "uBlogsyMemberSubscriptions", "Subscriptions", "A newLine delimited list in the form  postId|unsubscribeGuid|dateCreated", false);

            // create the info tab
            int infoTabId = memberType.uBlogsyCreateTab("Subscriber Info");

            // Add the properties to info tab
            memberType.uBlogsyCreateProperty(infoTabId, "Textstring", "uBlogsyMemberName", "Name", "The name of the subscriber", false);

            memberType.uBlogsyCreateProperty(infoTabId, "Textstring", "uBlogsyMemberWebsite", "Website", string.Empty, false);

            memberType.uBlogsyCreateProperty(infoTabId, "Textstring", "uBlogsyMemberCommentUrlName", "Name for Comment Url", "Used to distinguish members with the same name", true);
            
            
            memberType.Save();

            return memberType;
        }
    }
}