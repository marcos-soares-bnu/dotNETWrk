using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Data;
using System.IO;
using System.Security.Cryptography;
//
using Microsoft.Office.Interop.Word;
using Microsoft.Office.Interop.Excel;
using System.Linq;
using System.Data.OleDb;
//

namespace Shell_AutoArch_cmd
{
    /// <summary>
    /// This class provide all commands to execute all process of Shell Archiving into
    /// TEST/PRODUCTION environment.
    /// </summary>
    class Program
    {
        //=============================================================================================
        static string PathServer = System.Configuration.ConfigurationManager.AppSettings["PathServer"];
        static string PathCSV = System.Configuration.ConfigurationManager.AppSettings["PathCSV"];
        static string PathSQL = System.Configuration.ConfigurationManager.AppSettings["PathSQL"];
        static string PathCVR = System.Configuration.ConfigurationManager.AppSettings["PathCVR"];
        static string PathPST = System.Configuration.ConfigurationManager.AppSettings["PathPST"];
        //
        static string FileCSV = "";
        static string FileSQL = "";
        static string FileREQ = "";
        static StreamWriter fileSQL;
        static StreamWriter fileREQ;
        //
        static DateTime _dthrmmss = DateTime.Now;
        //
        static MPSfwk.Model.Configs cfg_plan;
        static MPSfwk.Model.Configs cfg_lapi;
        static string sysid = "";
        static string BU = "";
        //=============================================================================================


