using System;
using System.Data;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;
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
			var adapter = new MySqlDataAdapter(
@"
SELECT  WriteTime, 
        clientsinfo.UserName, 
        ShortName, 
        Region, 
        Message, 
        FirmCode, 
        clientsinfo.rowid  
FROM    logs.clientsinfo, 
        usersettings.clientsdata, 
        accessright.regionaladmins, 
        farm.regions  
WHERE   clientsinfo.clientcode = firmcode 
        AND regionaladmins.RegionMask & clientsdata.RegionCode > 0 
        AND writetime BETWEEN ?FromDate AND ?ToDate  
        AND regions.regioncode = clientsdata.RegionCode 
        AND regionaladmins.username = ?userName 
		and (ShortName like ?SearchText 
			or Message like ?SearchText 
			or clientsinfo.UserName like ?SearchText)
ORDER BY WriteTime DESC
", _connection);
			adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
			adapter.SelectCommand.Parameters.AddWithValue("?SearchText", '%' + SearchText.Text + '%');
			adapter.SelectCommand.Parameters.AddWithValue("?FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.AddWithValue("?ToDate", CalendarTo.SelectedDate.AddDays(1));			
			
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
			if (Convert.ToInt32(Session["AccessGrant"]) != 1)
				Response.Redirect("default.aspx");
			
			if (!IsPostBack)
			{
				CalendarFrom.SelectedDates.Add(DateTime.Now.AddDays(-14));		
				CalendarTo.SelectedDates.Add(DateTime.Now);

				ShowStatistic();
			}
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
			if ((e.Row.RowType == DataControlRowType.Header) && (_sortExpression != String.Empty))
			{
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
		}

		protected void Button1_Click(object sender, EventArgs e)
		{
			ShowStatistic();
		}
}
}