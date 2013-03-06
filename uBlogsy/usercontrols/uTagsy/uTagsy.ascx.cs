using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using uBlogsy.BusinessLogic;
using umbraco.cms;
using umbraco;
using umbraco.editorControls;
//using umbraco.cms.BusinessLogic;
using System.Web.Script.Serialization;
using System.IO;
using umbraco.editorControls.userControlGrapper;
using umbraco.presentation;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using System.Collections;

namespace uTagsy.Web.usercontrols.uTagsy.datatypes
{
    public partial class uTagsy : UserControl, IUsercontrolDataEditor
    {
        public string umbracoValue;
        public string Id { get; set; }
        public string DocumentType { get; set; }


        private const string FILE = "~/App_Data/uTagsy.json";

        #region OnLoad
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            this.Id = base.Request.QueryString["id"];
            var doc = new Document(int.Parse(this.Id));
            this.DocumentType = doc.ContentType.Alias;


            if (Page.IsPostBack)
            {
                SaveSettings();
            }

            LoadSettings();

            uTagsy_hfLandingId.Value = DocumentService.Instance.GetDocumentByAlias(doc, "uBlogsyLanding", "uBlogsyLanding").Id.ToString();
        }

        #endregion



        #region LoadSettings

        protected void LoadSettings()
        {
            this.uTagsy_hfTags.Value = this.umbracoValue;

            if (!File.Exists(HttpContext.Current.Server.MapPath(FILE)))
            {
                return;
            }

            // get complete tag list from json from file
            object fileJsonObject;
            string fileJsonString = File.ReadAllText(HttpContext.Current.Server.MapPath(FILE));
            try
            {
                fileJsonObject = new JavaScriptSerializer().DeserializeObject(fileJsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("The file settings.json had bad json. Try deleting the bad json. Or recreating with : {\"documents\":[]} \n" + ex.Message);
            }
            object documents = ((Dictionary<string, object>)fileJsonObject)["documents"];
            foreach (object d in (object[])documents)
            {
                string documentType = (string)((Dictionary<string, object>)d)["documentType"];
                if (documentType == this.DocumentType)
                {
                    this.uTagsy_hfAllTags.Value = "{\"document\":" + new JavaScriptSerializer().Serialize(d) + "}";
                    return;
                }
            }
        }







        #endregion



        #region SaveSettings
        /// <summary>
        /// Saves settings to umbracoValue or file.
        /// </summary>
        protected void SaveSettings()
        {
            // fix json string
            string input = this.uTagsy_hfTags.Value.Replace("\\\"", "\"");

            // deserialize and get tags
            object tags = ((Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(input))["tags"];

            // make new object and save to umbraco
            this.umbracoValue = string.Join(",", ((IEnumerable)tags).Cast<string>().Select(x => x.Replace(uTagsy_hfLandingId.Value + "_" , string.Empty)));

            // get json for this document type
            input = this.uTagsy_hfAllTags.Value.Replace("\\\"", "\"");

            // get the document
            object document = ((Dictionary<string, object>)new JavaScriptSerializer().DeserializeObject(input))["document"];

            // save json for document type
            SaveFileSettings(document);
        }


        private void SaveFileSettings(object document)
        {
            string jsonString;

            // get json from file
            string fileJsonString = File.ReadAllText(HttpContext.Current.Server.MapPath(FILE));

            object fileJsonObject;
            try
            {
                fileJsonObject = new JavaScriptSerializer().DeserializeObject(fileJsonString);
            }
            catch (Exception ex)
            {
                throw new Exception("The file settings.json had bad json. Try deleting the bad json. Or recreating with : {\"documents\":[]} \n" + ex.Message);
            }

            bool exists = false;
            object[] documents = (object[])((Dictionary<string, object>)fileJsonObject)["documents"];
            for (int i = 0; i < documents.Length; i++)
            {
                string docType = (string)((Dictionary<string, object>)documents[i])["documentType"];

                // replace document
                if (docType == this.DocumentType)
                {
                    exists = true;
                    documents[i] = document;
                    break;
                }
            }

            if (!exists)
            {
                List<object> list = documents.ToList<object>();
                list.Add(document);
                documents = list.ToArray();
            }

            string str4 = new JavaScriptSerializer().Serialize(documents);
            jsonString = "{\"documents\":" + new JavaScriptSerializer().Serialize(documents) + "}";

            File.WriteAllText(HttpContext.Current.Server.MapPath(FILE), jsonString);
        }







        #region value
        public object value
        {
            get
            {
                return umbracoValue;
            }
            set
            {
                umbracoValue = value.ToString();
            }
        }
        #endregion

    }
        #endregion


        
}