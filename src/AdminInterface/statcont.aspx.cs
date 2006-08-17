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
			MySqlConnection _connection = new MySqlConnection(Literals.GetConnectionString());
			MySqlDataAdapter adapter = new MySqlDataAdapter(
@"
SELECT  WriteTime, 
        clientsinfo.UserName, 
        ShortName, 
        Region, 
        Message, 
        FirmCode, 
        osuseraccessright.rowid OSUserID, 
        clientsinfo.rowid  
FROM    logs.clientsinfo, 
        usersettings.clientsdata, 
        accessright.showright, 
        usersettings.osuseraccessright, 
        farm.regions  
WHERE   clientsinfo.clientcode                            =firmcode 
        AND osuseraccessright.clientcode                  =clientsinfo.clientcode  
        AND showright.RegionMask & clientsdata.RegionCode > 0 
        AND writetime BETWEEN ?FromDate AND ?ToDate  
        AND regions.regioncode=clientsdata.RegionCode 
        AND showright.username=?userName 
ORDER BY WriteTime
", _connection);
			adapter.SelectCommand.Parameters.Add("UserName", Session["UserName"]);
			adapter.SelectCommand.Parameters.Add("FromDate", CalendarFrom.SelectedDate);
			adapter.SelectCommand.Parameters.Add("ToDate", CalendarTo.SelectedDate.AddDays(1));			
			
			DataSet data = new DataSet();
			adapter.Fill(data);
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
			GridView grid = (GridView) sender;
			
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
				GridView grid = sender as GridView;
				foreach (DataControlField field in grid.Columns)
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
}
}