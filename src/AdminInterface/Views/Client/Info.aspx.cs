using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ActiveDs;
using AddUser;
using Castle.MonoRail.Framework;
using MySql.Data.MySqlClient;

namespace AdminInterface.Views.Client
{
	partial class Info : Page, IControllerAware
	{
		private uint ClientCode
		{
			get { return (uint)(Session["ClientCode"] ?? 0u); }
			set { Session["ClientCode"] = value; }
		}

		private DataSet Data
		{
			get { return (DataSet)Session["InfoDataSet"]; }
			set { Session["InfoDataSet"] = value; }
		}

		private readonly MySqlCommand _command = new MySqlCommand();
		private readonly MySqlConnection _connection = new MySqlConnection(Literals.GetConnectionString());
		private Controller _controller;

		public Controller Controller
		{
			get { return _controller; }
		}

		private void GetMessages()
		{
			try
			{
				MySqlDataAdapter logsDataAddapter = new MySqlDataAdapter(@"
        SELECT  WriteTime as Date, 
                UserName, 
                Message 
        FROM    logs.clientsinfo 
        WHERE   clientcode=?ClientCode
		ORDER BY date DESC;
", _connection);
				logsDataAddapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
				logsDataAddapter.SelectCommand.Parameters.Add("?ClientCode", ClientCode);

				if (Data.Tables["Logs"] != null)
					Data.Tables["Logs"].Clear();
				_connection.Open();
				logsDataAddapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				logsDataAddapter.Fill(Data, "Logs");
				logsDataAddapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			try
			{
				if (String.IsNullOrEmpty(ProblemTB.Text))
					return;

				_connection.Open();
				_command.Connection = _connection;
				_command.Transaction = _connection.BeginTransaction(IsolationLevel.RepeatableRead);
				_command.CommandText =
					@"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;

INSERT 
INTO    logs.clientsinfo VALUES
        (
                null, 
                ?UserName, 
                now(), 
                ?ClientCode, 
                ?Problem
        );
";
				_command.Parameters.Add("?Problem", ProblemTB.Text);
				_command.Parameters.Add("?UserName", Session["UserName"]);
				_command.Parameters.Add("?UserHost", HttpContext.Current.Request.UserHostAddress);
				_command.Parameters.Add("?ClientCode", ClientCode);

				_command.ExecuteNonQuery();
				_command.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			ProblemTB.Text = String.Empty;
			IsMessafeSavedLabel.Visible = true;
			
			GetMessages();
			LogsGrid.DataSource = Data;
			LogsGrid.DataBind();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				ClientCode = Convert.ToUInt32(Request["cc"]);

				OrderHistoryHL.NavigateUrl = "~/orders.aspx?cc=" + ClientCode;
				OrderHistoryHL.NavigateUrl = "~/orders.aspx?cc=" + ClientCode;
				UpdateListHL.NavigateUrl = "~/Logs/UpdateLog.rails?clientCode=" + ClientCode;
				DocumentLog.NavigateUrl = "~/Logs/DocumentLog.rails?clientCode=" + ClientCode;
				UserInterfaceHL.NavigateUrl = "https://stat.analit.net/ci/auth/logon.aspx?sid=" + ClientCode;
				
				GetData();
				ConnectDataSource();
				DataBind();
			}
		}

		private void ConnectDataSource()
		{
			LogsGrid.DataSource = Data.Tables["Logs"];

			FullNameText.Text = Data.Tables["Info"].Rows[0]["FullName"].ToString();
			ShortNameText.Text = Data.Tables["Info"].Rows[0]["ShortName"].ToString();
			ShortNameLB.Text = Data.Tables["Info"].Rows[0]["ShortName"].ToString();
			AddressText.Text = Data.Tables["Info"].Rows[0]["Adress"].ToString();
			FaxText.Text = Data.Tables["Info"].Rows[0]["Fax"].ToString();
			UrlText.Text = Data.Tables["Info"].Rows[0]["URL"].ToString();
	
			UserInterfaceHL.Enabled = Convert.ToBoolean(Data.Tables["Info"].Rows[0]["AlowInterface"]);
			UpdateListHL.Enabled = Convert.ToBoolean(Data.Tables["Info"].Rows[0]["FirmType"]);
			if (Convert.ToInt32(Data.Tables["Info"].Rows[0]["FirmType"]) == 1)
				ConfigHL.NavigateUrl = "~/manageret";
			else
				ConfigHL.NavigateUrl = "~/managep";
			ConfigHL.NavigateUrl += ".aspx?cc=" + ClientCode;
			BillingLink.NavigateUrl = String.Format("~/Billing/edit.rails?ClientCode={0}", ClientCode);
		}

		private void GetData()
		{
			Data = new DataSet();

			GetMessages();

			MySqlDataAdapter infoDataAdapter = new MySqlDataAdapter(
				@"
SELECT  cd.FullName,   
        cd.ShortName,   
        cd.Adress,   
        cd.Fax,   
        cd.URL,    
        (if(regionaladmins.UseRegistrant                =1, Registrant=?UserName, 1=1))   
        AND (regionaladmins.regionmask & cd.regioncode  >0)   
        AND (if(AlowRetailInterface+AlowVendorInterface =2, 1=1, if(Alowretailinterface=1, firmtype=Alowretailinterface, if(AlowVendorInterface=1, firmtype=0, 0)))) as AlowInterface,   
        FirmType,   
        ouar.OsUserName,
		length(rui.UniqueCopyID) = 0 as Length
FROM    (clientsdata cd, accessright.regionaladmins)     
	LEFT JOIN OsUserAccessRight ouar ON ouar.ClientCode = cd.FirmCode  
	LEFT JOIN ret_update_info rui ON rui.ClientCode = cd.FirmCode
WHERE   FirmType                                 =if(ShowRetail + ShowVendor=2, FirmType, if(ShowRetail = 1, 1, 0))   
        AND cd.regioncode & regionaladmins.regionmask > 0   
        AND if(regionaladmins.UseRegistrant      =1, Registrant=?UserName, 1=1)   
        AND firmcode                             =?ClientCode   
        AND regionaladmins.username              =?UserName;
", _connection);
			infoDataAdapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
			infoDataAdapter.SelectCommand.Parameters.Add("?ClientCode", ClientCode);
				
			MySqlDataAdapter showClientsAdapter = new MySqlDataAdapter(@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName     
FROM    (accessright.regionaladmins, clientsdata as cd) 
	JOIN ShowRegulation sr ON sr.ShowClientCode = cd.FirmCode
WHERE   sr.PrimaryClientCode				 = ?ClientCode
		AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName               = ?UserName 
        AND FirmType                         = if(ShowRetail+ShowVendor = 2, FirmType, if(ShowRetail = 1, 1, 0)) 
        AND if(UseRegistrant                 = 1, Registrant = ?UserName, 1 = 1)   
        AND FirmStatus    = 1 
        AND billingstatus = 1 
ORDER BY cd.shortname;
", _connection);

			showClientsAdapter.SelectCommand.Parameters.Add("?ClientCode", ClientCode);
			showClientsAdapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
            
			try
			{
				_connection.Open();
				MySqlTransaction transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				showClientsAdapter.SelectCommand.Transaction = transaction;
				infoDataAdapter.SelectCommand.Transaction = transaction;
				showClientsAdapter.Fill(Data, "ShowClients");
				infoDataAdapter.Fill(Data, "Info");
				transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			if (Data.Tables["Info"].Rows[0]["FirmType"].ToString() == "1")
			{
				DeletePrepareDataButton.Enabled = File.Exists(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", ClientCode));
				bool isNotSet = false;
				if (Data.Tables["Info"].Rows[0]["Length"] != DBNull.Value)
					isNotSet = Convert.ToBoolean(Data.Tables["Info"].Rows[0]["Length"]);
				ResetCopyIDCB.Enabled = !isNotSet;
				ResetIDCause.Enabled = !isNotSet;
				ResetIDCause.Visible = !isNotSet;
				ResearReasonLable.Visible = !isNotSet;
				IsUniqueCopyIDSet.Visible = isNotSet;
			}
			else
			{
				ResetUINRow.Visible = false;
				DeletePrepareDataRow.Visible = false;
			}

			if (Data.Tables["Info"].Rows[0]["OsUserName"] != DBNull.Value || !String.IsNullOrEmpty(Data.Tables["Info"].Rows[0]["OsUserName"].ToString()))
			{
				try
				{
					IADsUser ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + Data.Tables["Info"].Rows[0]["OsUserName"]) as IADsUser;
					UnlockButton.Enabled = ADUser.IsAccountLocked;
					UnlockedLabel.Visible = false;
				}
				catch
				{
					UnlockButton.Enabled = false;
				}
			}
			else
				UnlockButton.Enabled = false;

		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			MySqlTransaction transaction = null;
			try
			{
				_connection.Open();
				transaction = _connection.BeginTransaction(IsolationLevel.RepeatableRead);

				_command.Connection = _connection;
				_command.CommandText = @"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;

UPDATE ClientsData
SET FullName = ?FullName,
	ShortName = ?ShortName,
	Adress = ?Address,
	Fax = ?Fax,
	URL = ?Url
WHERE firmcode = ?ClientCode  
";
				_command.Parameters.Add("?ClientCode", ClientCode);
				_command.Parameters.Add("?FullName", FullNameText.Text);
				_command.Parameters.Add("?ShortName", ShortNameText.Text);
				_command.Parameters.Add("?Address", AddressText.Text);
				_command.Parameters.Add("?Fax", FaxText.Text);
				_command.Parameters.Add("?Url", UrlText.Text);
				_command.Parameters.Add("?UserName", Session["UserName"]);
				_command.Parameters.Add("?UserHost", HttpContext.Current.Request.UserHostAddress);
				_command.ExecuteNonQuery();

				transaction.Commit();
				
				IsMessafeSavedLabel.Visible = false;
			}
			catch (Exception ex)
			{ 
				if (transaction != null)
					transaction.Rollback();
				throw new Exception("������ �� �������� Info.aspx", ex);
			}
			finally
			{
				_connection.Close();
			}
		}

		protected void UnlockButton_Click(object sender, EventArgs e)
		{
			IADsUser ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + Data.Tables["Info"].Rows[0]["OsUserName"]) as IADsUser;
			ADUser.IsAccountLocked = false;
			ADUser.SetInfo();
			UnlockButton.Visible = false;
			UnlockedLabel.Visible = true;
		}

		public void SetController(Controller controller)
		{
			_controller = controller;
		}

		protected void DeletePrepareDataButton_Click(object sender, EventArgs e)
		{
			try
			{
				File.Delete(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", ClientCode));
				DeletePrepareDataButton.Enabled = false;
				DeleteLabel.Text = "�������������� ������ �������";
				DeleteLabel.ForeColor = Color.Green;
			}
			catch
			{
				DeleteLabel.Text = "������ �������� �������������� ������, ���������� �������.";
				DeleteLabel.ForeColor = Color.Red;
			}
		}

		protected void ResetUniqueCopyID(object sender, EventArgs e)
		{
			using (MySqlConnection connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				MySqlCommand command = connection.CreateCommand();
				command.CommandText = @"
set @inHost = ?Host;
set @inUser = ?UserName;
set @ResetIdCause = ?ResetIdCause;

INSERT INTO `logs`.clientsinfo (UserName, WriteTime, ClientCode, Message)
VALUE (?UserName, now(), ?ClientCode, concat('$$$��������� ���: ', ?ResetIdCause));

UPDATE ret_update_info SET UniqueCopyID = ''  WHERE clientcode=?clientCode;
";
				command.Parameters.AddWithValue("?ClientCode", ClientCode);
				command.Parameters.AddWithValue("?ResetIdCause", ResetIDCause.Text);
				command.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
				command.Parameters.AddWithValue("?UserName", Session["UserName"]);
				command.ExecuteNonQuery();
				Response.Redirect(String.Format("info.rails?cc={0}", ClientCode));
			}
		}
	}
}