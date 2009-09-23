using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.MySql;
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
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.ViewSuppliers, PermissionType.ViewDrugstore);

			if(IsPostBack)
				return;

			BindUserRegions();

			if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers))
				return;

			if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewDrugstore))
				ClientType.Items.Remove(ClientType.Items[2]);

			if (SecurityContext.Administrator.HavePermisions(PermissionType.ViewSuppliers))
				ClientType.Items.Remove(ClientType.Items[1]);

			ClientType.Items.Remove(ClientType.Items[0]);
			ClientType.Enabled = false;
		}

		private void BindUserRegions()
		{
			var dataSet = new DataSet();
			With.Connection(c => {
				var dataAdapter = new MySqlDataAdapter(
@"select (select sum(regioncode) from farm.regions where RegionCode & ?AdminMaskRegion > 0) as RegionCode, 'Все' as Region, 1 as IsAll
union
SELECT  r.RegionCode,
        r.Region,
        0 as IsAll
FROM farm.regions as r
WHERE r.RegionCode & ?AdminMaskRegion > 0
ORDER BY IsAll Desc, Region;", c);
				dataAdapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
				dataAdapter.Fill(dataSet);
				ClientRegion.DataSource = dataSet.Tables[0];
				ClientRegion.DataBind();
			});
		}

		protected void GoFind_Click(object sender, EventArgs e)
		{
			if (!IsValid)
				return;

			var searchType = SearchType.ShortName;
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
			BindData(BuildQuery(searchType));
		}

		protected void ClientsGridView_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType == DataControlRowType.DataRow)
				FormatRow(e.Row);
		}

		private void FormatRow(GridViewRow row)
		{
			var data = row.DataItem as DataRowView;

			if (data.Row["FirmStatus"].ToString() == "1")
				row.BackColor = Color.FromArgb(255, 102, 0);

			if (ADCB.Checked)
				row.Cells[7].CssClass = data.Row["ADUserStatus"].ToString();

			if (data.Row["InvisibleOnFirm"].ToString() == "1" || data.Row["InvisibleOnFirm"].ToString() == "2")
				row.Cells[2].CssClass = "not-base-client";

			if (data.Row["FirstUpdate"] == DBNull.Value)
				return;

			if (DateTime.Now.Subtract(Convert.ToDateTime(data.Row["FirstUpdate"])).TotalDays > 2
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

		private void BindData(MySqlCommand command)
		{
			var startDate = DateTime.Now;

			var adapter = new MySqlDataAdapter();
			var data = new DataSet();

			With.Connection(c => {
				command.Connection = c;
				adapter.SelectCommand = command;
				adapter.Fill(data);
			});

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

		private MySqlCommand BuildQuery(SearchType searchType)
		{
			var orderStatement = "order by 3, 4";
			var firstPart = String.Format(@"
SELECT  cd.billingcode,
        cast(cd.firmcode as CHAR) as firmcode,
        cd.ShortName,
        region,
        null FirstUpdate,
        null SecondUpdate,
        null EXE,
        ouar2.OsUserName UserName,
        FirmSegment,
        FirmType,
        (Firmstatus = 0 or Billingstatus= 0) Firmstatus,
        cd.firmcode as bfc,
		NULL AS IncludeType,
		NULL AS InvisibleOnFirm
FROM clientsdata as cd
	JOIN billing.payers p on cd.BillingCode = p.PayerID
	JOIN farm.regions ON regions.regioncode = cd.regioncode
	LEFT JOIN showregulation ON ShowClientCode= cd.firmcode
	LEFT JOIN osuseraccessright as ouar2 ON ouar2.clientcode= cd.firmcode or ouar2.clientcode = if(primaryclientcode is null, cd.firmcode, primaryclientcode) 
WHERE   (cd.RegionCode & ?RegionMask & ?AdminMaskRegion) > 0
		and FirmType = 0
		{0}", SecurityContext.Administrator.GetClientFilterByType("cd"));
			var secondPart = String.Empty;
			var thirdPart = String.Format(@"
group by cd.firmcode 
union 
SELECT  cd.billingcode,
        cast(if(includeregulation.PrimaryClientCode is null, cd.firmcode, concat(cd.firmcode, '[', includeregulation.PrimaryClientCode, ']')) as CHAR) as firmcode,
        if(includeregulation.PrimaryClientCode is null, cd.ShortName, concat(cd.ShortName, '[', incd.shortname, ']')),
        r.region,
        max(uui2.UpdateDate) FirstUpdate,
        max(uui2.UncommitedUpdateDate) SecondUpdate,
        max(uui2.AFAppVersion) EXE,
        ouar2.OSUSERNAME as UserName,
        cd.FirmSegment,
        cd.FirmType,
        (cd.Firmstatus = 0 or cd.Billingstatus= 0) Firmstatus,
        cd.firmcode as bfc,
		CASE IncludeRegulation.IncludeType
			WHEN 0 THEN 'Базовый'
			WHEN 1 THEN 'Сеть'
			WHEN 2 THEN 'Скрытый'
			WHEN 3 THEN 'Базовый+'
		END AS IncludeType,
		rcs.InvisibleOnFirm
FROM clientsdata as cd
	JOIN billing.payers p on cd.BillingCode = p.PayerID
	JOIN farm.regions r on r.regioncode = cd.regioncode
	JOIN usersettings.retclientsset rcs on cd.FirmCode = rcs.ClientCode
	LEFT JOIN includeregulation ON includeclientcode = cd.firmcode
	LEFT JOIN clientsdata incd ON incd.firmcode= includeregulation.PrimaryClientCode 
	LEFT JOIN osuseraccessright as ouar2 ON ouar2.clientcode = if(IncludeRegulation.PrimaryClientCode is null, cd.FirmCode, if(IncludeRegulation.IncludeType = 0, IncludeRegulation.PrimaryClientCode, cd.FirmCode))
		LEFT JOIN UserUpdateInfo as uui2 on uui2.UserId = ouar2.RowId
WHERE	(cd.RegionCode & ?RegionMask & ?AdminMaskRegion) > 0
		and cd.FirmType = 1
		{0}", SecurityContext.Administrator.GetClientFilterByType("cd"));

			var fourthPart = String.Empty;

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

			var command = new MySqlCommand();

			switch (searchType)
			{
				case SearchType.ShortName:
					{
						secondPart += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
						fourthPart += " and (cd.shortname like ?Name or cd.fullname like ?Name)";
						command.Parameters.Add(new MySqlParameter("?Name", MySqlDbType.VarChar));
						command.Parameters["?Name"].Value = "%" + FindTB.Text + "%";
						break;
					}
				case SearchType.Code:
					{
						secondPart += " and cd.firmcode=?ClientCode";
						fourthPart += " and cd.firmcode=?ClientCode";
						command.Parameters.Add(new MySqlParameter("?ClientCode", MySqlDbType.Int32));
						command.Parameters["?ClientCode"].Value = FindTB.Text;
						break;
					}
				case SearchType.Login:
					{
						secondPart += " and ouar2.osusername like ?Login";
						fourthPart += " and ouar2.osusername like ?Login";
						command.Parameters.Add(new MySqlParameter("?Login", MySqlDbType.VarChar));
						command.Parameters["?Login"].Value = "%" + FindTB.Text + "%";
						break;
					}
				case SearchType.BillingCode:
					{
						secondPart += " and cd.billingcode=?BillingCode";
						fourthPart += " and cd.billingcode=?BillingCode";
						command.Parameters.Add(new MySqlParameter("?BillingCode", MySqlDbType.Int32));
						command.Parameters["?BillingCode"].Value = FindTB.Text;
						break;
					}
				case SearchType.JuridicalName:
					secondPart += " and p.JuridicalName like ?JuridicalName";
					fourthPart += " and p.JuridicalName like ?JuridicalName";
					command.Parameters.Add("?JuridicalName", MySqlDbType.VarChar);
					command.Parameters["?JuridicalName"].Value = "%" + FindTB.Text + "%";
					break;
			}
			command.CommandText = String.Format("{0}{1}{2}{3}{4}{5}", new[] { firstPart, secondPart, thirdPart, fourthPart, " group by cd.firmcode ", orderStatement });
			command.Parameters.AddWithValue("?RegionMask", Convert.ToUInt64(ClientRegion.SelectedItem.Value));
			command.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);

			return command;
		}

		protected void ClientsGridView_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType != DataControlRowType.Header) || (_sortExpression == String.Empty)) 
				return;
			var grid = sender as GridView;
			foreach (DataControlField field in grid.Columns)
			{
				if (field.SortExpression != _sortExpression) 
					continue;
				var sortIcon = new Image
		                             {
		                                 ImageUrl = (_sortDirection == SortDirection.Ascending
		                                          ? "./Images/arrow-down-blue-reversed.gif"
		                                          : "./Images/arrow-down-blue.gif")
		                             };
				e.Row.Cells[grid.Columns.IndexOf(field)].Controls.Add(sortIcon);
			}
		}

		private static void GetADUserStatus(DataTable data)
		{
			data.Columns.Add("ADUserStatus", typeof(String));
			foreach (DataRow row in data.Rows)
			{
				if (row["UserName"].ToString().Length <= 0) 
					continue;

				try
				{
					if (ADHelper.IsLocked(row["UserName"].ToString()))
						row["ADUserStatus"] = "BlockedLogin";
					if (ADHelper.IsDisabled(row["UserName"].ToString()))
						row["ADUserStatus"] = "DisabledLogin";
				}
				catch
				{
					row["aduserstatus"] = "LoginNotExists";
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