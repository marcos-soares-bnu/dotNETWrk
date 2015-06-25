using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECM_Shell_AutoArch
{
    public partial class _Default : Page
    {
        public static string perfil;

        protected void Page_Load(object sender, EventArgs e)
        {
            ////Check Permission of PHP param...
            //Session["permissoes"] = Request.QueryString["Profile"];

            ////Call chkUser function...
            //if (!ECM_Shell_AutoArch.Planning.chkProfUser((string)(Session["permissoes"])))
            //    Response.Redirect(ECM_Shell_AutoArch.Planning.LNK_MSG_USR_NOTAUTH);
            //else
            //    Response.Redirect("~/Planning.aspx");
        }
    }
}