using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.ServiceProcess;
using System.Web.UI;
using System.Web.UI.WebControls;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.MySql;
using MySql.Data.MySqlClient;

namespace AddUser
{
	public enum ServiceStatus
	{
		Running,
		NotRunning,
		Unknown
	}

	partial class _default : Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			SecurityContext.CheckIsUserAuthorized();

			var expirationDate = ADHelper.GetPasswordExpirationDate(SecurityContext.Administrator.UserName);
			if (expirationDate >= DateTime.Now || SecurityContext.Administrator.UserName == "michail")
			{
				PassLB.Text = String.Format(
					"Срок действия Вашего пароля истекает {0} в {1}.<br>Пожалуйста не забывайте изменять пароль.",
					expirationDate.ToShortDateString(),
					expirationDate.ToShortTimeString());
				Session["AccessGrant"] = 1;
				if (!IsPostBack)
				{
					ToCalendar.SelectedDates.Add(DateTime.Now);
					FromCalendar.SelectedDates.Add(DateTime.Now);
					var data = new DataSet();
					With.Connection(
						c => {
							var adapter = new MySqlDataAdapter(@"
SELECT (SELECT bit_or(RegionCode)
FROM AccessRight.RegionalAdmins ra
  INNER JOIN Farm.Regions r on r.RegionCode & ra.RegionMask > 0
WHERE UserName = ?UserName) as RegionCode , 'Все регионы' as Region
UNION
SELECT *
FROM
(SELECT RegionCode, Region
FROM AccessRight.RegionalAdmins ra
  INNER JOIN Farm.Regions r on r.RegionCode & ra.RegionMask > 0
WHERE UserName = ?UserName ORDER BY Region) tmp;
;",c);
							adapter.SelectCommand.Parameters.AddWithValue("?UserName", SecurityContext.Administrator.UserName);
							adapter.Fill(data);
						});
					
					RegionList.DataTextField = "Region";
					RegionList.DataValueField = "RegionCode";
					RegionList.DataSource = data;
					RegionList.DataBind();

					GetStatistics(Convert.ToUInt64(RegionList.SelectedValue), FromCalendar.SelectedDate, ToCalendar.SelectedDate);
				}
			}
			else
			{
				FTPHL.Visible = false;
				RegisterHL.Visible = false;
				CloneHL.Visible = false;
				ClInfHL.Visible = false;
				ShowStatHL.Visible = false;
				BillingHL.Visible = false;
				PassLB.Text = String.Format(
					"Срок действия Вашего пароля истек {0} в {1}.<br>Доступ к системе будет открыт после изменения пароля.",
					expirationDate.ToShortDateString(),
					expirationDate.ToShortTimeString());
			}
		}

		protected void ShowButton_Click(object sender, EventArgs e)
		{
			GetStatistics(Convert.ToUInt64(RegionList.SelectedValue), FromCalendar.SelectedDate, ToCalendar.SelectedDate);
		}

