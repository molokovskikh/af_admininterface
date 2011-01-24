using System;
using System.Data;
using System.Drawing;
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
			
			var data = new DataSet();
			With.Connection(c => {
			                	var adapter = new MySqlDataAdapter(String.Format(@"
SELECT  ci.WriteTime, 
        if(length(ra.ManagerName) > 0, ra.ManagerName, ra.UserName) as UserName, 
        cl.Name ShortName, 
        r.Region, 
        ci.Message, 
        cl.Id FirmCode, 
        ci.rowid ,
		usr.Id,
		usr.Login,
		ci.UserId
FROM    logs.clientsinfo ci
	JOIN future.Clients cl ON cl.Id = ci.clientcode
        JOIN farm.regions r ON r.regioncode = cl.RegionCode 
		LEFT JOIN future.Users usr ON usr.Id = ci.UserId
		LEFT JOIN `accessright`.`regionaladmins` ra ON ra.UserName = ci.UserName
WHERE   ci.WriteTime >= ?FromDate AND ci.WriteTime <= ?ToDate  
		and (cl.Name like ?SearchText 
			or ci.Message like ?SearchText 
			or ci.UserName like ?SearchText
            or usr.Login like ?SearchText)
		and cl.RegionCode & ?AdminMaskRegion > 0
		{0}
ORDER BY WriteTime DESC", SecurityContext.Administrator.GetClientFilterByType("cl")), c);
			                	adapter.SelectCommand.Parameters.AddWithValue("?SearchText", '%' + SearchText.Text + '%');
			                	adapter.SelectCommand.Parameters.AddWithValue("?FromDate", CalendarFrom.SelectedDate);
			                	adapter.SelectCommand.Parameters.AddWithValue("?ToDate", CalendarTo.SelectedDate.AddDays(1));
			                	adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
			                	adapter.Fill(data);
			                });
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