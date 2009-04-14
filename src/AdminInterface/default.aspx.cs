using System;
using System.ComponentModel;
using System.Data;
using System.Web.UI;
using AdminInterface.Helpers;
using AdminInterface.Models.Security;
using AdminInterface.Security;
using Common.MySql;
using Common.Web.Ui.Helpers;
using MySql.Data.MySqlClient;

namespace AddUser
{
	public enum ServiceStatus
	{
		[Description("Не запущена")] Running,
		[Description("Запущена")] NotRunning,
		[Description("Не доступена")] Unknown
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
			var urlTemplate = String.Format("BeginDate={0}&EndDate={1}&RegionMask={2}", fromDate, toDate.AddDays(1), regionMask);
				CUHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateCumulative, urlTemplate);
				ErrUpHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateError, urlTemplate);
				ADHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateBan, urlTemplate);
				ConfHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateNormal, urlTemplate);

				var data = new DataSet();
				With.Connection(
					c => {
						var adapter = new MySqlDataAdapter(@"
SELECT cast(ifnull(max(UpdateDate), '0:0:0') as CHAR) MaxUpdateTime
FROM (usersettings.clientsdata cd, accessright.regionaladmins showright)
	join usersettings.OsUserAccessRight ouar on ouar.ClientCode = cd.FirmCode
		JOIN usersettings.UserUpdateInfo uui on uui.UserId = ouar.RowId
WHERE   cd.RegionCode & showright.regionmask > 0
        and cd.RegionCode & ?RegionMaskParam > 0
        AND uui.UncommitedUpdateDate BETWEEN ?StartDateParam AND ?EndDateParam
        AND username = ?UserNameParam;	  
		 
SELECT cast(concat(count(if(resultid=2, PriceItemId, null)), '(', count(DISTINCT if(resultid=2, PriceItemId, null)), ')') as CHAR) as FormCount,
       cast(ifnull(max(if(resultid=2, logtime, null)), '2000-01-01') as CHAR) as LastForm
FROM logs.formlogs
WHERE logtime BETWEEN ?StartDateParam AND ?EndDateParam;
			 
SELECT cast(concat(count(if(resultcode=2, PriceItemId, null)), '(', count(DISTINCT if(resultcode=2, PriceItemId, null)), ')') as CHAR) as DownCount,
	   cast(ifnull(max(if(resultcode=2, logtime, null)), '2000-01-01') as CHAR) as LastDown
FROM logs.downlogs
WHERE logtime BETWEEN ?StartDateParam AND ?EndDateParam;
			 
SELECT cast(concat(count(DISTINCT oh.rowid), '(', count(DISTINCT oh.clientcode), ')') as CHAR) as OrdersCount,
       ifnull(sum(cost*quantity), 0) as OrderSum,
       count(DISTINCT if(processed = 0 AND if(SubmitOrders = 1, Submited AND not Deleted, 1), orderid, null)) as NonProcOrdersCount,
       cast(ifnull(Max(WriteTime), '0:0:0') as CHAR) as MaxOrderTime
FROM orders.ordershead oh,
     orders.orderslist, 
     usersettings.clientsdata cd, 
     accessright.regionaladmins showright,
     usersettings.retclientsset rcs
WHERE oh.rowid = orderid
      AND cd.firmcode                       = oh.clientcode
      AND cd.billingcode                    <> 921
      AND rcs.clientcode                    = oh.clientcode
      AND firmsegment                       = 0
      AND serviceclient                     = 0 
      AND showright.regionmask & maskregion > 0
      AND oh.regioncode & ?RegionMaskParam   > 0
      AND not Deleted
      AND showright.username = ?UserNameParam
      AND WriteTime BETWEEN ?StartDateParam AND ?EndDateParam;
                                                               
SELECT ifnull(sum(if(afu.UpdateType in (1,2), resultsize, 0)), 0) as DataSize,
       ifnull(sum(if(afu.UpdateType in (8), resultsize, 0)), 0) as DocSize,
       sum(if(afu.UpdateType = 6, 1, 0)) as UpdatesErr,
       cast(concat(Sum(afu.UpdateType IN (5)) ,'(' ,count(DISTINCT if(afu.UpdateType  IN (5), cd.FirmCode, null)) ,')') as CHAR) UpdatesAD,
       cast(concat(sum(afu.UpdateType = 2) ,'(' ,count(DISTINCT if(afu.UpdateType = 2, cd.FirmCode, null)) ,')') as CHAR) CumulativeUpdates,
       cast(concat(sum(afu.UpdateType = 1) ,'(' ,count(DISTINCT if(afu.UpdateType = 1, cd.FirmCode, null)) ,')') as CHAR) Updates
FROM (usersettings.clientsdata cd, accessright.regionaladmins showright)
	join usersettings.OsUserAccessRight ouar on ouar.ClientCode = cd.FirmCode
	join logs.AnalitFUpdates afu on afu.UserId = ouar.RowId
WHERE showright.regionmask & maskregion > 0
      AND cd.maskregion & ?RegionMaskParam > 0
      AND showright.username = ?UserNameParam
      AND afu.RequestTime BETWEEN ?StartDateParam AND ?EndDateParam;", c);
						adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
						adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
						adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask);
						adapter.SelectCommand.Parameters.AddWithValue("?UserNameParam", SecurityContext.Administrator.UserName);
						adapter.Fill(data);						
					});
				//Заказы
				//Количество принятых заказов
				OPLB.Text = data.Tables[3].Rows[0]["OrdersCount"].ToString();
				//Сумма заказов
				SumLB.Text = Convert.ToDouble(data.Tables[3].Rows[0]["OrderSum"]).ToString("C");
				//Не обработано заказов
				OprLB.Text = data.Tables[3].Rows[0]["NonProcOrdersCount"].ToString();
				//Время последнего заказа
				LOT.Text = Convert.ToDateTime(data.Tables[3].Rows[0]["MaxOrderTime"]).ToLongTimeString();

				//Обновления
				//Запретов обновлений
				ADHL.Text = data.Tables[4].Rows[0]["UpdatesAD"].ToString();
				//Ошибок обновлений
				ErrUpHL.Text = data.Tables[4].Rows[0]["UpdatesErr"].ToString();
				//Кумулятивных обновлений
				CUHL.Text = data.Tables[4].Rows[0]["CumulativeUpdates"].ToString();
				//Обычных обновлений
				ConfHL.Text = data.Tables[4].Rows[0]["Updates"].ToString();
				//Время последнего обновления
				MaxUpdateTime.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxUpdateTime"]).ToLongTimeString();
				//Обновлений в процессе
				RemoteServiceHelper.TryDoCall(s => {
				                           	ReqHL.Text = s.GetUpdatingClientCount().ToString();
				                           });
