﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace ECM_Shell_AutoArch
{
    public partial class Advise : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            //Load Message...
            lblMessage.Text = Request.QueryString["AdviseMsg"];
        }
    }
}