using System;
using uBlogsy.BusinessLogic;
using uBlogsy.BusinessLogic.Models;
using umbraco.MacroEngines;
using umbraco.presentation.templateControls;

namespace uBlogsy.Web.usercontrols.uBlogsy
{
    public partial class Contact : System.Web.UI.UserControl
    {
        private string m_NodeTypeAlias;
        private int m_PageId;

        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            btnSubmit.Text = umbraco.library.GetDictionaryItem("uBlogsyDicContactFormSubmit");

            Item item = new Item();
            m_PageId = int.Parse(item.PageElements["pageID"].ToString());

            // check what type of page this is
            m_NodeTypeAlias = item.PageElements["nodeTypeAlias"].ToString();

            if (m_NodeTypeAlias != "uBlogsyPost")
            {
                // hide the subscribe checkbox
                DivSubscribe.Visible = false;
            }

            string action = Request.QueryString["action"];
            string success = Request.QueryString["success"];

            // handle cases when this page redirected to itself after an operation
            HandleRedirects(action, success);

            if (m_NodeTypeAlias == "uBlogsyPost")
            {
                if (!string.IsNullOrEmpty(action) && action == "unsubscribe")
                {
                    if (string.IsNullOrEmpty(success))
                    {
                        DoUnsubscribe(); // does a redirect to this page after unsubscribing
                    }
                }
                else
                {
                    // this is a post page so check if we should disable comments 
                    var disableAllComments = DataService.Instance.GetValueFromLanding(m_PageId, "uBlogsySpamDisableComments");
                    var disableComments = new DynamicNode(m_PageId).GetProperty("uBlogsyPostDisableComments");
                    if ((disableComments != null && disableComments.Value == "1") || disableAllComments == "1")
                    {
                        mvForm.ActiveViewIndex = 2;
                    }
                }
            }
        }

        #endregion




        #region HandleRedirects

        private void HandleRedirects(string action, string success)
        {
            // was redirected to current page to ensure razor macro is refreshed
            if (!string.IsNullOrEmpty(action) && action == "comment")
            {
                if (!string.IsNullOrEmpty(success) && success == "true")
                {
                    mvForm.ActiveViewIndex = 1;
                }
                else if (!string.IsNullOrEmpty(success) && success == "error")
                {
                    mvForm.ActiveViewIndex = 3;
                }
                else if (!string.IsNullOrEmpty(success) && success == "spam")
                {
                    mvForm.ActiveViewIndex = 4;
                }
            }
            else if (!string.IsNullOrEmpty(action) && action == "unsubscribe")
            {
                if (!string.IsNullOrEmpty(success) && success == "true")
                {
                    mvUnsubscribe.ActiveViewIndex = 0;
                }
                else if (!string.IsNullOrEmpty(success) && success == "false")
                {
                    mvUnsubscribe.ActiveViewIndex = 1;
                }
                else if (!string.IsNullOrEmpty(success) && success == "error")
                {
                    mvUnsubscribe.ActiveViewIndex = 2;
                }
            }
        }

        #endregion



        #region DoUnsubscribe

        private void DoUnsubscribe()
        {
            // first test the query strings!
            string mid = Request.QueryString["mid"];
            string guid = Request.QueryString["guid"];
            int tempInt;
            if (string.IsNullOrEmpty(mid) || string.IsNullOrEmpty(guid) || !int.TryParse(mid, out tempInt))
            {
                Response.Redirect(Request.Url.AbsolutePath.ToString() + "?action=unsubscribe&success=false#unsubscribed");
            }

            // do the unsubscribe"
            Result res = SubscriptionService.Instance.UnsubscribeFromPost(Request, m_PageId, int.Parse(mid), guid);

            if (res == Result.Success || res == Result.SubscriptionNotFound)
            {
                // redirect to current page
                Response.Redirect(Request.Url.AbsolutePath.ToString() + "?action=unsubscribe&success=true#unsubscribed");
            }
            else if (res == Result.GuidNotFound || res == Result.MemberNotFound)
            {
                // redirect to current page
                Response.Redirect(Request.Url.AbsolutePath.ToString() + "?action=unsubscribe&success=false#unsubscribed");
            }
            else if (res == Result.Error)
            {
                Response.Redirect(Request.Url.AbsolutePath.ToString() + "?action=unsubscribe&success=error#unsubscribed");
            }
        }

        #endregion




        #region btnSubmit_Click
        protected void btnSubmit_Click(object sender, EventArgs e)
        {
            //TODO: add a check for author email... if is email but not authenticated, then show a password box under the email address.

            // create comment info for cleaner data passing
            var commentInfo = new CommentInfo(txtName.Text, txtEmail.Text, txtWebsite.Text, txtMessage.Text, Request.IsAuthenticated, GetUrl(), DateTime.UtcNow);

            if (m_NodeTypeAlias == "uBlogsyPost")
            {
                var res = CommentService.Instance.SubmitComment(Request, m_PageId, commentInfo, cbxSubscribe.Checked);

                if (res == Result.Spam)
                {
                    Response.Redirect(Request.Url.AbsolutePath + "?action=comment&success=spam#spam");
                }
                else if (res == Result.Error)
                {
                    Response.Redirect(Request.Url.AbsolutePath + "?action=comment&success=error#error");
                }
            }
            else
            {
                EmailService.Instance.SendContactEmail(Request, m_PageId, commentInfo);
            }

            // redirect to current page to ensure razor macro is refreshed
            Response.Redirect(Request.Url.AbsolutePath + "?action=comment&success=true#success");
        }



        #endregion


        string GetUrl()
        {
            var url = new Uri(Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Path);
            
            return url;
        }
    }
}