#if !DEBUG
				OrderProcStatus.Text = BindingHelper.GetDescription(RemoteServiceHelper.GetServiceStatus("offdc.adc.analit.net", "OrderProcService"));
				PriceProcessorMasterStatus.Text = BindingHelper.GetDescription(RemoteServiceHelper.GetServiceStatus("fms.adc.analit.net", "PriceProcessorService"));
				PriceProcessorSlaveStatus.Text = BindingHelper.GetDescription(RemoteServiceHelper.GetServiceStatus("fmsold.adc.analit.net", "PriceProcessorService"));
#endif

				DownloadDataSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[4].Rows[0]["DataSize"]));
				DownloadDocumentSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[4].Rows[0]["DocSize"]));
				//прайсы
				//Последняя формализация
				FormPLB.Text = Convert.ToDateTime(data.Tables[1].Rows[0]["LastForm"]).ToLongTimeString();
				//Последнее получение
				DownPLB.Text = Convert.ToDateTime(data.Tables[2].Rows[0]["LastDown"]).ToLongTimeString();
				//Получено прайсов
				PriceDOKLB.Text = data.Tables[2].Rows[0]["DownCount"].ToString();
				//Формализовано прайсов
				PriceFOKLB.Text = data.Tables[1].Rows[0]["FormCount"].ToString();

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
	}
}