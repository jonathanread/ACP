<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="CreatePost.ascx.cs" Inherits="uBlogsy.Web.usercontrols.uBlogsy.dashboard.CreatePost" %>

<script>
    $(document).ready(function () {
        var uBlogsySeconds = 6;
        var uBlogsyIntervalId;

        $('#uBlogsy_create_post a[id$=btnSubmit]').click(function () {
            $(this).hide();
            $('#uBlogsy_loading').show();
            uBlogsyIntervalId = window.setInterval(uBlogsyCountdown, 1000);
            uBlogsyCountdown();
        });

        function uBlogsyCountdown() {
            uBlogsySeconds--;
            $("#uBlogsyCount").text(uBlogsySeconds);
            if (uBlogsySeconds == 0) {
                window.clearInterval(uBlogsyIntervalId);
            }
        }
    });
</script>

<div id="uBlogsy_create_post">

<h1>Create a post</h1>

<label  for="<%=ddlRoots.ClientID %>">Select site root</label>
<asp:DropDownList ID="ddlRoots" runat="server"></asp:DropDownList>
<br />
<%--<br />
<asp:CheckBox ID="chkbxUseMonthFolder" runat="server" /> <label for="<%=chkbxUseMonthFolder.ClientID %>">Use month folder?</label>
--%>
<br />
<br />
<asp:LinkButton  ID="btnSubmit" runat="server" onclick="btnSubmit_Click" Text="Click to create" ValidationGroup="uBlogsyCreatePost" />

<div id="uBlogsy_loading" style="display:none;" >
    <span>Preparing new blog post... <span id="uBlogsyCount" style="padding-right:10px;"></span><img src="/umbraco_client/Tree/Themes/umbraco/throbber.gif" />
</div>

</div>
