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

namespace ECM_Shell_AutoArch
{
    public partial class Reports : System.Web.UI.Page
    {
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
                ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderReqPlan", "*.xls*", lstReqPlan);
                ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderCVRDoc", "*.doc*", lstCVRs);
                ECM_Shell_AutoArch.Planning.loadDBTabData("shell_Planning", "uniqueid", "", "WHERE status like '%In Progress%'", null, lstAOs);
                //
                ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderReqPlan", "*Report.csv", lstCsvRpt);
                //
                ExecShowCVRTask(true);
            }
        }


        protected void SetInitFields(bool setFlg)
        {
            dtSel1.Text = "-";
            dtSel1.Visible = false;
            dtSel2.Text = "-";
            dtSel2.Visible = false;
            dtSel3.Visible = false;
            divMessage.InnerHtml = "";
            GridView1.DataSource = null;
            GridView1.DataBind();
            GridView1.Visible = false;
            GridView2.DataSource = null;
            GridView2.DataBind();
            GridView2.Visible = false;

            divMessage.InnerHtml = "";
            btnGeraCVR.Enabled = setFlg;
            if (setFlg)
                btnGeraCVR.ForeColor = System.Drawing.Color.White;
            else
                btnGeraCVR.ForeColor = System.Drawing.Color.Silver;

            panAviso.Visible = true;
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
                    lstCsvRpt.Items.Clear();
                    ECM_Shell_AutoArch.Planning.loadFolderData("SrvFolderReqPlan", "*Report.csv", lstCsvRpt);

                    // Notify the user that their file was successfully uploaded.
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Message: " + "Your file " + savePath + " was uploaded successfully!" + "  </span>";
                }
                else
                {
                    // Notify the user why their file was not uploaded.
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + "Your file was not uploaded because it does not have a .csv, .xls or .xlsx extension." + "  </span>";
                }

            }
        }


        protected DataTable readViewTasks()
        {
            //find in Datatable of Schedule Tasks...
            //string s = "CVR";
            DataTable dtTask = MPSfwk.WMI.dtScheduleTaskList("CVR Shell_AutoArch");
            //
            GridView1.Visible = true;
            GridView1.DataSource = dtTask;
            GridView1.DataBind();
            //
            return dtTask;
        }


        protected void ExecShowCVRTask(bool sohShow)
        {
            string UIdSel = "";
            if (lstAOs.SelectedItem != null)
            {
                UIdSel += lstAOs.SelectedItem.Text;
                UIdSel += " " + lstReqPlan.SelectedItem.Text.Substring(0, lstReqPlan.SelectedItem.Text.IndexOf("|")).Trim();
            }
            //
            DataTable dt = readViewTasks();
            //
            string bat = dt.Rows[0]["Name"].ToString();
            int posi = bat.IndexOf("(") + 1;
            int lenf = bat.IndexOf(" )") - posi;

            string logbat = EditBatchRetLog(bat.Substring(posi, lenf), UIdSel, sohShow);
            string auxlog = File.ReadAllText(logbat);
            ECM_Shell_AutoArch.Planning.showLogTv(TreeView1, auxlog, "Schedule Task History Content");

            if (!sohShow)
            {
                //Chama a função para o Executar a Tarefa...
                string cmd = "schtasks /run /TN \"" + bat.Substring(0, bat.IndexOf("(")).Trim() + "\" ";

                //if ret_cmd error...abend
                string ret_cmd = MPSfwk.WMI.ExecConsole(cmd, "");
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Executed - " + ret_cmd + "! Please wait the task execution... </span>";
            }
        }


        protected void btnGeraCVR_Click(object sender, EventArgs e)
        {
            //
            ExecShowCVRTask(false);
            //
        }


        protected string EditBatchRetLog(string filePath, string txtEdit, bool sohView)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            //
            if (!sohView)
            { 
                int posi = lines[1].IndexOf("9 ") + 2;
                int lenf = lines[1].IndexOf(" >") - posi;
                string aux = lines[1].Substring(posi, lenf).Trim();
                lines[1] = lines[1].Replace(aux, txtEdit);
                File.WriteAllLines(filePath, lines); 
            }
            //
            GridView2.Visible = true;
            GridView2.DataSource = lines;
            GridView2.DataBind();
            //
            return lines[1].Substring(lines[1].IndexOf(">") + 1).Trim();
        }


        protected void lstAOs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //Search and set in lstReqPlan the Plan selected in lstAOs...
            string ret = ECM_Shell_AutoArch.Planning.FindSelList(lstAOs.SelectedItem.ToString().Replace("-", "").Replace("_", ""), lstReqPlan);

            if (ret != "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + ret;
            }
        }


        protected void lstCsvRpt_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected void lstReqPlan_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
        protected void btn_Upload1_Click(object sender, EventArgs e)
        {

        }
        protected void btnDownload_Click(object sender, EventArgs e)
        {

        }
        protected void btnListCVR_Click(object sender, EventArgs e)
        {

        }
    }
}