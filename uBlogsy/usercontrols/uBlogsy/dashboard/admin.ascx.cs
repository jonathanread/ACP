using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using uBlogsy.BusinessLogic;
using umbraco;

namespace uBlogsy.Web.usercontrols.uBlogsy.dashboard
{
    public partial class Admin : System.Web.UI.UserControl
    {
        public List<Document> Comments;



        #region OnLoad

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                Comments = new List<Document>();

                if (!IsPostBack)
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

             

                //LoadComments();

            }
            catch (Exception ex)
            {
                // display errors
                lblError.Text = ex.Message + "<br/><br/>StackTrace:<br/>" + ex.StackTrace;
            }

        }

        
        #endregion


        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            LoadComments();
        }



        #region LoadComments

        /// <summary>
        /// 
        /// </summary>
        protected void LoadComments()
        {
            Comments = new List<Document>();
            
            var root = new Document(int.Parse(ddlRoots.SelectedValue));

            // get landing
            Document landing = DocumentService.Instance.GetDocumentByAlias(root, "uBlogsyLanding", "uBlogsyLanding");

            if (landing == null)
            {
                return;
            }

            // get unpublished comments
            Comments = landing.GetDescendants()
                                .Cast<Document>()
                                .Where(x => x.ContentType.Alias == "uBlogsyComment")
                                .Where(x => !x.Published)
                                .ToList();
        }


        #endregion



       
        





        #region btnApprove_Click

        /// <summary>
        /// Click event for approve.
        /// Publishes comment node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnApprove_Click(object sender, EventArgs e)
        {
            int commentId = int.Parse(uBlogsy_HdnSelectedId.Value);

            // publish node with value v
            Document doc = new Document(commentId);
            doc.Publish(new umbraco.BusinessLogic.User(0));

            // update cache
           // umbraco.library.UpdateDocumentCache(commentId);

            //string postUrl = "http://" + HttpContext.Current.Request.Url.Host;

            //string postUrl = string.Format("http://{0}/", HttpContext.Current.Request.ServerVariables["HTTP_HOST"]) + library.NiceUrl(doc.Parent.ParentId);

            //// send notifications
            //EmailService.Instance.SendNotificationEmails(postUrl, doc.Parent.ParentId, commentId);

            // reload comments
            LoadComments();
        }
        
        #endregion




        #region btnDelete_Click

        /// <summary>
        /// Click event for delete.
        /// Deletes the comment node
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void btnDelete_Click(object sender, EventArgs e)
        {
            int id = int.Parse(uBlogsy_HdnSelectedId.Value);

            // publish node with value v
            Document doc = new Document(id);
            doc.delete();
            LoadComments();
        }

        
        #endregion




 

    }
}