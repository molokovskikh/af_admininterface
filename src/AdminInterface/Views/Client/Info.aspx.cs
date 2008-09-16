using System;
using System.Data;
using System.Web;
using System.Web.UI;
using AddUser;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
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
			if (Data.Tables["Info"].Rows[0]["RegistredBy"] != DBNull.Value)
				Registred.Text = String.Format("Зарегистрирован пользователем {0}, дата регистрации {1}",
				                               Data.Tables["Info"].Rows[0]["RegistredBy"],
				                               Data.Tables["Info"].Rows[0]["RegistrationDate"]);
			else
				Registred.Text = String.Format("Регистратор не указан, дата регистрации {0}",
											   Data.Tables["Info"].Rows[0]["RegistrationDate"]);
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
		if (ra.ManagerName is null or ra.ManagerName = '', cd.Registrant, ra.ManagerName) as RegistredBy,
		cd.RegistrationDate,
        ouar.OsUserName,
		length(rui.UniqueCopyID) = 0 as Length
FROM  clientsdata cd
	LEFT JOIN ret_update_info rui ON rui.ClientCode = cd.FirmCode
	LEFT JOIN OsUserAccessRight ouar ON ouar.ClientCode = cd.FirmCode  
	LEFT JOIN accessright.regionaladmins ra on ra.UserName = cd.Registrant
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

		public void SetController(IController controller, IControllerContext context)
		{
			Controller = (Controller) controller;
			ControllerContext = context;
		}
	}
}