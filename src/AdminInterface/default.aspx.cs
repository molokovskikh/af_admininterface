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