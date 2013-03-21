<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Admin.ascx.cs" Inherits="uBlogsy.Web.usercontrols.uBlogsy.dashboard.Admin" %>
<style type="text/css">
    #uBlogsy li { list-style-type:none; border-bottom:1px solid grey; display:block; padding-top:20px; padding-bottom:20px; }
    #uBlogsy li span { display:block; }
    #uBlogsy .approve { margin-right:10px; }
    #uBlogsy .approve, #uBlogsy .delete { cursor:pointer;cursor:hand; text-decoration:underlined; }
</style>

<script type="text/javascript">
    $(document).ready(function () {
        $('#uBlogsy .approve').click(function () {
            $('input[id$=uBlogsy_HdnSelectedId]').val($(this).attr('rel'));
            $('input[id$=uBlogsy_btnApprove]').trigger('click');
            //$(this).parent().remove();
        });

        $('#uBlogsy .delete').click(function () {
            $('input[id$=uBlogsy_HdnSelectedId]').val($(this).attr('rel'));
            $('input[id$=uBlogsy_btnDelete]').trigger('click');
            // $(this).parent().remove();
        });



    });
</script>
<div id="uBlogsy">
<label  for="<%=ddlRoots.ClientID %>">Select site root</label>
<asp:DropDownList ID="ddlRoots" runat="server"></asp:DropDownList>
<br />
<br />
<asp:LinkButton  ID="btnSubmit" runat="server" onclick="btnSubmit_Click" Text="Click to load" ValidationGroup="uBlogsyCreatePost" />



    <% if (Comments.Count > 0)
       { %>
    <h1>
        Comments awaiting your approval</h1>
    <%}
       else
       { %>
    <h1>
        No comments to approve</h1>
    <%} %>
    <ul>
        <% foreach (var c in Comments)
           {%>
        <li><span>Post:
            <%=new umbraco.cms.businesslogic.web.Document(c.Parent.ParentId).getProperty("uBlogsyContentTitle").Value %></span>
            <span>Date:
                <%=c.CreateDateTime%></span> <span>From:
                    <%=c.getProperty("uBlogsyCommentName").Value %></span> <span>Email:
                        <%=c.getProperty("uBlogsyCommentEmail").Value%></span> <span>Message:
            </span><span>
                <%=c.getProperty("uBlogsyCommentMessage").Value%></span>
            <br />
            <a class="approve" rel="<%=c.Id %>">Approve</a> <a class="delete" rel="<%=c.Id %>">Delete</a>
        </li>
        <% } %>
    </ul>
    <asp:HiddenField ID="uBlogsy_HdnSelectedId" runat="server" />
    <asp:Button Style="display: none;" ID="uBlogsy_btnApprove" Text="Approve" runat="server"
        OnClick="btnApprove_Click"  ValidationGroup="uBlogsy_Comments" />
    <asp:Button Style="display: none;" ID="uBlogsy_btnDelete" Text="Approve" runat="server" OnClick="btnDelete_Click" ValidationGroup="uBlogsy_Comments" />
</div>

<asp:Label ID="lblError" runat="server"></asp:Label>

