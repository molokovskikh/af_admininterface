using System;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Web.UI;
using MySql.Data.MySqlClient;

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
		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToDouble(Session["AccessGrant"]) != 1)
			    Response.Redirect("default.aspx");

			uint actionType = Convert.ToUInt32(Request["id"]);
			if (actionType > 5)
				throw new ArgumentException("id", "Значение id, значение id не может быть больше 5.");

			MyCn.ConnectionString = Literals.GetConnectionString();
			MyCn.Open();

			if (actionType != 4)
				MyCmd.CommandText =			
@"
SELECT  Logtime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.prgdataex p, usersettings.clientsdata, accessright.showright, farm.regions r, usersettings.retclientsset rcs)  
WHERE   p.LogTime                            >curDate() 
        AND rcs.clientcode                   =p.clientcode 
        AND firmcode                         =p.clientcode 
        AND r.regioncode                     =clientsdata.regioncode 
        AND showright.regionmask & maskregion>0 
";

			if (actionType == 0)
			{
				MyCmd.CommandText += " and EXEVersion>0 and UpdateType=5";
				HeaderLB.Text = "Запреты:";
			}

			if (actionType == 1)
			{
				MyCmd.CommandText += " and UpdateType=2";
				HeaderLB.Text = "Кумулятивные обновления:";
			}

			if (actionType == 2)
			{
				MyCmd.CommandText += " and UpdateType=1";
				HeaderLB.Text = "Обычные обновления:";
			}

			if (actionType == 3)
			{
				MyCmd.CommandText += " and EXEVersion>0 and UpdateType=6";
				HeaderLB.Text = "Ошибки подготовки данных:";
			}

			if (actionType == 4)
			{
				MyCmd.CommandText = 
@" 
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
WHERE     clientsdata.firmcode                            = rcs.clientcode  
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
				MyCmd.Parameters.Add("UserName", Session["UserName"]);
				HeaderLB.Text = "В процессе получения обновления:";
			}

			if (actionType == 5)
			{
				MyCmd.CommandText += " and UpdateType=3";
				HeaderLB.Text = "Докачки:";
			}
			if (actionType != 4)
				MyCmd.CommandText += " and showright.username='" + Session["UserName"] + "'" + " group by p.rowid" + " order by p.logtime desc ";

			CountLB.Text = Convert.ToString(MyDA.Fill(DS, "Table"));
			CLList.DataBind();
			MyCn.Close();
		}
	}


}

