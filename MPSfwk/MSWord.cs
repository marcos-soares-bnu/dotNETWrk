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
using Microsoft.Office.Interop.Outlook;
using System.Linq;
using System.Data.OleDb;

namespace MPSfwk
{
    public static class MSWord
    {
        static string PathTemp = "C:\\ShellVMs\\LOCAL_E\\Shell_AutoArch\\";

        /// <summary>
        /// Call these function to read PST file and WRITE TEXT file mails...
        /// </summary>
        /// <param name="PSTextpath"></param>
        public static bool WritePSText(string PSTextpath, string PSTpath)
        {
            try
            {
                File.WriteAllText(PSTextpath, ReadPST(PSTpath));
            }
            catch (System.Exception ex)
            {
                ConsErrMsg("WritePSText: ", "Error during generating PSText file..." + ex.Message);
                return false;
            }
            return true;
        }


        /// <summary>
        /// Call these function to read PST file and extract mails...
        /// </summary>
        /// <param name="PSTpath"></param>
        public static string ReadPST(string PSTpath)
        {
            //SHOW TRACE...
            string[] vetTRACE;
            string NEWALERT = "new! ";
            string UPDALERT = "upd! ";
            string INFALERT = "mail ";
            string strALERT = "";
            string strTRACE = "";
            //
            string UniqueID = "[]";
            string TitleIns = "[]";
            string last_Upd = "[]";
            string COstatus = "[]";
            string AOstatus = "[]";
            string dtForIni = "[01/01/9999]";
            string dtForFim = "[01/01/9999]";
            string Comments = "[]";

            try
            {
                IEnumerable<MailItem> mailItems = readPst(PSTpath, "Archives");
                foreach (MailItem mailItem in mailItems)
                {
                    vetTRACE = mailItem.Body.Split('\n');
                    //
                    foreach (string aux in vetTRACE)
                    {
                        if (aux.IndexOf("Unique ID:") >= 0)
                        {
                            UniqueID = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                        }
                        //
                        if (aux.IndexOf("Title:") >= 0)
                        {
                            int ind1 = aux.IndexOf(":");
                            TitleIns = aux.Substring(ind1 + 1).Replace("\t", "").Replace("\r", "").Trim();
                            TitleIns = "[" + TitleIns + "] ";
                        }
                        //
                        if (aux.IndexOf("Status:") >= 0)
                            if (aux.IndexOf("CurrentOrderStatus:") >= 0)
                                COstatus = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                            else
                                AOstatus = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                        //
                        if (aux.IndexOf("Forecast_StartDate:") >= 0)
                            dtForIni = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                        //
                        if (aux.IndexOf("Forecast_EndDate:") >= 0)
                            dtForFim = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                        //
                        if ((aux.IndexOf("Last Modified") >= 0))
                            last_Upd = "[" + aux.Substring(14).Replace("\t", "") + "] ";
                        //
                        if (aux.IndexOf("Comments:") >= 0)
                            Comments = "[" + aux.Substring(aux.IndexOf(":") + 1).Replace("\t", "").Replace("\r", "").Trim() + "] ";
                    }
                    //
                    if (COstatus.Trim() == "[Awaiting Acknowledgment]")
                        strALERT = NEWALERT;
                    else if (COstatus.Trim() == "[Awaiting CVR Submission]")
                        strALERT = UPDALERT;
                    else
                        strALERT = INFALERT;
                    //
                    strTRACE += "\n" + strALERT + "|Info| " + UniqueID + TitleIns.Replace(":", "] [") + last_Upd + AOstatus + COstatus + dtForIni + dtForFim + Comments + "...Start ";
                    //
                    UniqueID = "[]";
                    TitleIns = "[]";
                    last_Upd = "[]";
                    COstatus = "[]";
                    AOstatus = "[]";
                    dtForIni = "[01/01/9999]";
                    dtForFim = "[01/01/9999]";
                    Comments = "[]";
                    //
                }
            }
            catch (System.Exception ex)
            {
                strTRACE += "\n|!|" + (ex.Message);
            }
            //
            return strTRACE;
        }


        private static IEnumerable<MailItem> readPst(string pstFilePath, string pstName)
        {
            List<MailItem> mailItems = new List<MailItem>();
            Microsoft.Office.Interop.Outlook.Application app = new Microsoft.Office.Interop.Outlook.Application();
            NameSpace outlookNs = app.GetNamespace("MAPI");

            // Add PST file (Outlook Data File) to Default Profile
            //outlookNs.AddStore(pstFilePath);
            MAPIFolder rootFolder = outlookNs.Stores[pstName].GetRootFolder();
            //MAPIFolder rootFolder = outlookNs.GetDefaultFolder(Microsoft.Office.Interop.Outlook.OlDefaultFolders.olFolderInbox);
            // Traverse through all folders in the PST file
            // TODO: This is not recursive, refactor
            Folders subFolders = rootFolder.Folders;
            foreach (Folder folder in subFolders)
            {
                Items items = folder.Items;
                foreach (object item in items)
                {
                    if (item is MailItem)
                    {
                        MailItem mailItem = item as MailItem;
                        mailItems.Add(mailItem);
                    }
                }
            }
            // Remove PST file from Default Profile
            //outlookNs.RemoveStore(rootFolder);

            //Order by
            IEnumerable<MailItem> query = mailItems.OrderByDescending(s => s.ReceivedTime);
            return query;
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
                oIconFileName = PathTemp + "zip.ico";
            else
                oIconFileName = PathTemp + "xls.ico";

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
            catch (System.Exception ex)
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
            catch (System.Exception ex)
            {
                ErrMsg("Error Reading CSV", ex.Message);
                return retSum;
            }
        }


