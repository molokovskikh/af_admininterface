using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using MySql.Data.MySqlClient;
using Image = System.Web.UI.WebControls.Image;

namespace AddUser
{
	public partial class statcont : Page
	{
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
		
		private DataView _view
		{
			get { return (DataView) Session["StatisticView"];}
			set { Session["StatisticView"] = value;}
		}
		
		private void ShowStatistic()
		{
			var _connection = new MySqlConnection(Literals.GetConnectionString());
			var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  ci.WriteTime, 
        ci.UserName, 
        cd.ShortName, 
        r.Region, 
        ci.Message, 
        cd.FirmCode, 
        ci.rowid  
FROM    logs.clientsinfo ci
	JOIN usersettings.clientsdata cd ON ci.clientcode = firmcode 
        JOIN farm.regions r ON r.regioncode = cd.RegionCode 
WHERE   ci.WriteTime BETWEEN ?FromDate AND ?ToDate  
		and (cd.ShortName like ?SearchText 
			or ci.Message like ?SearchText 
			or ci.UserName like ?SearchText)
		and cd.RegionCode & ?AdminMaskRegion > 0
		{0}
ORDER BY WriteTime DESC
", SecurityContext.Administrator.GetClientFilterByType("cd")), _connection);
			adapter.SelectCommand.Parameters.AddWithValue("?SearchText", '%' + SearchText.Text + '%');
			adapter.SelectCommand.Parameters.AddWithValue("?FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?ToDate", CalendarTo.SelectedDate.AddDays(1));
			adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
			
			var data = new DataSet();
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
			_view = data.Tables[0].DefaultView;
			
			StatisticGrid.DataSource = _view;
			StatisticGrid.DataBind();
		}


		protected void CalendarTo_SelectionChanged(object sender, EventArgs e)
		{
			ShowStatistic();
		}

		protected void CalendarFrom_SelectionChanged(object sender, EventArgs e)
		{
			ShowStatistic();
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckAnyOfPermissions(PermissionType.ViewSuppliers, PermissionType.ViewDrugstore);

			if (IsPostBack) 
				return;

			CalendarFrom.SelectedDates.Add(DateTime.Now.AddDays(-14));		
			CalendarTo.SelectedDates.Add(DateTime.Now);

			ShowStatistic();
		}

		protected void StatisticGrid_Sorting(object sender, GridViewSortEventArgs e)
		{
			var grid = (GridView) sender;
			
			if (_sortExpression == e.SortExpression)
				_sortDirection = _sortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			_sortExpression = e.SortExpression;

			_view.Sort = _sortExpression + (_sortDirection == SortDirection.Ascending ? " ASC" : " DESC");
			
			grid.DataSource = _view;
			grid.DataBind();
		}
		
		protected void StatisticGrid_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType != DataControlRowType.Header) || (_sortExpression == String.Empty)) 
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

		protected void Button1_Click(object sender, EventArgs e)
		{
			ShowStatistic();
		}
}
}