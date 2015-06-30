using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Data;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
//
using System.Data.OleDb;
using System.Linq;

namespace MPSfwk
{
    public static class WMI
    {
        //
        //Find a string into Datatable...
        //
        public static DataRow dtRowTaskFind(DataTable dt, string sFind)
        {
            foreach (DataRow dr in dt.Rows) 
            {
                if (dr["Nome"].ToString().IndexOf(sFind) >= 0) { return dr; }
            }
            //
            return null;
        }


        //----------------------------------------------- 26/06/2015
        // Returns the query variable, not query results! 
        //
        static IEnumerable<string> RunQueryFilter(IEnumerable<string> source, int num, string filter)
        {
            // Split the string and sort on field[num] 
            var scoreQuery = from line in source
                             let fields = line.Split(',')
                             where fields[num].Contains(filter)
                             orderby fields[num] descending
                             select line;

            return scoreQuery;
        }
        //----------------------------------------------- 26/06/2015


        //
        // Create a Datatable with Schedule Tasks list
        //
        public static DataTable dtScheduleTaskList(string strFilter)
        {
            try
            {
                // Create new DataTable and DataSource objects.
                DataTable dt = new DataTable("Tasks");

                dt.Columns.Add("Name");
                dt.Columns.Add("Status");
                dt.Columns.Add("Dispatchers");
                dt.Columns.Add("Next Execution");
                dt.Columns.Add("Last Execution");
                dt.Columns.Add("Last Result");
                dt.Columns.Add("Author");
                //
                bool ehCabec = false;
                string[] stringSeparator1 = new string[] { "\",\"" };
                string[] stringSeparator2 = new string[] { "\"\r\n\"" };
                string msg = "";
                string cmd = "schtasks /Query /FO CSV /V | findstr /V /C:\"TaskName\" ";
                string ret_cmd = MPSfwk.WMI.ExecConsole(cmd, "");
                string[] tasks;
                string[] cols;
                tasks = ret_cmd.Split(stringSeparator2, StringSplitOptions.None);

                foreach (string ts in RunQueryFilter(tasks,1,strFilter))
                {
                    cols = ts.Split(stringSeparator1, StringSplitOptions.None);
                    ////
                    //if (!ehCabec)
                    //{
                    //    if (cols[0].ToUpper().IndexOf("NOME DO HOST") > 0)
                    //    { ehCabec = true; }
                    //}
                    //else
                    //{
                    //    if (cols[0].ToUpper().IndexOf("NOME DO HOST") > 0)
                    //    { break; }
                    //    else
                    //    {
                            if (cols.Length != 28)
                            { msg = msg + "Ocorreu um erro na leitura das Tasks! <br />"; }
                            else
                            {
                                //
                                //Add to show Filter per Task Name...
                                //
                                //if (cols[1].IndexOf(strFilter) >= 0)
                                //{
                                    DataRow dr = dt.NewRow();
                                    dr["Name"] = cols[1] + " (" + cols[8] + ")";
                                    dr["Status"] = ConvEncString(cols[3]);
                                    string disp = "";
                                    for (int i = 18; i < 28; i++)
                                    {
                                        if ((cols[i].ToUpper().IndexOf("DESATIVADO") == -1) &&
                                                (cols[i].ToUpper().IndexOf("DISABLED") == -1) &&
                                                (cols[i].ToUpper().IndexOf("N/A") == -1) &&
                                                (cols[i].ToUpper().IndexOf("NENHUM") == -1)
                                            ) { disp = disp + cols[i] + " "; }
                                    }
                                    dr["Dispatchers"] = ConvEncString(disp);
                                    dr["Next Execution"] = ConvEncString(cols[2]);
                                    dr["Last Execution"] = ConvEncString(cols[5]);
                                    dr["Last Result"] = cols[6];
                                    dr["Author"] = cols[7];
                                    dt.Rows.Add(dr);
                                //}
                            }
                    //    }
                    //}
                }
                return dt;
            }
            catch (Exception e)
            {
                Console.WriteLine("|!| dtScheduleTaskList Error - " + e.Message);
                return null;
            }
        }

        //-----------------------------------------------//OK MPS - 21/10/2014
        public static string ConvEncString(string enc)
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

        }
        //-----------------------------------------------//OK MPS - 21/10/2014





