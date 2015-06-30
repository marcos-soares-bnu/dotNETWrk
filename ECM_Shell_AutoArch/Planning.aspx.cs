using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Management;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.Security;
using System.Xml;
using System.Xml.Linq;
using System.Reflection;
//
using PetaPoco;

namespace ECM_Shell_AutoArch
{
    public partial class Planning : System.Web.UI.Page
    {
        public static string g_txt_status;
        public static string g_txt_actaoenddate;
        public static string g_txt_actaoreceivedate;
        public static string g_txt_actaostartdate;
        public static string g_txt_colsitenum;
        public static string g_txt_comment;
        public static string g_chk_dataexpctrchk;
        public static string g_txt_duname;
        public static string g_txt_durationworkdays;
        public static string g_txt_operatorname;
        public static string g_txt_planaoenddate;
        public static string g_txt_planaoreceivedate;
        public static string g_txt_planaostartdate;
        public static string g_txt_uniqueid;
        public static string g_txt_instanceid;
        public static string g_txt_volumegb;
        public static string g_chk_waiverchk;
        //
        public static StringBuilder g_WHERE;
        //
        public MembershipUser currentUser;
        //
        public static string LNK_MSG_USR_NOTAUTH = "~/Advise.aspx?AdviseMsg=User not Authorized!";

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Disable Initial buttons...
                SetInitFields(true);

                ////Call chkUser function...
                //if (!chkProfUser((string)(Session["permissoes"])))
                //    Response.Redirect(LNK_MSG_USR_NOTAUTH);

                ////Check if Prof = Admin = 9
                //if ((string)(Session["permissoes"]) == "9") { SetInitFields(true); }

