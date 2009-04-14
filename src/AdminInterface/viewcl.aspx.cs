using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;

namespace AddUser
{
	public class SessionOutDateException : ApplicationException
	{}

	public partial class viewcl : Page
	{
		protected DateTime BeginDate
		{
			get { return Convert.ToDateTime(Request["BeginDate"]); }
		}

		protected DateTime EndDate
		{
			get { return Convert.ToDateTime(Request["EndDate"]); }
		}

		protected ulong RegionMask
		{
			get { return Convert.ToUInt64(Request["RegionMask"]); }
		}

		protected StatisticsType RequestType
		{
			get { return (StatisticsType)Convert.ToUInt32(Request["id"]); }
		}

		protected DataView StatisticsDataView
		{
			get { return (DataView)Session["StatisticsDataView"]; }
			set { Session["StatisticsDataView"] = value; }
		}

		protected string SortExpression
		{
			get { return (string)(ViewState["SortExpression"] ?? String.Empty); }
			set { ViewState["SortExpression"] = value; }
		}

		protected SortDirection SortDirection
		{
			get { return (SortDirection)(ViewState["SortDirection"] ?? SortDirection.Ascending); }
			set { ViewState["SortDirection"] = value; }
		}

		protected void Page_Load(object sender, EventArgs e)
		{
			StateHelper.CheckSession(this, ViewState);
			SecurityContext.Administrator.CheckPermisions(PermissionType.ViewDrugstore);

			var adapter = new MySqlDataAdapter("", Literals.GetConnectionString()); 
			HeaderLB.Text = BindingHelper.GetDescription(RequestType);
			adapter.SelectCommand.CommandText = @"
SELECT  afu.RequestTime, 
        cd.FirmCode, 
        cd.ShortName, 
        r.Region, 
		afu.AppVersion,
		afu.ResultSize,
        afu.Addition
FROM usersettings.clientsdata cd
	join farm.regions r on r.regioncode = cd.regioncode 
	join usersettings.OsUserAccessRight ouar on ouar.ClientCode = cd.FirmCode
	join logs.AnalitFUpdates afu on afu.UserId = ouar.RowId
WHERE afu.UpdateType = ?UpdateType
	  and afu.RequestTime BETWEEN ?BeginDate AND ?EndDate
	  and r.regioncode & ?RegionMask > 0
	  and cd.RegionCode & ?AdminMaskRegion > 0
GROUP by afu.UpdateId
ORDER by afu.RequestTime desc;";
			adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
			adapter.SelectCommand.Parameters.AddWithValue("?BeginDate", BeginDate);
			adapter.SelectCommand.Parameters.AddWithValue("?EndDate", EndDate);
			adapter.SelectCommand.Parameters.AddWithValue("?RegionMask", RegionMask & SecurityContext.Administrator.RegionMask);
			adapter.SelectCommand.Parameters.AddWithValue("?AdminMaskRegion", SecurityContext.Administrator.RegionMask);
			adapter.SelectCommand.Parameters.AddWithValue("?UpdateType", RequestType);

			var data = new DataSet();

			CountLB.Text = Convert.ToString(adapter.Fill(data));
			StatisticsDataView = data.Tables[0].DefaultView;
			CLList.DataBind();
		}

		protected void CLList_Sorting(object sender, GridViewSortEventArgs e)
		{
			var grid = (GridView)sender;

			if (SortExpression == e.SortExpression)
				SortDirection = SortDirection == SortDirection.Ascending ? SortDirection.Descending : SortDirection.Ascending;
			SortExpression = e.SortExpression;

			StatisticsDataView.Sort = SortExpression + (SortDirection == SortDirection.Ascending ? " ASC" : " DESC");

			grid.DataBind();
		}

		protected void CLList_RowCreated(object sender, GridViewRowEventArgs e)
		{
			if ((e.Row.RowType != DataControlRowType.Header) || (SortExpression == String.Empty))
				return;
			var grid = (GridView)sender;
			foreach (DataControlField field in grid.Columns)
			{
				if (field.SortExpression != SortExpression)
					continue;
				var sortIcon = new Image
				{
					ImageUrl =
						(SortDirection == SortDirection.Ascending
							 ? "./Images/arrow-down-blue-reversed.gif"
							 : "./Images/arrow-down-blue.gif")
				};
				e.Row.Cells[grid.Columns.IndexOf(field)].Controls.Add(sortIcon);
			}
		}
	}
}
