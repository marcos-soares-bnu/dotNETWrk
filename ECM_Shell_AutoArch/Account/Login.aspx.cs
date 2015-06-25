using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
//

namespace ECM_Shell_AutoArch.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //---test
            //Check Permission of PHP param...
            //Session["permissoes"] = Request.QueryString["Profile"];

            ////Call chkUser function...
            //if (!ECM_Shell_AutoArch.Planning.chkProfUser((string)(Session["permissoes"])))
            //    Response.Redirect("~/Advise.aspx?AdviseMsg=\"User not Authorized!\"");
            //else
            //    Response.Redirect("~/Planning.aspx");

            //---test

            RegisterHyperLink.NavigateUrl = "Register";
            // ----------------------------------------//OpenAuthLogin.ReturnUrl = Request.QueryString["ReturnUrl"];

            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["ReturnUrl"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }


        }
    }
}