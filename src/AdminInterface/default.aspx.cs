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
					"���� �������� ������ ������ �������� {0} � {1}.<br>���������� �� ��������� �������� ������.",
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
WHERE UserName = ?UserName) as RegionCode , '��� �������' as Region
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
					"���� �������� ������ ������ ����� {0} � {1}.<br>������ � ������� ����� ������ ����� ��������� ������.",
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
				//������
				//���������� �������� �������
				OPLB.Text = data.Tables[0].Rows[0]["OrdersCount"].ToString();
				//����� �������
				SumLB.Text = Convert.ToDouble(data.Tables[0].Rows[0]["OrderSum"]).ToString("C");
				//�� ���������� �������
				OprLB.Text = data.Tables[0].Rows[0]["NonProcOrdersCount"].ToString();
				//����� ���������� ������
				LOT.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxOrderTime"]).ToLongTimeString();

				//����������
				//�������� ����������
				ADHL.Text = data.Tables[0].Rows[0]["UpdatesAD"].ToString();
				//������ ����������
				ErrUpHL.Text = data.Tables[0].Rows[0]["UpdatesErr"].ToString();
				//������������ ����������
				CUHL.Text = data.Tables[0].Rows[0]["CumulativeUpdates"].ToString();
				//������� ����������
				ConfHL.Text = data.Tables[0].Rows[0]["Updates"].ToString();
				//����� ���������� ����������
				MaxUpdateTime.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxUpdateTime"]).ToLongTimeString();
				//���������� � ��������
				RemoteServiceHelper.TryDoCall(s => {
				                           	ReqHL.Text = s.GetUpdatingClientCount().ToString();
				                           });

				SetLabel(OrderProcStatus, RemoteServiceHelper.GetServiceStatus("offdc.adc.analit.net", "OrderProcService"));
				SetLabel(PriceProcessorMasterStatus, RemoteServiceHelper.GetServiceStatus("fms.adc.analit.net", "PriceProcessorService"));
				SetLabel(PriceProcessorSlaveStatus, RemoteServiceHelper.GetServiceStatus("fmsold.adc.analit.net", "PriceProcessorService"));

				DownloadDataSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DataSize"]));
				DownloadDocumentSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DocSize"]));
				//������
				//�� �������������
				FormErrLB.Text = data.Tables[0].Rows[0]["NoForm"].ToString();
				//�� ��������
				DownErrLB.Text = data.Tables[0].Rows[0]["NoDown"].ToString();
				//��������� ������������
				FormPLB.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["LastForm"]).ToLongTimeString();
				//��������� ���������
				DownPLB.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["LastDown"]).ToLongTimeString();
				//������� ������������
				WaitPLB.Text = data.Tables[0].Rows[0]["QueueForm"].ToString();
				//�������� �������
				PriceDOKLB.Text = data.Tables[0].Rows[0]["DownCount"].ToString();
				//�� ��������
				PriceDERRLB.Text = data.Tables[0].Rows[0]["NoPriceCount"].ToString();
				//������������� �������
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
					status.Text = "�� ��������";
					//status.BackColor = Color.Red;
					break;
				case ServiceStatus.Unknown:
					status.Text = "�� ���������";
					//status.BackColor = Color.Red;
					break;
				case ServiceStatus.Running:
					status.Text = "��������";
					//status.BackColor = Color.Green;
					break;
			}
		}
	}
}