using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Xml;
using System.Data;
using MPSfwk.Model;

namespace SqlServer
{
    public static class SQLUtil
    {

        public class ConfigParams
        {
            public string dbname { get; set; }
            public string value { get; set; }
        }


        public static List<ConfigParams> ReadDefaultCfg()
        {
            List<ConfigParams> fields = new List<ConfigParams>();

            String sql = @"SELECT dbname, value  FROM Shell_AutoArchDB.dbo.shell_Defaultconf";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = sql;
            comm.CommandType = CommandType.Text;

            using (SqlDataReader dataReader = SQLServer.DataAccess.ExecuteReader(comm))
            {
                while (dataReader.Read())
                {
                    fields.Add(new ConfigParams
                    {
                        dbname = dataReader.GetString(0).Trim(),
                        value = dataReader.GetString(1).Trim(),
                    });                        
                }
            }

            return fields;
        }


        /// <summary>
        /// Get the Source System ID table to set configs vars...
        /// </summary>
        /// <param name="id_instance"></param>
        /// <returns></returns>
        public static string ReadSYSID(string id_instance)
        {
            String sql = @"SELECT ssystemid  FROM Shell_AutoArchDB.dbo.shell_sourcesys_x_inst  WHERE  ID_INSTANCE LIKE '%{0}%'";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = string.Format(sql, id_instance);
            comm.CommandType = CommandType.Text;

            using (SqlDataReader dataReader = SQLServer.DataAccess.ExecuteReader(comm))
            {
                if (dataReader.Read()) { return dataReader.GetString(0).Trim(); }
                else return "";
            }
        }


        /// <summary>
        /// Get the values of LAPI table to set configs vars...
        /// </summary>
        /// <param name="id_instance"></param>
        /// <returns></returns>
        public static Configs ReadLAPI(string id_instance)
        {
            Configs cfg = new Configs();
            //
            String sql = @"SELECT    ID_INSTANCE
                                    ,LL_IP
                                    ,LL_PORT
                                    ,LL_BASE_URI
                                    ,TSI_Managed
                                    ,ArchTool_Max_Thread_Ini
                                    ,ArchTool_Max_Thread_fim
                             FROM   Shell_AutoArchDB.dbo.shell_LAPIconf
                            WHERE   ID_INSTANCE LIKE '%{0}%'";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = string.Format(sql, id_instance);
            comm.CommandType = CommandType.Text;

            using (SqlDataReader dataReader = SQLServer.DataAccess.ExecuteReader(comm))
            {
                if (dataReader.Read())
                {
                    cfg.ID_INSTANCE = dataReader.GetString(0).Trim();
                    cfg.LL_IP = dataReader.GetString(1).Trim();
                    cfg.LL_PORT = dataReader.GetString(2).Trim();
                    cfg.LL_BASE_URI = dataReader.GetString(3).Trim();
                    cfg.TSI_MANAGED = dataReader.GetInt32(4);
                    cfg.ARCHTOOL_MAX_THREAD_INI = dataReader.GetInt32(5).ToString().Trim();
                    cfg.ARCHTOOL_MAX_THREAD_FIM = dataReader.GetInt32(6).ToString().Trim();
                }
            }

            return cfg;
        }