                //Load Grid...
                loadDBTabData("shell_Planning", "*", "ActAOReceiveDate DESC, UniqueId DESC", "", GridView1, null);
            }
            else
                //Record all fields in global vars...
                UpdFields();
        }


        //
        //Check Permission of PHP param...
        //
        public static bool chkProfUser(string profUsr)
        {
            if ((profUsr != "7") && (profUsr != "9"))   { return false; }
            else                                        { return true; }
        }


        //Set Default buttons...
        protected void SetInitFields(bool setFlg)
        {
            btn_Save.Enabled = setFlg;
            if (setFlg)
                btn_Save.ForeColor = System.Drawing.Color.White;
            else
                btn_Save.ForeColor = System.Drawing.Color.Silver;

            divMessage.InnerHtml = "";

            //********************************************************************************
            TreeView1.Visible = true;
            string ret = File.ReadAllText("C:\\public\\SHELL\\Shell_PST_Alerts\\Today.txt");
            //
            if (ret.IndexOf("|Info| [") >= 0)
                LoopchkNewUpd(ret);
            else
                ret = "|Info| " + ret.Replace("\n", "") + " ...Start";
            //
            showLogTv(TreeView1, ret, "Shell Outlook AO Folder Content");
            TreeView1.CollapseAll();
            //********************************************************************************
        }

        //*** TEST MPS - 15-6-2015 ***
        protected void LoopchkNewUpd(string Pstext)
        {
            var listMails = Pstext.Split('\n').Reverse();
            //
            foreach (string linha in listMails)
            {
                if (((linha.IndexOf("|Info|") >= 0) &&
                         (linha.IndexOf("Start") >= 0)) || (linha.IndexOf("/***") >= 0))
                {
                    //Check new and updates items to Update auto into Database...
                    string act = (linha.Substring(0, linha.IndexOf("|"))).Trim();
                    if ((act.IndexOf("new!") >= 0) || (act.IndexOf("upd!") >= 0))
                        chkNewUpdDB(act, linha.Split('['));
                    //
                }

            }
            ClrFields(false);
        }


        private string convDtime24H(string inpDt)
        {
            // Displays 06:09:01 du.
            // Last Modified 18/06/2015 01:15 PM by Blasius, Mateus SITI-ITSS-HNS
            int posi, posf;
            posi = 0;
            posf = inpDt.IndexOf(" by ");
            //
            if ((posf < posi) || (posf <= 0))
                return "";
            //
            posf = posf - 2;
            string str_Id24H = inpDt.Substring(posf, 2);
            string str_dt24H = inpDt.Substring(posi, posf-1);
            //
            if ((str_Id24H != "AM") && (str_Id24H != "PM"))
                return "";
            //
            if (str_dt24H.Length != 16)
                return "";
            //
            int YYYY = Convert.ToInt16(str_dt24H.Substring(6, 4));
            int MM = Convert.ToInt16(str_dt24H.Substring(3, 2));
            int DD = Convert.ToInt16(str_dt24H.Substring(0, 2));
            int hh = Convert.ToInt16(str_dt24H.Substring(11, 2));
            //
            if (str_Id24H == "PM") { hh += 12; }
            //
            int mm = Convert.ToInt16(str_dt24H.Substring(14, 2));
            //
            //DateTime date1 = new DateTime(2008, 1, 1, 18, 9, 1, 500);
            DateTime date24H = new DateTime(YYYY, MM, DD, hh, mm, 0, 000);
            //
            return date24H.ToString();
        }


        public void chkNewUpdDB(string action, string[] arrValues)
        {
            try
            {

                //upd fields...
                if ((action.IndexOf("new!") >= 0) || (action.IndexOf("upd!") >= 0))
                {
                    //
                    // test array length / field Edited...
                    //
                    if ((arrValues.Length < 10) || (arrValues[1].IndexOf("]") == -1))
                        return;
                    //
                    if (arrValues[1].IndexOf("Edited") > 0)
                    {
                        int posi = arrValues[1].IndexOf(" ") + 1;
                        int posf = arrValues[1].IndexOf("Edited");

                        string aux = arrValues[1].Substring(posi, (posf - posi)).Trim();
                        arrValues[1] = aux + "]";
                    }
                    else
                    {
                        if (arrValues[1].Length > 11) { arrValues[1] = arrValues[1].Replace(" - ","-"); }
                    }
                    //
                    txt_uniqueid.Text = arrValues[1].Substring(0, arrValues[1].IndexOf("]"));
                    txt_uniqueid.Text = txt_uniqueid.Text.Replace(" ", "");
                    if (txt_uniqueid.Text.Length > 15)
                        txt_uniqueid.Text = txt_uniqueid.Text.Substring(0, 15);
                    //
                    txt_duname.Text = arrValues[2].Substring(0, arrValues[2].IndexOf("]"));
                    txt_instanceid.Text = arrValues[3].Substring(0, arrValues[3].IndexOf("]"));
                    //
                    txt_planaoreceivedate.Text = arrValues[7].Substring(0, arrValues[7].IndexOf("]"));
                    txt_actaoreceivedate.Text = arrValues[7].Substring(0, arrValues[7].IndexOf("]"));
                    //
                    txt_planaostartdate.Text = arrValues[7].Substring(0, arrValues[7].IndexOf("]"));
                    txt_actaostartdate.Text = arrValues[7].Substring(0, arrValues[7].IndexOf("]"));
                    //
                    txt_planaoenddate.Text = arrValues[8].Substring(0, arrValues[8].IndexOf("]"));
                    txt_actaoenddate.Text = arrValues[8].Substring(0, arrValues[8].IndexOf("]"));
                    //
                    if (action.IndexOf("new!") >= 0)
                    {
                        txt_actaostartdate.Text = arrValues[4].Substring(0, 16);
                        txt_actaoreceivedate.Text = arrValues[4].Substring(0, 16);
                    }
                    else
                    {
                        txt_actaoenddate.Text = convDtime24H(arrValues[4]);
                    }
                    //
                    valid_DateTime(txt_planaoreceivedate);
                    valid_DateTime(txt_actaoreceivedate);
                    //
                    valid_DateTime(txt_planaostartdate);
                    valid_DateTime(txt_actaostartdate);
                    //
                    valid_DateTime(txt_planaoenddate);
                    valid_DateTime(txt_actaoenddate);
                    //
                    txt_operatorname.Text = arrValues[4].Substring(arrValues[4].IndexOf("by ") + 3).Replace("]", "");
                    //
                    if (arrValues[5].IndexOf("Rejected") >= 0)
                        txt_status.Text = arrValues[5].Substring(0, arrValues[5].IndexOf("]"));
                    else
                        txt_status.Text = "In Progress";
                    //
                    chk_dataexpctrchk.Checked = true;
                    chk_waiverchk.Checked = true;
                    txt_colsitenum.Text = "0";
                    txt_volumegb.Text = "0";
                    //
                    txt_comment.Text = arrValues[6].Substring(0, arrValues[6].IndexOf("]")) + " \n" +
                                       arrValues[9].Substring(0, arrValues[9].IndexOf("]"));
                    //
                    SaveNewUpd(true);
                }

            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + arrValues[1] + "  </span><br>";
            }

        }

        //Calc working days...17/06/2015
        public int GetWorkingDays(DateTime from, DateTime to)
        {
            var totalDays = 0;
            for (var date = from; date < to; date = date.AddDays(1))
            {
                if (date.DayOfWeek != DayOfWeek.Saturday
                    && date.DayOfWeek != DayOfWeek.Sunday)
                    totalDays++;
            }

            return totalDays;
        }

        //Clear all fields...
        protected void ClrFields(bool flgDiv)
        {
            txt_status.Text = "";
            txt_actaoenddate.Text = "";
            txt_actaoreceivedate.Text = "";
            txt_actaostartdate.Text = "";
            txt_colsitenum.Text = "";
            txt_comment.Text = "";
            //
            chk_dataexpctrchk.Checked = false;
            //
            txt_duname.Text = "";
            txt_durationworkdays.Text = "";
            txt_operatorname.Text = "";
            txt_planaoenddate.Text = "";
            txt_planaoreceivedate.Text = "";
            txt_planaostartdate.Text = "";
            txt_uniqueid.Text = "";
            txt_instanceid.Text = "";
            txt_volumegb.Text = "";
            //
            chk_waiverchk.Checked = false;
            //
            if (flgDiv)
                divMessage.InnerHtml = "";
        }

        //*** TEST MPS - 15-6-2015 ***


        //Record all fields in global vars...
        protected void UpdFields()
        {
            g_txt_status = txt_status.Text;
            g_txt_actaoenddate = txt_actaoenddate.Text;
            g_txt_actaoreceivedate = txt_actaoreceivedate.Text;
            g_txt_actaostartdate = txt_actaostartdate.Text;
            g_txt_colsitenum = txt_colsitenum.Text;
            g_txt_comment = txt_comment.Text;
            //
            if (chk_dataexpctrchk.Checked) { g_chk_dataexpctrchk = "1"; }
            else { g_chk_dataexpctrchk = null; }
            //
            g_txt_duname = txt_duname.Text;
            g_txt_durationworkdays = txt_durationworkdays.Text;
            g_txt_operatorname = txt_operatorname.Text;
            g_txt_planaoenddate = txt_planaoenddate.Text;
            g_txt_planaoreceivedate = txt_planaoreceivedate.Text;
            g_txt_planaostartdate = txt_planaostartdate.Text;
            g_txt_uniqueid = txt_uniqueid.Text;
            g_txt_instanceid = txt_instanceid.Text;
            g_txt_volumegb = txt_volumegb.Text;
            //
            if (chk_waiverchk.Checked) { g_chk_waiverchk = "1"; }
            else { g_chk_waiverchk = null; }
            //
            Monta_g_WHERE();
        }


        public static void Monta_g_WHERE()
        {
            //--------------------------------------------------
            //Fill g_WHERE to use in Filter option...
            //-------------------------------------------------
            g_WHERE = new StringBuilder("WHERE 1 = 1");
            g_WHERE.AppendLine();
            //------------------------------------------------------------
            if (g_txt_status != "")
                g_WHERE.AppendLine(string.Format("AND status like '%{0}%'", g_txt_status));
            //
            if ((g_txt_actaoenddate != "01/01/0001 00:00:00") && (g_txt_actaoenddate != ""))
                g_WHERE.AppendLine(string.Format("AND actaoenddate = '{0}'", g_txt_actaoenddate));
            //
            if ((g_txt_actaoreceivedate != "01/01/0001 00:00:00") && (g_txt_actaoreceivedate != ""))
                g_WHERE.AppendLine(string.Format("AND actaoreceivedate = '{0}'", g_txt_actaoreceivedate));
            //
            if ((g_txt_actaostartdate != "01/01/0001 00:00:00") && (g_txt_actaostartdate != ""))
                g_WHERE.AppendLine(string.Format("AND actaostartdate = '{0}'", g_txt_actaostartdate));
            //
            if (g_txt_colsitenum != "")
                g_WHERE.AppendLine(string.Format("AND colsitenum = {0}", g_txt_colsitenum));
            //
            if (g_txt_comment != "")
                g_WHERE.AppendLine(string.Format("AND comment like '%{0}%'", g_txt_comment));
            //
            if (g_chk_dataexpctrchk == "1")
                g_WHERE.AppendLine("AND dataexpctrchk = 1");
            //
            if (g_txt_duname != "")
                g_WHERE.AppendLine(string.Format("AND duname like '%{0}%'", g_txt_duname));
            //
            if (g_txt_durationworkdays != "")
                g_WHERE.AppendLine(string.Format("AND durationworkdays = '{0}'", g_txt_durationworkdays));

            if (g_txt_operatorname != "")
                g_WHERE.AppendLine(string.Format("AND operatorname like '%{0}%'", g_txt_operatorname));
            //
            if ((g_txt_planaoenddate != "01/01/0001 00:00:00") && (g_txt_planaoenddate != ""))
                g_WHERE.AppendLine(string.Format("AND planaoenddate = '{0}'", g_txt_planaoenddate));
            //
            if ((g_txt_planaoreceivedate != "01/01/0001 00:00:00") && (g_txt_planaoreceivedate != ""))
                g_WHERE.AppendLine(string.Format("AND planaoreceivedate = '{0}'", g_txt_planaoreceivedate));
            //
            if ((g_txt_planaostartdate != "01/01/0001 00:00:00") && (g_txt_planaostartdate != ""))
                g_WHERE.AppendLine(string.Format("AND planaostartdate = '{0}'", g_txt_planaostartdate));
            //
            if (g_txt_uniqueid != "")
                g_WHERE.AppendLine(string.Format("AND uniqueid like '%{0}%'", g_txt_uniqueid));
            //
            if (g_txt_instanceid != "")
                g_WHERE.AppendLine(string.Format("AND instanceid like '%{0}%'", g_txt_instanceid));
            //
            if (g_txt_volumegb != "")
                g_WHERE.AppendLine(string.Format("AND volumegb = '{0}'", g_txt_volumegb.Replace(",", ".")));
            //
            if (g_chk_waiverchk == "1")
                g_WHERE.AppendLine("AND waiverchk = 1");
        }


        private int GetColumnIndexByName(GridView grid, string name)
        {
            for (int i = 0; i < grid.HeaderRow.Cells.Count; i++)
            {
                if (grid.HeaderRow.Cells[i].Text.ToLower().Trim() == name.ToLower().Trim())
                {
                    return i;
                }
            }
            return -1;
        }


        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //Pega o indice e a linha selecionada
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView1.Rows[index];

            //Chama a rotina para ler o bat e mostrar no grid2...
            int indnom = GetColumnIndexByName(GridView1, "UniqueId");

            try
            {
                if (e.CommandName == "Del")
                {
                    DelUniqueId(row.Cells[indnom].Text.Trim());
                    //
                    Response.Redirect(Request.RawUrl);
                }
                else if (e.CommandName == "View")
                {
                    //Preenche os campos com a linha selecionada...
                    txt_actaoenddate.Text = row.Cells[GetColumnIndexByName(GridView1, "actaoenddate")].Text.Trim();
                    txt_actaoreceivedate.Text = row.Cells[GetColumnIndexByName(GridView1, "actaoreceivedate")].Text.Trim();
                    txt_actaostartdate.Text = row.Cells[GetColumnIndexByName(GridView1, "actaostartdate")].Text.Trim();
                    txt_colsitenum.Text = row.Cells[GetColumnIndexByName(GridView1, "colsitenum")].Text.Trim();
                    txt_comment.Text = row.Cells[GetColumnIndexByName(GridView1, "comment")].Text.Trim();
                    txt_duname.Text = row.Cells[GetColumnIndexByName(GridView1, "duname")].Text.Trim();
                    txt_durationworkdays.Text = row.Cells[GetColumnIndexByName(GridView1, "durationworkdays")].Text.Trim();
                    txt_instanceid.Text = row.Cells[GetColumnIndexByName(GridView1, "instanceid")].Text.Trim();
                    txt_operatorname.Text = row.Cells[GetColumnIndexByName(GridView1, "operatorname")].Text.Trim();
                    txt_planaoenddate.Text = row.Cells[GetColumnIndexByName(GridView1, "planaoenddate")].Text.Trim();
                    txt_planaoreceivedate.Text = row.Cells[GetColumnIndexByName(GridView1, "planaoreceivedate")].Text.Trim();
                    txt_planaostartdate.Text = row.Cells[GetColumnIndexByName(GridView1, "planaostartdate")].Text.Trim();
                    txt_status.Text = row.Cells[GetColumnIndexByName(GridView1, "status")].Text.Trim();
                    txt_uniqueid.Text = row.Cells[GetColumnIndexByName(GridView1, "uniqueid")].Text.Trim();
                    txt_volumegb.Text = row.Cells[GetColumnIndexByName(GridView1, "volumegb")].Text.Trim();

                    //Verify the checks fields...
                    if (row.Cells[GetColumnIndexByName(GridView1, "dataexpctrchk")].Text.Trim() == "1")
                        chk_dataexpctrchk.Checked = true;
                    else
                        chk_dataexpctrchk.Checked = false;
                    //
                    if (row.Cells[GetColumnIndexByName(GridView1, "waiverchk")].Text.Trim() == "1")
                        chk_waiverchk.Checked = true;
                        
                    else
                        chk_waiverchk.Checked = false;
                }
            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + "  </span>";
            }
        }


        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                //If User Adm, Enable button...
                //if (currentUser != null)
                //{
                //if ((string)(Session["permissoes"]) == "9")
                //{
                    ((ImageButton)e.Row.Cells[1].Controls[1]).OnClientClick = "return confirm('Are you sure you want to delete this Unique Id?');";
                //}
                //else
                //{
                //    ((ImageButton)e.Row.Cells[1].Controls[1]).Visible = false;
                //}
                //}
            }
        }


        protected string DelUniqueId(string UniqueId)
        {
            try
            {
                // Create a PetaPoco database object
                var db = new PetaPoco.Database("Shell_AutoArchDBConnectionString");

                // Get a record
                var a = db.SingleOrDefault<planning>("SELECT * FROM shell_Planning WHERE UniqueId=@0", UniqueId);

                //delete
                db.Delete(a);

                return "OK";
            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + "  </span>";
                return "Error";
            }
        }


        protected void btn_Save_Click(object sender, EventArgs e)
        {
            SaveNewUpd(false);
        }

        protected void SaveNewUpd(bool flgChkAuto)
        {
            bool flgNew = false;
            bool flgValid = false;

            try
            {
                //Validate first...
                flgValid = validateFields();
                //
                if (!flgValid)
                    return;

                // Create a PetaPoco database object
                var db = new PetaPoco.Database("Shell_AutoArchDBConnectionString");

                // Get a record
                var a = db.SingleOrDefault<planning>("SELECT * FROM shell_Planning WHERE UniqueId=@0", txt_uniqueid.Text);

                //Test if record exists...
                if (null == a)
                {
                    a = new planning();
                    flgNew = true;
                }
                //if update, check if status is not equal to CVR... or Closed...
                else if (flgChkAuto)
                {
                    if (txt_status.Text.IndexOf("Archival Order Closed") == -1)
                    {
                        if ((a.Status.IndexOf("Rejected") >= 0) || (a.Status.IndexOf("CVR Created") >= 0)) //Rejected
                        {
                            ClrFields(false);
                            return;
                        }
                    }
                }

                // Change it
                //
                if (txt_actaoenddate.Text != a.ActAOEndDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.ActAOEndDate = DateTime.Parse(txt_actaoenddate.Text);
                //
                if (txt_actaoreceivedate.Text != a.ActAOReceiveDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.ActAOReceiveDate = DateTime.Parse(txt_actaoreceivedate.Text);
                //
                if (txt_actaostartdate.Text != a.ActAOStartDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.ActAOStartDate = DateTime.Parse(txt_actaostartdate.Text);
                //
                if (txt_colsitenum.Text != "")
                    if (Convert.ToInt16(txt_colsitenum.Text) != a.ColSiteNum)
                        a.ColSiteNum = Convert.ToInt16(txt_colsitenum.Text);
                //
                if (txt_comment.Text != a.Comment)
                    a.Comment = txt_comment.Text;
                //
                if (chk_dataexpctrchk.Checked)
                    a.DataExpCtrChk = 1;
                else
                    a.DataExpCtrChk = 0;
                //
                if (txt_duname.Text != a.DUName)
                    a.DUName = txt_duname.Text;
                //
                //
                if ((txt_actaostartdate.Text != "") && (txt_actaoenddate.Text != ""))
                {
                    DateTime dtf = DateTime.Parse(txt_actaostartdate.Text);
                    DateTime dtt = DateTime.Parse(txt_actaoenddate.Text);
                    a.DurationWorkDays = GetWorkingDays(dtf, dtt);
                }
                else if (Convert.ToInt16(txt_durationworkdays.Text) != a.DurationWorkDays)
                {
                    a.DurationWorkDays = Convert.ToInt16(txt_durationworkdays.Text);
                }
                //
                //
                if (txt_operatorname.Text != a.OperatorName)
                    a.OperatorName = txt_operatorname.Text;
                //
                if (txt_planaoenddate.Text != a.PlanAOEndDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.PlanAOEndDate = DateTime.Parse(txt_planaoenddate.Text);
                //
                if (txt_planaoreceivedate.Text != a.PlanAOReceiveDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.PlanAOReceiveDate = DateTime.Parse(txt_planaoreceivedate.Text);
                //
                if (txt_planaostartdate.Text != a.PlanAOStartDate.ToString("yyyy-MM-dd HH:mm:ss.ttt"))
                    a.PlanAOStartDate = DateTime.Parse(txt_planaostartdate.Text);
                //
                if (txt_status.Text != a.Status)
                    a.Status = txt_status.Text;
                //
                if (txt_instanceid.Text != a.InstanceId)
                    a.InstanceId = txt_instanceid.Text;
                //
                if ((float)Convert.ToDouble(txt_volumegb.Text) != a.VolumeGB)
                {
                    a.VolumeGB = (float)Convert.ToDouble(txt_volumegb.Text);
                }
                //
                if (chk_waiverchk.Checked)
                    a.WaiverChk = 1;
                else
                    a.WaiverChk = 0;
                //

                string msg = "Archiving Order updated! - ";
                if (flgNew)
                {
                    a.UniqueId = txt_uniqueid.Text;
                    msg = "New Archiving Order inserted! - ";
                    db.Save(a);
                }
                else { db.Update(a); }

                if (flgChkAuto)
                {
                    divMessage.InnerHtml += "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + msg + txt_uniqueid.Text + "  </span><br>";
                }
                else
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + msg + txt_uniqueid.Text + "  </span>";
            }
            catch (Exception eg)
            {
                if (flgChkAuto)
                    divMessage.InnerHtml += "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + "  </span>";
                else
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Error Message: " + eg.Message + "  </span>";
            }
        }



        protected void btn_Clean_Click(object sender, EventArgs e)
        {
            ClrFields(true);
        }


        protected void btn_Filter_Click(object sender, EventArgs e)
        {
            loadDBTabData("shell_Planning", "*", "ActAOReceiveDate DESC, UniqueId DESC", "", GridView1, null);
        }


        public static string FindSelList(string searchString, ListBox lst)
        {
            string aux;
            bool flgfind = false;           

            for (int i = 0; i < lst.Items.Count; i++)
            {
                aux = lst.Items[i].ToString();
                aux = aux.Substring(0, aux.IndexOf(".xls"));
                aux = aux.Replace("_", "").Replace("-", "");

                if (aux.IndexOf(searchString, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    lst.SelectedIndex = i;
                    flgfind = true;
                }
            }

            if (!flgfind)
            {
                return "Error Message: The uniqueid (" + searchString + ") selected hasn't the Archiving Request Plan, please Upload before continue...  </span>";
            }
            else
                return "";
        }


        public static void loadFolderData(string SrvPath, string typFile, ListBox lst)
        {
            DirectoryInfo dinfo = new DirectoryInfo(ConfigurationManager.AppSettings[SrvPath]);
            FileInfo[] Files = dinfo.GetFiles(typFile).OrderByDescending(p => p.LastWriteTime).ToArray();
            //
            foreach (FileInfo file in Files) 
            {
                lst.Items.Add(file.Name + " | " + file.LastWriteTime.ToString() + " | " + (file.Length / 1024) + " KB"); 
            }
        }


        public static void loadDBTabData(string Table, string Fields, string OrderBy, string Where, GridView grd, ListBox lst)
        {
            // Create a PetaPoco database object
            var db = new PetaPoco.Database("Shell_AutoArchDBConnectionString");

            //------------------------------------------------------------
            string v_fields = "*";
            string v_orderby = "";
            //
            if (Fields != "")       { v_fields = Fields; }
            if (OrderBy != "")      { v_orderby = OrderBy; }
            if (Where != "") 
            {
                g_WHERE = new StringBuilder(Where); 
            }
            else 
            { 
                if (null == g_WHERE) { g_WHERE = new StringBuilder("WHERE 1 = 1"); } 
            }
            //
            string v_tabname = "SELECT " + v_fields + " FROM " + Table;
            string v_order = "";
            if (v_orderby != "")
                v_order = "ORDER BY " + v_orderby;
            //

            //------------------------------------------------------------
            // Use here g_WHERE because generic functions...
            //------------------------------------------------------------

            var sql = PetaPoco.Sql.Builder
                .Append(v_tabname + "\n" + g_WHERE + "\n" + v_order);
            //
            try
            {
                //Search the right table and execute Query...
                if (Table == "shell_Planning")
                {
                    var q = db.Query<planning>(sql);
                    if (null != grd)
                    {
                        grd.DataSource = q;
                        grd.DataBind();
                    }
                    else
                    {
                        //Create List and Show Only uniqueid Field... Obs. this field must be into param...
                        List<string> list = new List<string>();
                        foreach (var item in q) { list.Add(item.UniqueId.Trim()); }
                        //
                        lst.DataSource = list;
                        lst.DataBind();
                    }
                }
                else if (Table == "shell_LAPIconf")
                {
                    var q = db.Query<lapicfg>(sql);
                    if (null != grd)
                    {
                        grd.DataSource = q;
                        grd.DataBind();
                    }
                    else
                    {
                        //Create List and Show Only uniqueid Field... Obs. this field must be into param...
                        List<string> list = new List<string>();
                        foreach (var item in q) { list.Add(item.id_instance.Trim()); }
                        //
                        lst.DataSource = list;
                        lst.DataBind();
                    }
                }
                else
                {
                    var q = db.Query<sysid_istid>(sql);
                    if (null != grd)
                    {
                        grd.DataSource = q;
                        grd.DataBind();
                    }
                    else
                    {
                        //Create List and Show Only uniqueid Field... Obs. this field must be into param...
                        List<string> list = new List<string>();
                        foreach (var item in q) { list.Add(item.id_instance.Trim()); }
                        //
                        lst.DataSource = list;
                        lst.DataBind();
                    }
                }
                //
            }
            catch (Exception eg)
            {
                string exmsg = eg.Message;
            }

        }


        private bool validateFloat(TextBox txt)
        {
            try
            {
                float t = float.Parse(txt.Text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private bool validateInt(TextBox txt)
        {
            try
            {
                int t = int.Parse(txt.Text);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private bool valid_DateTime(TextBox txt)
        {
            try
            {
                if ( (txt.Text == "01/01/0001 00:00:00") || (txt.Text == "") )
                {
                    txt.Text = "9999-01-01 00:00:00.000";
                }
                else
                {
                    DateTime t = DateTime.Parse(txt.Text);
                    String result = t.ToString("yyyy-MM-dd HH:mm:ss.ttt", System.Globalization.CultureInfo.InvariantCulture);
                    txt.Text = result;
                }
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private bool valid_int(TextBox txt)
        {
            try
            {
                int t = int.Parse(txt.Text);
                String result = t.ToString();
                txt.Text = result;
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }


        private bool validateFields()
        {
            //Validate string fields that cannot be empty...
            if (txt_operatorname.Text == "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Empty Value, this field is Mandatory! (" + lbl1_2.Text + ") - " + txt_operatorname.Text + " (Please fill and try again!)  </span>";
                return false;
            }

            if (txt_uniqueid.Text == "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Empty Value, this field is Mandatory! (" + lbl2_1.Text + ") - " + txt_uniqueid.Text + " (Please fill and try again!)  </span>";
                return false;
            }

            if (txt_instanceid.Text == "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Empty Value, this field is Mandatory! (" + lbl2_2.Text + ") - " + txt_instanceid.Text + " (Please fill and try again!)  </span>";
                return false;
            }

            if (txt_duname.Text == "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Empty Value, this field is Mandatory! (" + lbl3_1.Text + ") - " + txt_duname.Text + " (Please fill and try again!)  </span>";
                return false;
            }

            if (txt_status.Text == "")
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Empty Value, this field is Mandatory! (" + lbl8_1.Text + ") - " + txt_status.Text + " (Please fill and try again!)  </span>";
                return false;
            }

            //Validate int Field...
            float n;
            if (!(float.TryParse(txt_volumegb.Text, out n)))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Number Format (" + lbl4_3.Text + ") - " + txt_volumegb.Text + " (Please correct and try again!)  </span>";
                return false;
            }

            int d;
            //if (! (int.TryParse(txt_durationworkdays.Text, out d)) )
            //{
            //    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Number Format (" + lbl4_4.Text + ") - " + txt_durationworkdays.Text + " (Please correct and try again!)  </span>";
            //    return false;
            //}

            //Test txt_colsitenum if is not empty...
            if (txt_colsitenum.Text != "")
            {
                if (!(int.TryParse(txt_colsitenum.Text, out d)))
                {
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Number Format (" + lbl1_1.Text + ") - " + txt_colsitenum.Text + " (Please correct and try again!)  </span>";
                    return false;
                }
            }

            //txt_planaoreceivedate
            if (!valid_DateTime(txt_planaoreceivedate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl5_1.Text + ") - " + txt_planaoreceivedate.Text + " (Please correct and try again!)  </span>";
                return false;
            }
            
            //txt_actaoreceivedate
            if (!valid_DateTime(txt_actaoreceivedate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl5_2.Text + ") - " + txt_actaoreceivedate.Text + " (Please correct and try again!)  </span>";
                return false;
            }

            //txt_planaostartdate
            if (!valid_DateTime(txt_planaostartdate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl6_1.Text + ") - " + txt_planaostartdate.Text + " (Please correct and try again!)  </span>";
                return false;            
            }

            //txt_actaostartdate
            if (!valid_DateTime(txt_actaostartdate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl6_2.Text + ") - " + txt_actaostartdate.Text + " (Please correct and try again!)  </span>";
                return false;
            }

            //txt_planaoenddate
            if (!valid_DateTime(txt_planaoenddate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl7_1.Text + ") - " + txt_planaoenddate.Text + " (Please correct and try again!)  </span>";
                return false;
            }

            //txt_actaoenddate
            if (!valid_DateTime(txt_actaoenddate))
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Invalid Date Format or Empty Value (" + lbl7_2.Text + ") - " + txt_actaoenddate.Text + " (Please correct and try again!)  </span>";
                return false;
            }

            return true;
        }


        protected void imgCheckSource_Click(object sender, ImageClickEventArgs e)
        {
            string scrText = "";
            scrText = scrText + "var Mleft = (screen.width/2)-(800/2);var Mtop = (screen.height/2)-(600/2);window.open( 'Util_list.aspx?SourceName=" + txt_instanceid.Text + "', null, 'height=600,width=800,status=yes,toolbar=no,scrollbars=yes,menubar=no,location=no,top=\'+Mtop+\', left=\'+Mleft+\'' );";
            // open a pop up window at the center of the page.
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", scrText, true);
        }


        protected void btnExport_Click(object sender, EventArgs e)
        {
            //Update fields of screen...
            UpdFields();

            string scrText = "";
            scrText = scrText + "var Mleft = (screen.width/2)-(800/2);var Mtop = (screen.height/2)-(600/2);window.open( 'Util_list.aspx?', null, 'height=600,width=800,status=yes,toolbar=no,scrollbars=yes,menubar=no,location=no,top=\'+Mtop+\', left=\'+Mleft+\'' );";
            // open a pop up window at the center of the page.
            ScriptManager.RegisterStartupScript(this, typeof(string), "OPEN_WINDOW", scrText, true);
        }


        public static void showLogTv(TreeView tv, string log, string tit)
        {
            tv.Visible = true;
            tv.Nodes.Clear();
            TreeNode raiz = new TreeNode("___________________________" + tit + "________________________________");
            raiz.SelectAction = TreeNodeSelectAction.Expand;
            tv.Nodes.Add(raiz);
            string[] vetlin = { "" };
            //
            TreeNode logini = null;

            foreach (string linha in log.Split('\n'))
            {
                if (((linha.IndexOf("|Info|") >= 0) &&
                         (linha.IndexOf("Start") >= 0)) || (linha.IndexOf("/***") >= 0))
                {
                    logini = new TreeNode(linha);
                    logini.SelectAction = TreeNodeSelectAction.Expand;
                    raiz.ChildNodes.Add(logini);
                }
                else
                {
                    if ((linha.IndexOf("Info|") >= 0) ||
                            (linha.IndexOf("?|") >= 0) ||
                            (linha.IndexOf("!|") >= 0) ||
                            (linha.ToUpper().IndexOf("SELECT") >= 0) ||
                            (linha.ToUpper().IndexOf("UPDATE") >= 0) ||
                            (linha.ToUpper().IndexOf("DELETE") >= 0) ||
                            (linha.ToUpper().IndexOf("INSERT") >= 0) ||
                            (linha.ToUpper().IndexOf(",") >= 0)
                       )
                    {
                        //Show the types above...
                        TreeNode filhos = new TreeNode(linha);
                        filhos.SelectAction = TreeNodeSelectAction.Expand;
                        logini.ChildNodes.Add(filhos);
                    }
                }
            }
        }


        //
        //MPS test 27/mai/2015
        //
        //-----------------------------------------------------------------------------------------------------------------------------------
        public static int lstBox_BindRefresh(bool showSel, bool ehBind, string XMLitem, ListBox _lstBox)
        {
            //Cria a Lista, carrega e destaca os ativos = 1...
            List<item> _lstData = setLista(showSel, XMLitem);
            if (ehBind)
            {
                _lstBox.DataSource = _lstData;
                _lstBox.DataValueField = "SEL";
                _lstBox.DataTextField = "VALUE";
                _lstBox.DataBind();
            }
            //
            bool ehVAR = false;
            string strVAR = "";
            string chkVAR = "";
            int contAtivos = 0;

            for (int i = 0; i < _lstBox.Items.Count; i++)
            {
                for (int j = 0; j < _lstData.Count; j++)
                {
                    strVAR = _lstData[j].VALUE;
                    ehVAR = _lstBox.Items[i].ToString().Contains(strVAR);
                    chkVAR = _lstData[j].CHK.ToString();

                    if ((ehVAR) && (chkVAR == "1"))
                    {
                        _lstBox.Items[i].Attributes.Add("style", "background-color: #90EE90");
                        contAtivos++;
                    }
                }
            }
            //Retorna o numero de Ativos...
            return contAtivos;
        }
        //-----------------------------------------------------------------------------------------------------------------------------------
        public static List<item> setLista(bool showSel, string src)
        {
            XDocument lbSrc = XDocument.Load(src);
            List<item> _lbList = new List<item>();

            foreach (XElement item in lbSrc.Descendants("item"))
            {
                if (showSel)
                {
                    _lbList.Add(new item
                    {
                        CHK = item.Element("CHK").Value,
                        SEL = item.Element("SEL").Value,
                        VALUE = item.Element("VALUE").Value + " " + item.Element("SEL").Value
                    });
                }
                else
                {
                    _lbList.Add(new item
                    {
                        CHK = item.Element("CHK").Value,
                        SEL = item.Element("SEL").Value,
                        VALUE = item.Element("VALUE").Value
                    });
                }
            }

            return _lbList;
        }
    }
}