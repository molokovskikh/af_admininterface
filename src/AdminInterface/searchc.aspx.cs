using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using MySql.Data.MySqlClient;
using Image = System.Web.UI.WebControls.Image;

namespace AddUser
{
	public enum SearchType
	{
		ShortName,
		Login,
		Code,
		BillingCode,
		JuridicalName
	}

	partial class searchc : Page
	{
		MySqlConnection _connection = new MySqlConnection();
		MySqlCommand _command = new MySqlCommand();

		public DataView ClientsDataView
		{
			get { return Session["ClientsDataView"] as DataView; }
			set { Session["ClientsDataView"] = value; }
		}

		private string _sortExpression
		{
			get { return (string)(ViewState["SortExpression"] ?? String.Empty); }
			set { ViewState["SortExpression"] = value; }
		}

		private SortDirection _sortDirection
		{
			get { return (SortDirection)(ViewState["SortDirection"] ?? SortDirection.Ascending); }
			set { ViewState["SortDirection"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");

            _connection.ConnectionString = Literals.GetConnectionString();

			if(!IsPostBack)
				BindUserRegions();
		}

		private void BindUserRegions()
		{
			DataSet dataSet = new DataSet();
			MySqlDataAdapter dataAdapter = new MySqlDataAdapter(
@"select (select sum(regioncode) from farm.regions) as RegionCode, 'Все' as Region, 1 as IsAll
union
SELECT  r.RegionCode,
        r.Region,
        0 as IsAll
FROM    farm.regions as r,
        accessright.regionaladmins as ra
WHERE   ra.username = ?UserName
        and ra.RegionMask & r.regioncode > 0
ORDER BY IsAll Desc, Region;", _connection);
			dataAdapter.SelectCommand.Parameters.Add("?UserName", Convert.ToString(Session["UserName"]));
			dataAdapter.Fill(dataSet);
			ClientRegion.DataSource = dataSet.Tables[0];
			ClientRegion.DataBind();
		}

		protected void GoFind_Click(object sender, EventArgs e)
		{
			SearchType searchType = SearchType.ShortName;
			switch(FindRB.SelectedValue)
			{
				case "Code":
					searchType = SearchType.Code;
					break;
				case "BillingCode":
					searchType = SearchType.BillingCode;
					break;
				case "ShortName":
					searchType = SearchType.ShortName;
					break;
				case "JuridicalName":
					searchType = SearchType.JuridicalName;
					break;
				case "Login":
					searchType = SearchType.Login;
					break;
				case "Automate":
					if (char.IsDigit(FindTB.Text[0]))
						searchType = SearchType.Code;
					else if (FindTB.Text[0] < 128)
						searchType = SearchType.Login;
					else
						searchType = SearchType.ShortName;
					break;
			}
			BuildQuery("order by 3, 4", searchType);
			BindData();
		}

		protected void ClientsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
				FormatRow(e.Row);
		}

		private void FormatRow(GridViewRow row)
		{
			DataRowView data = row.DataItem as DataRowView;

			if (data.Row["FirmStatus"].ToString() == "1")
				row.BackColor = Color.FromArgb(255, 102, 0);

			if (ADCB.Checked)
				row.Cells[8].BackColor = Color.FromName(data.Row["ADUserStatus"].ToString());

			if ((data.Row["FirstUpdate"] == DBNull.Value 
					|| DateTime.Now.Subtract(Convert.ToDateTime(data.Row["FirstUpdate"])).TotalDays > 2)
				&& data.Row["FirmStatus"].ToString() == "0")
				row.Cells[4].BackColor = Color.Gray;

		}
		protected void ClientsGridView_Sorting(object sender, GridViewSortEventArgs e)
		{
			if (_sortExpression == e.SortExpression)
				_sortDirection = _sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			_sortExpression = e.SortExpression;

			ClientsDataView.Sort = _sortExpression + (_sortDirection == SortDirection.Ascending ? " ASC" : " DESC");

			ClientsGridView.DataBind();
		}

		private void BindData()
		{
			DateTime startDate = DateTime.Now;

			MySqlDataAdapter adapter = new MySqlDataAdapter();
			DataSet data = new DataSet();

			try
			{
				_connection.Open();
				_command.Connection = _connection;
				_command.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.SelectCommand = _command;
				adapter.Fill(data);
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}

			if (ADCB.Checked)
				GetADUserStatus(data.Tables[0]);

			ClientsDataView = data.Tables[0].DefaultView;

			ClientsGridView.DataBind();

			if (data.Tables[0].Rows.Count > 0)
				Table4.Visible = true;
			else
				Table4.Visible = false;

			SearchTimeLabel.Text = string.Format("Время поиска:{0}", (DateTime.Now - startDate));
			SearchTimeLabel.Visible = true;
		}

		private void BuildQuery(string orderStatement, SearchType searchType)
		{
			string firstPart =
@"
SELECT  cd.billingcode, 
        cast(cd.firmcode as CHAR) as firmcode, 
        cd.ShortName, 
        region, 
        max(pui.datecurprice) FirstUpdate, 
        max(pui.dateprevprice) SecondUpdate, 
        null EXE, 
        null MDB, 
        if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName, 
        FirmSegment, 
        FirmType, 
        (Firmstatus     = 0 
        or Billingstatus= 0) Firmstatus, 
        if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, 
        cd.firmcode                                      as bfc,
		NULL AS IncludeType
FROM    (clientsdata as cd, farm.regions, accessright.regionaladmins, pricesdata, usersettings.price_update_info pui, billing.payers p) 
LEFT JOIN showregulation 
        ON ShowClientCode= cd.firmcode 
LEFT JOIN osuseraccessright as ouar2 
        ON ouar2.clientcode= cd.firmcode 
LEFT JOIN osuseraccessright as ouar 
        ON ouar.clientcode                       = if(primaryclientcode is null, cd.firmcode, primaryclientcode) 
WHERE   pui.pricecode	                         = pricesdata.pricecode 
		and cd.BillingCode						 = p.PayerID
        and pricesdata.firmcode                  = cd.firmcode 
        and regions.regioncode                   = cd.regioncode 
		and regionaladmins.UserName              = ?UserName 
        and (cd.regioncode & regionaladmins.regionmask) > 0 
		and (cd.regionCode & ?RegionMask) > 0
        and if(ShowVendor = 1, FirmType= 0, 0) 
        and if(UseRegistrant = 1, Registrant= regionaladmins.UserName, 1)
		
";
			string secondPart = String.Empty;
			string thirdPart =
@"
group by cd.firmcode union 
SELECT  cd. billingcode, 
        cast(if(includeregulation.PrimaryClientCode is null, cd.firmcode, concat(cd.firmcode, '[', includeregulation.PrimaryClientCode, ']')) as CHAR) as firmcode, 
        if(includeregulation.PrimaryClientCode is null, cd.ShortName, concat(cd.ShortName, '[', incd.shortname, ']')), 
        region, 
        UpdateTime FirstUpdate, 
        UncommittedUpdateTime SecondUpdate, 
        AppVersion as EXE, 
        DBVersion MDB, 
        if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName, 
        cd.FirmSegment, 
        cd.FirmType, 
        (cd.Firmstatus     = 0 
        or cd.Billingstatus= 0) Firmstatus, 
        if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, 
        cd.firmcode                                      as bfc,
		CASE IncludeRegulation.IncludeType
			WHEN 0 THEN 'Базовый'
			WHEN 1 THEN 'Сеть'
			WHEN 2 THEN 'Скрытый'
		END AS IncludeType
		
FROM    (clientsdata as cd, farm.regions, accessright.regionaladmins, ret_update_info as rts, billing.payers p) 
LEFT JOIN showregulation 
        ON ShowClientCode= cd.firmcode 
LEFT JOIN includeregulation 
        ON includeclientcode= cd.firmcode 
LEFT JOIN clientsdata incd 
        ON incd.firmcode= includeregulation.PrimaryClientCode 
LEFT JOIN osuseraccessright as ouar2 
		ON ouar2.clientcode= if(IncludeRegulation.PrimaryClientCode is null, cd.FirmCode, if(IncludeRegulation.IncludeType = 0, IncludeRegulation.PrimaryClientCode, cd.FirmCode))
LEFT JOIN osuseraccessright as ouar 
        ON ouar.clientcode= ifnull(ShowRegulation.PrimaryClientCode, cd.FirmCode) 
LEFT JOIN logs.AnalitFUpdates
        ON AnalitFUpdates.clientcode= if(IncludeRegulation.PrimaryClientCode is null, cd.FirmCode, if(IncludeRegulation.IncludeType = 0, IncludeRegulation.PrimaryClientCode, cd.FirmCode))
        and AnalitFUpdates.UpdateId    = 
        (SELECT max(UpdateId) 
        FROM    logs.AnalitFUpdates
        WHERE   clientcode= if(IncludeRegulation.PrimaryClientCode is null, cd.FirmCode, if(IncludeRegulation.IncludeType = 0, IncludeRegulation.PrimaryClientCode, cd.FirmCode))
                and updatetype in(1,2)
        ) 
WHERE   rts.clientcode                           = if(IncludeRegulation.PrimaryClientCode is null, cd.FirmCode, if(IncludeRegulation.IncludeType = 0, IncludeRegulation.PrimaryClientCode, cd.FirmCode))
        and regions.regioncode                   = cd.regioncode 
		and cd.BillingCode						 = p.PayerID
        and regionaladmins.UserName              = ?UserName 
		and (cd.regioncode & regionaladmins.regionmask) > 0 
		and (cd.regionCode & ?RegionMask) > 0
        and if(ShowRetail = 1, cd.FirmType= 1, 0) 
        and if(UseRegistrant = 1, cd.Registrant= regionaladmins.UserName, 1)";

			string fourthPart = String.Empty;

			switch(ClientState.SelectedValue)
			{
				case "Все":
					break;
				case "Включен":
					secondPart += " and (cd.Firmstatus = 1 and cd.Billingstatus= 1) ";
					fourthPart += " and (cd.Firmstatus = 1 and cd.Billingstatus= 1) ";
					break;
				case "Отключен":
					secondPart += " and (cd.Firmstatus = 0 or cd.Billingstatus= 0) ";
					fourthPart += " and (cd.Firmstatus = 0 or cd.Billingstatus= 0) ";
					break;
				default:
					throw new Exception(String.Format("Не известное состояние клиента {0}", ClientState.SelectedValue));
			}

			switch (ClientType.SelectedValue)
			{
				case "Все":
					break;
				case "Аптеки":
					secondPart += " and cd.firmtype=1";
					fourthPart += " and cd.firmtype=1";
					break;
				case "Поставщики":
					secondPart += " and cd.firmtype=0";
					fourthPart += " and cd.firmcode=0";
					break;
				default:
					throw new Exception(String.Format("Не известный тип клиента {0}", ClientType.SelectedValue));
			}

			switch (searchType)
			{
				case SearchType.ShortName:
					{
						secondPart += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
						fourthPart += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
						_command.Parameters.Add(new MySqlParameter("?Name", MySqlDbType.VarChar));
						_command.Parameters["?Name"].Value = "%" + FindTB.Text + "%";
						break;
					}
				case SearchType.Code:
					{
						secondPart += " and cd.firmcode=?ClientCode";
						fourthPart += " and cd.firmcode=?ClientCode";
						_command.Parameters.Add(new MySqlParameter("?ClientCode", MySqlDbType.Int32));
						_command.Parameters["?ClientCode"].Value = FindTB.Text;
						break;
					}
				case SearchType.Login:
					{
						secondPart += " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
						fourthPart += " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
						_command.Parameters.Add(new MySqlParameter("?Login", MySqlDbType.VarChar));
						_command.Parameters["?Login"].Value = "%" + FindTB.Text + "%";
						break;
					}
				case SearchType.BillingCode:
					{
						secondPart += " and cd.billingcode=?BillingCode";
						fourthPart += " and cd.billingcode=?BillingCode";
						_command.Parameters.Add(new MySqlParameter("?BillingCode", MySqlDbType.Int32));
						_command.Parameters["?BillingCode"].Value = FindTB.Text;
						break;
					}
				case SearchType.JuridicalName:
					secondPart += " and p.JuridicalName like ?JuridicalName";
					fourthPart += " and p.JuridicalName like ?JuridicalName";
					_command.Parameters.Add("?JuridicalName", MySqlDbType.VarChar);
					_command.Parameters["?JuridicalName"].Value = "%" + FindTB.Text + "%";
					break;
			}
			_command.CommandText = String.Format("{0}{1}{2}{3}{4}{5}", new string[] { firstPart, secondPart, thirdPart, fourthPart, " group by cd.firmcode ", orderStatement });
			_command.Parameters.Add("?UserName", Convert.ToString(Session["UserName"]));
			_command.Parameters.Add("?RegionMask", ClientRegion.SelectedItem.Value);
		}

		protected void ClientsGridView_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType == DataControlRowType.Header) && (_sortExpression != String.Empty))
			{
				GridView grid = sender as GridView;
				foreach (DataControlField field in grid.Columns)
				{
					if (field.SortExpression == _sortExpression)
					{
						Image sortIcon = new Image();
						sortIcon.ImageUrl = _sortDirection == SortDirection.Ascending ? "./Images/arrow-down-blue-reversed.gif" : "./Images/arrow-down-blue.gif";
						e.Row.Cells[grid.Columns.IndexOf(field)].Controls.Add(sortIcon);
					}
				}
			}
		}

		private void GetADUserStatus(DataTable data)
		{
			data.Columns.Add("ADUserStatus", typeof(String));
			foreach (DataRow row in data.Rows)
			{
				if (row["UserName"].ToString().Length > 0)
				{
					try
					{
						if (ADHelper.IsLocked(row["UserName"].ToString()))
							row["ADUserStatus"] = Color.Violet.Name;
						if (ADHelper.IsDisabled(row["UserName"].ToString()))
							row["ADUserStatus"] = Color.Aqua.Name;
					}
					catch
					{
						row["aduserstatus"] = Color.Red.Name;
					}
				}
			}
		}
		protected void SearchTextValidator_ServerValidate(object source, ServerValidateEventArgs args)
		{
			if (FindRB.SelectedValue == "Code" 
				|| FindRB.SelectedValue == "BillingCode")
				args.IsValid = new Regex("^\\d{1,10}$").IsMatch(args.Value);
			if (FindRB.SelectedValue == "ShortName"
				|| FindRB.SelectedValue == "JuridicalName"
				|| FindRB.SelectedValue == "Login"
				|| FindRB.SelectedValue == "Automate")
				args.IsValid = args.Value.Length > 0;
		}
	}
}