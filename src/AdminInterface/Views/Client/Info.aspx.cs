using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ActiveDs;
using AddUser;
using Castle.MonoRail.Framework;
using MySql.Data.MySqlClient;

namespace Views.Client
{
	partial class Info : Page, IControllerAware
	{
		private uint _clientCode
		{
			get { return (uint)(Session["ClientCode"] != null ? Session["ClientCode"] : 0u); }
			set { Session["ClientCode"] = value; }
		}

		private DataSet _data
		{
			get { return (DataSet)Session["InfoDataSet"]; }
			set { Session["InfoDataSet"] = value; }
		}

		private MySqlCommand _command = new MySqlCommand();
		private MySqlConnection _connection = new MySqlConnection(Literals.GetConnectionString());
		private Controller _controller;

		public Controller Controller
		{
			get { return _controller; }
		}

		private void GetMessages()
		{
            try
            {
                MySqlDataAdapter logsDataAddapter = new MySqlDataAdapter(
    @"
create temporary table Info(Date DateTime, UserName varchar(50), Message text); 
        INSERT 
        INTO    info 
		SELECT	LogTime as Date,
				OperatorName as UserName,
				Concat('$$$Изменение УИН: ', ResetIDCause) as Message
		FROM `Logs`.ret_clients_set_logs
		WHERE ClientCode=?ClientCode  AND ResetIDCause IS NOT NULL
		UNION
        SELECT  WriteTime as Date, 
                UserName, 
                Message 
        FROM    logs.clientsinfo 
        WHERE   clientcode=?ClientCode 
        UNION  
        SELECT  LogTime Date, 
                OperatorName UserName, 
                concat('###Сообщение: ', Message, '###, Показов:', ShowMessageCount) Message  
        FROM    logs.retclientssetupdate 
        WHERE   Message is not null 
                AND clientcode=?ClientCode  
        ORDER BY Date desc; 
        SELECT * FROM info; 
        DROP temporary table info;
", _connection);
                logsDataAddapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
                logsDataAddapter.SelectCommand.Parameters.Add("?ClientCode", _clientCode);

                if (_data.Tables["Logs"] != null)
                    _data.Tables["Logs"].Clear();
                _connection.Open();
                logsDataAddapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                logsDataAddapter.Fill(_data, "Logs");
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
                _command.Parameters.Add("?ClientCode", _clientCode);

                _command.ExecuteNonQuery();
                _command.Transaction.Commit();
            }
            finally
            {
                _connection.Close();
            }

			ProblemTB.Text = String.Empty;
			IsMessafeSavedLabel.Visible = true;
			UnlockRow.Visible = false;
			
			GetMessages();
			LogsGrid.DataSource = _data;
			LogsGrid.DataBind();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
#if !DEBUG
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");
#endif

			if (!IsPostBack)
			{
				_clientCode = Convert.ToUInt32(Request["cc"]);

				OrderHistoryHL.NavigateUrl = "~/orders.aspx?cc=" + _clientCode;
				OrderHistoryHL.NavigateUrl = "~/orders.aspx?cc=" + _clientCode;
				UpdateListHL.NavigateUrl = "~/updates.aspx?cc=" + _clientCode;
				ChPass.NavigateUrl = "~/chpassgo.aspx?cc=" + _clientCode + "&ouar=" + Request["ouar"];
				ChPass.Enabled = Convert.ToBoolean(Session["ChPass"]);
				UserInterfaceHL.NavigateUrl = "https://stat.analit.net/ci/auth/logon.aspx?sid=" + _clientCode;
				
				GetData();
				ConnectDataSource();
				DataBind();
			}
		}

		private void ConnectDataSource()
		{
			LogsGrid.DataSource = _data.Tables["Logs"];
			ShowClientsGrid.DataSource = _data.Tables["ShowClients"];

			FullNameText.Text = _data.Tables["Info"].Rows[0]["FullName"].ToString();
			ShortNameText.Text = _data.Tables["Info"].Rows[0]["ShortName"].ToString();
			ShortNameLB.Text = _data.Tables["Info"].Rows[0]["ShortName"].ToString();
//			PhoneText.Text = _data.Tables["Info"].Rows[0]["Phone"].ToString();
//			SetSipLink(PhoneText, PhoneText.Text);
			AddressText.Text = _data.Tables["Info"].Rows[0]["Adress"].ToString();
			FaxText.Text = _data.Tables["Info"].Rows[0]["Fax"].ToString();
//			EmailText.Text = _data.Tables["Info"].Rows[0]["Mail"].ToString();
//			SetEmailLink(EmailText);
			UrlText.Text = _data.Tables["Info"].Rows[0]["URL"].ToString();

//			OrderManagerPhoneText.Text = _data.Tables["Info"].Rows[0]["OrderManagerPhone"].ToString();
//			SetSipLink(OrderManagerPhoneText, OrderManagerPhoneText.Text);
//			SetEmailLink(OrderManagerEmailText);
			

//			ClientManagerPhoneText.Text = _data.Tables["Info"].Rows[0]["ClientManagerPhone"].ToString();
//			SetSipLink(ClientManagerPhoneText, ClientManagerPhoneText.Text);
//			ClientManagerEmailText.Text = _data.Tables["Info"].Rows[0]["ClientManagerMail"].ToString();
//			SetEmailLink(ClientManagerEmailText);
			

//			AccountantEmailText.Text = _data.Tables["Info"].Rows[0]["AccountantMail"].ToString();
//			SetEmailLink(AccountantEmailText);
//			AccountantPhoneText.Text = _data.Tables["Info"].Rows[0]["AccountantPhone"].ToString();
//			SetSipLink(AccountantPhoneText, AccountantPhoneText.Text);
			

			UserInterfaceHL.Enabled = Convert.ToBoolean(_data.Tables["Info"].Rows[0]["AlowInterface"]);
			UpdateListHL.Enabled = Convert.ToBoolean(_data.Tables["Info"].Rows[0]["FirmType"]);
			if (Convert.ToInt32(_data.Tables["Info"].Rows[0]["FirmType"]) == 1)
				ConfigHL.NavigateUrl = "~manageret";
			else
				ConfigHL.NavigateUrl = "~managep";
			ConfigHL.NavigateUrl += ".aspx?cc=" + _clientCode;
			BillingLink.NavigateUrl = String.Format("~/Billing/edit.rails?ClientCode={0}", 
													_clientCode);
		}

		private static void SetSipLink(HyperLink button, string phoneNumber)
        {
            if (!String.IsNullOrEmpty(phoneNumber))
            {
                button.Visible = true;
                button.NavigateUrl = String.Format("sip:8{0}", phoneNumber.Replace("-", ""));
            }
            else
                button.Visible = false;
        }

		private static void SetEmailLink(HyperLink hyperLink)
		{
			if (!String.IsNullOrEmpty(hyperLink.Text))
			{
				hyperLink.Visible = true;
				hyperLink.NavigateUrl = String.Format("mailto:{0}", hyperLink.Text);
			}
			else
				hyperLink.Visible = false;
		}

		private void GetData()
		{
            _data = new DataSet();

            GetMessages();

            MySqlDataAdapter infoDataAdapter = new MySqlDataAdapter(
@"
SELECT  cd.FullName,   
        cd.ShortName,   
        cd.Phone,   
        cd.Adress,   
        cd.Fax,   
        cd.Mail,   
        cd.URL,   
        cd.OrderManagerName,   
        cd.OrderManagerPhone,   
        cd.OrderManagerMail,   
        cd.ClientManagerName,   
        cd.ClientManagerPhone,   
        cd.ClientManagerMail,   
        cd.AccountantName,   
        cd.AccountantPhone,   
        cd.AccountantMail,   
        (if(regionaladmins.UseRegistrant                =1, Registrant=?UserName, 1=1))   
        AND (regionaladmins.regionmask & cd.regioncode  >0)   
        AND (if(AlowRetailInterface+AlowVendorInterface =2, 1=1, if(Alowretailinterface=1, firmtype=Alowretailinterface, if(AlowVendorInterface=1, firmtype=0, 0)))) as AlowInterface,   
        FirmType,   
        ouar.OsUserName  
FROM    (clientsdata cd,   accessright.regionaladmins)     
LEFT JOIN OsUserAccessRight ouar 
        ON ouar.ClientCode                       = cd.FirmCode  
WHERE   FirmType                                 =if(ShowRetail + ShowVendor=2, FirmType, if(ShowRetail = 1, 1, 0))   
        AND cd.regioncode & regionaladmins.regionmask > 0   
        AND if(regionaladmins.UseRegistrant      =1, Registrant=?UserName, 1=1)   
        AND firmcode                             =?ClientCode   
        AND regionaladmins.username              =?UserName;
", _connection);
            infoDataAdapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
            infoDataAdapter.SelectCommand.Parameters.Add("?ClientCode", _clientCode);
				
			MySqlDataAdapter showClientsAdapter = new MySqlDataAdapter(
@"
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

			showClientsAdapter.SelectCommand.Parameters.Add("?ClientCode", _clientCode);
			showClientsAdapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
            
            try
            {
                _connection.Open();
                MySqlTransaction transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                showClientsAdapter.SelectCommand.Transaction = transaction;
                infoDataAdapter.SelectCommand.Transaction = transaction;
                showClientsAdapter.Fill(_data, "ShowClients");
                infoDataAdapter.Fill(_data, "Info");
                transaction.Commit();
            }
            finally
            {
                _connection.Close();
            }

            if (_data.Tables["Info"].Rows[0]["OsUserName"] != DBNull.Value || !String.IsNullOrEmpty(_data.Tables["Info"].Rows[0]["OsUserName"].ToString()))
            {
				try
				{
					IADsUser ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + _data.Tables["Info"].Rows[0]["OsUserName"]) as IADsUser;
					UnlockRow.Visible = ADUser.IsAccountLocked;
					UnlockedLabel.Visible = false;
				}
				catch
				{
					UnlockRow.Visible = false;
				}
            }
            else
                UnlockRow.Visible = false;

		}

		protected void SaveButton_Click(object sender, EventArgs e)
		{
			MySqlTransaction transaction = null;
			try
			{
				ProcessChanges();
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
	URL = ?Url, 
	OrderManagerName = ?OrderManagerName,
	ClientManagerName = ?ClientManagerName,
	AccountantName = ?AccountantName
WHERE firmcode = ?ClientCode  
";
				_command.Parameters.Add("?ClientCode", _clientCode);
				_command.Parameters.Add("?FullName", FullNameText.Text);
				_command.Parameters.Add("?ShortName", ShortNameText.Text);
				_command.Parameters.Add("?Address", AddressText.Text);
				_command.Parameters.Add("?Fax", FaxText.Text);
				_command.Parameters.Add("?Url", UrlText.Text);
				_command.Parameters.Add("?UserName", Session["UserName"]);
				_command.Parameters.Add("?UserHost", HttpContext.Current.Request.UserHostAddress);
				_command.ExecuteNonQuery();

				MySqlDataAdapter showClientsAdapter = new MySqlDataAdapter();
				showClientsAdapter.DeleteCommand = new MySqlCommand(@"
DELETE FROM showregulation 
WHERE PrimaryClientCode = ?PrimaryClientCode AND ShowClientCode = ?ShowClientCode;
", _connection);
				showClientsAdapter.DeleteCommand.Parameters.Add("?PrimaryClientCode", _clientCode);
				showClientsAdapter.DeleteCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");

				showClientsAdapter.InsertCommand = new MySqlCommand(
	@"
INSERT INTO showregulation 
SET PrimaryClientCode = ?PrimaryClientCode,
	ShowClientCode = ?ShowClientCode;
", _connection);
				showClientsAdapter.InsertCommand.Parameters.Add("?PrimaryClientCode", _clientCode);
				showClientsAdapter.InsertCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");

				showClientsAdapter.Update(_data, "ShowClients");
				_data.Tables["ShowClients"].AcceptChanges();

				transaction.Commit();
				
				IsMessafeSavedLabel.Visible = false;
				UnlockRow.Visible = false;
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
			IADsUser ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + _data.Tables["Info"].Rows[0]["OsUserName"]) as IADsUser;
			ADUser.IsAccountLocked = false;
			ADUser.SetInfo();
			UnlockRow.Visible = true;
			UnlockButton.Visible = false;
			UnlockedLabel.Visible = true;
		}

		protected void ShowClientsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
			{
				DataRowView rowView = (DataRowView)e.Row.DataItem;
				if (rowView.Row.RowState == DataRowState.Added)
				{
					((Button)e.Row.FindControl("SearchButton")).CommandArgument = e.Row.RowIndex.ToString();
					((Button)e.Row.FindControl("SearchButton")).Visible = true;
					((TextBox)e.Row.FindControl("SearchText")).Visible = true;
				}
				else
				{
					((Button)e.Row.FindControl("SearchButton")).Visible = false;
					((TextBox)e.Row.FindControl("SearchText")).Visible = false;
				}
				DropDownList list = ((DropDownList)e.Row.FindControl("ShowClientsList"));
				list.Items.Add(new ListItem(rowView["ShortName"].ToString(), rowView["FirmCode"].ToString()));
				list.Width = new Unit(90, UnitType.Percentage);
			}
		}

		protected void ShowClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			switch (e.CommandName)
			{
				case "Add":
					ProcessChanges();
					_data.Tables["ShowClients"].Rows.Add(_data.Tables["ShowClients"].NewRow());
					ShowClientsGrid.DataSource = _data.Tables["ShowClients"].DefaultView;
					ShowClientsGrid.DataBind();
					break;
				case "Search":
					MySqlDataAdapter adapter = new MySqlDataAdapter
(@"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName
FROM    (accessright.regionaladmins, clientsdata as cd)  
LEFT JOIN showregulation sr
        ON sr.ShowClientCode	             =cd.firmcode  
WHERE   cd.regioncode & regionaladmins.regionmask > 0  
        AND regionaladmins.UserName               =?UserName  
        AND FirmType                         =if(ShowRetail+ShowVendor=2, FirmType, if(ShowRetail=1, 1, 0)) 
        AND if(UseRegistrant                 =1, Registrant=?UserName, 1=1)  
        AND cd.ShortName like ?SearchText
        AND FirmStatus   =1  
        AND billingstatus=1  
ORDER BY cd.shortname;
", Literals.GetConnectionString());
					adapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
					adapter.SelectCommand.Parameters.Add("?SearchText", string.Format("%{0}%", ((TextBox)ShowClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText")).Text));

					DataSet data = new DataSet();
                    try
                    {
                        _connection.Open();
                        adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                        adapter.Fill(data);
                        adapter.SelectCommand.Transaction.Commit();
                    }
                    finally
                    {
                        _connection.Close();
                    }

					DropDownList ShowList = ((DropDownList)ShowClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ShowClientsList"));
					ShowList.DataSource = data;
					ShowList.DataBind();
					ShowList.Visible = data.Tables[0].Rows.Count > 0;
					break;
			}
		}

		protected void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
		{
			ProcessChanges();
			_data.Tables["ShowClients"].DefaultView[e.RowIndex].Delete();
			ShowClientsGrid.DataSource = _data.Tables["ShowClients"].DefaultView;
			ShowClientsGrid.DataBind();
		}

		private void ProcessChanges()
		{
			foreach (GridViewRow row in ShowClientsGrid.Rows)
			{
				if (_data.Tables["ShowClients"].DefaultView[row.RowIndex]["ShortName"].ToString() != ((DropDownList)row.FindControl("ShowClientsList")).SelectedItem.Text)
				{
					_data.Tables["ShowClients"].DefaultView[row.RowIndex]["ShortName"] = ((DropDownList)row.FindControl("ShowClientsList")).SelectedItem.Text;
					_data.Tables["ShowClients"].DefaultView[row.RowIndex]["FirmCode"] = ((DropDownList)row.FindControl("ShowClientsList")).SelectedValue;
				}
			}
		}
		protected void ShowCleintsValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			args.IsValid = !String.IsNullOrEmpty(args.Value);
		}

		public void SetController(Controller controller)
		{
			_controller = controller;
		}
	}
}