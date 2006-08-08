using System;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Web.UI;
using System.Web.UI.WebControls;
using ActiveDs;
using MySql.Data.MySqlClient;
using Image=System.Web.UI.WebControls.Image;

namespace AddUser
{
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
			get {return (string) (ViewState["SortExpression"] ?? String.Empty);}
			set {ViewState["SortExpression"] = value;}
		}

		private SortDirection _sortDirection
		{
			get { return (SortDirection) (ViewState["SortDirection"] ?? SortDirection.Ascending); }
			set {ViewState["SortDirection"] = value;}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			    Response.Redirect("default.aspx");
			_connection.ConnectionString = Literals.GetConnectionString();
			_connection.Open();
			_command.CommandText = "select max(UserName='" + Session["UserName"] + "') from accessright.showright";
			_command.Connection = _connection;
			if (_command.ExecuteScalar().ToString() == "0")
			{
			    Session["strError"] = "Пользователь " + Session["UserName"] + " не найден!";
			    _connection.Close();
			    Response.Redirect("error.aspx");
			}
			_connection.Close();
		}
		
		protected void GoFind_Click(object sender, EventArgs e)
		{
			BuildQuery("order by 3, 4");
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

			if ((DateTime.Now.Subtract(Convert.ToDateTime(data.Row["FirstUpdate"])).TotalDays > 2)
				&& (data.Row["FirmStatus"].ToString() == "0"))
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
			_connection.Open();

			_command.Connection = _connection;
			adapter.SelectCommand = _command;
			adapter.Fill(data);
			_connection.Close();
			
			if (ADCB.Checked)
				GetADUserStatus(data.Tables[0]);

			ClientsDataView = data.Tables[0].DefaultView;

			ClientsGridView.DataBind();

			if (data.Tables[0].Rows.Count > 0)
				Table4.Visible = true;
			else
				Table4.Visible = false;

			SearchTimeLabel.Text = string.Format("Время поиска:{0}", (DateTime.Now - startDate).ToString());
		}

		private void BuildQuery(string orderStatement)
		{
			string firstPart = 
@"
SELECT  cd. billingcode, 
        cd.firmcode, 
        ShortName, 
        region, 
        max(datecurprice) FirstUpdate, 
        max(dateprevprice) SecondUpdate, 
        null EXE, 
        null MDB, 
        if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName, 
        FirmSegment, 
        FirmType, 
        (Firmstatus     = 0 
        or Billingstatus= 0) Firmstatus, 
        if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, 
        cd.firmcode                                      as bfc 
FROM    (clientsdata as cd, farm.regions, accessright.showright, pricesdata, farm.formrules) 
LEFT JOIN showregulation 
        ON ShowClientCode= cd.firmcode 
LEFT JOIN osuseraccessright as ouar2 
        ON ouar2.clientcode= cd.firmcode 
LEFT JOIN osuseraccessright as ouar 
        ON ouar.clientcode                       = if(primaryclientcode is null, cd.firmcode, primaryclientcode) 
WHERE   formrules.firmcode                       = pricesdata.pricecode 
        and pricesdata.firmcode                  = cd.firmcode 
        and regions.regioncode                   = cd.regioncode 
        and cd.regioncode & showright.regionmask > 0 
        and showright.UserName                   = ?UserName 
        and if(ShowOpt                           = 1, FirmType= 0, 0) 
        and if(UseRegistrant                     = 1, Registrant= showright.UserName, 1)
";
			string secondPart = String.Empty;
			string thirdPart = 
@"
group by cd.firmcode union 
SELECT  cd. billingcode, 
        if(includeregulation.PrimaryClientCode is null, cd.firmcode, concat(cd.firmcode, '[', includeregulation.PrimaryClientCode, ']')), 
        if(includeregulation.PrimaryClientCode is null, cd.ShortName, concat(cd.ShortName, '[', incd.shortname, ']')), 
        region, 
        UpdateTime FirstUpdate, 
        UncommittedUpdateTime SecondUpdate, 
        EXEVersion as EXE, 
        MDBVersion MDB, 
        if(ouar2.rowid is null, ouar.OSUSERNAME, ouar2.OSUSERNAME) as UserName, 
        cd.FirmSegment, 
        cd.FirmType, 
        (cd.Firmstatus     = 0 
        or cd.Billingstatus= 0) Firmstatus, 
        if(ouar2.rowid is null, ouar.rowid, ouar2.rowid) as ouarid, 
        cd.firmcode                                      as bfc 
FROM    (clientsdata as cd, farm.regions, accessright.showright, retclientsset as rts) 
LEFT JOIN showregulation 
        ON ShowClientCode= cd.firmcode 
LEFT JOIN includeregulation 
        ON includeclientcode= cd.firmcode 
LEFT JOIN clientsdata incd 
        ON incd.firmcode= includeregulation.PrimaryClientCode 
LEFT JOIN osuseraccessright as ouar2 
		ON ouar2.clientcode= if(IncludeRegulation.PrimaryClientCode is null or IncludeRegulation.IncludeType = 0, cd.FirmCode, IncludeRegulation.PrimaryClientCode) 
--        ON ouar2.clientcode= ifnull(IncludeRegulation.PrimaryClientCode, cd.FirmCode) 
LEFT JOIN osuseraccessright as ouar 
        ON ouar.clientcode= ifnull(ShowRegulation.PrimaryClientCode, cd.FirmCode) 
LEFT JOIN logs.prgdataex 
        ON prgdataex.clientcode= ifnull(includeregulation.PrimaryClientCode, cd.firmcode) 
        and prgdataex.rowid    = 
        (SELECT max(rowid) 
        FROM    logs.prgdataex 
        WHERE   clientcode= ifnull(includeregulation.PrimaryClientCode, cd.firmcode) 
                and updatetype in(1,2)
        ) 
WHERE   rts.clientcode                           = if(IncludeRegulation.PrimaryClientCode is null or IncludeRegulation.IncludeType = 0, cd.FirmCode, IncludeRegulation.PrimaryClientCode) 
        and regions.regioncode                   = cd.regioncode 
        and cd.regioncode & showright.regionmask > 0 
        and showright.UserName                   = ?UserName 
        and if(ShowRet                           = 1, cd.FirmType= 1, 0) 
        and if(UseRegistrant                     = 1, cd.Registrant= showright.UserName, 1)
";
			string fourthPart = String.Empty;

			switch (FindRB.SelectedItem.Value)
			{
				case "0":
				{
					secondPart = " and (cd.shortname like ?Name or cd.fullname like ?Name)";
					fourthPart = " and (cd.shortname like ?Name or cd.fullname like ?Name)";
					_command.Parameters.Add(new MySqlParameter("Name", MySqlDbType.VarChar));
					_command.Parameters["Name"].Value = "%" + FindTB.Text + "%";
					break;
				}
				case "1":
				{
					secondPart = " and cd.firmcode=?ClientCode";
					fourthPart = " and cd.firmcode=?ClientCode";
					_command.Parameters.Add(new MySqlParameter("ClientCode", MySqlDbType.Int32));
					_command.Parameters["ClientCode"].Value = FindTB.Text;
					break;
				}
				case "2":
				{
					secondPart = " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
					fourthPart = " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)";
					_command.Parameters.Add(new MySqlParameter("Login", MySqlDbType.VarChar));
					_command.Parameters["Login"].Value = "%" + FindTB.Text + "%";
					break;
				}
				case "3":
				{
					secondPart = " and cd.billingcode=?BillingCode";
					fourthPart = " and cd.billingcode=?BillingCode";
					_command.Parameters.Add(new MySqlParameter("BillingCode", MySqlDbType.Int32));
					_command.Parameters["BillingCode"].Value = FindTB.Text;
					break;
				}
			}
			_command.CommandText = String.Format("{0}{1}{2}{3}{4}{5}", new string[] { firstPart, secondPart, thirdPart, fourthPart, " group by cd.firmcode ", orderStatement });
			_command.Parameters.Add("UserName", Convert.ToString(Session["UserName"]));
		}

		protected void ClientsGridView_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType == DataControlRowType.Header) && (_sortExpression != String.Empty))
			{
			    GridView grid = sender as GridView;
			    foreach (DataControlField field in  grid.Columns)
			    {
			        if (field.SortExpression == _sortExpression)
			        {
			            Image sortIcon = new Image();
						sortIcon.ImageUrl = _sortDirection == SortDirection.Ascending ? "arrow-down-blue.gif" : "arrow-down-blue-reversed.gif";
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
						IADsUser ADUser = Marshal.BindToMoniker("WinNT://adc.analit.net/" + row["UserName"].ToString()) as IADsUser;
						if (ADUser.IsAccountLocked)
						{
							row["ADUserStatus"] = Color.Violet.Name;
						}
						if (ADUser.AccountDisabled)
						{
							row["ADUserStatus"] = Color.Aqua.Name;
						}
					}
					catch
					{
						row["aduserstatus"] = Color.Red.Name;
					}
				}
			}
		}
	}
}