        /// <summary>
        /// Show Generic error e show msg...
        /// </summary>
        /// <param name="tit"></param>
        /// <param name="msg"></param>
        static bool ConsErrMsg(string tit, string msg)
        {
            Console.WriteLine("\n|----------------------------------------------------===================|");
            Console.WriteLine("\n|!| " + tit + " - " + msg);
            return false;
        }


        /// <summary>
        /// Show Generic error e show msg...
        /// </summary>
        /// <param name="tit"></param>
        /// <param name="msg"></param>
        static string ErrMsg(string tit, string msg)
        {
            string aux = "";
            aux+= ("\n|----------------------------------------------------===================|");
            aux += ("\n|!| " + tit + " - " + msg);
            return aux;
        }


        /// <summary>
        /// Call the function of generate de CVR document that we will send to Shell Contact via OTRS...
        /// </summary>
        /// <returns></returns>
        public static string fillWordCVR(string PathServer, string PathCVR, string DUNAME, string BU, string FileREQ, string UniqueID)
        {
            //SHOW TRACE...
            DateTime _dthrmmss = DateTime.Now;
            string dthrmmss = _dthrmmss.ToString("yyyyMMddHHmmss");
            string strTRACE = "";
            strTRACE += ("\n|----------------------------------------------------===================|");
            strTRACE += ("\n|Info| Shell_AutoArch_cmd - Starting Process - | " + _dthrmmss.ToString("dd/MM/yyyy HH:mm:ss"));
            strTRACE += ("\n|Info| (" + PathServer + ") ");
            strTRACE += ("\n|----------------------------------------------------===================|");

            //Declare a array to get Sum of ArchivingOrderReport  
            int[] retSum = { 0, 0, 0, 0 };
            double[] retSizeSum = { 0, 0 };
            string Local = DUNAME;

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
            if (File.Exists(fileArchOrder) == false) { ErrMsg("File Not Found!", (path + fileArchOrder)); return ""; }
            if (File.Exists(path + fileArchOrderRpt) == false) { ErrMsg("File Not Found!", (path + fileArchOrderRpt)); return ""; }
            if (File.Exists(path + fileSuccessRpt) == false) { ErrMsg("File Not Found!", (path + fileSuccessRpt)); return ""; }
            if (File.Exists(path + fileFilteredRpt) == false) { ErrMsg("File Not Found!", (path + fileFilteredRpt)); return ""; }
            if (File.Exists(path + fileFailedRpt) == false) { ErrMsg("File Not Found!", (path + fileFailedRpt)); return ""; }

            //Zipa os Arquivos acima...
            //
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq(PathTemp, "listZIP.txt", ZipFile1, ZipFile1[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq(PathTemp, "listZIP.txt", ZipFile2, ZipFile2[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq(PathTemp, "listZIP.txt", ZipFile3, ZipFile3[0].Replace(".csv", ".zip"));
            if (File.Exists("listZIP.txt")) { File.Delete("listZIP.txt"); }
            MPSfwk.WMI.zipListArq(PathTemp, "listZIP.txt", ZipFile4, ZipFile4[0].Replace(".csv", ".zip"));
            //

            //Read CSV e get summarize values of Archiving Report... - rev. 26/03/2015
            retSum = Count_ReadCSVfiles(path + fileArchOrderRpt);
            retSizeSum = Sum_ReadCSVfiles(path + fileArchOrderRpt);

            //Object of missing "null value"
            Object oMissing = System.Reflection.Missing.Value;
            Object oTemplatePath = PathTemp + "ArchivingReport_Template.dotm";
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

                //
                //Show the Doc created, in html format...
                string lnk_cvrname = "<a href=\"" + aux_cvrname.Replace("C:\\", "\\\\10.58.87.19\\").Replace("\\", "/") + "\">" + aux_cvrname.Replace("C:\\", "\\\\10.58.87.19\\") + "</a>";
                //
                return strTRACE + ErrMsg("CVR document: ", lnk_cvrname);
            }
            catch (System.Exception ex)
            {
                return strTRACE + ErrMsg("CVR document: ", "Error during generating Word document..." + ex.Message);
            }
            //*** rev. MPS - 08/4 *********************************************************************************************
        }




    }
}
