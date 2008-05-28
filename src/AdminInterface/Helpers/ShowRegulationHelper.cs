using System;
using System.Data;
using System.Web.UI.WebControls;
using AddUser;
using MySql.Data.MySqlClient;

namespace AdminInterface.Helpers
{
	public class ShowRegulationHelper
	{
		public static void Load(MySqlDataAdapter adapter, DataSet data)
		{
			adapter.SelectCommand.CommandText = @"
SELECT  DISTINCT cd.FirmCode, 
        convert(concat(cd.FirmCode, '. ', cd.ShortName) using cp1251) ShortName,
		sr.IncludeType as ShowType,
		sr.Id
FROM    (accessright.regionaladmins, clientsdata as cd) 
	JOIN ShowRegulation sr ON sr.ShowClientCode = cd.FirmCode
WHERE   sr.PrimaryClientCode				 = ?ClientCode
		AND cd.regioncode & regionaladmins.regionmask > 0 
        AND regionaladmins.UserName               = ?UserName 
        AND FirmType                         = if(ShowRetail+ShowVendor = 2, FirmType, if(ShowRetail = 1, 1, 0)) 
        AND if(UseRegistrant                 = 1, Registrant = ?UserName, 1 = 1)   
        AND FirmStatus    = 1 
        AND billingstatus = 1 
ORDER BY cd.shortname;";

			if (adapter.SelectCommand.Parameters.IndexOf("?UserName") < 0)
				adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			adapter.Fill(data, "ShowClients");
		}

		public static void Update(MySqlConnection connection, MySqlTransaction transaction, DataSet data, int clientCode)
		{
			var table = data.Tables["ShowClients"];
			var showClientsAdapter = new MySqlDataAdapter
			{
				DeleteCommand = new MySqlCommand(@"
DELETE FROM showregulation 
WHERE id = ?id;", connection, transaction)
			};
			showClientsAdapter.DeleteCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");

			showClientsAdapter.UpdateCommand = new MySqlCommand(@"
UPDATE showregulation 
SET PrimaryClientCode = ?PrimaryClientCode,
	ShowClientCode = ?ShowClientCode,
	IncludeType = ?ShowType
WHERE id = ?Id;", connection, transaction);
			showClientsAdapter.UpdateCommand.Parameters.AddWithValue("?PrimaryClientCode", clientCode);
			showClientsAdapter.UpdateCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");
			showClientsAdapter.UpdateCommand.Parameters.Add("?ShowType", MySqlDbType.Int32, 0, "ShowType");
			showClientsAdapter.UpdateCommand.Parameters.Add("?Id", MySqlDbType.Int32, 0, "Id");

			showClientsAdapter.InsertCommand = new MySqlCommand(@"
INSERT INTO showregulation 
SET PrimaryClientCode = ?PrimaryClientCode,
	ShowClientCode = ?ShowClientCode,
	IncludeType = ?ShowType;", connection, transaction);
			showClientsAdapter.InsertCommand.Parameters.AddWithValue("?PrimaryClientCode", clientCode);
			showClientsAdapter.InsertCommand.Parameters.Add("?ShowClientCode", MySqlDbType.Int32, 0, "FirmCode");
			showClientsAdapter.InsertCommand.Parameters.Add("?ShowType", MySqlDbType.Int32, 0, "ShowType");

			showClientsAdapter.Update(table);
			table.AcceptChanges();
		}

		public static void ProcessChanges(GridView showClientGrid, DataSet data)
		{
			var showClientsTable = data.Tables["ShowClients"];
			foreach (GridViewRow row in showClientGrid.Rows)
			{
				if (showClientsTable.DefaultView[row.RowIndex]["ShortName"].ToString() != ((DropDownList)row.FindControl("ShowClientsList")).SelectedItem.Text)
				{
					showClientsTable.DefaultView[row.RowIndex]["ShortName"] = ((DropDownList) row.FindControl("ShowClientsList")).SelectedItem.Text;
					showClientsTable.DefaultView[row.RowIndex]["FirmCode"] = ((DropDownList) row.FindControl("ShowClientsList")).SelectedValue;
				}
				if (showClientsTable.DefaultView[row.RowIndex]["ShowType"].ToString() != ((DropDownList)row.FindControl("ShowType")).SelectedValue)
					showClientsTable.DefaultView[row.RowIndex]["ShowType"] = ((DropDownList)row.FindControl("ShowType")).SelectedValue;
			}
		}

		public static void ShowClientsGrid_RowDeleting(object sender, GridViewDeleteEventArgs e, DataSet data)
		{
			var showClientsTable = data.Tables["ShowClients"];
			var showClientsGrid = (GridView) sender;
			ProcessChanges(showClientsGrid, data);
			showClientsTable.DefaultView[e.RowIndex].Delete();
			showClientsGrid.DataSource = showClientsTable.DefaultView;
			showClientsGrid.DataBind();
		}

		public static void ShowClientsGrid_RowCommand(object sender, GridViewCommandEventArgs e, DataSet data)
		{
			var showClientsGrid = (GridView)sender;
			var showClientsTable = data.Tables["ShowClients"];
			switch (e.CommandName)
			{
				case "Add":
					ProcessChanges(showClientsGrid, data);
					var row = showClientsTable.NewRow();
					row["ShowType"] = 0;
					showClientsTable.Rows.Add(row);
					showClientsGrid.DataSource = showClientsTable.DefaultView;
					showClientsGrid.DataBind();
					break;
				case "Search":
					var adapter = new MySqlDataAdapter
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
					adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
					adapter.SelectCommand.Parameters.AddWithValue("?SearchText", string.Format("%{0}%", ((TextBox)showClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("SearchText")).Text));

					var searchData = new DataSet();
					using (var connection = new MySqlConnection(Literals.GetConnectionString()))
					{
						connection.Open();
						adapter.Fill(searchData);
					}

					var ShowList = ((DropDownList)showClientsGrid.Rows[Convert.ToInt32(e.CommandArgument)].FindControl("ShowClientsList"));
					ShowList.DataSource = searchData;
					ShowList.DataBind();
					ShowList.Visible = searchData.Tables[0].Rows.Count > 0;
					break;
			}
		}

		public static void ShowClientsGrid_RowDataBound(object sender, GridViewRowEventArgs e)
		{
			if (e.Row.RowType != DataControlRowType.DataRow)
				return;

			var rowView = (DataRowView)e.Row.DataItem;
			if (rowView.Row.RowState == DataRowState.Added)
			{
				((Button)e.Row.FindControl("SearchButton")).CommandArgument = e.Row.RowIndex.ToString();
				e.Row.FindControl("SearchButton").Visible = true;
				e.Row.FindControl("SearchText").Visible = true;
			}
			else
			{
				e.Row.FindControl("SearchButton").Visible = false;
				e.Row.FindControl("SearchText").Visible = false;
			}
			var list = ((DropDownList)e.Row.FindControl("ShowClientsList"));
			list.Items.Add(new ListItem(rowView["ShortName"].ToString(), rowView["FirmCode"].ToString()));
			list.Width = new Unit(90, UnitType.Percentage);
		}
	}
}