        /// <summary>
        /// Get the values of Planning table to set configs vars...
        /// </summary>
        /// <param name="unique_id"></param>
        /// <returns></returns>
        public static Configs ReadPlanning(string unique_id)
        {
            Configs cfg = new Configs();
            //
            String sql = @"SELECT   ColSiteNum
                                  , OperatorName
                                  , UniqueId
                                  , DUName
                                  , InstanceId
                                  , DataExpCtrChk
                                  , WaiverChk
                                  , VolumeGB
                                  , DurationWorkDays
                                  , PlanAOReceiveDate
                                  , ActAOReceiveDate
                                  , PlanAOStartDate
                                  , ActAOStartDate
                                  , PlanAOEndDate
                                  , ActAOEndDate
                                  , Status
                                  , Comment
                             FROM   Shell_AutoArchDB.dbo.shell_Planning
                            WHERE   UniqueId LIKE '%{0}%'";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = string.Format(sql, unique_id);
            comm.CommandType = CommandType.Text;

            using (SqlDataReader dataReader = SQLServer.DataAccess.ExecuteReader(comm))
            {
                if (dataReader.Read())
                {
                    cfg.COLSITENUM = dataReader.GetInt32(0).ToString().Trim();
                    //
                    if (!dataReader.IsDBNull(1))
                        cfg.OPERATORNAME = dataReader.GetString(1).Trim();
                    //
                    cfg.UNIQUEID = dataReader.GetString(2).Trim();
                    cfg.DUNAME = dataReader.GetString(3).Trim();
                    cfg.INSTANCEID = dataReader.GetString(4).Trim();
                    //
                    if (!dataReader.IsDBNull(5))
                        cfg.DATAEXPCTRCHK = dataReader.GetInt32(5).ToString().Trim();
                    if (!dataReader.IsDBNull(6))
                        cfg.WAIVERCHK = dataReader.GetInt32(6).ToString().Trim();
                    cfg.VOLUMEGB = dataReader.GetDecimal(7).ToString().Trim();
                    //
                    if (!dataReader.IsDBNull(8))
                        cfg.DURATIONWORKDAYS = dataReader.GetInt32(8).ToString().Trim();
                    //
                    if (!dataReader.IsDBNull(9)) { cfg.PLANAORECEIVEDATE = dataReader.GetDateTime(9).ToString().Trim(); }
                    if (!dataReader.IsDBNull(10)) { cfg.ACTAORECEIVEDATE = dataReader.GetDateTime(10).ToString().Trim(); }
                    if (!dataReader.IsDBNull(11)) { cfg.PLANAOSTARTDATE = dataReader.GetDateTime(11).ToString().Trim(); }
                    if (!dataReader.IsDBNull(12)) { cfg.ACTAOSTARTDATE = dataReader.GetDateTime(12).ToString().Trim(); }
                    if (!dataReader.IsDBNull(13)) { cfg.PLANAOENDDATE = dataReader.GetDateTime(13).ToString().Trim(); }
                    if (!dataReader.IsDBNull(14)) { cfg.ACTAOENDDATE = dataReader.GetDateTime(14).ToString().Trim(); }
                    //
                    cfg.STATUS = dataReader.GetString(15).Trim();
                    //
                    if (!dataReader.IsDBNull(16))
                        cfg.COMMENT = dataReader.GetString(16).Trim();
                }
            }

            return cfg;
        }


        /// <summary>
        /// This Function creates all control Views into Shell SQL Server...
        /// </summary>
        /// <param name="DBname"></param>
        /// <returns></returns>
        public static string CreateArchViews(Boolean ehScript, string DBname)
        {
            string ret = "";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = @"CREATE VIEW [dbo].[REPORTING] AS
                                select crequestid, 'tjob_ll_crawl' as ctable, cstatus, cmessage,  count(cstatus) as ccount from [dbo].[TJOB_LL_CRAWL] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_document' as ctable, cstatus, cmessage,  count(cstatus) as ccount from [dbo].[TJOB_DOCUMENT] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_revision' as ctable, cstatus, cmessage,  count(cstatus) as ccount from  [dbo].[TJOB_REVISION] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_archive_latest' as ctable, cstatus, cmessage, count(cstatus) as ccount from [dbo].[TJOB_ARCHIVE_LATEST] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_archive_other' as ctable, cstatus, cmessage,  count(cstatus) as ccount from [dbo].[TJOB_ARCHIVE_OTHER] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_audit' as ctable, cstatus, cmessage,  count(cstatus) as ccount from  [dbo].[TJOB_AUDIT] group by crequestid, cstatus, cmessage Union
                                select crequestid, 'tjob_delete_fs' as ctable, cstatus, cmessage,  count(cstatus) as ccount from  [dbo].[TJOB_DELETE_FS] group by crequestid, cstatus, cmessage;

                                CREATE VIEW [dbo].[CRAWL_SP_ITEMS_VIEW] AS
                                SELECT I.CITEMID, I.CSITEID, I.CLIST, I.CSTATUS, I.CMESSAGE, I.CTIMESTAMP,
                                       S.CATTRBUSUNIT, S.CATTRLOCATION, S.CWHITELIST,
	                                   S.CREQUESTID, I.CWEBSITEURL
                                FROM   [dbo].[TJOB_SP_CRAWL_ITEMS] AS I INNER JOIN
                                       [dbo].[TJOB_SP_CRAWL_SITES] AS S ON I.CSITEID = S.CCOLID;
                                ";

            try
            {
                if (ehScript)
                    ret = comm.CommandText;
                else
                {
                    SQLServer.DataAccess.ExecuteReader(comm);
                    ret = "1";
                }
            }
            catch (Exception ex)
            {
                ret = "Database (tables) error - " + ex.ToString();
            }

            return ret;
        }



