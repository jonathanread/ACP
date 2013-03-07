using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Profile;
using System.Web.Security;

/// <summary>
/// Summary description for MembershipHelper
/// </summary>
public class MembershipHelper
{
    private static MembershipUser member { get; set; }
    
    public MembershipHelper()
	{
       
	}

    public static bool CreateCustomer(string name, string email){
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(email))
        {
            return false;
        }
        else
        {
           member = Membership.CreateUser(email, "password", email);
        }

        return true;
    }

    public static bool CreateCustomer(string name, string email, string phone,object address, string customerType, DateTime firstContact, DateTime mostRecentContact, string typeOfContact, bool recieveNewsletter){
        try
        {
            if (member != null)
            {

            }
            else
            {
                member = Membership.CreateUser(email, "password", email);
                var profile = ProfileBase.Create(member.UserName);
                profile[""] = "";
                profile.Save();
            }
        }
        catch (Exception)
        {
            
            throw;
        }

        return true;
    }
}