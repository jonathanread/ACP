using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using uBlogsy.Web.usercontrols.uBlogsy.dashboard;
using uBlogsy.Tests;

namespace uBlogsy.Web
{
    public partial class Test : System.Web.UI.Page
    {
        #region Create Member tests

        //protected void TestCreateMemberType_Click(object sender, EventArgs e)
        //{
        //    Installer inst = new Installer();
        //    inst.CreateMemberType("uBlogsySubscriberTest", "uBlogsy Subscriber Test");
        //}

        //protected void TestCreateMember_Click(object sender, EventArgs e)
        //{
        //    DateFolderTests ut = new DateFolderTests();
        //    ut.TestCreateMember();
        //}

        #endregion



        protected void TestAll_Click(object sender, EventArgs e)
        {
            TestAllDateFolderService_Click(sender, e);
            TestAllPostService_Click(sender, e);
            TestAllDocumentService_Click(sender, e);
            TestAllDocumentExtensions_Click(sender, e);
        }









        #region DateFolderService Tests

        protected void TestAllDateFolderService_Click(object sender, EventArgs e)
        {
            TestUseAutoDateFolders_Click(sender, e);
            TestEnsureCorrectParentForPost_Click(sender, e);
            TestGetCorrectParentForPost_Click(sender, e);
            TestEnsureCorrectDate_Click(sender, e);
            TestGetYearFolder_Click(sender, e);
            TestGetMonthFolder_Click(sender, e);
            TestGetDayFolder_Click(sender, e);
        }

        protected void TestUseAutoDateFolders_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().UseAutoDateFolders();
                lblPass.Text += ResultString(string.Empty, "TestUseAutoSorting");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestUseAutoSorting : " + ex.Message);
            }
        }

        protected void TestEnsureCorrectParentForPost_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestEnsureCorrectParentForPost();
                lblPass.Text += ResultString(string.Empty, "TestEnsureCorrectParentForPost");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestEnsureCorrectParentForPost : " + ex.Message);
            }
        }

        protected void TestGetCorrectParentForPost_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestGetCorrectParentForPost();
                lblPass.Text += ResultString(string.Empty, "TestGetCorrectParentForPost");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestGetCorrectParentForPost : " + ex.Message);
            }
        }

        protected void TestEnsureCorrectDate_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestEnsureCorrectDate();
                lblPass.Text += ResultString(string.Empty, "TestEnsureCorrectDate");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestEnsureCorrectDate : " + ex.Message);
            }

        }

        protected void TestGetYearFolder_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestGetYearFolder();
                lblPass.Text += ResultString(string.Empty, "TestGetYearFolder");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestGetYearFolder : " + ex.Message);
            }
        }

        protected void TestGetMonthFolder_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestGetMonthFolder();
                lblPass.Text += ResultString(string.Empty, "TestGetMonthFolder");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestGetMonthFolder : " + ex.Message);
            }
        }

        protected void TestGetDayFolder_Click(object sender, EventArgs e)
        {
            try
            {
                new DateFolderServiceTest().TestGetDayFolder();
                lblPass.Text += ResultString(string.Empty, "TestGetDayFolder");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "TestGetDayFolder : " + ex.Message);
            }
        }

   






        #endregion





        #region PostService Tests

        protected void TestAllPostService_Click(object sender, EventArgs e)
        {
            TestAllPostService_Click(sender, e);
            DoSearchTest_Click(sender, e);
        }



        protected void CreatePostTest_Click(object sender, EventArgs e)
        {
            try
            {
                new PostServiceTest().CreatePostTest();
                lblPass.Text += ResultString(string.Empty, "CreatePostTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "CreatePostTest : " + ex.Message);
            }
        }



        protected void DoSearchTest_Click(object sender, EventArgs e)
        {
            try
            {
                new PostServiceTest().DoSearchTest();
                lblPass.Text += ResultString(string.Empty, "DoSearchTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "DoSearchTest : " + ex.Message);
            }
        }
        #endregion








        #region DocumentService Tests

        protected void TestAllDocumentService_Click(object sender, EventArgs e)
        {
            EnsureCorrectPostNodeNameTest_Click(sender, e);
            EnsureNodeExistsTest_Click(sender, e);
            GetDefaultDictionaryTest_Click(sender, e);
            GetDocumentByAliasTest_Click(sender, e);
        }
     


        protected void EnsureCorrectPostNodeNameTest_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentServiceTest().EnsureCorrectPostNodeNameTest();
                lblPass.Text += ResultString(string.Empty, "EnsureCorrectPostNodeNameTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "EnsureCorrectPostNodeNameTest : " + ex.Message);
            }
        }

        protected void EnsureNodeExistsTest_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentServiceTest().EnsureNodeExistsTest();
                lblPass.Text += ResultString(string.Empty, "EnsureNodeExistsTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "EnsureNodeExistsTest : " + ex.Message);
            }
        }



        protected void GetDefaultDictionaryTest_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentServiceTest().GetDefaultDictionaryTest();
                lblPass.Text += ResultString(string.Empty, "GetDefaultDictionaryTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "GetDefaultDictionaryTest : " + ex.Message);
            }
        }



        protected void GetDocumentByAliasTest_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentServiceTest().GetDocumentByAliasTest();
                lblPass.Text += ResultString(string.Empty, "GetDocumentByAliasTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "GetDocumentByAliasTest : " + ex.Message);
            }
        }

    





        protected void DeleteAllPosts_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentServiceTest().DeleteAllPosts();
                lblPass.Text += ResultString(string.Empty, "DeleteAllPosts");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "DeleteAllPosts : " + ex.Message);
            }
        }
        

        #endregion








        protected void TestAllDocumentExtensions_Click(object sender, EventArgs e)
        {
            TestuBlogsyGetValueFromAncestorTest_Click(sender, e);
        }



        protected void TestuBlogsyGetValueFromAncestorTest_Click(object sender, EventArgs e)
        {
            try
            {
                new DocumentExtensionsTest().uBlogsyGetValueFromAncestorTest();
                lblPass.Text += ResultString(string.Empty, "EnsureCommentsFolderTest");
            }
            catch (Exception ex)
            {
                lblError.Text += ResultString(string.Empty, "EnsureCommentsFolderTest : " + ex.Message);
            }
        }








        #region ResultString
        /// <summary>
        /// Returns a formated string with <br />
        /// </summary>
        /// <param name="result"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        protected string ResultString(string result, string method)
        {
            return result + " " + method + @"<br />";
        }

        #endregion




    }
}