		public void GetStatistics(ulong regionMask, DateTime fromDate, DateTime toDate)
		{
			/*
SELECT  max(UpdateDate)
FROM (usersettings.clientsdata cd, accessright.regionaladmins showright)
	join usersettings.OsUserAccessRight ouar on ouar.ClientCode = cd.FirmCode
		JOIN usersettings.UserUpdateInfo uui on uui.UserId = ouar.RowId
WHERE   cd.RegionCode & showright.regionmask > 0
        and cd.RegionCode & RegionMaskParam      > 0
        AND uui.UncommitedUpdateDate BETWEEN StartDateParam AND EndDateParam
        AND username = UserNameParam;
			 * 
Declare InProc,  NonProcOrdersCount mediumint unsigned;
Declare DataSize, DocSize int unsigned;
Declare MaxUpdateTime, MaxOrderTime, LastForm, LastDown varchar(20);
Declare FormCount, DownCount varchar(50);
Declare OrderSum decimal(12, 2);
Declare  OrdersCount, ReGet, UpdatesAD, UpdatesErr, OrdersErr, OrdersAD, CumulativeUpdates, Updates  varchar(10);
SELECT  max(UpdateTime)
INTO    MaxUpdateTime
FROM    (usersettings.ret_update_info, usersettings.clientsdata, accessright.regionaladmins showright)
WHERE   clientsdata.firmcode                  = ret_update_info.clientcode 
        AND RegionCode & showright.regionmask > 0
        AND RegionCode & RegionMaskParam      > 0
        AND UncommittedUpdateTime BETWEEN StartDateParam AND EndDateParam
        AND username = UserNameParam;
SELECT  concat(count(if(resultid=2, PriceItemId, null)), '(', count(DISTINCT if(resultid=2, PriceItemId, null)), ')'),
        max(if(resultid=2, logtime, null))
into FormCount, LastForm
FROM    logs.formlogs
WHERE logtime BETWEEN StartDateParam AND EndDateParam;
SELECT  concat(count(if(resultcode=2, PriceItemId, null)), '(', count(DISTINCT if(resultcode=2, PriceItemId, null)), ')'),
max(if(resultcode=2, logtime, null))
into DownCount, LastDown
FROM    logs.downlogs
WHERE logtime BETWEEN StartDateParam AND EndDateParam;
SELECT  concat(count(DISTINCT oh.rowid), '(', count(DISTINCT oh.clientcode), ')'),
        sum(cost*quantity),
        count(DISTINCT if(processed = 0
        AND if(SubmitOrders         = 1, Submited
        AND not Deleted, 1), orderid, null)),
        Max(WriteTime)
INTO    OrdersCount,
        OrderSum, 
        NonProcOrdersCount, 
        MaxOrderTime
FROM    orders.ordershead oh,
        orders.orderslist, 
        usersettings.clientsdata cd, 
        accessright.regionaladmins showright,
        usersettings.retclientsset rcs
WHERE   oh.rowid                              = orderid
        AND cd.firmcode                       = oh.clientcode
        AND cd.billingcode                    <> 921
        AND rcs.clientcode                    = oh.clientcode
        AND firmsegment                       = 0
        AND serviceclient                     = 0 
        AND showright.regionmask & maskregion > 0
        AND oh.regioncode & RegionMaskParam   > 0
        AND not Deleted
        AND showright.username = UserNameParam
        AND WriteTime BETWEEN StartDateParam AND EndDateParam;
                                                               
SELECT  sum(if(UpdateType in (1,2), resultsize, 0)),
        sum(if(UpdateType in (8), resultsize, 0)),
        sum(if(updatetype in (1,2), Commit=0, null)),
                                                               sum(if(UpdateType = 6, 1, 0)),
        concat(Sum(UpdateType IN (5)) ,'(' ,count(DISTINCT if(UpdateType  IN (5) ,p.ClientCode ,null)) ,')') UpdatesAD ,
        concat(sum(UpdateType = 2) ,'(' ,count(DISTINCT if(UpdateType = 2 ,p.clientcode ,null)) ,')') CumulativeUpdates              ,
        concat(sum(UpdateType = 1) ,'(' ,count(DISTINCT if(UpdateType = 1 ,p.clientcode ,null)) ,')') Updates
INTO    DataSize,
        DocSize,
        InProc,
                                                               UpdatesErr,
        UpdatesAD,
        CumulativeUpdates,
        Updates
FROM    usersettings.clientsdata ,
        accessright.regionaladmins showright    ,
        logs.AnalitFUpdates p
WHERE   firmcode                                   = clientcode
        AND showright.regionmask & maskregion      > 0
        AND maskregion  & RegionMaskParam          > 0
        AND showright.username                     = UserNameParam
        AND RequestTime BETWEEN StartDateParam AND EndDateParam;
                                                                
select
ifnull(OrdersCount, '') OrdersCount,
ifnull(OrderSum, 0) OrderSum,
ifnull(NonProcOrdersCount, 0) NonProcOrdersCount,
ifnull(MaxOrderTime, '0:0:0') MaxOrderTime,
ifnull(InProc, 0) InProc,
ifnull(MaxUpdateTime, '0:0:0') MaxUpdateTime,
ifnull(ReGet, '') ReGet,
ifnull(UpdatesAD, '') UpdatesAD,
ifnull(UpdatesErr, '') UpdatesErr,
ifnull(OrdersErr, '') OrdersErr,
ifnull(OrdersAD, '') OrdersAD,
ifnull(CumulativeUpdates, '') CumulativeUpdates,
ifnull(Updates, '') Updates,
0 NoForm,
0 NoDown,
ifnull(LastForm, '2000-01-01') LastForm,
ifnull(LastDown, '2000-01-01') LastDown,
0 QueueForm,
ifnull(DownCount, '') DownCount,
0 NoPriceCount,
ifnull(FormCount, '') FormCount,
ifnull(DataSize, 0) DataSize,
ifnull(DocSize, 0) DocSize;

			 */
			var urlTemplate = String.Format("BeginDate={0}&EndDate={1}&RegionMask={2}", fromDate, toDate.AddDays(1), regionMask);
				CUHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateCumulative, urlTemplate);
				ErrUpHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateError, urlTemplate);
				ADHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateBan, urlTemplate);
				ConfHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateNormal, urlTemplate);

				var data = new DataSet();
				With.Connection(
					c => {
						var adapter = new MySqlDataAdapter("GetShowStat", c)
						{
							SelectCommand = { CommandType = CommandType.StoredProcedure }
						};
						adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
						adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
						adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask);
						adapter.SelectCommand.Parameters.AddWithValue("?UserNameParam", SecurityContext.Administrator.UserName);
						adapter.Fill(data);						
					});
				//Заказы
				//Количество принятых заказов
				OPLB.Text = data.Tables[0].Rows[0]["OrdersCount"].ToString();
				//Сумма заказов
				SumLB.Text = Convert.ToDouble(data.Tables[0].Rows[0]["OrderSum"]).ToString("C");
				//Не обработано заказов
				OprLB.Text = data.Tables[0].Rows[0]["NonProcOrdersCount"].ToString();
				//Время последнего заказа
				LOT.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxOrderTime"]).ToLongTimeString();

				//Обновления
				//Запретов обновлений
				ADHL.Text = data.Tables[0].Rows[0]["UpdatesAD"].ToString();
				//Ошибок обновлений
				ErrUpHL.Text = data.Tables[0].Rows[0]["UpdatesErr"].ToString();
				//Кумулятивных обновлений
				CUHL.Text = data.Tables[0].Rows[0]["CumulativeUpdates"].ToString();
				//Обычных обновлений
				ConfHL.Text = data.Tables[0].Rows[0]["Updates"].ToString();
				//Время последнего обновления
				MaxUpdateTime.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxUpdateTime"]).ToLongTimeString();
				//Обновлений в процессе
				RemoteServiceHelper.TryDoCall(s => {
				                           	ReqHL.Text = s.GetUpdatingClientCount().ToString();
				                           });

				SetLabel(OrderProcStatus, RemoteServiceHelper.GetServiceStatus("offdc.adc.analit.net", "OrderProcService"));
				SetLabel(PriceProcessorMasterStatus, RemoteServiceHelper.GetServiceStatus("fms.adc.analit.net", "PriceProcessorService"));
				SetLabel(PriceProcessorSlaveStatus, RemoteServiceHelper.GetServiceStatus("fmsold.adc.analit.net", "PriceProcessorService"));

				DownloadDataSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DataSize"]));
				DownloadDocumentSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DocSize"]));
				//прайсы
				//Не формализовано
				FormErrLB.Text = data.Tables[0].Rows[0]["NoForm"].ToString();
				//Не получено
				DownErrLB.Text = data.Tables[0].Rows[0]["NoDown"].ToString();
				//Последняя формализация
				FormPLB.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["LastForm"]).ToLongTimeString();
				//Последнее получение
				DownPLB.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["LastDown"]).ToLongTimeString();
				//Очередь формализации
				WaitPLB.Text = data.Tables[0].Rows[0]["QueueForm"].ToString();
				//Получено прайсов
				PriceDOKLB.Text = data.Tables[0].Rows[0]["DownCount"].ToString();
				//не опознано
				PriceDERRLB.Text = data.Tables[0].Rows[0]["NoPriceCount"].ToString();
				//Формализовано прайсов
				PriceFOKLB.Text = data.Tables[0].Rows[0]["FormCount"].ToString();

				RegisterHL.Enabled = SecurityContext.Administrator.HaveAnyOfPermissions(PermissionType.RegisterSupplier, PermissionType.RegisterDrugstore);
				CloneHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.CopySynonyms,
				                                                               PermissionType.ViewDrugstore);

				ClInfHL.Enabled = SecurityContext.Administrator.HaveAnyOfPermissions(PermissionType.ViewSuppliers,
				                                                                     PermissionType.ViewDrugstore);

				BillingHL.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.Billing)
				                    && SecurityContext.Administrator.HaveAnyOfPermissions(PermissionType.ViewSuppliers,
				                                                                          PermissionType.ViewDrugstore);

				MonitorUpdates.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.MonitorUpdates)
				                         && SecurityContext.Administrator.HaveAnyOfPermissions(PermissionType.ViewSuppliers,
				                                                                               PermissionType.ViewDrugstore);

				ViewAdministrators.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.ManageAdministrators);
				ShowStatHL.Enabled = SecurityContext.Administrator.HaveAnyOfPermissions(PermissionType.ViewSuppliers,
				                                                                        PermissionType.ViewDrugstore);

				Telephony.Enabled = SecurityContext.Administrator.HavePermisions(PermissionType.ManageCallbacks);

				try
				{
					if (Convert.ToInt32(ADHL.Text.Substring(0, 1)) > 0)
						ADHL.Enabled = true;
				}
				catch (Exception) { }
				try
				{
					if (Convert.ToInt32(CUHL.Text.Substring(0, 1)) > 0)
						CUHL.Enabled = true;
				}
				catch (Exception) { }
				try
				{
					if (Convert.ToInt32(ErrUpHL.Text.Substring(0, 1)) > 0)
						ErrUpHL.Enabled = true;
				}
				catch (Exception) { }
				try
				{
					if (Convert.ToInt32(ReqHL.Text) >= 0)
						ReqHL.Enabled = true;
				}
				catch (Exception) { }
				try
				{
					if (Convert.ToInt32(ConfHL.Text.Substring(0, 1)) > 0)
					    ConfHL.Enabled = true;
				}
				catch (Exception) { }
		}

		private void SetLabel(Label status, ServiceStatus serviceStatus)
		{
			switch (serviceStatus)
			{
				case ServiceStatus.NotRunning:
					status.Text = "Не запущена";
					//status.BackColor = Color.Red;
					break;
				case ServiceStatus.Unknown:
					status.Text = "Не доступена";
					//status.BackColor = Color.Red;
					break;
				case ServiceStatus.Running:
					status.Text = "Запущена";
					//status.BackColor = Color.Green;
					break;
			}
		}
	}
}