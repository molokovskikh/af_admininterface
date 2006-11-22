using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using MySql.Data.MySqlClient;
using System.Text;

namespace AddUser
{
	public partial class viewcl : Page
	{

		protected DataSet DS;
		protected DataTable DataTable1;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected MySqlCommand MyCmd;
		protected MySqlDataAdapter MyDA;
		protected MySqlConnection MyCn;
		protected DataColumn DataColumn5;

		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			DS = new DataSet();
			DataTable1 = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			DataColumn5 = new DataColumn();
			MyCmd = new MySqlCommand();
			MyCn = new MySqlConnection();
			MyDA = new MySqlDataAdapter();
			DS.BeginInit();
			DataTable1.BeginInit();
			// 
			// DS
			// 
			DS.DataSetName = "DS";
			DS.Locale = new CultureInfo("ru-RU");
			DS.Tables.AddRange(new DataTable[] { DataTable1 });
			// 
			// DataTable1
			// 
			DataTable1.Columns.AddRange(new DataColumn[] { DataColumn1, DataColumn2, DataColumn3, DataColumn4, DataColumn5 });
			DataTable1.TableName = "Table";
			// 
			// DataColumn1
			// 
			DataColumn1.ColumnName = "LogTime";
			DataColumn1.DataType = typeof(DateTime);
			// 
			// DataColumn2
			// 
			DataColumn2.ColumnName = "FirmCode";
			// 
			// DataColumn3
			// 
			DataColumn3.ColumnName = "ShortName";
			// 
			// DataColumn4
			// 
			DataColumn4.ColumnName = "Addition";
			// 
			// DataColumn5
			// 
			DataColumn5.ColumnName = "Region";
			// 
			// _command
			// 
			MyCmd.CommandText = null;
			MyCmd.CommandTimeout = 0;
			MyCmd.CommandType = CommandType.Text;
			MyCmd.Connection = MyCn;
			MyCmd.Transaction = null;
			MyCmd.UpdatedRowSource = UpdateRowSource.Both;
			// 
			// MyDA
			// 
			MyDA.DeleteCommand = null;
			MyDA.InsertCommand = null;
			MyDA.SelectCommand = MyCmd;
			MyDA.UpdateCommand = null;

			DS.EndInit();
			DataTable1.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		protected DateTime BeginDate
		{
			get { return Convert.ToDateTime(Request["BeginDate"]); }
		}

		protected DateTime EndDate
		{
			get { return Convert.ToDateTime(Request["EndDate"]); }
		}

		protected ulong RegionMask
		{
			get { return Convert.ToUInt64(Request["RegionMask"]); }
		}

		protected StatisticsType RequestType
		{
			get { return (StatisticsType)Convert.ToUInt32(Request["id"]); }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToDouble(Session["AccessGrant"]) != 1)
			    Response.Redirect("default.aspx");

			string headerText = String.Empty;
			switch (RequestType)
			{ 
				case StatisticsType.UpdateBan:
					headerText = "Запреты:";
					MyCmd.CommandText = @"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND EXEVersion						  > 0 
		AND UpdateType						  = 5
        AND showright.regionmask & maskregion > 0 
		AND showright.username = ?UserName 
		AND p.LogTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.rowid 
ORDER by p.logtime desc;
";
					break;
				case StatisticsType.UpdateCumulative:
					headerText = "Кумулятивные обновления:";
					MyCmd.CommandText = @"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType					  = 2
        AND showright.regionmask & maskregion > 0 
		AND showright.username = ?UserName 
		AND p.LogTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.rowid 
ORDER by p.logtime desc;
";
					break;
				case StatisticsType.UpdateError:
					headerText = "Ошибки подготовки данных:";
					MyCmd.CommandText = @"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND EXEVersion						  > 0 
		AND UpdateType						  = 6
        AND showright.regionmask & maskregion > 0 
		AND showright.username = ?UserName 
		AND p.LogTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.rowid 
ORDER by p.logtime desc;
";
					break;
				case StatisticsType.UpdateNormal:
					headerText = "Обычные обновления:";
					MyCmd.CommandText = @"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 1
        AND showright.regionmask & maskregion > 0 
		AND showright.username = ?UserName 
		AND p.LogTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.rowid 
ORDER by p.logtime desc;
";
					break;
				case StatisticsType.InUpdateProcess:
					MyCmd.CommandText = @" 
SELECT  showright.RowID,   
        p.Logtime,   
        clientsdata.FirmCode,   
        clientsdata.ShortName,   
        r.Region,   
        p.Addition  
FROM      (usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)   
LEFT JOIN logs.prgdataex p 
        ON p.clientcode                                   = rcs.clientcode 
        AND p.Logtime                                     > curdate()  
WHERE   clientsdata.firmcode                            = rcs.clientcode  
        AND r.regioncode                                  = clientsdata.regioncode  
        AND showright.username                            = ?UserName
        AND showright.regionmask & clientsdata.maskregion > 0  
        AND rcs.UncommittedUpdateTime                    >= CURDATE()  
        AND rcs.UpdateTime                               <> rcs.UncommittedUpdateTime  
        AND p.RowID                                       = 
        (SELECT max(pl.RowID) 
        FROM    logs.prgdataex pl 
        WHERE   pl.clientcode = rcs.clientcode
        )  
ORDER BY p.Logtime desc;
";
					headerText = "В процессе получения обновления:";
					break;
				case StatisticsType.Download:
					MyCmd.CommandText = @"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)
WHERE   p.LogTime > curDate()
		AND rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 3
        AND showright.regionmask & maskregion > 0 
		AND showright.username = ?UserName 
GROUP by p.rowid 
ORDER by p.logtime desc;
";
					headerText = "Докачки:";
					break;
			}
			try
			{
				HeaderLB.Text = headerText;
				MyCn.ConnectionString = Literals.GetConnectionString();
				MyCn.Open();
				MyDA.SelectCommand.Transaction = MyCn.BeginTransaction(IsolationLevel.ReadCommitted);
				MyDA.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
				MyDA.SelectCommand.Parameters.Add("?BeginDate", BeginDate);
				MyDA.SelectCommand.Parameters.Add("?EndDate", EndDate);
				MyDA.SelectCommand.Parameters.Add("?RegionMask", RegionMask);

				CountLB.Text = Convert.ToString(MyDA.Fill(DS, "Table"));
				MyDA.SelectCommand.Transaction.Commit();
			}
			finally
			{
				MyCn.Close();
			}
			CLList.DataBind();
		}
	}
}