        /// <summary>
        /// This Function creates all control Tables into Shell SQL Server...
        /// </summary>
        /// <param name="DBname"></param>
        /// <returns></returns>
        public static string CreateArchTables(Boolean ehScript, string DBname)
        {
            string ret = "";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = @"CREATE TABLE [dbo].[TJOB_DOCUMENT](
	                                [CMIGID] [bigint] IDENTITY(1,1) NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CPARENTID] [bigint] NOT NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CSUBTYPE] [varchar](128) NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CFILENAME] [nvarchar](1000) NULL,
	                                [CMIMETYPE] [varchar](80) NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL,
	                                [CARCHIVETIME] [datetime] NULL,
	                                [CMODIFICATIONTIME] [datetime] NULL,
	                                [CSOURCEPATH] [nvarchar](2048) NULL
                                ) ON [PRIMARY]
         
                                CREATE UNIQUE NONCLUSTERED INDEX [IX1_TJOB_DOCUMENT] ON [dbo].[TJOB_DOCUMENT] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE NONCLUSTERED INDEX [IX2_TJOB_DOCUMENT] ON [dbo].[TJOB_DOCUMENT] 
                                (
	                                [CREQUESTID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE UNIQUE NONCLUSTERED INDEX [IX3_TJOB_DOCUMENT] ON [dbo].[TJOB_DOCUMENT] 
                                (
	                                [CEXPORTPATH] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_REVISION](
	                                [CJOBID] [bigint] IDENTITY(1,1) NOT NULL,
	                                [CMIGID] [bigint] NOT NULL,
	                                [CVERSION] [int] NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL,
	                                [CLATEST] [int] NOT NULL,
	                                [CARCHIVETIME] [datetime] NULL,
	                                [CMODIFICATIONTIME] [datetime] NULL,
	                                [CSOURCEPATH] [nvarchar](2048) NULL
                                ) ON [PRIMARY]

                                ALTER TABLE [dbo].[TJOB_REVISION]  WITH CHECK ADD CONSTRAINT [CK1_TJOB_REVISION] CHECK  (([CLATEST]=(1) OR [CLATEST]=(0)))

                                ALTER TABLE [dbo].[TJOB_REVISION] CHECK CONSTRAINT [CK1_TJOB_REVISION]

                                CREATE NONCLUSTERED INDEX [IX1_TJOB_REVISION] ON [dbo].[TJOB_REVISION] 
                                (
	                                [CJOBID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE NONCLUSTERED INDEX [IX2_TJOB_REVISION] ON [dbo].[TJOB_REVISION] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_ARCHIVE_LATEST](
	                                [CMIGID] [bigint] NOT NULL,
	                                [CVERSION] [int] NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CIMADOCID] [varchar](36) NULL,
	                                [CIMAREVISIONID] [varchar](36) NULL,
	                                [CHASH] [varchar](256) NOT NULL,
	                                [CHASHALGO] [varchar](20) NOT NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL
                                ) ON [PRIMARY]

                                CREATE UNIQUE NONCLUSTERED INDEX [IX1_TJOB_ARCHIVE_LATEST] ON [dbo].[TJOB_ARCHIVE_LATEST] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_ARCHIVE_OTHER](
	                                [CMIGID] [bigint] NOT NULL,
	                                [CVERSION] [int] NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CIMADOCID] [varchar](36) NULL,
	                                [CIMAREVISIONID] [varchar](36) NULL,
	                                [CHASH] [varchar](256) NOT NULL,
	                                [CHASHALGO] [varchar](20) NOT NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL,
	                                [CMIGIDEX] [bigint] IDENTITY(1,1) NOT NULL
                                ) ON [PRIMARY]

                                CREATE NONCLUSTERED INDEX [IX1_TJOB_ARCHIVE_OTHER] ON [dbo].[TJOB_ARCHIVE_OTHER] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE NONCLUSTERED INDEX [IX2_TJOB_ARCHIVE_OTHER] ON [dbo].[TJOB_ARCHIVE_OTHER] 
                                (
	                                [CMIGIDEX] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_AUDIT](
	                                [CMIGID] [bigint] NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CIMADOCID] [varchar](36) NOT NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL
                                ) ON [PRIMARY]

                                CREATE UNIQUE NONCLUSTERED INDEX [IX1_TJOB_AUDIT] ON [dbo].[TJOB_AUDIT] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_DELETE_FS](
	                                [CMIGID] [bigint] NOT NULL,
	                                [CVERSION] [int] NOT NULL,
	                                [CISAUDITTRAIL] [int] NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CARCHIVETIME] [datetime] NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
	                                [CMIGIDEX] [bigint] IDENTITY(1,1) NOT NULL,
	                                [CEXPORTPATH] [nvarchar](255) NOT NULL
                                ) ON [PRIMARY]

                                ALTER TABLE [dbo].[TJOB_DELETE_FS]  WITH CHECK ADD CONSTRAINT [CK1_TJOB_DELETE_FS] CHECK  (([CISAUDITTRAIL]=(1) OR [CISAUDITTRAIL]=(0)))

                                ALTER TABLE [dbo].[TJOB_DELETE_FS] CHECK CONSTRAINT [CK1_TJOB_DELETE_FS]

                                CREATE NONCLUSTERED INDEX [IX1_TJOB_DELETE_FS] ON [dbo].[TJOB_DELETE_FS] 
                                (
	                                [CMIGID] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE NONCLUSTERED INDEX [IX2_TJOB_DELETE_FS] ON [dbo].[TJOB_DELETE_FS] 
                                (
	                                [CMIGIDEX] ASC
                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_DELETE_SRC](
	                                [CMIGID] [bigint] NOT NULL,
	                                [CVERSION] [int] NOT NULL,
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CDOCUMENTID] [varchar](38) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CARCHIVETIME] [datetime] NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL
                                ) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_SP_CRAWL_SITES](
	                                [CCOLID] [bigint] IDENTITY(1,1) NOT NULL,
	                                [CCOLADR] [varchar](256) NOT NULL,
	                                [CSITE] [varchar](256) NULL,
	                                [CLIST] [varchar](256) NULL,
	                                [CSOURCECOUNT] [int] NOT NULL,
	                                [CATTRBUSUNIT] [nvarchar](150) NULL,
	                                [CATTRLOCATION] [nvarchar](50) NULL,
	                                [CATTRSECURECLASS] [nvarchar](50) NULL,
	                                [CWHITELIST] [nvarchar](255) NULL,
	                                [CSTATUS] [varchar](10) NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CREQUESTID] [nvarchar](10) NOT NULL,
	                                [CTIMESTAMP] [datetime] NULL
                                ) ON [PRIMARY]

                                ALTER TABLE [dbo].[TJOB_SP_CRAWL_SITES] ADD CONSTRAINT [DF_TJOB_SP_CRAWL_SITES_CSTATUS] DEFAULT ('todo') FOR [CSTATUS]

                                ALTER TABLE [dbo].[TJOB_SP_CRAWL_SITES] ADD CONSTRAINT [DF_TJOB_SP_CRAWL_SITES_CMESSAGE] DEFAULT (N'new') FOR [CMESSAGE]

                                CREATE TABLE [dbo].[TJOB_SP_CRAWL_ITEMS](
	                                [CITEMID] [int] IDENTITY(1,1) NOT NULL,
	                                [CSITEID] [int] NOT NULL,
	                                [CLIST] [varchar](50) NOT NULL,
	                                [CWEBSITEURL] [varchar](256) NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NOT NULL,
                                    [CTIMESTAMP] [datetime] NULL,
                                    [CITEMCOUNT] [int] NOT NULL
                                ) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_LL_ORDER](
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CNODEID] [bigint] NOT NULL,
                                    [CCURRENTIDS] [nvarchar](2048) NULL,
	                                [CSOURCECOUNT] [int] NOT NULL,
	                                [CSUBTYPE] [varchar](255) NOT NULL,
	                                [CARCHIVE] [varchar](6) NOT NULL,
	                                [CRECURSIVE] [varchar](3) NOT NULL,
	                                [CNAME] [varchar](255) NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CTIMESTAMP] [datetime] NULL
                                ) ON [PRIMARY]

                                ALTER TABLE [dbo].[TJOB_LL_ORDER] WITH CHECK ADD CONSTRAINT [CK1_TJOB_LL_ORDER] CHECK  (([CSUBTYPE]=('Folder') OR [CSUBTYPE]=('Document'))
                                                                                                                        AND ([CARCHIVE]=('YES') OR [CARCHIVE]=('IGNORE'))
                                                                                                                        AND ([CRECURSIVE]=('YES') OR [CRECURSIVE]=('NO')))

                                ALTER TABLE [dbo].[TJOB_LL_ORDER] CHECK CONSTRAINT [CK1_TJOB_LL_ORDER]

                                ALTER TABLE [dbo].[TJOB_LL_ORDER] ADD CONSTRAINT [DF_TJOB_LL_ORDER_CSTATUS] DEFAULT ('todo') FOR [CSTATUS]

                                ALTER TABLE [dbo].[TJOB_LL_ORDER] ADD CONSTRAINT [DF_TJOB_LL_ORDER_CMESSAGE] DEFAULT (N'new') FOR [CMESSAGE]

                                CREATE TABLE [dbo].[TJOB_LL_CRAWL](
	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
	                                [CREQUESTID] [nvarchar](20) NOT NULL,
	                                [CORDERID] [bigint] NOT NULL,
	                                [CNODEID] [bigint] NOT NULL,
	                                [CSTATUS] [varchar](10) NOT NULL,
	                                [CMESSAGE] [nvarchar](255) NULL,
	                                [CBUSINESSUNIT] [nvarchar](30) NOT NULL,
	                                [CTIMESTAMP] [datetime] NOT NULL,
                                    [CALTERNATEPATH] [nvarchar](2048) NULL
                                ) ON [PRIMARY]

                                CREATE TABLE [dbo].[TJOB_CONFIGURATION](
	                                [dbName] [varchar](50) NOT NULL,
	                                [value] [varchar](255) NOT NULL
                                ) ON [PRIMARY]

                                ";

