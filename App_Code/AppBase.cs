using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic;
using umbraco.cms.businesslogic.web;
using PerceptiveMCAPI;
using PerceptiveMCAPI.Types;
using PerceptiveMCAPI.Methods;

/// <summary>
/// Summary description for AppBase
/// </summary>
public class AppBase : umbraco.businesslogic.ApplicationStartupHandler
{
	public AppBase()
	{
        Document.BeforePublish += new Document.PublishEventHandler(Document_BeforePublish);
	}

    void Document_BeforePublish(Document sender, PublishEventArgs e) {

        try
        {
            if (sender.ContentType.Alias == "uBlogsyPost")
            {
                bool sendNewsletter = (sender.getProperty("sendNewsletter").Value.ToString() == "0") ? false : true;
                if (sendNewsletter)
                {
                    var sum = sender.getProperty("uBlogsyContentSummary").Value;
                    string title = sender.getProperty("uBlogsyContentTitle").Value.ToString();
                    Dictionary<string, object> lookup = new Dictionary<string, object>() { { "title", "" } };
                    campaignsInput input = new campaignsInput(lookup);
                    campaigns camps = new campaigns();
                    if (camps.Execute(input).result.Where(t => t.title == title).Count() == 0)
                    {
                        campaignCreateInput campInput = new campaignCreateInput();
                        campInput.parms.apikey = PerceptiveMCAPI.MCAPISettings.default_apikey;
                        campInput.parms.options.title = title;
                        campInput.parms.options.list_id = "68972d2e33";
                        campInput.parms.options.auto_footer = true;
                        campInput.parms.options.subject = "The Newsletter - " + title;
                        campInput.parms.options.tracking = new campaignTracking(true, true, true);
                        campInput.parms.options.template_id = 88565;
                        campInput.parms.options.analytics.Add("google", title);
                        campInput.parms.options.to_email = "*|FNAME|*";
                        campInput.parms.options.from_email = "jonathan.c.read@gmail.com";
                        campInput.parms.options.from_name = "American City Plumbing";

                        campInput.parms.content.Add("html_std_content", sum.ToString());
                        campaignCreate create = new campaignCreate();
                        campaignCreateOutput campOut = create.Execute(campInput);
                        var r = campOut.result;

                        if (campOut != null)
                        {
                            var c = camps.Execute(new campaignsInput(new Dictionary<string, object>() { { "title", title } }));

                            campaignSendNowInput sendInput = new campaignSendNowInput(r);
                            campaignSendNow now = new campaignSendNow();
                            var sI = now.Execute(sendInput);
                            var s = sI.result;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ex.ToString();
            throw;
        }
    
        //cancel the publishing
       // e.Cancel = true;
    }
}