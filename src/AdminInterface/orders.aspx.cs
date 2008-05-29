using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models;
using AdminInterface.Security;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class orders : Page
	{
		private readonly MySqlConnection _connection = new MySqlConnection(Literals.GetConnectionString());
		
		private string _sortExpression
		{
			get { return (string) Session["OrdersSortExpression"] ?? String.Empty; }
			set { Session["OrdersSortExpression"] = value; }
		}
		
		private SortDirection _sortDirection
		{
			get { return (SortDirection) (Session["OrdersSortDirection"] ?? SortDirection.Ascending); }
			set{ Session["OrdersSortDirection"] = value;}
		}

		private DataSet _data
		{
			get { return (DataSet) Session["OrdersDataSet"]; }
			set { Session["OrdersDataSet"] = value;}
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			var adapter = new MySqlDataAdapter(@"
SELECT  oh.rowid, 
        oh.WriteTime, 
        PriceDate, 
        client.shortname as Customer, 
        firm.shortname as Supplier, 
        PriceName, 
        oh.RowCount, 
        oh.Processed  
FROM    orders.ordershead oh, 
        usersettings.pricesdata, 
        usersettings.clientsdata as firm, 
        usersettings.clientsdata as client, 
        usersettings.clientsdata as sel 
WHERE   clientcode = client.firmcode  
        AND client.firmcode = if(sel.firmtype = 1, sel.firmcode, client.firmcode)  
        AND firm.firmcode = if(sel.firmtype = 0, sel.firmcode, firm.firmcode)  
        AND oh.writetime BETWEEN ?FromDate AND ADDDATE(?ToDate, INTERVAL 1 DAY)
        AND pricesdata.pricecode = oh.pricecode  
        AND firm.firmcode = pricesdata.firmcode 
        AND sel.firmcode = ?clientCode 
		AND oh.RegionCode & ?RegionCode > 0
ORDER BY writetime desc;
", _connection);
			adapter.SelectCommand.Parameters.AddWithValue("?FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?ToDate", CalendarTo.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?clientCode", Convert.ToUInt32(Request["cc"]));
			adapter.SelectCommand.Parameters.AddWithValue("?RegionCode", SecurityContext.Administrator.RegionMask);

			_data = new DataSet();
			try
			{
				_connection.Open();
				adapter.SelectCommand.Transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				adapter.Fill(_data);
				adapter.SelectCommand.Transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
			OrdersGrid.DataSource = _data.DefaultViewManager.CreateDataView(_data.Tables[0]); 
			DataBind();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers);

			if (Page.IsPostBack) 
				return;

			CalendarFrom.SelectedDates.Add(DateTime.Now.AddDays(-1));
			CalendarTo.SelectedDates.Add(DateTime.Now);
		}
		protected void OrdersGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType != DataControlRowType.Header) || (String.IsNullOrEmpty(_sortExpression))) 
				return;

			var grid = sender as GridView;
			foreach (DataControlField field in grid.Columns)
			{
				if (field.SortExpression == _sortExpression)
				{
					var sortIcon = new Image();
					sortIcon.ImageUrl = _sortDirection == SortDirection.Ascending ? "./Images/arrow-down-blue-reversed.gif" : "./Images/arrow-down-blue.gif";
					e.Row.Cells[grid.Columns.IndexOf(field)].Controls.Add(sortIcon);
				}
			}
		}
		
		protected void OrdersGrid_Sorting(object sender, GridViewSortEventArgs e)
		{
			if (_sortExpression == e.SortExpression)
				_sortDirection = _sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			_sortExpression = e.SortExpression;

			_data.Tables[0].DefaultView.Sort = _sortExpression + (_sortDirection == SortDirection.Ascending ? " ASC" : " DESC");

			OrdersGrid.DataSource = _data.Tables[0].DefaultView;
			DataBind();
		}
}
}