<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="RSSImport.ascx.cs" Inherits="uBlogsy.Web.usercontrols.uBlogsy.dashboard.RSSImport" %>
<style type="text/css">
    .uBlogsy_row
    {
        overflow: hidden;
        height: 35px;
        width: 100%;
        
    }
    
    .uBlogsy_row input[type=text]
    {
        width: 300px;
        float: left;
    }
    
    .uBlogsy_row label
    {
        display: block;
        overflow: hidden;
        width: 145px;
        
        float: left;
        text-align: right;
        padding-right: 10px;
        padding-top:6px;
    }
</style>
<h1>
    Blog Import tool</h1>
<asp:MultiView ID="mv" runat="server" ActiveViewIndex="0">
    <asp:View ID="vwImport" runat="server">
        <p>
            Enter an RSS feed to import the posts into your blog</p>
            <p>eg. http://feeds.feedburner.com/midcodecrisis</p>
        <div class="uBlogsy_row">
            <label>
                RSS Url</label>
            <asp:TextBox ID="txtRssUrl" runat="server" ValidationGroup="uBlogsyRssImport" ></asp:TextBox><asp:RequiredFieldValidator
                ID="RequiredFieldValidator1" ControlToValidate="txtRssUrl" runat="server">*</asp:RequiredFieldValidator>
        </div>
        
        
        <div class="uBlogsy_row">
            <label>
                Default Author</label>
            <asp:TextBox ID="txtAuthor" runat="server" ValidationGroup="uBlogsyRssImport" ></asp:TextBox>
        </div>


        <div class="uBlogsy_row">
            <label>
                Use only year and month folders? (First disable date folders in /config/uBlogsy.config) </label>
            <asp:CheckBox ID="cbxUseMonth" runat="server" ValidationGroup="uBlogsyRssImport" />
        </div>
        <div class="uBlogsy_row">
            <label>
                &nbsp;
            </label>
            <asp:Button ID="btnRssImport" runat="server" ValidationGroup="uBlogsyRssImport" Text="Rss Import"
                OnClick="btnRssImport_Click" />
        </div>


        <div class="uBlogsy_row">
            This will take a few seconds per post. May take several minutes depending on the
            number of posts and the speed of your connection.
        </div>
        Note: This tool is no longer being supported. I recommend you now use the much more powerful <a href="http://www.cmsimport.com/">CMS Import</a> 
        
    </asp:View>

    <asp:View ID="vwSuccess" runat="server">
        Blog import successfull!
    </asp:View>

    <asp:View ID="vwFail" runat="server">
        Blog import failed :( <br />
        Please report issues <a href="http://our.umbraco.org/projects/starter-kits/ublogsy/ublogsy-razor-blog">
            here</a> with the stacktrace below.<br /><br />
        <asp:Label ID="lblError" runat="server" style="line-height: 15px;"/>
    </asp:View>
</asp:MultiView>