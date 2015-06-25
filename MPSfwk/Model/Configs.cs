using System;
using System.Collections.Generic;
using System.Text;

namespace MPSfwk.Model
{
    public class Configs : IDisposable
    {
        private String db_IP;
        public String DB_IP
        {
            get { return db_IP; }
            set { db_IP = value; }
        }
        //
        private String db_Port;
        public String DB_PORT
        {
            get { return db_Port; }
            set { db_Port = value; }
        }
        //
        private String ima_chunk_size;
        public String IMA_CHUNK_SIZE
        {
            get { return ima_chunk_size; }
            set { ima_chunk_size = value; }
        }
        //
        private String ima_ip;
        public String IMA_IP                                                                                              
        {
            get { return ima_ip; }
            set { ima_ip = value; }
        }
        //
        private String ima_port;
        public String IMA_PORT                                                                                            
        {
            get { return ima_port; }
            set { ima_port = value; }
        }
        //
        private String ima_role;
        public String IMA_ROLE
        {
            get { return ima_role; }
            set { ima_role = value; }
        }
        //
        private String ima_user;
        public String IMA_USER
        {
            get { return ima_user; }
            set { ima_user = value; }
        }
        //
        private String mail_from;
        public String MAIL_FROM
        {
            get { return mail_from; }
            set { mail_from = value; }
        }
        //
        private String mail_send;
        public String MAIL_SEND
        {
            get { return mail_send; }
            set { mail_send = value; }
        }
        //
        private String mail_server;
        public String MAIL_SERVER
        {
            get { return mail_server; }
            set { mail_server = value; }
        }
        //
        private String mail_to;
        public String MAIL_TO
        {
            get { return mail_to; }
            set { mail_to = value; }
        }
        //
        private String mail_use_surrogate_file;
        public String MAIL_USE_SURROGATE_FILE
        {
            get { return mail_use_surrogate_file; }
            set { mail_use_surrogate_file = value; }
        }
        //
        private String moreen_home;
        public String MOREEN_HOME
        {
            get { return moreen_home; }
            set { moreen_home = value; }
        }
        //
        private String moreen_log_level;
        public String MOREEN_LOG_LEVEL
        {
            get { return moreen_log_level; }
            set { moreen_log_level = value; }
        }
        //
        private String moreen_log_max_files;
        public String MOREEN_LOG_MAX_FILES
        {
            get { return moreen_log_max_files; }
            set { moreen_log_max_files = value; }
        }
        //
        private String moreen_log_size;
        public String MOREEN_LOG_SIZE
        {
            get { return moreen_log_size; }
            set { moreen_log_size = value; }
        }
        //
        private String port_set;
        public String PORT_SET
        {
            get { return port_set; }
            set { port_set = value; }
        }
        //
        private String secretstore_location;
        public String SECRETSTORE_LOCATION
        {
            get { return secretstore_location; }
            set { secretstore_location = value; }
        }
        //
        private String security_classification_attribute;
        public String SECURITY_CLASSIFICATION_ATTRIBUTE
        {
            get { return security_classification_attribute; }
            set { security_classification_attribute = value; }
        }
        //
        private String source_system_location;
        public String SOURCE_SYSTEM_LOCATION
        {
            get { return source_system_location; }
            set { source_system_location = value; }
        }
        //
        private String source_system_type;
        public String SOURCE_SYSTEM_TYPE
        {
            get { return source_system_type; }
            set { source_system_type = value; }
        }
        //
        private String sp_proxy_enc;
        public String SP_PROXY_ENC
        {
            get { return sp_proxy_enc; }
            set { sp_proxy_enc = value; }
        }
        //
        private String thread_amount_archive_latest;
        public String THREAD_AMOUNT_ARCHIVE_LATEST
        {
            get { return thread_amount_archive_latest; }
            set { thread_amount_archive_latest = value; }
        }
        //
        private String thread_amount_archive_other;
        public String THREAD_AMOUNT_ARCHIVE_OTHER
        {
            get { return thread_amount_archive_other; }
            set { thread_amount_archive_other = value; }
        }
        //
        private Int32 thread_amount_retrieval;
        public Int32 THREAD_AMOUNT_RETRIEVAL
        {
            get { return thread_amount_retrieval; }
            set { thread_amount_retrieval = value; }
        }
        //
        private String db_name;
        public String DB_NAME
        {
            get { return db_name; }
            set { db_name = value; }
        }
        //
        private String ll_base_uri;
        public String LL_BASE_URI
        {
            get { return ll_base_uri; }
            set { ll_base_uri = value; }
        }
        //
        private String ll_ip;
        public String LL_IP
        {
            get { return ll_ip; }
            set { ll_ip = value; }
        }
        //
        private String ll_name;
        public String LL_NAME
        {
            get { return ll_name; }
            set { ll_name = value; }
        }
        //
        private String ll_port;
        public String LL_PORT
        {
            get { return ll_port; }
            set { ll_port = value; }
        }
        //
        private String myrequestid;
        public String MYREQUESTID
        {
            get { return myrequestid; }
            set { myrequestid = value; }
        }
        //
        private String workspace_home;
        public String WORKSPACE_HOME
        {
            get { return workspace_home; }
            set { workspace_home = value; }
        }
        //
        private String id_instance;
        public String ID_INSTANCE
        {
            get { return id_instance; }
            set { id_instance = value; }
        }
        //
        private Int32 tsi_managed;
        public Int32 TSI_MANAGED
        {
            get { return tsi_managed; }
            set { tsi_managed = value; }
        }
        //
        private String archtool_max_thread_ini;
        public String ARCHTOOL_MAX_THREAD_INI
        {
            get { return archtool_max_thread_ini; }
            set { archtool_max_thread_ini = value; }
        }
        //
        private String archtool_max_thread_fim;
        public String ARCHTOOL_MAX_THREAD_FIM
        {
            get { return archtool_max_thread_fim; }
            set { archtool_max_thread_fim = value; }
        }

