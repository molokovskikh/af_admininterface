using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Web;
using System.Web.Mail;
using System.Web.UI;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class CopySynonym : Page
	{
		protected DataSet DS;
		protected DataTable Regions;
		protected DataColumn DataColumn1;
		protected DataColumn DataColumn2;
		MySqlConnection _connection = new MySqlConnection();
		MySqlDataAdapter DA = new MySqlDataAdapter();
		protected DataTable From;
		protected DataColumn DataColumn3;
		protected DataColumn DataColumn4;
		protected DataColumn DataColumn5;
		protected DataColumn DataColumn6;
		protected DataTable ToT;
		string UserName;
		protected DataColumn DataColumn7;

		[DebuggerStepThrough()]
		private void InitializeComponent()
		{
			DS = new DataSet();
			Regions = new DataTable();
			DataColumn1 = new DataColumn();
			DataColumn2 = new DataColumn();
			DataColumn7 = new DataColumn();
			From = new DataTable();
			DataColumn3 = new DataColumn();
			DataColumn4 = new DataColumn();
			ToT = new DataTable();
			DataColumn5 = new DataColumn();
			DataColumn6 = new DataColumn();
			DS.BeginInit();
			Regions.BeginInit();
			From.BeginInit();
			ToT.BeginInit();
			DS.DataSetName = "NewDataSet";
			DS.Locale = new CultureInfo("ru-RU");
			DS.Tables.AddRange(new DataTable[] {Regions, From, ToT});
			Regions.Columns.AddRange(new DataColumn[] {DataColumn1, DataColumn2, DataColumn7});
			Regions.TableName = "Regions";
			DataColumn1.ColumnName = "Region";
			DataColumn2.ColumnName = "RegionCode";
			DataColumn2.DataType = typeof (Int64);
			DataColumn7.ColumnName = "Email";
			From.Columns.AddRange(new DataColumn[] {DataColumn3, DataColumn4});
			From.TableName = "From";
			DataColumn3.ColumnName = "Name";
			DataColumn4.ColumnName = "ClientCode";
			DataColumn4.DataType = typeof (Int32);
			ToT.Columns.AddRange(new DataColumn[] {DataColumn5, DataColumn6});
			ToT.TableName = "ToT";
			DataColumn5.ColumnName = "Name";
			DataColumn6.ColumnName = "ClientCode";
			DataColumn6.DataType = typeof (Int32);
			DS.EndInit();
			Regions.EndInit();
			From.EndInit();
			ToT.EndInit();
		}

		protected override void OnInit(EventArgs e)
		{
			base.OnInit(e);
			InitializeComponent();
		}

		protected void FindBT_Click(object sender, EventArgs e)
		{
			FindClient(FromTB.Text, "From");
			FindClient(ToTB.Text, "ToT");
			FromL.Text = DS.Tables["from"].Rows.Count.ToString();
			ToL.Text = DS.Tables["tot"].Rows.Count.ToString();
			if (DS.Tables["from"].Rows.Count > 0)
			{
				if (DS.Tables["from"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"От\". Уточните условие поиска.";
					return;
				}
				FromTB.Visible = false;
				FromDD.Visible = true;
				FromDD.DataBind();
			}
			if (DS.Tables["tot"].Rows.Count > 0)
			{
				if (DS.Tables["tot"].Rows.Count > 50)
				{
					LabelErr.Text = "Найдено более 50 записей \"Для\". Уточните условие поиска.";
					return;
				}
				ToTB.Visible = false;
				ToDD.Visible = true;
				ToDD.DataBind();
			}
			if (DS.Tables["tot"].Rows.Count > 0 & DS.Tables["from"].Rows.Count > 0)
			{
				SetBT.Enabled = true;
				FindBT.Enabled = false;
				SetBT.Visible = true;
			}
			else
			{
				SetBT.Visible = false;
			}
		}

		public void FindClient(string NameStr, string Where)
		{
			DA.SelectCommand = new MySqlCommand(
@"
SELECT  FirmCode as ClientCode, 
        convert(concat(FirmCode, '. ', ShortName) using cp1251) name 
FROM    clientsdata, 
        accessright.regionaladmins 
WHERE   regioncode     = ?RegionCode 
        AND firmtype   = 1 
        AND firmstatus = 1 
        AND shortname like ?NameStr  
        AND UserName         = ?UserName 
        AND FirmSegment      = if(regionaladmins.AlowChangeSegment = 1, FirmSegment, DefaultSegment) 
        AND if(UseRegistrant = 1, Registrant = ?UserName, 1 = 1)  
        AND username         = ?UserName;
", _connection);
			DA.SelectCommand.Parameters.Add("?NameStr", String.Format("%{0}%", NameStr));
			DA.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
			DA.SelectCommand.Parameters.Add("?RegionCode", RegionDD.SelectedItem.Value);
			try
			{
				_connection.Open();
				DA.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				DA.Fill(DS, Where);
				DA.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
		}

		protected void SetBT_Click(object sender, EventArgs e)
		{
			int ClientCode = Convert.ToInt32(ToDD.SelectedItem.Value); 
			int ParentClientCode = Convert.ToInt32(FromDD.SelectedItem.Value);
			MySqlCommand MyCommand = new MySqlCommand();
			MySqlTransaction MyTrans = null;
			try
			{
				MyCommand.CommandText =
@"
set @inHost = ?Host;
set @inUser = ?UserName;

UPDATE intersection_update_info   
        SET MaxSynonymCode   = 0,  
        MaxSynonymFirmCrCode = 0,  
        LastSent             = default  
WHERE   ClientCode           = ?ClientCode;    

UPDATE ret_update_info  as a,  
        ret_update_info as b   
        SET b.updatetime = a.updatetime  
WHERE   a.clientcode     = ?ParentClientCode   
        AND b.clientcode = ?ClientCode;    

UPDATE intersection_update_info  as a,  
        intersection_update_info as b   
        SET a.MaxSynonymFirmCrCode = b.MaxSynonymFirmCrCode,  
        a.MaxSynonymCode           = b.MaxSynonymCode  
WHERE   a.clientcode               = ?ClientCode   
        AND b.clientcode           = ?ParentClientCode   
        AND a.pricecode            = b.pricecode;   

UPDATE intersection 
        SET MaxSynonymCode   = 0, 
        MaxSynonymFirmCrCode = 0,   
        lastsent             = default 
WHERE   clientcode           = ?ClientCode;  
UPDATE retclientsset  as a, 
        retclientsset as b  
        SET b.updatetime       = a.updatetime, 
        b.AlowCumulativeUpdate = 0, 
        b.Active               = 0 
WHERE   a.clientcode           = ?ParentClientCode  
        AND b.clientcode       = ?ClientCode;  
UPDATE intersection  as a, 
        intersection as b  
        SET a.MaxSynonymFirmCrCode = b.MaxSynonymFirmCrCode,   
        a.MaxSynonymCode           = b.MaxSynonymCode  
WHERE   a.clientcode               = ?ClientCode  
        AND b.clientcode           = ?ParentClientCode 
        AND a.pricecode            = b.pricecode;  
INSERT 
INTO    logs.clone 
        (
                LogTime, 
                UserName, 
                FromClientCode, 
                ToClientCode
        ) 
        VALUES 
        (
                now(), 
                ?UserName, 
                ?ParentClientCode, 
                ?ClientCode
        );
";
				MyCommand.Parameters.Add("?Host", HttpContext.Current.Request.UserHostAddress);
				MyCommand.Parameters.Add("?UserName", Session["UserName"]);
				MyCommand.Parameters.Add("?ClientCode", ClientCode);
				MyCommand.Parameters.Add("?ParentClientCode", ParentClientCode);

				_connection.Open();
				MyTrans = _connection.BeginTransaction(IsolationLevel.RepeatableRead);
				MyCommand.Transaction = MyTrans;
				MyCommand.Connection = _connection;
				MyCommand.ExecuteNonQuery();
				MyTrans.Commit();
			}
			catch (Exception ex)
			{
				if (MyTrans != null)
					MyTrans.Rollback();
				throw new Exception("Ошибка сохранения данных на форме CopySynonym.aspx", ex);
			}
			finally
			{
				_connection.Close();
			}
#if !DEBUG
			Func.Mail("register@analit.net", String.Empty, "Успешное присвоение кодов(" + ParentClientCode + " > " + ClientCode + ")",
					false,
					"От: " + FromDD.SelectedItem.Text + "\nДля: " + ToDD.SelectedItem.Text + "\nОператор: " + UserName,
					DS.Tables["Regions"].Rows[0]["email"].ToString(), String.Empty, "RegisterList@subscribe.analit.net", Encoding.UTF8);
#endif
			LabelErr.ForeColor = Color.Green;
			LabelErr.Text = "Присвоение успешно завершено.Время операции: " + DateTime.Now;
			FromDD.Visible = false;
			ToDD.Visible = false;
			FromTB.Visible = true;
			ToTB.Visible = true;
			FindBT.Visible = true;
			FindBT.Enabled = true;
			SetBT.Visible = false;

		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

			_connection.ConnectionString = Literals.GetConnectionString();
			UserName = Session["UserName"].ToString();
			DA.SelectCommand = new MySqlCommand(
@"
SELECT  regions.region, 
        regions.regioncode, 
        Email 
FROM    accessright.regionaladmins, 
        farm.regions 
WHERE   accessright.regionaladmins.regionmask & farm.regions.regioncode > 0 
        AND username                                                    = ?UserName 
ORDER BY region;
", _connection);
            DA.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
			try
			{
				_connection.Open();
				DA.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				DA.Fill(DS, "Regions");
				DA.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			if (!IsPostBack)
				RegionDD.DataBind();
		}
	}
}
