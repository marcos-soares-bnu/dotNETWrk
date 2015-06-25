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
//

namespace ECM_Shell_AutoArch
{
    public partial class ScheduleTasks : System.Web.UI.Page
    {
        public MembershipUser currentUser;
        public static MPSfwk.Model.Audits aud;
        public static string XMLATcmds = ConfigurationManager.AppSettings["SrvFolderSTasks"] + "\\TaskList.xml";
        public static byte[] bytes;


        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //Disable Initial buttons...
                SetInitFields();

                //Chama a rotina para gerar a lista de Cmds e destacar os ativos...
                ECM_Shell_AutoArch.Planning.lstBox_BindRefresh(false, true, XMLATcmds, lstATcmds);
                //
                FillGridTaskList(GridView1);
                //
            }
            else
            {
                int indSel = lstATcmds.SelectedIndex;
                //Chama a rotina para gerar a lista de Cmds e destacar os ativos...
                //DS_AuditXML.Util_list.lstBox_BindRefresh(false, true, XMLATcmds, lstATcmds);
                lstATcmds.SelectedIndex = indSel;
            }

        } //OK MPS - 06/10/2014


        //---MPS02062015-----------------------------------------------------------------
        protected void SetInitFields()
        {
            //Pega o usuário logado...
            currentUser = Membership.GetUser();

            if ((currentUser.UserName.ToUpper().IndexOf("F0FP186") >= 0) ||
                    (currentUser.UserName.ToUpper().IndexOf("MSOARES") >= 0) ||
                    (currentUser.UserName.ToUpper().IndexOf("UMPSOAR") >= 0) ||
                    (currentUser.UserName.ToUpper().IndexOf("SHELL") >= 0)
                )
            {
                Button1.Text = "Atualiza - " + "AT_" + currentUser.UserName.Replace("\\", "_") + ".txt" + " !";
                Button1.Visible = true;
                Button2.Text = "LIST_USERS";
                Button2.Visible = true;
            }

            panAviso.Visible = true;
            dtSel1.Visible = false;
            dtSel2.Visible = false;
            dtSel3.Visible = false;
            divMessage.InnerHtml = "";
            txtHoraIni.Text = DateTime.Now.ToString("yyyyMMddHHmm");
            txtHH.Text = DateTime.Now.ToString("HH");
            txtmm.Text = DateTime.Now.ToString("mm");

            //Visualiza a Lista de Tasks...
            dtSel1.Text = "Schedule Tasks List";
            dtSel1.Visible = true;
            GridView1.Visible = true;
        }


        protected void imbRefresh_Click(object sender, ImageClickEventArgs e)
        {
            //
            FillGridTaskList(GridView1);
            //
        }


        public void FillGridTaskList(GridView grd)
        {
            //find in Datatable of Schedule Tasks...
            DataTable dtTask = MPSfwk.WMI.dtScheduleTaskList("Shell_AutoArch");
            //
            grd.Visible = true;
            grd.DataSource = dtTask;
            grd.DataBind();
        }
        //---MPS02062015-----------------------------------------------------------------


        protected string readfile_Click(string txtfile)
        {
            //Preenche a tabela com o conteudo do BAT...
            DataTable table = new DataTable();
            table.Columns.Add("Conteudo");
            string lin = "";
            string logfile = "";

            try
            {
                using (StreamReader sr = new StreamReader(txtfile))
                {
                    while (!sr.EndOfStream)
                    {
                        lin = sr.ReadLine();
                        table.Rows.Add(lin);
                        if (lin.IndexOf("> ") > 0)
                        {
                            logfile = lin.Substring(lin.IndexOf("> ") + 2);
                        }
                    }
                }
                GridView2.DataSource = table;
                GridView2.DataBind();
                return logfile;
            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Ocorreu um erro: " + eg.Message + "  </span>";
                return null;
            }

        } //OK MPS - 06/10/2014

        public string getDayWeek() //Int32
        {
            //Int32 sum = 0;
            string week = "";
            chkSemana.ToolTip = "";
            for (int i = 0; i < chkSemana.Items.Count; i++)
            {
                if (chkSemana.Items[i].Selected)
                {
                    week = week + chkSemana.Items[i].Value + ",";
                    //sum += Convert.ToInt32(chkSemana.Items[i].Value); 
                }
            }
            if (week != "")
            {
                chkSemana.ToolTip = "/SC WEEKLY /D " + week.Substring(0, (week.Length - 1));
            }
            return chkSemana.ToolTip;
            //return sum;

        } //OK MPS - 06/10/2014

        public string getDayMonth() //Int32
        {
            if (chkSemana.ToolTip == "")
            {
                string mont = "";
                //Int32 sum = 0;
                for (int i = 0; i < chkDiaMes.Items.Count; i++)
                {
                    if (chkDiaMes.Items[i].Selected)
                    {
                        mont = mont + chkDiaMes.Items[i].Text + "-";
                        //sum += Convert.ToInt32(chkDiaMes.Items[i].Value); 
                    }
                }
                if (mont != "")
                {
                    chkDiaMes.ToolTip = "/SC MONTHLY /D " + mont.Substring(0, mont.Length - 1);
                }
                //return sum;
            }
            return chkDiaMes.ToolTip;

        } //OK MPS - 06/10/2014

        protected string GeraBatAT(bool sohUpdArqUser)
        {
            try
            {
                //Pega o usuário logado...
                currentUser = Membership.GetUser();

                //Pega a chave de Criptografia...
                var Section = ConfigurationManager.GetSection("system.web/machineKey");
                string aux = ((System.Web.Configuration.MachineKeySection)(Section)).ValidationKey.ToString();
                bytes = ASCIIEncoding.ASCII.GetBytes(aux.Substring(0, 8));

                string pathCMD = ConfigurationManager.AppSettings["SrvFolderSTasks"] + "\\";
                string nomeCMD = "";

                //MPS - 21/OUT---------------------------------------------
                //Testa se existe parametros nos cmds...
                if (lstATcmds.SelectedValue.ToString().IndexOf("[p1]") > 0)
                {
                    string findParam = "";
                    string findParamConv = "";
                    string[] stringSeparators = new string[] { "[", "]" };
                    string[] vetParam = lstATcmds.SelectedValue.ToString().Split(stringSeparators, StringSplitOptions.None);
                    string[] vetParamConc = txtParams.Text.Split(stringSeparators, StringSplitOptions.None);
                    if (vetParam.Length == vetParamConc.Length)
                    {
                        foreach (string s in vetParam)
                        {
                            if (s.IndexOf(">>") == -1)
                            {
                                if (s.ToUpper().IndexOf("P") >= 0)
                                { findParam = findParam + "[" + s + "] "; }
                            }
                        }
                        foreach (string s in vetParamConc)
                        {
                            if ((s != " ") && (s != ""))
                            { findParamConv = findParamConv + s + " "; }
                        }
                    }
                    else
                    { return "Erro"; }

                    nomeCMD = lstATcmds.SelectedValue.ToString().Replace(findParam, findParamConv);
                }
                else
                { nomeCMD = lstATcmds.SelectedValue.ToString(); }
                //
                //MPS - 21/OUT---------------------------------------------

                string param = currentUser.UserName.Replace("\\", "_");
                param += " ";
                param += currentUser.GetPassword();

                string cryptedparam = ""; //DS_AuditXML.Util_list.Encrypt(param, bytes);

                //Grava param criptografado em arquivo...
                string batCMD = pathCMD + "AT_" + currentUser.UserName.Replace("\\", "_") + "_" + txtHoraIni.Text + DateTime.Now.Second + ".bat";
                string arqParam = pathCMD + "AT_" + currentUser.UserName.Replace("\\", "_") + ".txt";

                //Cria/Atualiza o Arquivo de usuario para utilizar no app BATCH...
                using (StreamWriter writer = new StreamWriter(arqParam, false))
                { writer.WriteLine(cryptedparam); }

                if (!sohUpdArqUser)
                {
                    //Cria um Arquivo BAt na pasta das ATs para executar via AT...
                    using (StreamWriter writer = new StreamWriter(batCMD, false))
                    {
                        writer.WriteLine("cd " + pathCMD.Replace("Tasks",""));
                        writer.WriteLine(nomeCMD);
                    }
                    return batCMD;
                }
                else
                {
                    return arqParam;
                }
            }
            catch (Exception ex)
            {
                return "Ocorreu um erro: " + ex.Message;
            }

        } //OK MPS - 21/10/2014

        protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            //Pega o usuário logado...
            MembershipUser currentUser = Membership.GetUser();

            //Pega o indice e a linha selecionada
            int index = Convert.ToInt32(e.CommandArgument);
            GridViewRow row = GridView1.Rows[index];

            //Chama a rotina para ler o bat e mostrar no grid2...
            int indnom = GetColumnIndexByName(GridView1, "Name");
            string nomSel = row.Cells[indnom].Text;
            int posSp1 = nomSel.IndexOf(" (");
            int posSp2 = nomSel.IndexOf("C:");
            string batSel = nomSel.Substring(posSp2);
            nomSel = nomSel.Substring(0, posSp1);
            batSel = batSel.Substring(0, batSel.IndexOf(" )"));
            string logbat = readfile_Click(batSel);
            string cmd = "";
            int ret = 0;
            //
            dtSel2.Visible = false;
            dtSel3.Visible = false;
            GridView2.Visible = false;
            TreeView1.Visible = false;

            try
            {
                if (e.CommandName == "Del")
                {
                    //Chama a função para o Executar a Tarefa...
                    cmd = "schtasks /delete /TN \"" + nomSel + "\" /F ";
                    ret = MPSfwk.WMI.ExecuteCommand(cmd, 5000, "");
                    //
                    if (File.Exists(batSel)) { File.Delete(batSel); }
                    //
                    //ListTasks(dtSel1, GridView1, true, "");
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Deletou - " + nomSel + " com sucesso! </span>";
                }
                else if (e.CommandName == "Run")
                {
                    //Chama a função para o Executar a Tarefa...
                    cmd = "schtasks /run /TN \"" + nomSel + "\" ";
                    ret = MPSfwk.WMI.ExecuteCommand(cmd, 5000, "");
                    //
                    //ListTasks(dtSel1, GridView1, true, "");
                    divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Executou - " + nomSel + " com sucesso! </span>";
                }
                else if (e.CommandName == "View")
                {
                    //
                    dtSel2.Visible = true;
                    dtSel3.Visible = true;
                    GridView2.Visible = true;

                    //Chama a rotina para gerar a lista de Servidores/Classes/Cmds e destacar os ativos...
                    dtSel2.Text = "[" + batSel + "]";

                    //Visualiza os Logs do Sistema...(Treeview)
                    dtSel3.Text = "[" + logbat + "]";
                    string aux = File.ReadAllText(logbat);
                    ECM_Shell_AutoArch.Planning.showLogTv(TreeView1, aux, "Schedule Task History Content");
                }
            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Ocorreu um erro: " + nomSel + " | " + batSel + " | " + cmd + " | " + " | " + ret + " | " + eg.Message + "  </span>";
            }

        } //OK MPS - 21/10/2014

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

        } //OK MPS - 09/10/2014

        protected void lnkHelp_Click(object sender, EventArgs e)
        {
            if (dtSel3.Visible == true)
            {
                lnkHelp.Text = "(Ver)?";
                dtSel1.Visible = true;
                GridView1.Visible = true;
                dtSel3.Visible = false;
                TreeView1.Visible = false;
            }
            else
            {
                lnkHelp.Text = "(Ocultar)?";
                dtSel1.Visible = false;
                GridView1.Visible = false;
                dtSel2.Visible = false;
                GridView2.Visible = false;
                //Visualiza os Logs do Sistema...(Treeview)
                string logbat = ConfigurationManager.AppSettings["XMLData"] + "ATs\\ds_ajuda-H.log";
                if (File.Exists(logbat))
                {
                    dtSel3.Text = "[" + logbat + "]";
                    dtSel3.Visible = true;
                    //showLogTv(logbat);
                }
            }

        } //OK MPS - 21/10/2014

        protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                ((ImageButton)e.Row.Cells[2].Controls[1]).OnClientClick = "return confirm('Are you sure you want to delete this Task?');";
            }

        } //OK MPS - 21/10/2014


        protected void imbAddVazio_Click(object sender, ImageClickEventArgs e)
        {
            try
            {
                //Pega o user local\senha para criar a task...
                MembershipUser userTasks = Membership.GetUser("shell_autoarch", false);
                string passTasks = userTasks.GetPassword();

                string cmd = "";
                string nomBAT = GeraBatAT(false);
                string nomTSK = lstATcmds.SelectedItem.ToString() + " - " + txtHoraIni.Text + DateTime.Now.Second;
                string HHmm = txtHoraIni.Text.Substring(8, 2) + ":" + txtHoraIni.Text.Substring(10, 2);
                if (chkRepetir.Checked)
                {
                    cmd = "schtasks /Create " + getDayWeek() + getDayMonth() +
                                            " /RU " + userTasks.UserName + " /RP " + passTasks +
                                            " /TN  \"" + nomTSK + "\" " +
                                            " /TR  \"" + nomBAT + "\" " +
                                            " /ST " + HHmm + " ";
                }
                else
                {
                    cmd = "schtasks /Create   /SC ONCE " +
                                            " /RU " + userTasks.UserName + " /RP " + passTasks +
                                            " /TN  \"" + nomTSK + "\" " +
                                            " /TR  \"" + nomBAT + "\" " +
                                            " /ST " + HHmm + " ";
                }

                //Chama a função para o Exutar a Tarefa...
                int ret = MPSfwk.WMI.ExecuteCommand(cmd, 3000, "");

                //ListTasks(dtSel1, GridView1, true, "");
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + " Nova Tarefa - " + nomTSK + " criada com sucesso!  </span>";
            }
            catch (Exception eg)
            {
                divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'>" + "Ocorreu um erro: " + eg.Message + "  </span>";
            }

        } //OK MPS - 21/10/2014


        protected void Button2_Click(object sender, EventArgs e)
        {
            dtSel1.Visible = false;
            GridView1.Visible = false;
            dtSel3.Visible = false;
            TreeView1.Visible = false;
            //
            GridView2.DataSource = null;
            GridView2.DataBind();
            //
            dtSel2.Text = Button1.Text;
            GridView2.DataSource = Membership.GetAllUsers();
            GridView2.DataBind();
            GridView2.Visible = true;
            dtSel2.Visible = true;

        } //OK MPS - 23/10/2014


        protected void Button1_Click(object sender, EventArgs e)
        {
            string msg = GeraBatAT(true);
            divMessage.InnerHtml = "<span id='msg' style='color:#FF3300;font-size:Smaller;font-weight:bold;'> Arquivo - " + msg + " atualizado!  </span>";

        } //OK MPS - 21/10/2014


        protected static string ConvEncString(string enc)
        {
            char[] originalString = enc.ToCharArray();
            StringBuilder asAscii = new StringBuilder(); // store final ascii string and Unicode points
            foreach (char c in originalString)
            {
                // test if char is ascii, otherwise convert to Unicode Code Point
                int cint = Convert.ToInt32(c);
                if (cint <= 127 && cint >= 0)
                    asAscii.Append(c);
                else
                    if (cint == 65533)
                        asAscii.Append('a');
                    else
                        asAscii.Append(String.Format("{0}", cint.ToString()).Trim());
            }

            return asAscii.ToString();

        } //OK MPS - 21/10/2014

        protected void GridView3_RowCommand(object sender, GridViewCommandEventArgs e)
        {

        }

        protected void GridView3_RowDataBound(object sender, GridViewRowEventArgs e)
        {

        }


    
    }
}