        //===========================================
        //shell_Planning - Rev. by MPS - 09/04/2015
        //===========================================
        /*

        ColSiteNum
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
        
        */

        private String colsitenum;
        public String COLSITENUM
        {
            get { return colsitenum; }
            set { colsitenum = value; }
        }
        //
        private String operatorname;
        public String OPERATORNAME
        {
            get { return operatorname; }
            set { operatorname = value; }
        }
        //
        private String uniqueid;
        public String UNIQUEID
        {
            get { return uniqueid; }
            set { uniqueid = value; }
        }
        //
        private String duname;
        public String DUNAME
        {
            get { return duname; }
            set { duname = value; }
        }
        //
        private String instanceid;
        public String INSTANCEID
        {
            get { return instanceid; }
            set { instanceid = value; }
        }
        //
        private String dataexpctrchk;
        public String DATAEXPCTRCHK
        {
            get { return dataexpctrchk; }
            set { dataexpctrchk = value; }
        }
        //
        private String waiverchk;
        public String WAIVERCHK
        {
            get { return waiverchk; }
            set { waiverchk = value; }
        }
        //
        private String volumegb;
        public String VOLUMEGB
        {
            get { return volumegb; }
            set { volumegb = value; }
        }
        //
        private String durationworkdays;
        public String DURATIONWORKDAYS
        {
            get { return durationworkdays; }
            set { durationworkdays = value; }
        }
        //
        private String planaoreceivedate;
        public String PLANAORECEIVEDATE
        {
            get { return planaoreceivedate; }
            set { planaoreceivedate = value; }
        }
        //
        private String actaoreceivedate;
        public String ACTAORECEIVEDATE
        {
            get { return actaoreceivedate; }
            set { actaoreceivedate = value; }
        }
        //
        private String planaostartdate;
        public String PLANAOSTARTDATE
        {
            get { return planaostartdate; }
            set { planaostartdate = value; }
        }
        //
        private String actaostartdate;
        public String ACTAOSTARTDATE
        {
            get { return actaostartdate; }
            set { actaostartdate = value; }
        }
        //
        private String planaoenddate;
        public String PLANAOENDDATE
        {
            get { return planaoenddate; }
            set { planaoenddate = value; }
        }
        //
        private String actaoenddate;
        public String ACTAOENDDATE
        {
            get { return actaoenddate; }
            set { actaoenddate = value; }
        }
        //
        private String status;
        public String STATUS
        {
            get { return status; }
            set { status = value; }
        }
        //
        private String comment;
        public String COMMENT
        {
            get { return comment; }
            set { comment = value; }
        }
        //
        //===========================================


        //===========================================
        //shell_sourcesys_x_inst
        //===========================================
        private String ssystemid;
        public String SSYSTEMID
        {
            get { return ssystemid; }
            set { ssystemid = value; }
        }


        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            GC.ReRegisterForFinalize(this);
        }

        #endregion
    }
}
