using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class orders : Page
	{
		private MySqlConnection _connection = new MySqlConnection(Literals.GetConnectionString());
		
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
			MySqlDataAdapter adapter = new MySqlDataAdapter(
@"
SELECT  ordershead.rowid, 
        WriteTime, 
        PriceDate, 
        client.shortname as Customer, 
        firm.shortname as Supplier, 
        PriceName, 
        RowCount, 
        Processed  
FROM    orders.ordershead, 
        usersettings.pricesdata, 
        usersettings.clientsdata as firm, 
        usersettings.clientsdata as client, 
        usersettings.clientsdata as sel 
WHERE   clientcode         =client.firmcode  
        AND client.firmcode=if(sel.firmtype=1, sel.firmcode, client.firmcode)  
        AND firm.firmcode  =if(sel.firmtype=0, sel.firmcode, firm.firmcode)  
        AND writetime BETWEEN ?FromDate AND ADDDATE(?ToDate, INTERVAL 1 DAY)
        AND pricesdata.pricecode=ordershead.pricecode  
        AND firm.firmcode       =pricesdata.firmcode 
        AND sel.firmcode        =?clientCode 
ORDER BY writetime desc;
", _connection);
			adapter.SelectCommand.Parameters.Add("FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.Add("ToDate", CalendarTo.SelectedDate);
			adapter.SelectCommand.Parameters.Add("?clientCode", Convert.ToUInt32(Request["cc"]));

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
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
			{
				Response.Redirect("default.aspx");
			}
			if (!Page.IsPostBack)
			{
				CalendarFrom.SelectedDates.Add(DateTime.Now.AddDays(-1));
				CalendarTo.SelectedDates.Add(DateTime.Now);
			}
		}
		protected void OrdersGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType == DataControlRowType.Header) && (!String.IsNullOrEmpty(_sortExpression)))
			{
				GridView grid = sender as GridView;
				foreach (DataControlField field in grid.Columns)
				{
					if (field.SortExpression == _sortExpression)
					{
						Image sortIcon = new Image();
						sortIcon.ImageUrl = _sortDirection == SortDirection.Ascending ? "./Images/arrow-down-blue.gif" : "./Images/arrow-down-blue-reversed.gif";
						e.Row.Cells[grid.Columns.IndexOf(field)].Controls.Add(sortIcon);
					}
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