        //
        // Create a SQL file to insert in the Order Table (Configurator) on Shell Database Server
        // Note: This function will change after June/2015 when the CSV mode will change the Order table...
        //
        public static string ShowSQLInsOrders(string uniqueid, string InstID, string SysID, string fileLocation, string REQname)
        {
            //Header query to INSERT config fields e create script SQL file...
            string db_name = InstID + "_" + uniqueid.Replace("-","_");
            string sql_head0 = "\n\n/*** ==== INSERT [TJOB_LL_ORDER] - archive order request table ==== ***/";
            string sql_head1 = "INSERT INTO  [" + db_name + @"].[dbo].[TJOB_LL_ORDER]";
            string sql_head2 = "\n             ([CSYSTEMID],[CREQUESTID],[CNODEID],[CCURRENTIDS],[CSOURCECOUNT],[CSOURCEVOLUME],[CSUBTYPE],[CARCHIVE],[CRECURSIVE],[CNAME],[CSTATUS],[CMESSAGE],[CBUSINESSUNIT],[CTIMESTAMP]) ";
            string sql_parms = "\n     VALUES  ( '@CSYSTEMID','@CREQUESTID','@CNODEID','@CCURRENTIDS','@CSOURCECOUNT',@CSOURCEVOLUME,'@CSUBTYPE','@CARCHIVE','@CRECURSIVE',SUBSTRING('@CNAME', 1, 255),'@CSTATUS','@CMESSAGE','@CBUSINESSUNIT',GETDATE());";

            //Call function to create a Datatable of Request File... (UniqueID.xlsx - until Jun/2015)
            DataTable dt = ReadReqAOChecked(fileLocation, REQname);

            //Loop and fill SQL script...
            StringBuilder result = new StringBuilder();
            result.AppendLine(sql_head0);
            //
            foreach (DataRow dro in dt.Rows)
            {
                //Set fields...
                sql_parms = sql_parms.Replace("@CSYSTEMID", SysID);
                sql_parms = sql_parms.Replace("@CREQUESTID", uniqueid);
                sql_parms = sql_parms.Replace("@CNODEID", dro[2].ToString());
                sql_parms = sql_parms.Replace("@CCURRENTIDS", "");
                sql_parms = sql_parms.Replace("@CSOURCECOUNT", dro[4].ToString());
                sql_parms = sql_parms.Replace("@CSOURCEVOLUME", dro[5].ToString().Replace(",", "."));
                sql_parms = sql_parms.Replace("@CSUBTYPE", "Folder");
                sql_parms = sql_parms.Replace("@CARCHIVE", "YES");
                sql_parms = sql_parms.Replace("@CRECURSIVE", "YES");
                sql_parms = sql_parms.Replace("@CNAME", dro[3].ToString());
                sql_parms = sql_parms.Replace("@CSTATUS", "todo");
                sql_parms = sql_parms.Replace("@CMESSAGE", "new");
                sql_parms = sql_parms.Replace("@CBUSINESSUNIT", dro[8].ToString());
                //
                result.AppendLine(sql_head1 + sql_head2 + sql_parms);
                sql_parms = "\n     VALUES  ( '@CSYSTEMID','@CREQUESTID','@CNODEID','@CCURRENTIDS','@CSOURCECOUNT',@CSOURCEVOLUME,'@CSUBTYPE','@CARCHIVE','@CRECURSIVE',SUBSTRING('@CNAME', 1, 255),'@CSTATUS','@CMESSAGE','@CBUSINESSUNIT',GETDATE());";
            }
            //
            return result.ToString();
        }


        //
        // Read CSV file - 03/06/2015...
        //
        public static DataTable CsvFileToDatatable(string path, bool IsFirstRowHeader)//here Path is root of file and IsFirstRowHeader is header is there or not
        {
            string header = "No";
            string sql = string.Empty;
            DataTable dataTable = null;
            string pathOnly = string.Empty;
            string fileName = string.Empty;

            try
            {
                pathOnly = Path.GetDirectoryName(path);
                fileName = Path.GetFileName(path);

                sql = @"SELECT * FROM [" + fileName + "]";

                if (IsFirstRowHeader)
                {
                    header = "Yes";
                }

                using (OleDbConnection connection = new OleDbConnection(
                        @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                        ";Extended Properties=\"Text;HDR=" + header + "\""))
                {
                    using (OleDbCommand command = new OleDbCommand(sql, connection))
                    {
                        using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
                        {
                            dataTable = new DataTable();
                            dataTable.Locale = System.Globalization.CultureInfo.CurrentCulture;
                            adapter.Fill(dataTable);
                        }
                    }
                }
            }
            finally
            {

            }

            return dataTable;
        }


