﻿using System;
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
using PetaPoco;

namespace ECM_Shell_AutoArch
{
    public partial class item
    {
        public object CHK { get; set; }
        public string SEL { get; set; }
        public string VALUE { get; set; }
    }


    public partial class Util_list : System.Web.UI.Page
    {

        public MembershipUser   currentUser;
        public static           MPSfwk.Model.Audits aud;
        public string           pathxml     = ConfigurationManager.AppSettings["XMLData"];
        public static string    XMLATcmds   = ConfigurationManager.AppSettings["XMLData"] + "ListATcmds.xml";
        public static string    ServerAT    = ConfigurationManager.AppSettings["ServerAT"];
        public string           flgComp;
        public string           strTipo;
        public string           xmlFile1;
        public string           xmlFile2;
        //
        public string           SourceName;

        protected void Page_Load(object sender, EventArgs e)
        {
            flgComp = Request.QueryString["flgComp"];
            strTipo = Request.QueryString["rptTipo"];
            xmlFile1 = Request.QueryString["xmlFile1"];
            xmlFile2 = Request.QueryString["xmlFile2"];
            //
            SourceName = Request.QueryString["SourceName"];

            if (!IsPostBack)
            {
                if (SourceName != null)
                {
                    //
                    Button1.Visible = true;
                    Button1.Enabled = false;
                    GridView1.Visible = true;
                    Button1.Text = "Shell LAPI Configuration";

                    //Load Data with the Source Name...
                    string aux_where = "WHERE id_instance like '%" + SourceName + "%' ";
                    ECM_Shell_AutoArch.Planning.loadDBTabData("shell_LAPIconf", "*", "id_instance", aux_where, GridView1, null);
                    //
                    Button2.Visible = true;
                    Button2.Enabled = false;
                    GridView2.Visible = true;
                    Button2.Text = "Shell LiveLink Id";

                    //Load Data with the Source Name...
                    aux_where = "WHERE id_instance like '%" + SourceName + "%' ";
                    ECM_Shell_AutoArch.Planning.loadDBTabData("shell_sourcesys_x_inst", "*", "id_instance", aux_where, GridView2, null);
                    //
                    Label1.Visible = false;
                    Label2.Visible = false;
                    Label3.Visible = false;
                    Label4.Visible = false;

                }
                else
                {
                    //
                    Button1.Visible = true;
                    Button1.Enabled = true;
                    GridView1.Visible = true;
                    Button1.Text = "Export to Excel File...";
                    //
                    Label1.Visible = false;
                    Label2.Visible = false;
                    Label3.Visible = false;
                    Label4.Visible = false;
                    //
                    ECM_Shell_AutoArch.Planning.loadDBTabData("shell_Planning", "*", "", "", GridView1, null);
                }



                //if (flgComp == null)
                //{
                //    //MPS OK - 26/11/2014
                //    currentUser = Membership.GetUser("ds_auditxml_tasks");
                //    //
                //    Button1.Visible = false;
                //    Button2.Visible = false;
                //    GridView2.Visible = false;
                //    Label4.Text = "";
                //    //
                //    string[] vet_aud    = Request.QueryString[0].Replace("@", " ").Split(';');
                //    if (vet_aud.Count() >= 4)
                //    {
                //        Button1.Visible = true;
                //        Button1.Text = vet_aud[0] + " / " + vet_aud[1] + " / " + vet_aud[2];
                //        //Cria e carrega a lista de Dts disponiveis...
                //        aud = new MPSfwk.Model.Audits();

                //        aud.IDClasse    = vet_aud[0];
                //        aud.IDServer    = vet_aud[1];
                //        aud.IDGeracao   = vet_aud[2];
                //        //escreve o Inicial...
                //        DbXMLBindGrid(currentUser, aud, GridView1);

                //        if (vet_aud[3] == "F")
                //        {
                //            aud.IDGeracao = vet_aud[4];
                //            //
                //            Button2.Visible = true;
                //            GridView2.Visible = true;
                //            Button2.Text = vet_aud[0] + " / " + vet_aud[1] + " / " + vet_aud[4];
                //            DbXMLBindGrid(currentUser, aud, GridView2);
                //            //
                //            //Chama a rotina de comparação que destaca as linhas diferentes...
                //            compare(GridView1, GridView2, System.Drawing.Color.LightGreen, System.Drawing.Color.LightGray);
                //            Button2.Text = Button2.Text = Button2.Text + " (Mostrar Tudo)";
                //            //
                //        }
                //    }
                //    else if (vet_aud.Count() == 2)
                //    {
                //        if (vet_aud[0].IndexOf(".htm") > 0)
                //        {
                //            string HTMFile = ConfigurationManager.AppSettings["XMLData"] + vet_aud[0].Substring(vet_aud[0].IndexOf("RPTs")).Replace("/", "\\");
                //            if (File.Exists(HTMFile))
                //            {
                //                panGrids.Visible = false;
                //                //
                //                panAviso.Height = 600;
                //                panAviso.Visible = true;
                //                divMessage.InnerHtml = divMessage.InnerHtml + "<br />====================================================================================================";
                //                divMessage.InnerHtml = divMessage.InnerHtml + "<br />" + File.ReadAllText(HTMFile);
                //            }
                //        }
                //    }
                //    //
                //}
                ////
                //if (flgComp == "S")
                //{
                //    Button2.Visible = true;
                //    GridView2.Visible = true;

                //    fillGrid1();
                //    fillGrid2();
                //    compare(GridView1, GridView2, System.Drawing.Color.LightGreen, System.Drawing.Color.LightGray);
                //    getGeracao();
                //}
                //else
                //{
                //    if (flgComp == "R")
                //    {
                //        Button2.Visible = false;
                //        GridView2.Visible = false;
                //        //
                //        Button1.Text = "Exportar para EXCEL";
                //        pathxml = pathxml + "RPTs\\";
                //        fillGrid1();
                //        Label4.Text = "** Clique no botão acima, para fazer download do arquivo para EXCEL.";
                //    }
                //    else
                //    { Label4.Text = "** Clique no título acima, para atualizar a visualização do arquivo OU fazer a comparação caso visualize duas listas na tela."; }
                //    //updSharesList();
                //    Label1.Visible = false;
                //    Label2.Visible = false;
                //    Label3.Visible = false;
                //}

                //
                VerifyRenderingInServerForm(GridView1);
            }
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            if (Button1.Text.IndexOf("Excel") >= 0)
            { 
                ExportGridToExcel(); 
            }

            
            
            //if (flgComp == "S")
            //{
            //    if (ehfull.Text == "1")
            //    {
            //        setGridDiff(GridView1, System.Drawing.Color.LightGray);
            //        setGridDiff(GridView2, System.Drawing.Color.LightGray);
            //        Button1.Text = Button1.Text.Replace("(total)", "(dif.)");
            //        Button2.Text = Button2.Text.Replace("(total)", "(dif.)");
            //        ehfull.Text = "0";
            //    }
            //    else
            //    {
            //        setGridFull(GridView1);
            //        setGridFull(GridView2);
            //        Button1.Text = Button1.Text.Replace("(dif.)", "(total)");
            //        Button2.Text = Button2.Text.Replace("(dif.)", "(total)");
            //        ehfull.Text = "1";
            //    }
            //}
            //else
            //{   //updSharesList(); *** MPS 01/12/2014
            //    if (Button2.Text.IndexOf("(Mostrar Tudo)") > 0)
            //    {
            //        //Chama a rotina para mostra as diff...
            //        setGridFull(GridView1);
            //        setGridFull(GridView2);
            //        Button2.Text = Button2.Text.Replace("(Mostrar Tudo)", "(Mostrar Diferença)");
            //    }
            //    else if (Button2.Text.IndexOf("(Mostrar Diferença)") > 0)
            //    {
            //        //Chama a rotina para mostra tudo...
            //        setGridDiff(GridView1, System.Drawing.Color.LightGray);
            //        setGridDiff(GridView2, System.Drawing.Color.LightGray);
            //        Button2.Text = Button2.Text.Replace("(Mostrar Diferença)", "(Mostrar Tudo)");
            //    }
            //    else
            //    {
            //        //Chama a rotina de comparação que destaca as linhas diferentes...
            //        compare(GridView1, GridView2, System.Drawing.Color.LightGreen, System.Drawing.Color.LightGray);
            //        Button2.Text = Button2.Text = Button2.Text + " (Mostrar Tudo)";
            //    }
            //}
        }

