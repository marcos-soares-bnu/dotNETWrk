using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
//
using System.Data.OleDb;
//
using System.Text.RegularExpressions;

namespace ECM_Shell_AutoArch
{
    public partial class Operation : System.Web.UI.Page
    {
        public MembershipUser currentUser;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Disable Initial buttons...
                SetInitFields(true);

                ////Call chkUser function...
                //if (!ECM_Shell_AutoArch.Planning.chkProfUser((string)(Session["permissoes"])))
                //    Response.Redirect(ECM_Shell_AutoArch.Planning.LNK_MSG_USR_NOTAUTH);

                ////Check if Prof = Admin = 9
                //if ((string)(Session["permissoes"]) == "9") { SetInitFields(true); }

                //Load ListBoxes...
                ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderSQLScr", "*.sql", lstSQLs);
                ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderReqPlan", "*.*s*", lstReqPlan);
                ECM_Shell_AutoArch.Planning.loadDBTabData("shell_Planning", "uniqueid", "", "WHERE status like '%Pre%'", null, lstAOs);
            }
        }


        protected void SetInitFields(bool setFlg)
        {
            //Set initial definitions...
            //dtSel1.Text = "-";
            dtSel1.Visible = false;
            //dtSel2.Text = "-";
            dtSel2.Visible = false;
            dtSel3.Visible = false;
            //
            btnExport.Enabled = setFlg;
            if (setFlg)
                btnExport.ForeColor = System.Drawing.Color.White;
            else
                btnExport.ForeColor = System.Drawing.Color.Silver;

            divMessage.InnerHtml = "";
            btnGeraSQL.Enabled = setFlg;
            if (setFlg)
                btnGeraSQL.ForeColor = System.Drawing.Color.White;
            else
                btnGeraSQL.ForeColor = System.Drawing.Color.Silver;

            GridView1.DataSource = null;
            GridView1.DataBind();
            GridView2.DataSource = null;
            GridView2.DataBind();
            GridView2.Visible = false;
            panAviso.Visible = true;
            //
        }


        protected void lstAOs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Clear GridView...
            GridView1.DataSource = null;
            GridView1.DataBind();

            //Hide Treeview...
            TreeView1.Nodes.Clear();

            if (btnExport.Text.IndexOf("Open") >= 0)
                btnExport.Text = "Preview Orders...";

            //Search and set in lstReqPlan the Plan selected in lstAOs...
            string ret = ECM_Shell_AutoArch.Planning.FindSelList(lstAOs.SelectedItem.ToString().Replace("-", "").Replace("_", ""), lstReqPlan);

            if (ret != "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + ret;
            }
        }


        protected void lstReqPlan_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Clear GridView...
            GridView1.DataSource = null;
            GridView1.DataBind();

            //Hide Treeview...
            TreeView1.Nodes.Clear();

            if (btnExport.Text.IndexOf("Open") >= 0)
                btnExport.Text = "Preview Orders...";
        }


        protected void btn_Upload_Click(object sender, EventArgs e)
        {
            // Specify the path on the server to 
            // save the uploaded file to.
            string savePath = (ConfigurationManager.AppSettings["SrvFolderReqPlan"] + "\\").Trim();

            // Before attempting to save the file, verify 
            // that the FileUpload control contains a file. 
            if (FileUpload1.HasFile)
            {
                // Get the name of the file to upload.
                string fileName = Server.HtmlEncode(FileUpload1.FileName);

                // Get the extension of the uploaded file.
                string extension = System.IO.Path.GetExtension(fileName);

                // Allow only files with .doc or .xls extensions 
                // to be uploaded. 
                if ((extension == ".xls") || (extension == ".xlsx") || (extension == ".csv"))
                {
                    // Append the name of the file to upload to the path.
                    savePath += fileName;

                    // Call the SaveAs method to save the  
                    // uploaded file to the specified path. 
                    // This example does not perform all 
                    // the necessary error checking.                
                    // If a file with the same name 
                    // already exists in the specified path,   
                    // the uploaded file overwrites it.
                    FileUpload1.SaveAs(savePath);

                    //Update list...
                    lstReqPlan.Items.Clear();
                    ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderReqPlan", ("*" + extension), lstReqPlan);

                    // Notify the user that their file was successfully uploaded.
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Message: " + "Your Request Form file " + savePath + " was uploaded successfully." + "  </span>";
                }
                else
                {
                    // Notify the user why their file was not uploaded.
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + "Your file was not uploaded because it does not have a .csv, .xls or .xlsx extension." + "  </span>";
                }

            }
        }


        protected void lstSQLs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Clear GridView...
            GridView1.DataSource = null;
            GridView1.DataBind();

            //Hide Treeview...
            TreeView1.Nodes.Clear();

            if (btnExport.Text.IndexOf("Preview") >= 0)
                btnExport.Text = "Open SQL Script";
        }


        protected void btnGeraSQL_Click(object sender, EventArgs e)
        {
            //UniqueID and Req Filename - Regex Identification...
            Regex regex = new Regex("(.*?).xls");
            var v = regex.Match(lstReqPlan.SelectedValue);

            string uniqueid = v.Groups[1].ToString().Replace("_-_", "-").Replace("_", "-");
            if (uniqueid.Length > 9)
                uniqueid = uniqueid.Substring(uniqueid.IndexOf("-")+1).Trim();
            else if (uniqueid.Length == 0)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: This function still not unable to CSV file!  </span>";
                return;
            }

            string reqfilename = lstReqPlan.SelectedValue.Substring(0, lstReqPlan.SelectedValue.IndexOf("|")).Trim();
            string locfile = (ConfigurationManager.AppSettings["SrvFolderReqPlan"] + "\\").Replace("\\\\10.58.87.19\\", "C:\\").Trim();

            //Declare and show SQL of GridView...
            string aux_where = "";
            string aux_sql = "";

            //Load Data from DBase...
            aux_where = "WHERE UniqueId like '%" + uniqueid + "%' ";
            ECM_Shell_AutoArch.Planning.loadDBTabData("shell_Planning", "InstanceId", "", aux_where, GridView2, null);
            if (GridView2.Rows.Count >= 1)
            {
                if (GridView2.Rows[0].Cells.Count == 18)
                {
                    string instid = GridView2.Rows[0].Cells[4].Text.Trim();
                    int indSpc = instid.IndexOf("&#160;");
                    if (indSpc > 0)
                        instid = instid.Substring(0, indSpc).Trim();
                    //
                    aux_where = "WHERE id_instance like '%" + instid + "%' ";
                    ECM_Shell_AutoArch.Planning.loadDBTabData("shell_sourcesys_x_inst", "ssystemid", "", aux_where, GridView2, null);

                    if (GridView2.Rows.Count >= 1)
                    {
                        if (GridView2.Rows[0].Cells.Count == 2)
                        {
                            string sysid = GridView2.Rows[0].Cells[0].Text.Trim();
                            aux_sql = MPSfwk.WMI.ShowSQLInsOrders(uniqueid, instid, sysid, locfile, reqfilename);
                        }
                        else
                        {
                            divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: (loadDBTabData - Sysid)" + aux_where + "  </span>";
                        }
                    }
                    else
                    {
                        divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: (loadDBTabData - Sysid Not Found)" + aux_where + "  </span>";
                    }
                }
                else
                {
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: (loadDBTabData - Planning)" + aux_where + "  </span>";
                }
            }
            //
            ECM_Shell_AutoArch.Planning.showLogTv(TreeView1, aux_sql, "SQL Content");
            //
            // ECM_Shell_AutoArch.Planning.showLogTv (aux_sql);
        }


        protected void btnExport_Click(object sender, EventArgs e)
        {
            //
            if (btnExport.Text.IndexOf("Open") >= 0)
            {
                if (lstSQLs.SelectedValue == "")
                    return;

                string filename = ConfigurationManager.AppSettings["SrvFolderSQLScr"] + "\\" + lstSQLs.SelectedValue.Substring(0, lstSQLs.SelectedValue.IndexOf("|"));
                filename = filename.Replace("\\\\10.58.87.19\\", "C:\\").Trim();

                try
                {
                    string aux = File.ReadAllText(filename);
                    ECM_Shell_AutoArch.Planning.showLogTv(TreeView1, aux, "SQL Content");
                    //showLogTv(aux);
                }
                catch (Exception eg)
                {
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + "  </span>";
                }
            }
            else
            {
                if (lstReqPlan.SelectedValue != "")
                {
                    //
                    string locfile = (ConfigurationManager.AppSettings["SrvFolderReqPlan"] + "\\").Replace("\\\\10.58.87.19\\", "C:\\").Trim();
                    string reqname = lstReqPlan.SelectedValue.Substring(0, lstReqPlan.SelectedValue.IndexOf("|")).Trim();
                    //
                    if (reqname.IndexOf(".csv") >= 0)
                    {
                        //
                        //Call Function to read CSV Request Form selected... (2015/Jun)
                        DataTable dt = MPSfwk.WMI.CsvFileToDatatable((locfile + reqname), true);
                        //
                        //Fill the Grid e paint the rows changed...
                        GridView1.DataSource = dt;
                        GridView1.DataBind();
                    }
                    else
                    {
                        //
                        //Call Function to read and revise XLS Request Form selected...
                        DataTable dt = MPSfwk.WMI.ReadReqAOChecked(locfile, reqname);

                        if (dt != null)
                        {
                            //Fill the Grid e paint the rows changed...
                            GridView1.DataSource = dt;
                            GridView1.DataBind();

                            //Set yellow rows revised...
                            int j = 0;
                            foreach (DataRow dro in dt.Rows)
                            {
                                if (dro[1].ToString() == "True") { GridView1.Rows[j].BackColor = System.Drawing.Color.Yellow; }
                                j++;
                            }
                        }
                        else
                        {
                            divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error While Openning Request Form File: " + (locfile + reqname) + "  </span>";
                        }
                    }
                }
            }
        }










    }
}