        /// <summary>
        /// Call the function of create DB with the input parameter...
        /// </summary>
        /// <param name="unique_id"></param>
        /// <returns></returns>
        static bool confDBSQLServer(string unique_id)
        {
            //Header query to INSERT config fields e create script SQL file...
            string db_name = cfg_plan.ID_INSTANCE + "_" + unique_id;
            string sql_head1 = "INSERT INTO  [" + db_name + "].[dbo].[TJOB_CONFIGURATION]  ([dbName],[value])  VALUES ";
            string sql_parms = " ('@CNAME','@CVALUE');";

            //Write SQL file to run on Shell Server...
            //Write SQL...
            fileSQL.WriteLine("/*** ==== " + "|Info| Shell_AutoArch_cmd - Configuration Script - | " + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + " ==== ***/");
            fileSQL.WriteLine("\n\n/*** ==== TRUNCATE [TJOB_CONFIGURATION] - clear all before configuration table ==== ***/");
            fileSQL.WriteLine("TRUNCATE TABLE  [" + db_name + "].[dbo].[TJOB_CONFIGURATION]");
            fileSQL.WriteLine("GO");
            fileSQL.WriteLine("\n\n/*** ==== INSERT [TJOB_CONFIGURATION] - variables of configuration table ==== ***/");
            string str_lin = "";

            foreach (var item in SqlServer.SQLUtil.ReadDefaultCfg())
            {
                str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", item.value);

                //Write SQL...
                if (item.dbname.IndexOf("DB_NAME") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", db_name); }
                else if (item.dbname.IndexOf("LL_BASE_URI") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", cfg_lapi.LL_BASE_URI); }
                else if (item.dbname.IndexOf("LL_IP") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", cfg_lapi.LL_IP); }
                else if (item.dbname.IndexOf("LL_NAME") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", sysid); }
                else if (item.dbname.IndexOf("LL_PORT") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", cfg_lapi.LL_PORT); }
                else if (item.dbname.IndexOf("myRequestId") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", unique_id); }
                else if (item.dbname.IndexOf("WORKSPACE_HOME") >= 0) { str_lin = sql_head1 + sql_parms.Replace("@CNAME", item.dbname).Replace("@CVALUE", ("G:/" + db_name)); }
                //
                fileSQL.WriteLine(str_lin);
                //
                //Console.WriteLine("|Debug| Reading fields objects - " + str_lin);
            }

            return true;
        }


        /// <summary>
        /// Read Archive Order Excel File and create CSV with values/orders to Archiving...
        /// </summary>
        /// <param name="UniqueID"></param>
        public static bool ReadWriteCSV(string UniqueID)
        {
            //Test if Path exists...
            if (File.Exists(PathServer + UniqueID + ".xlsx") == false) 
            {
                ErrMsg("File Not Found!", (PathServer + UniqueID + ".xlsx")); 
                return false; 
            }
            //

            //Open the XLSX file to read the fields...
            Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
            Workbook wb = excel.Workbooks.Open(PathServer + UniqueID + ".xlsx");
            Worksheet excelSheet;
            string txt_val;

            //Reads all tabs to find the information...
            foreach (Worksheet sheet in wb.Sheets)
            {
                excelSheet = wb.Worksheets[sheet.Index];
                if (sheet.Name == "Baisc Information")
                {
                    txt_val = ReadExcelFindTxt(UniqueID, excelSheet, 1, 1, "Unique Identifier of Archiving Order");
                    Console.WriteLine("|Info| Reading Archive Order objects (UniqueID)... " + txt_val.Replace(" ", ""));
                    txt_val = ReadExcelFindTxt(UniqueID, excelSheet, 1, 1, "Name of LiveLink Instance");
                    Console.WriteLine("|Info| Reading Archive Order objects (LiveLink)... " + txt_val);
                }
                if (sheet.Name == "Archiving Order")
                {
                    FileCSV = PathCSV + UniqueID + ".csv";
                    txt_val = ReadExcelFindTxt(UniqueID, excelSheet, 2, 1, "");
                    Console.WriteLine("|Info| Writing Archive Order objects..." + txt_val);
                }

            }

            //close the app...
            wb.Close(false); //false = Descartar alterações... (31/03)
            excel.Quit();
            return true;
        }


        /// <summary>
        /// Find values/orders in UniqueID to Archiving...
        /// </summary>
        /// <param name="path"></param>
        public static string ReadExcelFindTxt(string UniqueID, Worksheet excelSheet, int rIni, int cIni, string str_find)
        {
            if (str_find == "")
            {
                //Header query to INSERT config fields e create script SQL file...
                string db_name = cfg_plan.ID_INSTANCE + "_" + UniqueID;
                string sql_head1 = "INSERT INTO  [" + db_name + @"].[dbo].[TJOB_LL_ORDER]";
                string sql_head2 = "\n             ([CSYSTEMID],[CREQUESTID],[CNODEID],[CCURRENTIDS],[CSOURCECOUNT],[CSOURCEVOLUME],[CSUBTYPE],[CARCHIVE],[CRECURSIVE],[CNAME],[CSTATUS],[CMESSAGE],[CBUSINESSUNIT],[CTIMESTAMP]) ";
                string sql_parms = "\n     VALUES  ( '@CSYSTEMID','@CREQUESTID','@CNODEID','@CCURRENTIDS','@CSOURCECOUNT',@CSOURCEVOLUME,'@CSUBTYPE','@CARCHIVE','@CRECURSIVE',SUBSTRING('@CNAME', 1, 255),'@CSTATUS','@CMESSAGE','@CBUSINESSUNIT',GETDATE());";

                //Write archive orders to SQL file to load into Shell Database
                //Write SQL...opened!
                fileSQL.WriteLine("\n\n/*** ==== INSERT [TJOB_LL_ORDER] - archive order request table ==== ***/");
                string[] vet_linSQL;

                //Write CSV file to upload to Shell Server...
                StreamWriter fileCSV = new StreamWriter(FileCSV, false); //false = not append, create new (31/3)
                string str_aux = "1";
                string str_lin = "";

                while (str_aux != "")
                {
                    //get values of columns 1,2,3,4(file size) and 8...
                    for (int i = 0; i < 4; i++)
                    {
                        if (excelSheet.Cells[rIni, cIni + i].Value != null)
                            str_lin = str_lin + excelSheet.Cells[rIni, cIni + i].Value.ToString() + ";";
                        else
                            str_lin = str_lin + " ;";
                    }
                    //
                    cIni = 8;
                    if (excelSheet.Cells[rIni, cIni].Value != null)
                    {
                        str_lin = str_lin + excelSheet.Cells[rIni, cIni].Value.ToString() + ";";
                        BU = excelSheet.Cells[rIni, cIni].Value.ToString();
                    }
                    else
                        str_lin = str_lin + " ;";

                    //Write CSV...
                    //Test if 1st line is empty, then not write... 27/03/2015
                    if (str_lin.Substring(0, 2) != " ;")
                        fileCSV.WriteLine(str_lin);
                    //Console.WriteLine("|Debug| New LINE found...:\n" + str_lin);

                    //=====================================================================
                    //Set vector with XLS fields...
                    vet_linSQL = str_lin.Split(';');
                    string str_linSQL = "";

                    //Set fields...
                    sql_parms = sql_parms.Replace("@CSYSTEMID", sysid);
                    sql_parms = sql_parms.Replace("@CREQUESTID", UniqueID);
                    sql_parms = sql_parms.Replace("@CNODEID", vet_linSQL[0]);
                    sql_parms = sql_parms.Replace("@CCURRENTIDS", "");
                    sql_parms = sql_parms.Replace("@CSOURCECOUNT", vet_linSQL[2]);
                    sql_parms = sql_parms.Replace("@CSOURCEVOLUME", vet_linSQL[3].Replace(",", "."));
                    sql_parms = sql_parms.Replace("@CSUBTYPE", "Folder");
                    sql_parms = sql_parms.Replace("@CARCHIVE", "YES");
                    sql_parms = sql_parms.Replace("@CRECURSIVE", "YES");
                    sql_parms = sql_parms.Replace("@CNAME", vet_linSQL[1]);
                    sql_parms = sql_parms.Replace("@CSTATUS", "todo");
                    sql_parms = sql_parms.Replace("@CMESSAGE", "new");
                    sql_parms = sql_parms.Replace("@CBUSINESSUNIT", BU);
                    //
                    str_linSQL = sql_head1 + sql_head2 + sql_parms;
                    //Write SQL...
                    //Test if 1st line is empty, then not write... 27/03/2015
                    if (str_lin.Substring(0, 2) != " ;")
                        fileSQL.WriteLine(str_linSQL);
                    //=====================================================================

                    //Counts new line
                    sql_parms = "\n     VALUES  ( '@CSYSTEMID','@CREQUESTID','@CNODEID','@CCURRENTIDS','@CSOURCECOUNT',@CSOURCEVOLUME,'@CSUBTYPE','@CARCHIVE','@CRECURSIVE',SUBSTRING('@CNAME', 1, 255),'@CSTATUS','@CMESSAGE','@CBUSINESSUNIT',GETDATE());";
                    str_linSQL = "";
                    str_lin = "";
                    rIni++;
                    cIni = 1;
                    //
                    if (excelSheet.Cells[rIni, cIni].Value != null)
                        str_aux = excelSheet.Cells[rIni, cIni].Value.ToString();
                    else
                        str_aux = "";
                }

                fileCSV.Close();
                return FileCSV;
            }
            else
            {
                //Find the input string...
                //Read the first cell
                int LIM = 100;
                int[] ret = { 1, 1 };
                int i = 0;
                //Read the first cell
                string aux = "";

                //search a row...
                while ((aux != str_find) && (i < LIM))
                {
                    rIni++;
                    if (excelSheet.Cells[rIni, cIni].Value == null)
                        aux = "";
                    else
                        aux = excelSheet.Cells[rIni, cIni].Value.ToString();

                    i++;
                }
                if (i < LIM)
                {
                    ret[0] = rIni;
                    aux = "";
                    //search a column...
                    i = 0;
                    while ((aux == "") && (i < LIM))
                    {
                        cIni++;
                        if (excelSheet.Cells[rIni, cIni].Value == null)
                            aux = "";
                        else
                            aux = excelSheet.Cells[rIni, cIni].Value.ToString();

                        i++;
                    }
                    if (i < LIM) { ret[1] = cIni; }
                }

                if (excelSheet.Cells[rIni, cIni].Value != null)
                    aux = excelSheet.Cells[rIni, cIni].Value.ToString();
                else
                    aux = "";

                if (aux.IndexOf("Unique Identifier") >= 0)
                    aux.Trim();

                return aux;
            }
        }


        /// <summary>
        /// Show Generic error e show msg...
        /// </summary>
        /// <param name="tit"></param>
        /// <param name="msg"></param>
        static bool ErrMsg(string tit, string msg)
        {
            Console.WriteLine("\n|----------------------------------------------------===================|");
            Console.WriteLine("|!| " + tit + " - " + msg);
            return false;
        }


        /// <summary>
        /// Call these function to read UDR CSV files to get Volumes SUM of reports...
        /// </summary>
        /// <param name="csvname"></param>
        static double[] Sum_ReadCSVfiles(string csvname)
        {
            //Declare a array to get Sum of ArchivingOrderReport  
            double[] retSum = { 0, 0 };

            try
            {
                //new count of volumes... 26/03/2015
                double sumAOVol = 0;
                double sumAdVol = 0;

                var lines = File.ReadAllLines(csvname).Select(a => a.Split(';'));

                //Teste Size of columns, if 2nd col is No.Docs - skip 1, else skip 2
                int colskip = 1;
                if (lines.ElementAt(1)[1].IndexOf("No. of Documents") >= 0)
                    colskip = 1;

                if (lines.ElementAt(1)[2].IndexOf("No. of Documents") >= 0)
                    colskip = 2;

                var csv = (from line in lines
                           select (from col in line
                                   select col).Skip(colskip).ToArray()    // skip 2 columns
                          ).Skip(2).ToArray();                      // skip 1 headlines

                foreach (var item in csv)
                {
                    if (item[4] != "")
                        sumAOVol = sumAOVol + Double.Parse(item[4], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture); //Convert.ToDouble(item[4].Replace(".",","));
                    if (item[5] != "")
                        sumAdVol = sumAdVol + Double.Parse(item[5], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture); //Convert.ToDouble(item[5].Replace(".", ","));
                }

                //Assign values to retSum
                retSum[0] = sumAOVol;
                retSum[1] = sumAdVol;

                return retSum;
            }
            catch (Exception ex)
            {
                ErrMsg("Error Reading CSV", ex.Message);
                return retSum;
            }
        }


        /// <summary>
        /// Call these function to read UDR CSV files to get counts of reports...
        /// </summary>
        /// <param name="csvname"></param>
        static int[] Count_ReadCSVfiles(string csvname)
        {
            //Declare a array to get Sum of ArchivingOrderReport  
            int[] retSum = { 0, 0, 0, 0 };

            try
            {
                int countSrc = 0;
                int countTgt = 0;
                int countFil = 0;
                int countFai = 0;

                var lines = File.ReadAllLines(csvname).Select(a => a.Split(';'));

                //Teste Size of columns, if 2nd col is No.Docs - skip 1, else skip 2
                int colskip = 1;
                if (lines.ElementAt(1)[1].IndexOf("No. of Documents") >= 0)
                    colskip = 1;

                if (lines.ElementAt(1)[2].IndexOf("No. of Documents") >= 0)
                    colskip = 2;

                var csv = (from line in lines
                           select (from col in line
                                   select col).Skip(colskip).ToArray()    // skip 2 columns
                          ).Skip(2).ToArray();                      // skip 1 headlines

                foreach (var item in csv)
                {
                    countSrc = countSrc + Convert.ToInt32(item[0]);
                    countTgt = countTgt + Convert.ToInt32(item[1]);
                    countFai = countFai + Convert.ToInt32(item[2]);
                    countFil = countFil + Convert.ToInt32(item[3]);
                }

                //Assign values to retSum
                retSum[0] = countSrc;
                retSum[1] = countTgt;
                retSum[2] = countFil;
                retSum[3] = countFai;

                return retSum;
            }
            catch (Exception ex)
            {
                ErrMsg("Error Reading CSV", ex.Message);
                return retSum;
            }
        }


        /// <summary>
        /// Call these function to insert files attachments to the CVR document...
        /// </summary>
        /// <param name="wordApp"></param>
        /// <param name="path"></param>
        /// <param name="fileAtt"></param>
        static void InsertExcelObj(Microsoft.Office.Interop.Word.Application wordApp, string path, string fileAtt)
        {
            //Name displayed besides the embedded document
            Object oIconLabel = fileAtt.Substring(path.Length);

            //Display a specific icon
            //Test extension
            Object oIconFileName;
            if (fileAtt.IndexOf(".zip") > 0)
                oIconFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + "zip.ico";
            else
                oIconFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + "xls.ico";

            //The location of the file
            Object oFileDesignInfo = fileAtt; //path + fileAtt;

            Object oClassType;
            oClassType = "Excel.Sheet.12";
            Object oTrue = true;
            Object oFalse = false;
            Object oMissing = System.Reflection.Missing.Value;

            //Method to embed the document
            wordApp.Selection.InlineShapes.AddOLEObject(
                ref oClassType, ref oFileDesignInfo, ref oFalse, ref oTrue, ref oIconFileName,
                ref oMissing, ref oIconLabel, ref oMissing);
        }


        /// <summary>
        /// Call the function of generate de CVR document that we will send to Shell Contact via OTRS...
        /// </summary>
        /// <returns></returns>
        static bool fillWordCVR(string UniqueID)
        {
            //Declare a array to get Sum of ArchivingOrderReport  
            int[] retSum = { 0, 0, 0, 0 };
            double[] retSizeSum = { 0, 0 };
            string Local = cfg_plan.DUNAME;

            //Files to attach into doc file...
            string path = PathServer;
            string fileArchOrder = FileREQ; // UniqueID + ".xlsx";      // Add Request by Param... - 12/5/2015
            string fileArchOrderRpt = "ArchivingOrderReport.csv";
            string fileSuccessRpt = "successReport.csv";
            string fileFilteredRpt = "filteredReport.csv";
            string fileFailedRpt = "failedReport.csv";
            //
            string[] ZipFile1 = { path + fileArchOrderRpt };
            string[] ZipFile2 = { path + fileSuccessRpt };
            string[] ZipFile3 = { path + fileFilteredRpt };
            string[] ZipFile4 = { path + fileFailedRpt };

            //Test if files existis, so ZIP each one...
            if (File.Exists(fileArchOrder) == false) { ErrMsg("File Not Found!", (path + fileArchOrder)); return false; }
            if (File.Exists(path + fileArchOrderRpt) == false) { ErrMsg("File Not Found!", (path + fileArchOrderRpt)); return false; }
            if (File.Exists(path + fileSuccessRpt) == false) { ErrMsg("File Not Found!", (path + fileSuccessRpt)); return false; }
            if (File.Exists(path + fileFilteredRpt) == false) { ErrMsg("File Not Found!", (path + fileFilteredRpt)); return false; }
            if (File.Exists(path + fileFailedRpt) == false) { ErrMsg("File Not Found!", (path + fileFailedRpt)); return false; }

            //Se os arquivos ZIP existirem, deleta-os antes...
            //
            if (File.Exists(ZipFile1[0].Replace(".csv", ".zip"))) { File.Delete(ZipFile1[0].Replace(".csv", ".zip")); }
            if (File.Exists(ZipFile2[0].Replace(".csv", ".zip"))) { File.Delete(ZipFile2[0].Replace(".csv", ".zip")); }
            if (File.Exists(ZipFile3[0].Replace(".csv", ".zip"))) { File.Delete(ZipFile3[0].Replace(".csv", ".zip")); }
            if (File.Exists(ZipFile4[0].Replace(".csv", ".zip"))) { File.Delete(ZipFile4[0].Replace(".csv", ".zip")); }
            
            //Zipa os Arquivos acima...
            //
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq("", "listZIP.txt", ZipFile1, ZipFile1[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq("", "listZIP.txt", ZipFile2, ZipFile2[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq("", "listZIP.txt", ZipFile3, ZipFile3[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq("", "listZIP.txt", ZipFile4, ZipFile4[0].Replace(".csv", ".zip"));
            //

            //Read CSV e get summarize values of Archiving Report... - rev. 26/03/2015
            retSum = Count_ReadCSVfiles(path + fileArchOrderRpt);
            retSizeSum = Sum_ReadCSVfiles(path + fileArchOrderRpt);

            //Object of missing "null value"
            Object oMissing = System.Reflection.Missing.Value;
            Object oTemplatePath = System.IO.Directory.GetCurrentDirectory() + "\\" +"ArchivingReport_Template.dotm";
            Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
            Document wordDoc = new Document();
            wordDoc = wordApp.Documents.Add(ref oTemplatePath, ref oMissing, ref oMissing, ref oMissing);

            foreach (Field myMergeField in wordDoc.Fields)
            {
                Microsoft.Office.Interop.Word.Range rngFieldCode = myMergeField.Code;
                String fieldText = rngFieldCode.Text;

                //Only getting the merge fields
                if (fieldText.StartsWith(" MERGEFIELD"))
                {
                    //The text comes in the format of mergefield  myfieldname  \\* mergeformat
                    //This has to be edited to get only the fieldname "myfieldname"
                    Int32 endMerge = fieldText.IndexOf("\\");
                    Int32 fieldNameLength = fieldText.Length - endMerge;
                    String fieldName = fieldText.Substring(11, endMerge - 11);

                    //Gives the fieldnames as the user had entered in .dot file
                    fieldName = fieldName.Trim();

                    // **** FIELD REPLACEMENT IMPLEMENTATION GOES HERE ****//
                    // THE PROGRAMMER CAN HAVE HIS OWN IMPLEMENTATIONS HERE
                    //
                    if (fieldName == "BU")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(BU);
                    }
                    //
                    if (fieldName == "Local")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(Local);
                    }
                    //
                    if (fieldName == "LL_Instance")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(UniqueID);
                    }
                    //==========================================================
                    //Add SumSizeFile... 26/03/2015
                    if (fieldName == "ArchOrdSize")
                    {
                        myMergeField.Select();
                        string val0 = String.Format("{0:0.00}", retSizeSum[0]);
                        //wordApp.Selection.TypeText(retSizeSum[0].ToString());
                        wordApp.Selection.TypeText(val0);
                    }
                    if (fieldName == "ArchVolume")
                    {
                        myMergeField.Select();
                        string val1 = String.Format("{0:0.00}", retSizeSum[1]);
                        //wordApp.Selection.TypeText(retSizeSum[1].ToString());
                        wordApp.Selection.TypeText(val1);
                    }
                    //
                    //==========================================================
                    if (fieldName == "SourceCount")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(retSum[0].ToString());
                    }
                    //
                    if (fieldName == "DestCount")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(retSum[1].ToString());
                    }
                    if (fieldName == "DiffCount")
                    {
                        myMergeField.Select();
                        int diff;
                        if (retSum[1] > retSum[0])
                            diff = retSum[1] - retSum[0];
                        else
                            diff = retSum[0] - retSum[1];
                        wordApp.Selection.TypeText(diff.ToString());
                    }
                    //
                    if (fieldName == "NumFilterCount")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(retSum[2].ToString());
                    }
                    //
                    if (fieldName == "NumFailedCount")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(retSum[3].ToString());
                    }
                    //
                    if (fieldName == "SumNums")
                    {
                        myMergeField.Select();
                        int sum = retSum[2] + retSum[3];
                        wordApp.Selection.TypeText(sum.ToString());
                    }
                    //
                    // Attachements of CVR files...
                    //
                    if (fieldName == "fileArchOrder")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(" ");
                        //
                        InsertExcelObj(wordApp, path, fileArchOrder);
                    }
                    //
                    if (fieldName == "fileArchOrderRpt")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(" ");
                        //
                        InsertExcelObj(wordApp, path, ZipFile1[0].Replace(".csv", ".zip"));
                    }
                    //
                    if (fieldName == "fileSuccessRpt")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(" ");
                        //
                        InsertExcelObj(wordApp, path, ZipFile2[0].Replace(".csv", ".zip"));
                    }
                    //
                    if (fieldName == "fileFilteredRpt")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(" ");
                        //
                        InsertExcelObj(wordApp, path, ZipFile3[0].Replace(".csv", ".zip"));
                    }
                    //
                    if (fieldName == "fileFailedRpt")
                    {
                        myMergeField.Select();
                        wordApp.Selection.TypeText(" ");
                        //
                        InsertExcelObj(wordApp, path, ZipFile4[0].Replace(".csv", ".zip"));
                    }
                }
            }
            //*** rev. MPS - 08/4 *********************************************************************************************
            string aux_cvrname = PathCVR + "ArchivingReport_" + UniqueID + ".docx"; //Add CVR Path... - 14/5/2015
            try
            {
                //Microsoft.Office.Interop.Word.Document oDoc = oWord.Documents.Open(f, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                wordDoc.SaveAs2(aux_cvrname, Microsoft.Office.Interop.Word.WdSaveFormat.wdFormatDocumentDefault, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing);
                wordDoc.Close(oMissing, oMissing, oMissing);

                //----------------------------------------------------------------------------- 23/06/2015
                //Updating AO volume in Planning table...
                //
                string aux = updAOVolume(UniqueID, "CVR Created", String.Format("{0:0.00}", retSizeSum[1])).ToString();
                ErrMsg("Update AO Volume in Planning table... ", aux);
                //
                //----------------------------------------------------------------------------- 23/06/2015

                //
                //Show the Doc created, in html format...
                string lnk_cvrname = "<a href=\"" + aux_cvrname.Replace("C:\\", "FILE:\\\\10.58.87.19\\").Replace("\\","/") + "\">" + aux_cvrname.Replace("C:\\", "\\\\10.58.87.19\\") + "</a>";
                ErrMsg("CVR document: ", lnk_cvrname);
                return true;
            }
            catch (Exception ex)
            {
                ErrMsg("CVR document: ", "Error during generating Word document..." + ex.Message);
                return false;
            }
            //*** rev. MPS - 08/4 *********************************************************************************************
        }


        /// <summary>
        /// Call the function of update Planning DB table with the uniqueID and Volume parameter...
        /// 
        /// </summary>
        /// <param name="nameBD"></param>
        /// <returns></returns>
        //----------------------------------------------------------------------------- 23/06/2015
        static bool updAOVolume(string uid, string statAO, string volAO)
        {
            string ret = "";
            ret = SqlServer.SQLUtil.updAOVolume(uid, statAO, volAO);

            if (ret.Length > 1)
            {
                Console.WriteLine("\n|----------------------------------------------------===================|");
                Console.WriteLine("|!| Database Error! Try again... - " + ret);
                return false;
            }
            if (ret.Length == 1)
                return true;
            else
                return false;
        }
        //----------------------------------------------------------------------------- 23/06/2015


        /// <summary>
        /// Call the function of create DB with the input parameter...
        /// </summary>
        /// <param name="nameBD"></param>
        /// <returns></returns>
        static bool newDBSQLServer(string nameDB)
        {
            string ret = "";
            ret = SqlServer.SQLUtil.CreateArchDB(nameDB);

            if (ret.Length > 1)
            {
                Console.WriteLine("\n|----------------------------------------------------===================|");
                Console.WriteLine("|!| Invalid Database! Try again... - " + nameDB);
                return false;
            }
            //Firstly create tables...
            ret = SqlServer.SQLUtil.CreateArchTables(false, nameDB);

            if (ret.Length == 1)
                //After create views...
                ret = SqlServer.SQLUtil.CreateArchViews(false, nameDB);

            if (ret.Length == 1)
                return true;
            else
                return false;
        }


        //=============================================================================================
        /// <summary>
        /// This process starts with defined inputs parameters another ones functions of Shell Archiving process...
        /// The valid parameters are bellow...
        /// </summary>
        /// <param name="args"></param>
        //=============================================================================================
        static void Main(string[] args)
        {
            string dthrmmss = _dthrmmss.ToString("yyyyMMddHHmmss");
            string path = System.IO.Directory.GetCurrentDirectory();
            //
            Console.WriteLine("\n|----------------------------------------------------===================|");
            Console.WriteLine("|Info| Shell_AutoArch_cmd - Starting Process - | " + _dthrmmss.ToString("dd/MM/yyyy HH:mm:ss"));
            Console.WriteLine("|Info| (" + path + ") Using: ");
            Console.WriteLine("|------------------------------------------------------");
            Console.WriteLine("|Info| (Config DB ) Shell_AutoArch_cmd  <1>  <UniqueID> <FileREQ>");
            Console.WriteLine("|Info| (Create CSV) Shell_AutoArch_cmd  <2>  <UniqueID> <FileREQ>");
            Console.WriteLine("|Info| (Mail PSTxt) Shell_AutoArch_cmd  <3>  <Path\\File_PST.pst>");
            Console.WriteLine("|Info| (Create CVR) Shell_AutoArch_cmd  <9>  <UniqueID> <FileREQ>");
            Console.WriteLine("\n|----------------------------------------------------===================|");
            Console.WriteLine("|Info| _PathServer - " + PathServer);
            Console.WriteLine("|Info| _PathCSV_AO - " + PathCSV);
            Console.WriteLine("|Info| _PathSQL_AO - " + PathSQL);
            Console.WriteLine("|Info| _PathCVR_AO - " + PathCVR);
            Console.WriteLine("|Info| _PathPST_AO - " + PathPST);
            Console.WriteLine("|Info| Parameters  : ");
            int cont = 1;
            foreach (string s in args)
            {
                Console.WriteLine(String.Format("|Info| param [{0, 2}]  = {1}", cont, s));
                cont++;
            }
            Console.WriteLine("|----------------------------------------------------===================|");
            //
            if ((args == null) || (args.Length == 0)) { ErrMsg("Invalid arguments!", "Try again with correct values."); }
            else
            {
                if (args.Length >= 3)
                {
                    string unique_id = args[1];

                    //Search and get all configuration values in tables...
                    cfg_plan = new MPSfwk.Model.Configs();
                    cfg_lapi = new MPSfwk.Model.Configs();
                    cfg_plan = SqlServer.SQLUtil.ReadPlanning(unique_id);
                    cfg_lapi = SqlServer.SQLUtil.ReadLAPI(cfg_plan.INSTANCEID);
                    sysid = SqlServer.SQLUtil.ReadSYSID(cfg_plan.INSTANCEID);
                    FileSQL = PathSQL + unique_id + "_conf.sql";
                    FileREQ = PathServer + args[2];

                    //Change "-" to "_" 
                    unique_id = args[1].Replace("-", "_");
                    string db_name = cfg_plan.INSTANCEID + "_" + unique_id; //Rev. INSTANCEID - 12/5/2015

                    // Select wich functionality was choice...
                    if (args[0] == "1")
                    {
                        dthrmmss = DateTime.Now.ToString("yyyyMMddHHmmss");
                        //Write SQL file to run on Shell Server...
                        fileSQL = new StreamWriter(FileSQL, false); //false = not append, create new (31/3)
                        //
                        fileSQL.WriteLine("/*** ==== " + "|Info| Shell_AutoArch_cmd - Database Initialization - | " + _dthrmmss.ToString("dd/MM/yyyy HH:mm:ss") + " ==== ***/");
                        //
                        //Generate Insert Orders...
                        Console.WriteLine("|Info| Starting Archive Orders objects... " + ReadWriteCSV(unique_id));
                        //
                        fileSQL.Close();
                    }
                    else if (args[0] == "2") { Console.WriteLine("|Info| Starting Archive Orders objects... " + ReadWriteCSV(unique_id)); }
                    else if (args[0] == "9")
                    {
                        //Write SQL file to run on Shell Server...
                        fileSQL = new StreamWriter(FileSQL, false); //false = not append, create new (31/3)
                        //
                        Console.WriteLine("|Info| Starting Archive Orders objects... " + ReadWriteCSV(unique_id));
                        Console.WriteLine("|Info| Starting Document CVR objects... " + fillWordCVR(unique_id)); 
                    }
                    //
                    else if (args[0] == "0") { Console.WriteLine("|Info| Starting Test updAOVolume objects... " + updAOVolume(unique_id,args[2],args[3])); }
                    //
                    else { ErrMsg("Invalid arguments!", "Try again with correct values."); }
                }
                else if (args.Length == 2)
                {
                    string path_pstfile = args[1];
                    if (args[0] == "3") 
                    {
                        if (path_pstfile.IndexOf("\\") >= 0)
                            Console.WriteLine("|Info| Starting MailPST Text exporting... " + MPSfwk.MSWord.WritePSText((PathPST + "Today.txt"), path_pstfile)); 
                        else
                            Console.WriteLine("|Info| Starting MailPST Text exporting... " + MPSfwk.MSWord.WritePSText((PathPST + "Today.txt"), (PathPST + path_pstfile)));
                    }
                }
            }
        }
    }
}




/* -------------------------------------------------------------------------------------------------------------------
 * Old Library...
 * ------------------------------------------------------------------------------------------------------------------- 
    //Create Database and Moreens tables... Write SQL...opened!
    //fileSQL.WriteLine("/*** ==== " + "|Info| Shell_AutoArch_cmd - Database Initialization - | " + _dthrmmss.ToString("dd/MM/yyyy HH:mm:ss") + " ==== ***/
    //fileSQL.WriteLine("\n\n/*** ==== CREATE DATABASE <UniqueID> - Create a Database UniqueID ==== ***/");
    //fileSQL.WriteLine("\nCREATE DATABASE  [" + db_name + "]");
    //fileSQL.WriteLine("GO");
    //fileSQL.WriteLine("\nUSE [" + db_name + "]");
    //fileSQL.WriteLine("GO");
    //fileSQL.WriteLine("\n\n/*** ==== CREATE TABLE [Moreen Tables] - Create all tables to Moreen ==== ***/");

    //Generate Script Create Moreen tables...
    //fileSQL.WriteLine("\n" + SqlServer.SQLUtil.CreateArchTables(true, db_name));
    //fileSQL.WriteLine("GO");

    //Generate Script Configs and insert Orders...
    //Console.WriteLine("|Info| Starting Configuration objects... " + confDBSQLServer(unique_id));
    //Console.WriteLine("|Info| Starting Archive Orders objects... " + ReadWriteCSV(unique_id));
    //Generate Script Moreen Views, commented because needs execute alone at the end...
    //fileSQL.WriteLine("\n\n/*** ==== CREATE VIEW [Moreen Views] - Create all views to Moreen ==== ***/");
    //fileSQL.WriteLine("\n/*** ---- PLEASE DON'T FORGET, uncomment and execute at the end!! ---- ***/");
    //Comment this part to execute in separate, because create view cant' combine in batch script...
    //fileSQL.WriteLine("\n/*\n" + SqlServer.SQLUtil.CreateArchViews(true, db_name));
    //fileSQL.WriteLine("*/GO");
//*
