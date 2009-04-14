using System;
using System.Data;
using System.Web;
using System.Web.UI;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Castle.MonoRail.Framework;
using Common.MySql;
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

		protected Controller Controller;
		protected IControllerContext ControllerContext;

		private void GetMessages()
		{
			With.Connection(
				c => {
					var logsDataAddapter = new MySqlDataAdapter(@"
SELECT  WriteTime as Date, 
		UserName, 
        Message 
FROM  logs.clientsinfo 
WHERE clientcode=?ClientCode
ORDER BY date DESC;
", c);
					logsDataAddapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
					logsDataAddapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);

					if (Data.Tables["Logs"] != null)
						Data.Tables["Logs"].Clear();
					logsDataAddapter.Fill(Data, "Logs");

				});
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			if (String.IsNullOrEmpty(ProblemTB.Text))
				return;

			With.Transaction(
				(c, t) => {
					var command = new MySqlCommand(
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
        );", c, t);
					command.Parameters.AddWithValue("?Problem", ProblemTB.Text);
					command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
					command.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
					command.Parameters.AddWithValue("?ClientCode", ClientCode);

					command.ExecuteNonQuery();
				});
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
				Registred.Text = String.Format("«арегистрирован пользователем {0}, дата регистрации {1}",
				                               Data.Tables["Info"].Rows[0]["RegistredBy"],
				                               Data.Tables["Info"].Rows[0]["RegistrationDate"]);
			else
				Registred.Text = String.Format("–егистратор не указан, дата регистрации {0}",
											   Data.Tables["Info"].Rows[0]["RegistrationDate"]);
		}

		private void GetData()
		{
			Data = new DataSet();

			GetMessages();

			With.Connection(
				c => {
					var infoDataAdapter = new MySqlDataAdapter(@"
SELECT  cd.FullName,   
        cd.ShortName,   
        cd.Adress,   
        cd.Fax,   
		cd.RegionCode,
        FirmType, 
		if (ra.ManagerName is null or ra.ManagerName = '', cd.Registrant, ra.ManagerName) as RegistredBy,
		cd.RegistrationDate,
        ouar.OsUserName
FROM  clientsdata cd
	LEFT JOIN OsUserAccessRight ouar ON ouar.ClientCode = cd.FirmCode  
	LEFT JOIN accessright.regionaladmins ra on ra.UserName = cd.Registrant
WHERE firmcode = ?ClientCode;
", c);
					infoDataAdapter.SelectCommand.Parameters.AddWithValue("?ClientCode", ClientCode);
					infoDataAdapter.Fill(Data, "Info");
				});

			var clientType = (ClientType) Convert.ToUInt32(Data.Tables["Info"].Rows[0]["FirmType"]);
			var homeRegion = Convert.ToUInt64(Data.Tables["Info"].Rows[0]["RegionCode"]);
			SecurityContext.Administrator.CheckClientType(clientType);
			SecurityContext.Administrator.CheckClientHomeRegion(homeRegion);
		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			With.Transaction(
				(c, t) => {
					var command = new MySqlCommand(@"
SET @InHost = ?UserHost;
Set @InUser = ?UserName;

UPDATE ClientsData
SET FullName = ?FullName,
	ShortName = ?ShortName,
	Adress = ?Address,
	Fax = ?Fax
WHERE firmcode = ?ClientCode  
", c, t);
					command.Parameters.AddWithValue("?ClientCode", ClientCode);
					command.Parameters.AddWithValue("?FullName", FullNameText.Text);
					command.Parameters.AddWithValue("?ShortName", ShortNameText.Text);
					command.Parameters.AddWithValue("?Address", AddressText.Text);
					command.Parameters.AddWithValue("?Fax", FaxText.Text);
					command.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
					command.Parameters.AddWithValue("?UserHost", HttpContext.Current.Request.UserHostAddress);
					command.ExecuteNonQuery();

					IsMessafeSavedLabel.Visible = false;
				});
		}

		public void SetController(IController controller, IControllerContext context)
		{
			Controller = (Controller) controller;
			ControllerContext = context;
		}
	}
}