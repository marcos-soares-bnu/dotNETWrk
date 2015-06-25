// Represents a record in the "planning" table
[PetaPoco.TableName("shell_Planning")]
[PetaPoco.PrimaryKey("KeyCount")]
public class planning
{
    public int ColSiteNum { get; set; }
    public string OperatorName { get; set; }
    public string UniqueId { get; set; }
    public string DUName { get; set; }
    public string InstanceId { get; set; }
    public int DataExpCtrChk { get; set; }
    public int WaiverChk { get; set; }
    public float VolumeGB { get; set; }
    public int DurationWorkDays { get; set; }
    public System.DateTime PlanAOReceiveDate { get; set; }
    public System.DateTime ActAOReceiveDate { get; set; }
    public System.DateTime PlanAOStartDate { get; set; }
    public System.DateTime ActAOStartDate { get; set; }
    public System.DateTime PlanAOEndDate { get; set; }
    public System.DateTime ActAOEndDate { get; set; }
    public string Status { get; set; }
    public string Comment { get; set; }
    public int KeyCount { get; set; }
}

public class lapicfg
{ 
	public string id_instance { get; set; }
	public string ll_ip { get; set; }
	public string ll_port { get; set; }
	public string ll_base_uri { get; set; }
	public int tsi_managed { get; set; }
	public int archtool_max_thread_ini { get; set; }
    public int archtool_max_thread_fim { get; set; }
}

public class sysid_istid
{
    public string ssystemid { get; set; }
    public string id_instance { get; set; }
}