        //
        // Read a Excel file (Tab - 'Archiving Order$') via ODBC and create a DataTable with 
        //                   Data checked and corrected to create SQL script... - 12/5/2015
        //
        public static DataTable ReadReqAOChecked(string fileLocation, string REQname)
        {
            try
            {
                //
                string connectionString = "";
                //
                string fileName = fileLocation + REQname;
                string fileExtension = Path.GetExtension(fileName);
                //
                if (fileExtension == ".xls")
                {
                    connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" +
                      fileName + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                }
                else if (fileExtension == ".xlsx")
                {
                    connectionString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" +
                      fileName + ";Extended Properties=\"Excel 12.0;HDR=Yes;IMEX=2\"";
                }
                OleDbConnection con = new OleDbConnection(connectionString);
                OleDbCommand cmd = new OleDbCommand();
                cmd.CommandType = System.Data.CommandType.Text;
                cmd.Connection = con;
                OleDbDataAdapter dAdapter = new OleDbDataAdapter(cmd);
                DataTable dtExcelRecords = new DataTable();
                con.Open();
                DataTable dtExcelSheetName = con.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                string getExcelSheetName = "'Archiving Order$'";
                cmd.CommandText = "SELECT * FROM [" + getExcelSheetName + "] ";
                dAdapter.SelectCommand = cmd;
                dAdapter.Fill(dtExcelRecords);

                //=================================================================================
                //Check and Revised Columns...
                //=================================================================================

                // Create new DataTable and DataSource objects.
                DataTable table = new DataTable();

                // Declare DataColumn and DataRow variables.
                DataColumn column;

                // Create new DataColumn, set DataType, ColumnName and add to DataTable.    
                // 0nd
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.Int32");
                column.ColumnName = "Count";
                table.Columns.Add(column);
                //
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.Boolean");
                column.ColumnName = "Rev.?";
                table.Columns.Add(column);
                // 1nd
                column = new DataColumn();
                column.DataType = System.Type.GetType("System.String");
                column.ColumnName = "NodeID";
                table.Columns.Add(column);
                // 2nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "Name";
                table.Columns.Add(column);
                // 3nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "No.Docs";
                table.Columns.Add(column);
                // 4nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "Sizeof.Docs";
                table.Columns.Add(column);
                // 5nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "No.Vers";
                table.Columns.Add(column);
                // 6nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "Sizeof.Vers";
                table.Columns.Add(column);
                // 7nd
                column = new DataColumn();
                column.DataType = Type.GetType("System.String");
                column.ColumnName = "BU";
                table.Columns.Add(column);

                //
                //test MPS ... Check Grid!
                //
                int[] arrChanges = { 0 };
                int ind = 0;
                int i = -1;
                //
                foreach (DataRow dro in dtExcelRecords.Rows) //(GridViewRow r in GridView1.Rows)
                {
                    bool flgchange = false;
                    string nodeid = dro[0].ToString(); // r.Cells[0].Text;
                    //
                    if ((nodeid != "") && (nodeid != "&nbsp;"))
                    {
                        i++;
                        DataRow dr = table.NewRow();
                        //
                        string name = dro[1].ToString(); //r.Cells[1].Text;
                        string nodocs = dro[2].ToString(); //r.Cells[2].Text;
                        string sizeofdocs = dro[3].ToString(); //r.Cells[3].Text;
                        string novers = dro[4].ToString(); //r.Cells[4].Text;
                        string sizeofvers = dro[5].ToString(); //r.Cells[5].Text;
                        string bu = dro[7].ToString(); //r.Cells[7].Text;
                        //
                        if ((name.IndexOf("'") >= 0) || (name.IndexOf(";") >= 0) || (name.Length >= 255))
                        {
                            ind++;
                            Array.Resize(ref arrChanges, ind);
                            arrChanges[ind - 1] = i;
                            flgchange = true;
                        }
                        //
                        dr["Count"] = i + 1;
                        dr["Rev.?"] = flgchange;
                        dr["NodeID"] = nodeid;
                        //
                        if (name.Length >= 255)
                            dr["Name"] = name.Replace("'", "@").Replace(";", "@").Substring(0, 255);
                        else
                            dr["Name"] = name.Replace("'", "@").Replace(";", "@");
                        //
                        dr["No.Docs"] = nodocs;
                        dr["Sizeof.Docs"] = sizeofdocs;
                        dr["No.Vers"] = novers;
                        dr["Sizeof.Vers"] = sizeofvers;
                        //if (bu.Length >= 30)
                        //    dr["BU"] = bu.Replace("'", "@").Replace(";", "@").Substring(0, 30);
                        //else
                        dr["BU"] = bu.Replace("'", "@").Replace(";", "@");
                        //
                        //fill table with row
                        //
                        table.Rows.Add(dr);
                    }
                }

                return table;
                //
            }
            catch (Exception e)
            {
                Console.WriteLine("|!| ReadReqAOChecked Error - " + e.Message);
                return null;
            }
        }