//===============================================================================================================================
// TABLES in TEST and not in PRODUCTION...
//----------------------------------------------------------------------------
//                                CREATE TABLE [dbo].[LL_INACTIVE_CONTAINERS](
//	                                [CSYSTEMID] [nvarchar](20) NOT NULL,
//	                                [CDATAID] [bigint] NOT NULL,
//	                                [CPARENTID] [bigint],
//	                                [CNAME] [varchar](512) NULL,
//	                                [CPATH] [varchar](5000) NULL,
//	                                [CBUNIT] [varchar](512) NULL
//                                ) ON [PRIMARY]
//
//                                CREATE UNIQUE NONCLUSTERED INDEX [IX1_LL_INACTIVE_CONTAINERS] ON [dbo].[LL_INACTIVE_CONTAINERS] 
//                                (
//	                                [CDATAID] ASC
//                                )WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
//
//                                CREATE TABLE [dbo].[LL_FOLDER_INFORMATION](
//	                                [SYSTEMID] [nvarchar](20) NOT NULL,
//	                                [DATAID] [bigint] NOT NULL,
//	                                [PARENTID] [bigint] NOT NULL,
//	                                [PATH] [varchar](5000),
//	                                [BUNIT] [nvarchar](255) 
//                                ) ON [PRIMARY]
//===============================================================================================================================
 
            try
            {
                if (ehScript)
                    ret = comm.CommandText; 
                else
                {
                    SQLServer.DataAccess.ExecuteReader(comm);
                    ret = "1";
                }
            }
            catch (Exception ex)
            {
                ret = "Database (tables) error - " + ex.ToString();
            }

            return ret;
        }



        /// <summary>
        /// update Planning DB table with the uniqueID and Volume parameter...
        /// </summary>
        /// <param name="DBname"></param>
        /// <returns></returns>
        public static string updAOVolume(string Uid, string Status, string Volume)
        {
            string ret = "";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = @"  UPDATE [dbo].[shell_Planning]
                                   SET [VolumeGB]   = '" + Volume.Replace(",",".")  + "' " +
                                     ",[Status]     = '" + Status                   + "' " +
                                "WHERE [UniqueId]   = '" + Uid                      + "' ;"; 

            try
            {
                SQLServer.DataAccess.ExecuteReader(comm);
                ret = "1";
            }
            catch (Exception ex)
            {
                ret = "Database error - " + ex.ToString();
            }

            return ret;
        }



        /// <summary>
        /// Create a Database in Shell SQL Server to manage 
        /// the information related to the Moreen process.
        /// </summary>
        /// <param name="DBname"></param>
        /// <returns></returns>
        public static string CreateArchDB(string DBname)
        {
            string ret = "";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = "CREATE DATABASE " + DBname + ";";

            try
            {
                SQLServer.DataAccess.ExecuteReader(comm);
                ret = "1";
            }
            catch (Exception ex)
            {
                ret = "Database error - " + ex.ToString();
            }

            return ret;
        }


        public static List<Audits> lstAudits(MPSfwk.Model.Audits aud_param, int tipLista, string ordBY)
        {
            SqlCommand comm = new SqlCommand();
            if (tipLista == 0)
            {
                comm.CommandText = @"SELECT
                                            ServerName,
                                            ClasseName,
                                            GeracaoDate,
                                            UltimaAcaoDate
                                       FROM ASPNETDB.dbo.ds_audit_xml" + MontaWhere(aud_param) +
                                    " ORDER BY GeracaoDate DESC";
            }
            else if (tipLista == 1)
            {
                comm.CommandText = @"SELECT
                                            ServerName,
                                            ClasseName,
                                            GeracaoDate,
                                            convert(datetime,stuff(stuff(stuff(GeracaoDate, 9, 0, ' '), 12, 0, ':'), 15, 0, ':')) ConvGeracaoDate 
                                       FROM ASPNETDB.dbo.ds_audit_xml" + MontaWhere(aud_param) +
                                    " GROUP BY ServerName, ClasseName, GeracaoDate" +
                                    " ORDER BY convert(datetime,stuff(stuff(stuff(GeracaoDate, 9, 0, ' '), 12, 0, ':'), 15, 0, ':')) " + ordBY;
            }
            else if (tipLista == 2)
            {
                comm.CommandText = @"( SELECT DISTINCT ServerName, ClasseName, MAX(GeracaoDate)
                                         FROM ASPNETDB.dbo.ds_audit_xml 
                                        WHERE GeracaoDate like '%" + aud_param.DTGeracaoFim + "%'" +
                                    "   GROUP BY ServerName, ClasseName, GeracaoDate " +
                                    ")  ORDER BY ServerName, ClasseName " + ordBY;
            }
            else if (tipLista == 3)
            {
                comm.CommandText = @"SELECT
                                            ServerName,
                                            ClasseName,
                                            GeracaoDate,
                                            UltimaAcaoDate
                                       FROM ASPNETDB.dbo.vw_ds_audit_xml_Ativos7days
                                      WHERE GeracaoDate like '" + aud_param.IDGeracao + "%' " +
                                    " ORDER BY ClasseName ASC, ServerName ASC";
            }
            else if (tipLista == 4)
            {
                comm.CommandText = @"SELECT
                                            VALUE + ' ' + SEL
                                       FROM ASPNETDB.dbo.ds_ListHosts
                                      WHERE CHK = '1'
                                        AND VALUE NOT IN (SELECT DISTINCT ServerName
                                                            FROM ASPNETDB.dbo.vw_ds_audit_xml_Ativos7days
                                                           WHERE GeracaoDate like '" + aud_param.IDGeracao + "%')";
            }


            comm.CommandType = CommandType.Text;

            using (SqlDataReader dataReader = SQLServer.DataAccess.ExecuteReader(comm))
            {
                List<Audits> _lst = new List<Audits>();
                while (dataReader.Read())
                {
                    Audits aud = new Audits();
                    aud.IDServer = dataReader.GetString(0);
                    if (tipLista != 4)
                    {
                        aud.IDClasse = dataReader.GetString(1);
                        aud.IDGeracao = dataReader.GetString(2);
                    }
                    if ((tipLista == 0) || (tipLista == 3))
                    { aud.DataUltimaAcao = dataReader.GetDateTime(3); }
                    else if (tipLista == 1)
                    { aud.CVGeracao = dataReader.GetDateTime(3).ToString(); }
                    _lst.Add(aud);
                }
                return _lst;
            }
        }

        private static string MontaWhere(MPSfwk.Model.Audits aud_param)
        {
            string Server = "  AND ServerName IN ({0}) ";
            string Classe = "  AND ClasseName IN ({0}) ";
            string Geracao = "  AND        convert(datetime,stuff(stuff(stuff(GeracaoDate, 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))" +
                             "    BETWEEN  convert(datetime,stuff(stuff(stuff('{0}', 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))" +
                             "      AND    convert(datetime,stuff(stuff(stuff('{1}', 9, 0, ' '), 12, 0, ':'), 15, 0, ':'))";

            System.Text.StringBuilder sbWhere = new System.Text.StringBuilder();

            if (!string.IsNullOrEmpty(aud_param.IDServer))
                sbWhere.AppendFormat(Server, aud_param.IDServer.ToString());

            if (!string.IsNullOrEmpty(aud_param.IDClasse))
                sbWhere.AppendFormat(Classe, aud_param.IDClasse.ToString());

            if ((!string.IsNullOrEmpty(aud_param.DTGeracaoIni)) && 
                (!string.IsNullOrEmpty(aud_param.DTGeracaoFim)) &&
                (aud_param.DTGeracaoIni != "000000")            && 
                (aud_param.DTGeracaoFim != "235900") 
               )
                sbWhere.AppendFormat(Geracao, aud_param.DTGeracaoIni.ToString(), aud_param.DTGeracaoFim.ToString());

            if (sbWhere.Length > 0)
            {
                sbWhere.Remove(0, 5);
                sbWhere.Insert(1, " Where ", 1);
            }
            else
            {
                return string.Empty;
            }

            return sbWhere.ToString();
        }


        public static Boolean Gravar(string classe, string server, string geracao, XmlDocument xmlToSave)
        {
            SqlCommand comm = new SqlCommand();
            comm.CommandText = @"[ds_auditxml_InsertXML]";
            comm.CommandType = CommandType.StoredProcedure;

            //Transformar de XML para String
            String xml = xmlToSave.OuterXml;

            //Tipando o parâmetro para XML.
            SqlParameter param = new SqlParameter("@XmlFile", SqlDbType.Xml);
            param.Value = xml;
            comm.Parameters.Add(param);
            //
            comm.Parameters.Add(new SqlParameter("@ClasseName", classe));
            comm.Parameters.Add(new SqlParameter("@ServerName", server));
            comm.Parameters.Add(new SqlParameter("@GeracaoDate", geracao));
            //
            SQLServer.DataAccess.ExecuteNonQuery(comm);

            return true;
        }


        public static Boolean Atualizar(string classe, string server, string geracao, XmlDocument xmlToSave)
        {
            SqlCommand comm = new SqlCommand();
            comm.CommandText = @"[ds_auditxml_UpdateXML]";
            comm.CommandType = CommandType.StoredProcedure;

            //Transformar de XML para String
            String xml = xmlToSave.OuterXml;

            //Tipando o parâmetro para XML.
            SqlParameter param = new SqlParameter("@XmlFile", SqlDbType.Xml);
            param.Value = xml;
            comm.Parameters.Add(param);
            //
            comm.Parameters.Add(new SqlParameter("@ClasseName", classe));
            comm.Parameters.Add(new SqlParameter("@ServerName", server));
            comm.Parameters.Add(new SqlParameter("@GeracaoDate", geracao));
            //
            SQLServer.DataAccess.ExecuteNonQuery(comm);

            return true;

        } //MPS Add upd 08/10...


        public static XmlDocument LerXML(string classe, string server, string geracao)
        {
            XmlDocument xml = new XmlDocument();
            //
            String sql = @"SELECT XmlFile 
                             FROM dbo.ds_audit_xml
                            WHERE ClasseName  = '{0}'
                              AND ServerName  = '{1}'
                              AND GeracaoDate LIKE '%{2}%'
                         ORDER BY GeracaoDate DESC";

            SqlCommand comm = new SqlCommand();
            comm.CommandText = string.Format(sql, classe, server, geracao);
            comm.CommandType = CommandType.Text;

            string xmlDb = (String)SQLServer.DataAccess.ExecuteScalar(comm);

            if (!string.IsNullOrEmpty(xmlDb))
            {
                xml.LoadXml(xmlDb);
            }

            return xml;
        }

    }
}
