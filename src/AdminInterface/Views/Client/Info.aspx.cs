using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Web;
using System.Web.UI;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using Castle.MonoRail.Framework;
using Common.Web.Ui.Models;
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
		protected Controller Controller;
		protected IControllerContext ControllerContext;

		private void GetMessages()
		{
			try
			{
				var logsDataAddapter = new MySqlDataAdapter(@"
SELECT  WriteTime as Date, 
		UserName, 
        Message 
FROM  logs.clientsinfo 
WHERE clientcode=?ClientCode
ORDER BY date DESC;
", _connection);
				logsDataAddapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				logsDataAddapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

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
				_command.CommandText = @"
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
				_command.Parameters.AddWithValue("?Problem", ProblemTB.Text);
				_command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				_command.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
				_command.Parameters.AddWithValue("?ClientCode", ClientCode);

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
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.ViewSuppliers,
			                                                    PermissionType.ViewDrugstore);

			if (IsPostBack) 
				return;

			ClientCode = Convert.ToUInt32(Request["cc"]);

			OrderHistoryHL.NavigateUrl = "~/orders.aspx?cc=" + ClientCode;
			UpdateListHL.NavigateUrl = "~/Logs/UpdateLog.rails?clientCode=" + ClientCode;
			DocumentLog.NavigateUrl = "~/Logs/DocumentLog.rails?clientCode=" + ClientCode;
			UserInterfaceHL.NavigateUrl = "https://stat.analit.net/ci/auth/logon.aspx?sid=" + ClientCode;
				
			GetData();
			ConnectDataSource();
			DataBind();
		}

		private void ConnectDataSource()
		{
			LogsGrid.DataSource = Data.Tables["Logs"];

			FullNameText.Text = Data.Tables["Info"].Rows[0]["FullName"].ToString();
			ShortNameText.Text = Data.Tables["Info"].Rows[0]["ShortName"].ToString();
			ShortNameLB.Text = Data.Tables["Info"].Rows[0]["ShortName"].ToString();
			AddressText.Text = Data.Tables["Info"].Rows[0]["Adress"].ToString();
			FaxText.Text = Data.Tables["Info"].Rows[0]["Fax"].ToString();

			var clientType = (ClientType) Convert.ToUInt32(Data.Tables["Info"].Rows[0]["FirmType"]);
			if (clientType == ClientType.Supplier)
				UpdateListHL.Enabled = false;

			if (clientType == ClientType.Drugstore)
			{
				ConfigHL.NavigateUrl = "~/manageret.aspx?cc=" + ClientCode;
				ConfigHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.ManageDrugstore);
				UserInterfaceHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.DrugstoreInterface);
			}
			else
			{
				ConfigHL.NavigateUrl = "~/managep.aspx?cc=" + ClientCode;
				ConfigHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.ManageSuppliers);
				UserInterfaceHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.SupplierInterface);
			}

			BillingLink.NavigateUrl = String.Format("~/Billing/edit.rails?ClientCode={0}", ClientCode);
			BillingLink.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.Billing);
		}

		private void GetData()
		{
			Data = new DataSet();

			GetMessages();

			var infoDataAdapter = new MySqlDataAdapter(@"
SELECT  cd.FullName,   
        cd.ShortName,   
        cd.Adress,   
        cd.Fax,   
		cd.RegionCode,
        FirmType,   
        ouar.OsUserName,
		length(rui.UniqueCopyID) = 0 as Length
FROM  clientsdata cd
	LEFT JOIN ret_update_info rui ON rui.ClientCode = cd.FirmCode
	LEFT JOIN OsUserAccessRight ouar ON ouar.ClientCode = cd.FirmCode  
WHERE firmcode = ?ClientCode;
", _connection);
			infoDataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
				        
			try
			{
				_connection.Open();
				var transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				infoDataAdapter.SelectCommand.Transaction = transaction;
				infoDataAdapter.Fill(Data, "Info");
				transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}


			var clientType = (ClientType) Convert.ToUInt32(Data.Tables["Info"].Rows[0]["FirmType"]);
			var homeRegion = Convert.ToUInt64(Data.Tables["Info"].Rows[0]["RegionCode"]);
			SecurityContext.Administrator.CheckClientType(clientType);
			SecurityContext.Administrator.CheckClientHomeRegion(homeRegion);

			if (clientType == ClientType.Drugstore)
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
					UnlockButton.Enabled = ADHelper.IsLocked(Data.Tables["Info"].Rows[0]["OsUserName"].ToString());
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
	Fax = ?Fax
WHERE firmcode = ?ClientCode  
";
				_command.Parameters.AddWithValue("?ClientCode", ClientCode);
				_command.Parameters.AddWithValue("?FullName", FullNameText.Text);
				_command.Parameters.AddWithValue("?ShortName", ShortNameText.Text);
				_command.Parameters.AddWithValue("?Address", AddressText.Text);
				_command.Parameters.AddWithValue("?Fax", FaxText.Text);
				_command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				_command.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
				_command.ExecuteNonQuery();

				transaction.Commit();
				
				IsMessafeSavedLabel.Visible = false;
			}
			catch (Exception ex)
			{ 
				if (transaction != null)
					transaction.Rollback();
				throw new Exception("Ошибка на странице Info.aspx", ex);
			}
			finally
			{
				_connection.Close();
			}
		}

		protected void UnlockButton_Click(object sender, EventArgs e)
		{
			ADHelper.Unlock(Data.Tables["Info"].Rows[0]["OsUserName"].ToString());
			UnlockButton.Visible = false;
			UnlockedLabel.Visible = true;
		}

		protected void DeletePrepareDataButton_Click(object sender, EventArgs e)
		{
			try
			{
				File.Delete(String.Format(@"U:\wwwroot\ios\Results\{0}.zip", ClientCode));
				DeletePrepareDataButton.Enabled = false;
				DeleteLabel.Text = "Подготовленные данные удалены";
				DeleteLabel.ForeColor = Color.Green;
			}
			catch
			{
				DeleteLabel.Text = "Ошибка удаления подготовленных данных, попробуйте позднее.";
				DeleteLabel.ForeColor = Color.Red;
			}
		}

		protected void ResetUniqueCopyID(object sender, EventArgs e)
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				var command = connection.CreateCommand();
				command.CommandText = @"
set @inHost = ?Host;
set @inUser = ?UserName;
set @ResetIdCause = ?ResetIdCause;

INSERT INTO `logs`.clientsinfo (UserName, WriteTime, ClientCode, Message)
VALUE (?UserName, now(), ?ClientCode, concat('$$$Изменение УИН: ', ?ResetIdCause));

UPDATE ret_update_info SET UniqueCopyID = ''  WHERE clientcode=?clientCode;
";
				command.Parameters.AddWithValue("?ClientCode", ClientCode);
				command.Parameters.AddWithValue("?ResetIdCause", ResetIDCause.Text);
				command.Parameters.AddWithValue("?Host", HttpContext.Current.Request.UserHostAddress);
				command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
				command.ExecuteNonQuery();
				Response.Redirect(String.Format("info.rails?cc={0}", ClientCode));
			}
		}

		public void SetController(IController controller, IControllerContext context)
		{
			Controller = (Controller) controller;
			ControllerContext = context;
		}
	}
}