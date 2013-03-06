using System;
using System.Linq;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using uBlogsy.BusinessLogic;

namespace uBlogsy.Web.usercontrols.uBlogsy.dashboard
{
   


    public partial class CreatePost : System.Web.UI.UserControl
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (!IsPostBack)
            {
                InitBlogRoots();
            }
        }




        private void InitBlogRoots()
        {
            // get roots
            var roots = Document.GetRootDocuments();

            foreach (var root in roots)
            {
                if (root.ContentType.Alias == "uBlogsyLanding")
                {
                    ddlRoots.Items.Add(new ListItem(root.Text, root.Id.ToString()));
                }
                else
                {
                    // get landings that may be in this root
                    var landings = root.GetDescendants().Cast<Document>().Where(x => x.ContentType.Alias == "uBlogsyLanding");

                    foreach (var landing in landings)
                    {
                        // add landing to ddl
                        ddlRoots.Items.Add(new ListItem(landing.Text, landing.Id.ToString()));
                    }
                }
            }

            ddlRoots.DataBind();
        }



        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            var landing = new Document(int.Parse(ddlRoots.SelectedValue));

            var postsNode = landing.Children.SingleOrDefault(x => x.ContentType.Alias == "uBlogsyContainerBlog");

            var rootId = postsNode != null ? postsNode.Id : landing.Id;

            // when there are multiple roots we need to pass in the root!
            Document post = PostService.Instance.CreatePost(rootId);
            Response.Redirect("~/umbraco/editContent.aspx?id=" + post.Id + "&isNew=true");
        }

    }
}