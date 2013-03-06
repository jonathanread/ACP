<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Test.aspx.cs" Inherits="uBlogsy.Web.Test" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
    <style type="text/css">
        input[type=submit] { display: block; font-size: 11px; height: 20px; }
        * { font-family: Arial; }
        span { font-size: 11px; }
        
        #passes { color: darkgreen; }
        #fails { color: Red; }
        
        #buttons { float: left; width: 400px; }
        #results { float: left; width: 400px; }
        
        h1 { font-size: 16px; }
        h2 { font-size: 14px; }
        h3 { font-size: 12px; }
    </style>
</head>
<body>
    <form id="form1" runat="server">
    <div id="buttons">
        <%-- <div>
        <asp:Button runat="server" Text="Test MemberType creation" OnClick="TestCreateMemberType_Click" />
    </div>
    <div>
        <asp:Button ID="Button1" runat="server" Text="Test Member creation" OnClick="TestCreateMember_Click" />
    </div>--%>
        <%--Date Folder Service Tests--%>
        <%--Date Folder Service Tests--%>
        <%--Date Folder Service Tests--%>
        <h1>
            Tests</h1>
        <asp:Button ID="Button5" runat="server" Text="Test All" OnClick="TestAll_Click" />
        <h2>
            Date Folder Service Tests</h2>
        <div>
            <asp:Button ID="Button6" runat="server" Text="All" OnClick="TestAllDateFolderService_Click" />
            <asp:Button ID="Button2" runat="server" Text="Use Auto Sorting" OnClick="TestUseAutoDateFolders_Click" />
            <asp:Button ID="Button3" runat="server" Text="Ensure Correct Parent For Post" OnClick="TestEnsureCorrectParentForPost_Click" />
            <asp:Button ID="Button8" runat="server" Text="Get Correct Parent For Post" OnClick="TestGetCorrectParentForPost_Click" />
            <asp:Button ID="Button4" runat="server" Text="Ensure Correct Date" OnClick="TestEnsureCorrectDate_Click" />
            <asp:Button ID="Button9" runat="server" Text="Get Year Folder" OnClick="TestGetYearFolder_Click" />
            <asp:Button ID="Button10" runat="server" Text="Get Month Folder" OnClick="TestGetMonthFolder_Click" />
            <asp:Button ID="Button11" runat="server" Text="Get Day Folder" OnClick="TestGetDayFolder_Click" />
        </div>
        <hr />
        <%--Post Service Tests--%>
        <%--Post Service Tests--%>
        <%--Post Service Tests--%>
        <h2>
            Post Service Tests</h2>
        <div>
            <asp:Button ID="Button7" runat="server" Text="All" OnClick="TestAllPostService_Click" />
            <asp:Button ID="Button1" runat="server" Text="Create Post" OnClick="CreatePostTest_Click" />
            <asp:Button ID="Button27" runat="server" Text="Do Search Test" OnClick="DoSearchTest_Click" />
        </div>
        <hr />
        <%--Document Service Tests--%>
        <%--Document Service Tests--%>
        <%--Document Service Tests--%>
        <h2>
            Document Service Tests</h2>
        <div>
            <asp:Button ID="Button14" runat="server" Text="All" OnClick="TestAllDocumentService_Click" />
            <asp:Button ID="Button16" runat="server" Text="Ensure Correct Post Node Name" OnClick="EnsureCorrectPostNodeNameTest_Click" />
            <asp:Button ID="Button17" runat="server" Text="Ensure Node Exists" OnClick="EnsureNodeExistsTest_Click" />
            <asp:Button ID="Button18" runat="server" Text="Get Default Dictionary" OnClick="GetDefaultDictionaryTest_Click" />
            <asp:Button ID="Button19" runat="server" Text="Get Document By Alias" OnClick="GetDocumentByAliasTest_Click" />
        </div>
        <hr />
        <%--Document Extension Tests--%>
        <%--Document Extension Tests--%>
        <%--Document Extension Tests--%>
        <h2>
            Document Extension Tests</h2>
        <div>
            <asp:Button ID="Button25" runat="server" Text="All" OnClick="TestAllDocumentExtensions_Click" />
            <asp:Button ID="Button26" runat="server" Text="uBlogsy Get Value From Ancestor" OnClick="TestuBlogsyGetValueFromAncestorTest_Click" />
        </div>
        <hr />
    </div>
    <div id="results">
        <h2>
            Results</h2>
        <h3>
            Passes</h3>
        <div id="passes">
            <asp:Label ID="lblPass" runat="server"></asp:Label>
        </div>
        <h3>
            Fails</h3>
        <div id="fails">
            <asp:Label ID="lblError" runat="server"></asp:Label>
        </div>
    </div>
    </form>
</body>
</html>
