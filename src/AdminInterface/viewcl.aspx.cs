using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;
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
			get
			{
				if (Session["StatisticsDataView"] == null)
					throw new SessionOutDateException();
				return (DataView)Session["StatisticsDataView"];
			}
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
			if (Convert.ToDouble(Session["AccessGrant"]) != 1)
			    Response.Redirect("default.aspx");

			var headerText = String.Empty;

			var adapter = new MySqlDataAdapter("", Literals.GetConnectionString()); 

			switch (RequestType)
			{ 
				case StatisticsType.UpdateBan:
					headerText = "Запреты:";
					adapter.SelectCommand.CommandText = @"
SELECT  RequestTime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.AnalitFUpdates p, usersettings.clientsdata, accessright.regionaladmins, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND p.UpdateType					  = 5
        AND regionaladmins.regionmask & maskregion > 0 
		AND regionaladmins.username = ?UserName 
		AND p.RequestTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.UpdateId
ORDER by p.RequestTime desc;
";
					break;
				case StatisticsType.UpdateCumulative:
					headerText = "Кумулятивные обновления:";
					adapter.SelectCommand.CommandText = @"
SELECT  RequestTime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.AnalitFUpdates p, usersettings.clientsdata, accessright.regionaladmins, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 2
        AND regionaladmins.regionmask & maskregion > 0 
		AND regionaladmins.username = ?UserName 
		AND p.RequestTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.UpdateId
ORDER by p.RequestTime desc;
";
					break;
				case StatisticsType.UpdateError:
					headerText = "Ошибки подготовки данных:";
					adapter.SelectCommand.CommandText = @"
SELECT  RequestTime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.AnalitFUpdates p, usersettings.clientsdata, accessright.regionaladmins, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 6
        AND regionaladmins.regionmask & maskregion > 0 
		AND regionaladmins.username = ?UserName 
		AND p.RequestTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.UpdateId
ORDER by p.RequestTime desc;
";
					break;
				case StatisticsType.UpdateNormal:
					headerText = "Обычные обновления:";
					adapter.SelectCommand.CommandText = @"
SELECT  RequestTime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.AnalitFUpdates p, usersettings.clientsdata, accessright.regionaladmins, farm.regions r, usersettings.retclientsset rcs)
WHERE   rcs.clientcode						  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 1
        AND regionaladmins.regionmask & maskregion > 0 
		AND regionaladmins.username = ?UserName 
		AND p.RequestTime BETWEEN ?BeginDate AND ?EndDate
		AND r.regioncode & ?RegionMask > 0
GROUP by p.UpdateId
ORDER by p.RequestTime desc;
";
					break;
				case StatisticsType.InUpdateProcess:
					adapter.SelectCommand.CommandText = @" 
SELECT  regionaladmins.RowID,   
        p.RequestTime,   
        clientsdata.FirmCode,   
        clientsdata.ShortName,   
        r.Region,   
        p.Addition  
FROM (usersettings.clientsdata, accessright.regionaladmins, farm.regions r)
	JOIN usersettings.ret_update_info rui ON rui.ClientCode = clientsdata.firmcode
LEFT JOIN logs.AnalitFUpdates p
        ON p.clientcode                                   = rui.clientcode 
        AND p.RequestTime                                 > curdate()  
WHERE   r.regioncode                                  = clientsdata.regioncode  
        AND regionaladmins.username                       = ?UserName
        AND regionaladmins.regionmask & clientsdata.maskregion > 0  
        AND rui.UncommittedUpdateTime                    >= CURDATE()  
        AND rui.UpdateTime                               <> rui.UncommittedUpdateTime  
        AND p.UpdateId                                   = 
        (SELECT max(pl.UpdateId) 
        FROM    logs.AnalitFUpdates pl 
        WHERE   pl.clientcode = rui.clientcode)  
ORDER BY p.RequestTime desc;
";
					headerText = "В процессе получения обновления:";
					break;
				case StatisticsType.Download:
					adapter.SelectCommand.CommandText = @"
SELECT  RequestTime, 
        FirmCode, 
        ShortName, 
        Region, 
        Addition  
FROM    (logs.AnalitFUpdates p, usersettings.clientsdata, accessright.regionaladmins, farm.regions r, usersettings.retclientsset rcs)
WHERE   p.RequestTime > curDate()
		AND rcs.clientcode					  = p.clientcode 
        AND firmcode                          = p.clientcode 
        AND r.regioncode                      = clientsdata.regioncode 
		AND UpdateType						  = 3
        AND regionaladmins.regionmask & maskregion > 0 
		AND regionaladmins.username = ?UserName 
GROUP by p.UpdateId
ORDER by p.RequestTime desc;
";
					headerText = "Докачки:";
					break;
			}
			HeaderLB.Text = headerText;
			adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
			adapter.SelectCommand.Parameters.AddWithValue("?BeginDate", BeginDate);
			adapter.SelectCommand.Parameters.AddWithValue("?EndDate", EndDate);
			adapter.SelectCommand.Parameters.AddWithValue("?RegionMask", RegionMask);

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