        //
        // Execute a Console command and redirect output to string - 5/5/15
        //
        public static string ExecConsole(string command, string workDir)
        {
            //
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                WorkingDirectory = workDir,
            };
            //
            string output;

            try
            {
                using (Process p = Process.Start(processInfo))
                {
                    output = p.StandardOutput.ReadToEnd();
                }
                return output;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        // fim 5/5/15


        public static ConnectionOptions wmInstConOpt(string UserName, string Password)
        {
            ConnectionOptions oConn = new ConnectionOptions();
            oConn.Username = UserName;
            oConn.Password = Password;
            oConn.EnablePrivileges = true;

            return oConn;
        }


        public static ManagementPath wmInstPath(string MachineName, string ClassName, string NameSpace)
        {
            ManagementPath path = new ManagementPath();
            path.Server         = MachineName;
            path.ClassName      = ClassName;
            path.NamespacePath  = NameSpace;

            return path;
        }


        public static ManagementScope wmInstScope(string UserName, ManagementPath path, ConnectionOptions oConn)
        {
            ManagementScope scope;
            if (UserName == "")
            { scope = new ManagementScope(path); }
            else
            { scope = new ManagementScope(path, oConn); }

            return scope;
        }


        public static string ExecProcesso(Model.Server s, ConnectionOptions oConn, string localSrv, string commandLine)
        {
            ManagementPath path2 = wmInstPath(s.IPHOST, "Win32_Process", "root\\CIMV2");
            ManagementScope scopeProcess = wmInstScope(s.USUARIO, path2, wmInstConOpt(s.USUARIO, s.SENHA));

            using (ManagementClass process = new ManagementClass(scopeProcess, path2, null))
            {
                using (ManagementBaseObject inParams = process.GetMethodParameters("Create"))
                {
                    inParams["CommandLine"] = commandLine;
                    inParams["CurrentDirectory"] = localSrv;//DriveLetter + @":\\";
                    inParams["ProcessStartupInformation"] = null;
                    using (ManagementBaseObject outParams = process.InvokeMethod("Create", inParams, null))
                    {
                        int retVal = Convert.ToInt32(outParams.Properties["ReturnValue"].Value);
                        return retVal.ToString();
                    }
                }
            }
        }


        public static string CriaPastaSite(string localSrv, string remoteDir, Model.Server s)
        {
            string TempName = remoteDir;
            int Index = TempName.IndexOf(":");
            string DriveLetter = "C";
            if (Index != -1)
            {
                string[] arr = TempName.Split(new char[] { ':' });
                DriveLetter = arr[0];
                TempName = TempName.Substring(Index + 2);
            }

            try
            {
                ManagementPath myPath = wmInstPath(s.IPHOST, "", "root\\CIMV2");
                ConnectionOptions oConn = wmInstConOpt(s.USUARIO, s.SENHA);
                ManagementScope scope = wmInstScope(s.USUARIO, myPath, oConn);
                scope.Connect();

                //without next strange manipulation, the os.Get().Count will throw the "Invalid query" exception
                remoteDir = remoteDir.Replace("\\", "\\\\");
                ObjectQuery oq = new ObjectQuery("select Name from Win32_Directory where Name = '" + remoteDir + "'");
                using (ManagementObjectSearcher os = new ManagementObjectSearcher(scope, oq))
                {
                    if (os.Get().Count == 0)      //It don't exist, so create it!
                    {
                        string commandLine = String.Format(@"cmd /C  mkdir {0} ", TempName);
                        return ExecProcesso(s, oConn, localSrv, commandLine);
                    }
                    else
                        return "O usuário já possui um perfil neste Servidor!";
                }
            }
            catch (Exception ex)
            {
                return "Ocorreu um erro: " + ex.Source + "\nDetail: " + ex.Message;
            }
        
        } //OK MPS - 09/10/2014


        public static bool zipListArq(string wrkDir, string lstArq, string[] fileCompressList, string targetCompressName)
        {
            try
            {
                string PZipPath = wrkDir + "7za.exe";
                if (!File.Exists(PZipPath))
                {
                    Console.WriteLine("|!| O arquivo 7za.exe, nao foi encontrado na pasta atual!");
                    return false;
                }
                if (fileCompressList.Length == 0)
                {
                    Console.WriteLine("|!| Nenhum arquivo na pasta informada!");
                    return false;
                }

                // Cria a list em arquivo...list.txt
                StreamWriter fileList = new StreamWriter(lstArq, true);
                foreach (string filename in fileCompressList)
                {
                    if ((File.Exists(filename)) && (filename != lstArq))
                    {
                        fileList.WriteLine(filename);
                    }
                }
                fileList.Close();

                if (!File.Exists(lstArq))
                {
                    Console.WriteLine("|!| O arquivo - " + lstArq + ", nao foi encontrado na pasta atual!");
                    return false;
                }

                ProcessStartInfo pCompress = new ProcessStartInfo();
                pCompress.FileName = PZipPath;
                pCompress.Arguments = "a -tzip -mx=1 -mmt=off \"" + targetCompressName + "\" " + "@" + lstArq; // + "-mx=1"; //" -mx=9";
                pCompress.WindowStyle = ProcessWindowStyle.Hidden;
                pCompress.UseShellExecute = false;
                pCompress.RedirectStandardOutput = false;
                Process x = Process.Start(pCompress);
                x.WaitForExit();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("|!| Ocorreu um Erro no zipListArq - " + e.Message);
                return false;
            }
        }
        

        public static bool findNetPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            string pathRoot = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(pathRoot)) return false;
            ProcessStartInfo pinfo = new ProcessStartInfo("net", "use");
            pinfo.CreateNoWindow = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.UseShellExecute = false;
            string output;
            using (Process p = Process.Start(pinfo))
            {
                output = p.StandardOutput.ReadToEnd();
            }
            foreach (string line in output.Split('\n'))
            {
                if (line.Contains(pathRoot) && line.Contains("OK"))
                {
                    return true; // shareIsProbablyConnected
                }
            }
            return false;
        }


        public static bool netToServer(string usr, string pwd, string srvPath, int timeout)
        {
            if (!findNetPath(srvPath))
            {
                var directory = Path.GetDirectoryName(srvPath).Trim();
                var command = "NET USE " + directory + " /user:" + usr + " " + pwd;
                ExecuteCommand(command, timeout, "C:\\");
                return true;
            }
            return true; // se já existe a conexao, apenas utiliza...
        }


        public static bool delToServer(string srvPath, int timeout)
        {
            if (findNetPath(srvPath))
            {
                var directory = Path.GetDirectoryName(srvPath).Trim();
                var command = "NET USE " + directory + " /delete";
                ExecuteCommand(command, timeout, "C:\\");
                return true;
            }
            return true;
        }


        //MPS 25/OUT - mover por ext - vários arquivos...
        public static void MoveExtfileToServer(string fromPathExt, string toPathExt, int timeout)
        {
            var command = " move /Y \"" + fromPathExt + "\"  \"" + toPathExt + "\"";
            ExecuteCommand(command, timeout, "C:\\");
        }
        //MPS 25/OUT - mover por ext - vários arquivos...


        public static void MovefileToServer(string filePath, string savePath, int timeout)
        {
            var directory = Path.GetDirectoryName(savePath).Trim();
            var filenameToSave = Path.GetFileName(savePath);

            if (!directory.EndsWith("\\"))
                filenameToSave = "\\" + filenameToSave;

            var command = " move /Y \"" + filePath + "\"  \"" + directory + filenameToSave + "\"";
            ExecuteCommand(command, timeout, "C:\\");
        }

        
        public static void SaveACopyfileToServer(string filePath, string savePath, int timeout)
        {
            var directory = Path.GetDirectoryName(savePath).Trim();
            var filenameToSave = Path.GetFileName(savePath);

            if (!directory.EndsWith("\\"))
                filenameToSave = "\\" + filenameToSave;

            var command = " copy /Y \"" + filePath + "\"  \"" + directory + filenameToSave + "\"";
            ExecuteCommand(command, timeout, "C:\\");
        }


        public static int ExecuteCommand(string command, int timeout, string workDir)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/C " + command)
                                {
                                    CreateNoWindow = true, 
                                    UseShellExecute = false,
                                    WorkingDirectory = workDir,
                                };
            var process = Process.Start(processInfo);
            //System.Threading.Thread.Sleep(timeout);
            process.WaitForExit(timeout);
            var exitCode = process.ExitCode;
            process.Close();
            return exitCode;
        }


        public static DataTable GroupMembers(string srv, string usr, string pwd)
        {
            StringBuilder result = new StringBuilder();
            try
            {
                //
                MPSfwk.Model.Server s = new MPSfwk.Model.Server();
                s.IPHOST = srv;
                s.USUARIO = usr;
                s.SENHA = pwd;
                ManagementScope ms = scopeMgmt(false, s);
                ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                //
                string targethost = "";
                string groupname = "";
                string aux_qry = "";
                if ((srv.IndexOf(".") == -1) && (srv.ToUpper() != "LOCALHOST"))
                {   aux_qry = "select * from Win32_Group Where Domain = '" + srv + "'"; }
                else
                { aux_qry = "select * from Win32_Group Where LocalAccount = True"; }
                //
                //MPS teste - 10/out
                //
                Console.WriteLine("DEBUG - aux_qry = " + aux_qry);
                //
                DataTable dt_aux = dtlistaClasse("Win32_Group",
                                                    aux_qry,
                                                    srv,
                                                    usr,
                                                    pwd);
                //
                //Cria tabela para preencher os campos
                DataTable dt1 = new DataTable();
                dt1.TableName = "GroupMembers";
                dt1.Columns.Add("Domain");
                dt1.Columns.Add("Group Name");
                dt1.Columns.Add("Users");
                //
                foreach (DataRow drow in dt_aux.Rows)
                {
                    //
                    DataRow dr = dt1.NewRow();
                    //
                    targethost = drow["Domain"].ToString();
                    groupname = drow["Name"].ToString();

                    StringBuilder qs = new StringBuilder();
                    qs.Append("SELECT PartComponent FROM Win32_GroupUser WHERE GroupComponent = \"Win32_Group.Domain='");
                    qs.Append(targethost);
                    qs.Append("',Name='");
                    qs.Append(groupname);
                    qs.AppendLine("'\"");
                    ObjectQuery query = new ObjectQuery(qs.ToString());
                    ManagementObjectSearcher searcher = new ManagementObjectSearcher(ms, query);
                    ManagementObjectCollection queryCollection = searcher.Get();
                    foreach (ManagementObject m in queryCollection)
                    {
                        ManagementPath path = new ManagementPath(m["PartComponent"].ToString());
                        {
                            String[] names = path.RelativePath.Split(',');
                            result.Append(names[0].Substring(names[0].IndexOf("=") + 1).Replace("\"", " ").Trim() + "\\");
                            result.AppendLine(names[1].Substring(names[1].IndexOf("=") + 1).Replace("\"", " ").Trim() + " ; ");
                        }
                    }
                    //Console.WriteLine("Domain =  " + targethost + " Name = " + groupname + " Users = " + result.ToString());
                    dr["Domain"] = targethost;
                    dr["Group Name"] = groupname;
                    dr["Users"] = result.ToString();
                    dt1.Rows.Add(dr);
                    //
                    result = new StringBuilder();
                    
                }
                return dt1;
                //
            }
            catch (Exception e)
            {
                Console.WriteLine("|!| GroupMembers Error - " + e.Message);
                return null;
            }
        }



        public static string InsertAT(  string srv, string usr, string pwd, 
                                        string inCMD, string inRPT, string inDOW, string inDOM, string inSTM)
        {
            try
            {
                MPSfwk.Model.Server s = new MPSfwk.Model.Server();
                s.IPHOST = srv;
                s.USUARIO = usr;
                s.SENHA = pwd;
                string strJobId = "";
                ManagementScope ms = scopeMgmt(false, s);
                ObjectGetOptions objectGetOptions = new ObjectGetOptions();
                ManagementPath managementPath = new ManagementPath("Win32_ScheduledJob");
                ManagementClass processClass = new ManagementClass(ms, managementPath, objectGetOptions);
                ManagementBaseObject inParams = processClass.GetMethodParameters("Create");
                inParams["Command"] = inCMD;
                inParams["InteractWithDesktop"] = "False";
                inParams["RunRepeatedly"] = inRPT;
                inParams["DaysOfMonth"] = inDOM;
                inParams["DaysOfWeek"] = inDOW;
                inParams["StartTime"] = inSTM + "00.000000-180";
                ManagementBaseObject outParams =
                        processClass.InvokeMethod("Create", inParams, null);

                strJobId = outParams["JobId"].ToString();

                return "Novo JobId (" + strJobId + ") criado com sucesso!";
            }
            catch (UnauthorizedAccessException uex)
            {
                return "Ocorreu um erro: " + uex.Message;
            }
            catch (ManagementException mex)
            {
                return "Ocorreu um erro: " + mex.Message;
            }
        }


        public static string DeleteAT(string srv, string JobID)
        {
            try
            {
                string strJobId = "";

                ManagementObject mo;
                ManagementPath path = ManagementPath.DefaultPath;
                path.RelativePath = "Win32_ScheduledJob.JobId=" + "\"" + JobID + "\"";
                path.Server = srv;
                mo = new ManagementObject(path);
                ManagementBaseObject inParams = null;
                // use late binding to invoke "Delete" method on "Win32_ScheduledJob" WMI class
                ManagementBaseObject outParams = mo.InvokeMethod("Delete", inParams, null);

                strJobId = outParams.Properties["ReturnValue"].Value.ToString();
                if (strJobId == "0") { return "O JobId ( " + JobID + " ) selecionado foi Apagado!"; }
                else { return "Out parameters: ReturnValue= " + strJobId; }
            }
            catch (UnauthorizedAccessException uex)
            {
                return "Ocorreu um erro: " + uex.Message;
            }
            catch (ManagementException mex)
            {
                return "Ocorreu um erro: " + mex.Message;
            }
        }


        public static DataTable dtlistaClasse(string cls, string cls_SEL, string srv, string usr, string pwd)
        {
            try
            {
                MPSfwk.Model.Server s = new MPSfwk.Model.Server();
                s.IPHOST = srv;
                s.USUARIO = usr;
                s.SENHA = pwd;

                ManagementScope ms = scopeMgmt(true, s);   //true = testa a conexao remota, senao
                                                            //       acaba retornando a local***
                                                            //       extrai as classes localmente...
                //teste de conexao...
                if (ms == null) { return null; }

                ManagementObjectSearcher srcd;
                //
                //testa se a Classe possui o Host ao inves do IP, se for muda o LocalAccount
                string aux_qry = "";
                if (    (srv.IndexOf(".") == -1)                                && 
                        (cls_SEL.ToUpper().IndexOf("LOCALACCOUNT = TRUE") > 0)  &&
                        (srv.ToUpper() != "LOCALHOST")
                   )
                {   aux_qry = cls_SEL.ToUpper().Replace("LOCALACCOUNT = TRUE", ("Domain = '" + srv.ToUpper() + "'")); }
                else
                {   aux_qry = cls_SEL; }
                //
                //MPS teste - 10/out
                Console.WriteLine("DEBUG - aux_qry = " + aux_qry);
                //
                srcd = new ManagementObjectSearcher(ms, new ObjectQuery(aux_qry));
                ManagementObjectCollection moc = srcd.Get();

                //Cria tabela para preencher os campos
                DataTable dt1 = new DataTable();
                dt1.TableName = cls;

                //teste...
                string aux_cls = "";
                string[] aux = cls_SEL.Split(' ');
                if (aux.Length == 3)
                { aux_cls = aux[3]; }
                else
                {
                    for (int i = 1; i < aux.Length; i++)
                    {
                        if (aux[i].ToUpper() == "FROM")
                        {
                            aux_cls = aux[i+1];
                            break;
                        }
                    }                
                }

                //Preenche o Grid com as colunas da classe WMI
                //(Caso haja campos determinados, seleciona somente os campos determinados...)
                //
                //ordena, conforme entrada..
                string[] ordem = null;
                if (cls_SEL.IndexOf("*") > 0)
                {
                    var wmiClasse = new ManagementClass(aux_cls);
                    foreach (var prop in wmiClasse.Properties)
                    { if ((cls_SEL.IndexOf(prop.Name) > 0) || (cls_SEL.IndexOf("*") > 0)) { dt1.Columns.Add(prop.Name); } }
                }
                else
                { 
                    int pos1 = cls_SEL.ToUpper().IndexOf("SELECT") + 6;
                    int pos2 = cls_SEL.ToUpper().IndexOf("FROM");
                    if (pos1 < pos2)
                    {
                        if (cls_SEL.IndexOf(",") > 0)
                        { ordem = cls_SEL.Substring(pos1, (pos2 - pos1)).Trim().Split(',', ' '); }
                        else
                        { ordem[0] = cls_SEL.Substring(pos1, (pos2 - pos1)); }
                        //
                        //Preenche as colunas com os campos determinados...
                        for (int i = 0; i < ordem.Length; i++)
                        {
                            if (ordem[i] != "")
                            { dt1.Columns.Add(ordem[i]); }
                        }
                    }
                }

                //Preenche o Grid com os valores da classe WMI
                foreach (ManagementObject mo in moc)
                {
                    DataRow dr = dt1.NewRow();

                    System.Management.PropertyDataCollection pdc = mo.Properties;
                    foreach (System.Management.PropertyData pd in pdc) { dr[pd.Name] = pd.Value; }

                    dt1.Rows.Add(dr);
                }
                //
                //
                return dt1;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
            catch (ManagementException)
            {
                return null;
            }
        }


        public static bool testeConnWMI(string srv, string usr, string pwd)
        {
            MPSfwk.Model.Server s = new MPSfwk.Model.Server();
            s.IPHOST = srv;
            s.USUARIO = usr;
            s.SENHA = pwd;
            //Chama a função para o Gerenciamento WMI remoto/local
            // se ocorre erro, tenta a conexão local...
            ManagementScope ms = scopeMgmt(true, s);
            //
            try
            {
                if (ms.IsConnected == true)
                { return true; }
                else
                { return false; }
            }
            catch (Exception)
            {
                return false;
            }

        }


        private static ManagementScope scopeMgmt(bool test, Model.Server s)
        {
            ManagementPath mp;
            ManagementScope ms = null;
            try
            {
                ConnectionOptions co = new ConnectionOptions();
                co.Impersonation = ImpersonationLevel.Impersonate;
                co.Authentication = AuthenticationLevel.Packet;
                co.Timeout = new TimeSpan(0, 0, 30);
                co.EnablePrivileges = true;
                co.Username = s.USUARIO;
                co.Password = s.SENHA;

                mp = new ManagementPath();
                mp.NamespacePath = @"\root\cimv2";
                mp.Server = s.IPHOST;

                ms = new ManagementScope(mp, co);
                //Se ocorrer erro com a conexão remota acima, tenta a local...
                ms.Connect();
            }
            catch (ManagementException me)
            {
                if (me.ErrorCode.ToString() != "LocalCredentials")
                {
                    if ((test) && (s.IPHOST.ToUpper() != "LOCALHOST"))
                    {
                        Console.WriteLine("|Info| Ocorreu um Erro de Gerenciamento - " + me.Message);
                        return null;
                    }
                }
                else
                {
                    mp = new ManagementPath();
                    mp.NamespacePath = @"\root\cimv2";
                    mp.Server = s.IPHOST;

                    ms = new ManagementScope(mp);
                    ms.Connect();
                }
            }
            catch (Exception eg)
            {
                Console.WriteLine("|Info| Ocorreu um Erro Geral - " + eg.Message);
                return null;
            }


            return ms;
        }

    }
}
