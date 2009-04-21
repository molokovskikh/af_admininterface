using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class orders : Page
	{
		
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
			Update();
		}

		private void Update()
		{
			var clientCode = Convert.ToUInt32(Request["cc"]);
			var adapter = new MySqlDataAdapter(@"
SELECT  oh.rowid, 
        oh.WriteTime, 
        PriceDate, 
        client.shortname as Customer, 
        firm.shortname as Supplier, 
        PriceName, 
		oh.PriceCode,
        oh.RowCount, 
        oh.Processed, 
		max(o.ResultCode) as ResultCode,
		o.TransportType,
		oh.SubmitDate,
		oh.ClientOrderId
FROM    (orders.ordershead oh, usersettings.clientsdata as sel)
		join usersettings.pricesdata on pricesdata.pricecode = oh.pricecode
			join usersettings.clientsdata as firm on firm.firmcode = pricesdata.firmcode 
        join usersettings.clientsdata as client on oh.clientcode = client.firmcode
		left join logs.orders o on oh.rowid = o.orderid
WHERE   client.firmcode = if(sel.firmtype = 1, sel.firmcode, client.firmcode)  
        AND firm.firmcode = if(sel.firmtype = 0, sel.firmcode, firm.firmcode)  
        AND oh.writetime BETWEEN ?FromDate AND ADDDATE(?ToDate, INTERVAL 1 DAY)
        AND sel.firmcode = ?clientCode 
		AND oh.RegionCode & ?RegionCode > 0
		and oh.Deleted = 0
group by oh.rowid
ORDER BY writetime desc;
", Literals.GetConnectionString());
			adapter.SelectCommand.Parameters.AddWithValue("?FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?ToDate", CalendarTo.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?clientCode", clientCode);
			adapter.SelectCommand.Parameters.AddWithValue("?RegionCode", SecurityContext.Administrator.RegionMask);
			_data = new DataSet();
			adapter.Fill(_data);

			OrdersGrid.Columns[OrdersGrid.Columns.Count - 1].Visible = IsOrderSubmitEnabled(clientCode);
			OrdersGrid.DataSource = _data.DefaultViewManager.CreateDataView(_data.Tables[0]); 
			DataBind();
		}

		private static bool IsOrderSubmitEnabled(uint clientCode)
		{
			using (var connection = new MySqlConnection(Literals.GetConnectionString()))
			{
				connection.Open();
				return Convert.ToInt32(MySqlHelper.ExecuteScalar(connection,
				                          @"
select SubmitOrders = 1 and AllowSubmitOrders = 1
from retclientsset 
where clientcode = ?ClientCode", new MySqlParameter("?ClientCode", clientCode))) == 1;
			}
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.ViewDrugstore, PermissionType.ViewSuppliers);

			if (Page.IsPostBack) 
				return;

			CalendarFrom.SelectedDates.Add(DateTime.Now.AddDays(-1));
			CalendarTo.SelectedDates.Add(DateTime.Now);
			Update();
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

		public static string GetSubmiteDate(DataRowView row)
		{
			if (row["SubmitDate"] == DBNull.Value)
				return "Заказ не подтвержден";
			return row["SubmitDate"].ToString();
		}

		public static string GetResult(DataRowView row)
		{
			if (row["TransportType"] == DBNull.Value || Convert.ToInt32(row["ResultCode"]) == 0)
				return "Не отправлен";

			if (Convert.ToInt32(row["PriceCode"]) == 2647)
				return "ok (Обезличенный заказ)";

			switch (Convert.ToInt32(row["TransportType"]))
			{
				case 1:
					return row["ResultCode"].ToString();
				case 2:
					return "ok (Ftp Инфорум)";
				case 4:
					return "ok (Ftp Поставщика)";
				default:
					return "ok (Собственный отправщик)";
			}
		}
	}
}