        /// <summary>
        /// Encrypt a string.
        /// </summary>
        /// <param name="originalString">The original string.</param>
        /// <returns>The encrypted string.</returns>
        /// <exception cref="ArgumentNullException">This exception will be thrown when the original string is null or empty.</exception>
        public static string Encrypt(string originalString, byte[] bytes)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                throw new ArgumentNullException("The string which needs to be encrypted can not be null.");
            }

            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);

            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();

            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);

        } //OK MPS - 06/10/2014

        protected void getGeracao()
        {
            string[] separators = {"\\", "/", ",", ".", "!", "?", ";", ":", "_"};
            string[] lstParam1 = null;
            string[] lstParam2 = null;
            string dtHrGera1 = "";
            string dtHrGera2 = "";
            string Host1 = "";
            string Host2 = "";
            int i = 0;

            if (!String.IsNullOrEmpty(xmlFile1))
            { 
                lstParam1 = xmlFile1.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in lstParam1.Reverse())
                {
                    if (i == 1) { dtHrGera1 = s; }
                    if (i == 2) { Host1 = s; break; }
                    i++;
                }
            }

            if (!String.IsNullOrEmpty(xmlFile2))
            { 
                lstParam2 = xmlFile2.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                i = 0;
                foreach (string s in lstParam2.Reverse())
                {
                    if (i == 1) { dtHrGera2 = s; }
                    if (i == 2) { Host2 = s; break; }
                    i++;
                }
            }

            if (flgComp == "S")
            {
                Button1.Text = "Comparação (dif.) " + strTipo + " - " + Host1 + " - " + dtHrGera1;
                Button2.Text = "Comparação (dif.) " + strTipo + " - " + Host2 + " - " + dtHrGera2;
            }
            else if (flgComp == "R")
            { Button1.Text = "Exportar para EXCEL"; }
            else { Button1.Text = "Relatório " + strTipo + " - " + Host1 + " - " + dtHrGera1; }
        }

        protected void updSharesList()
        {
            getGeracao();
            fillGrid1();
        }

        public static void bindGrid(string xml, GridView grd)
        {
            DataSet xmlDataSet;
            xmlDataSet = new DataSet("DS_AuditXML - Relatório de Auditoria");
            //Read the contents of the XML file into the DataSet
            xmlDataSet.ReadXml(xml);

            if (xmlDataSet.Tables.Count != 0)
            {
                grd.DataSource = xmlDataSet.Tables[0].DefaultView;
                grd.DataBind();
            }

        } //MPS 23/10/2014

        protected void fillGrid1()
        {
            string filePath = pathxml + xmlFile1;
            bindGrid(filePath, GridView1);
        }

        protected void fillGrid2()
        {
            string filePath = pathxml + xmlFile2;
            bindGrid(filePath, GridView2);
        }




        public override void VerifyRenderingInServerForm(Control control)
        {
            //base.VerifyRenderingInServerForm(control);
        }


        //*** MPS ***********************
        private void ExportGridToExcel()
        {
            string FileName = "Planning_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xls";

            Response.Clear();
            Response.AddHeader("content-disposition", "attachment;filename=" + FileName);
            Response.ContentType = "application/vnd.xls";

            System.IO.StringWriter stringWrite = new System.IO.StringWriter();
            System.Web.UI.HtmlTextWriter htmlWrite = new HtmlTextWriter(stringWrite);

            GridView1.RenderControl(htmlWrite);
            Response.Write(stringWrite.ToString());
            Response.End();
        }
        //*** MPS ***********************


        //===================================================================================================================================
        //MPS - *** AJUSTE BUGS - 01-02/OUT ***
        //-----------------------------------------------------------------------------------------------------------------------------------
        public static void compare(GridView gridview1, GridView gridview2, System.Drawing.Color corDif, System.Drawing.Color corDup)
        {
            for (int currentRow = 0; currentRow < gridview1.Rows.Count; currentRow++)
            {
                GridViewRow rowToCompare = gridview1.Rows[currentRow];
                for (int otherRow = 0; otherRow < gridview2.Rows.Count; otherRow++)
                {
                    GridViewRow row = gridview2.Rows[otherRow];
                    bool duplicateRow = true;
                    //-------------------------------------------------------------
                    // Concatenar todas as celulas para comparar a linha inteira...
                    //
                    for (int i = 0; i < rowToCompare.Cells.Count; i++)
                    {
                        if (rowToCompare.Cells[i].Text != row.Cells[i].Text)
                        {
                            duplicateRow = false;
                            if (currentRow == otherRow)
                            {
                                rowToCompare.Cells[i].BackColor = corDif;
                                row.Cells[i].BackColor = corDif;
                            }
                        }
                    }
                    //-------------------------------------------------------------
                    if (duplicateRow)
                    {
                        rowToCompare.BackColor = corDup;
                        row.BackColor = corDup;
                        rowToCompare.Visible = false;
                        row.Visible = false;
                    }
                }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static void setGridFull(GridView grd)
        {
            for (int currentRow = 0; currentRow < grd.Rows.Count; currentRow++)
            {
                GridViewRow row = grd.Rows[currentRow];
                if (row.Visible == false) { row.Visible = true; }
            }
        }

        public static void setGridDiff(GridView grd, System.Drawing.Color corDup)
        {
            for (int currentRow = 0; currentRow < grd.Rows.Count; currentRow++)
            {
                GridViewRow row = grd.Rows[currentRow];
                if (row.BackColor == corDup) { row.Visible = false; }
            }
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        public static void DbXMLBindGrid(MembershipUser currentUser, MPSfwk.Model.Audits aud, GridView _grd)
        {
            //Busca o XML da Geração selecionada...
            XmlDocument xmlDB = null; // SqlServer.AuditXML.LerXML(aud.IDClasse,
                                      //                       aud.IDServer,
                                      //                       aud.IDGeracao);
            //
            string auxFile = ConfigurationManager.AppSettings["XMLData"] + currentUser.UserName.Replace("\\", "_") + "_tmp.xml";
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(xmlDB.InnerXml);
            xdoc.Save(auxFile);
            //
            DataSet xmlDataSet;
            xmlDataSet = new DataSet();
            xmlDataSet.ReadXml(auxFile);
            //
            if (xmlDataSet.Tables.Count == 0)
            { _grd.DataSource = null; }
            else
            { _grd.DataSource = xmlDataSet; }
            _grd.DataBind();
        }


        //-----------------------------------------------------------------------------------------------------------------------------------
        public static void setListSelONOFF(bool setSel, ListBox _lst, List<item> _lstSel)
        {
            //ListBox1.SelectionMode = ListSelectionMode.Multiple;
            for (int i = 0; i < _lst.Items.Count; i++)
            {
                foreach (var ilst in _lstSel)
                {
                    if ((_lst.Items[i].Text == ilst.VALUE) && (ilst.CHK.ToString() == "1"))
                    {
                        _lst.Items[i].Selected = setSel;
                    }
                }
            }
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
        public static string setListSel(ListBox _lst)
        {
            int posF = 0;
            string strLast = "";
            string strSel = "";
            var qry = from ListItem item in _lst.Items
                      select new { item.Selected, Texto = item.Text };

            string[] arrSel = qry.Where(x => x.Selected).Select(x => x.Texto).ToArray();

            for (int i = 0; i < arrSel.Length; i++)
            {
                //MPS 27/10/2014 - Add hostname e retirar na selecao...
                if (arrSel[i].IndexOf(" [") > 0)
                {
                    strSel = strSel + "'" + arrSel[i].Substring(0, (arrSel[i].IndexOf(" ["))) + "' , ";
                    strLast = "'" + arrSel[i].Substring(0, (arrSel[i].IndexOf(" ["))) + "'";
                    posF = strSel.IndexOf(strLast) + strLast.Length + 1;
                }
                else
                {
                    strSel = strSel + "'" + arrSel[i] + "' , ";
                    strLast = "'" + arrSel[i] + "'";
                    posF = strSel.IndexOf(strLast) + strLast.Length + 1;
                }
            }

            if (posF > 1)
            { strSel = strSel.Substring(0, (posF - 1)); }

            return strSel;
        }


        //-----------------------------------------------------------------------------------------------------------------------------------
        public static void ListaDTsGeracao(string[] ordBY, MPSfwk.Model.Audits aud, ListBox _lstSrv, ListBox _lstCls, DropDownList _drpI, DropDownList _drpF)
        {
            //limpa as drpboxs...
            _drpI.Items.Clear();
            _drpF.Items.Clear();

            List<MPSfwk.Model.Audits> DatasAudit;
            string[] arrCls0 = null;
            string[] arrCls1 = null;

            // Cria a string com os itens selecionados para filtrar no Where...
            string strHosts = setListSel(_lstSrv);
            string strClasses = setListSel(_lstCls);

            if (strHosts != "")
                aud.IDServer = strHosts;
            if (strClasses != "")
                aud.IDClasse = strClasses;

            //Cria a Lista de pesquisa no DB passando os filtros ordBY[0]...
            DatasAudit = null; // SqlServer.AuditXML.lstAudits(aud, 1, ordBY[0]);

            //Cria o array para comparação e retirada das diferenças
            var qry = (from m in DatasAudit select m).Distinct().ToList();
            arrCls0 = qry.Select(x => x.CVGeracao).ToArray();

            //Cria a Lista de pesquisa no DB passando os filtros ordBY[1]...
            DatasAudit = null; // SqlServer.AuditXML.lstAudits(aud, 1, ordBY[1]);

            //Cria o array para comparação e retirada das diferenças
            qry = (from m in DatasAudit select m).Distinct().ToList();
            arrCls1 = qry.Select(x => x.CVGeracao).ToArray();

            //Pega somente as datas diferentes..
            var inter = arrCls0.Intersect(arrCls1);
            foreach (var s in inter)
            {
                // Testa se for o mesmo componente, add apenas uma vez...
                if (_drpI.UniqueID == _drpF.UniqueID) { _drpI.Items.Add(s); }
                else
                {
                    _drpI.Items.Add(s);
                    _drpF.Items.Add(s);
                }
            }
        } //OK MPS - 01/10/2014


    }
}


//Get the Current logged user...
//currentUser = Membership.GetUser();
//
//If User Adm, Enable button...
//if (currentUser != null)
//{
//    if (Roles.Provider.IsUserInRole(currentUser.UserName, "Adm"))
//    {
//        btn_Save.Enabled = true;
//        btn_Save.ForeColor = System.Drawing.Color.White;
//    }
//}
//else
//{
//    Response.Redirect("~/Account/Login.aspx");
//}
