using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Web.UI;
using AdminInterface.Helpers;
using DAL;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class _default : Page
	{
		private MySqlConnection _connection = new MySqlConnection();

		protected void Page_Load(object sender, EventArgs e)
		{
			_connection.ConnectionString = Literals.GetConnectionString();
			var expirationDate = ADHelper.GetPasswordExpirationDate(Session["UserName"].ToString());
			if (expirationDate >= DateTime.Now || Convert.ToString(Session["UserName"]) == "michail")
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
					DataSet data = new DataSet();
					MySqlDataAdapter adapter = new MySqlDataAdapter(@"
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
;
", _connection);
					adapter.SelectCommand.Parameters.Add("?UserName", Session["UserName"]);
					adapter.Fill(data);
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
			try
			{
				string urlTemplate = String.Format("BeginDate={0}&EndDate={1}&RegionMask={2}", fromDate, toDate.AddDays(1), regionMask);
				CUHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateCumulative, urlTemplate);
				ErrUpHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateError, urlTemplate);
				ADHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateBan, urlTemplate);
				ConfHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateNormal, urlTemplate);
				
				_connection.Open();
				MySqlTransaction transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				MySqlDataAdapter adapter = new MySqlDataAdapter("GetShowStat", _connection);
				adapter.SelectCommand.Transaction = transaction;
				adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
				adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
				adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
				adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask);
				adapter.SelectCommand.Parameters.AddWithValue("?UserNameParam", Session["UserName"]);
				DataSet data = new DataSet();
				adapter.Fill(data);
				//������
				//���������� �������� �������
				OPLB.Text = data.Tables[0].Rows[0]["OrdersCount"].ToString();
				//����� �������
				SumLB.Text = Convert.ToDouble(data.Tables[0].Rows[0]["OrderSum"]).ToString("C");
				//�� ���������� �������
				OprLB.Text = data.Tables[0].Rows[0]["NonProcOrdersCount"].ToString();
				//����� ���������� ������
				LOT.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxOrderTime"]).ToLongTimeString();
				//������ �������
				OErrHL.Text = data.Tables[0].Rows[0]["OrdersErr"].ToString();
				//�������� �������
				OADHL.Text = data.Tables[0].Rows[0]["OrdersAD"].ToString();

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
				ReqHL.Text = data.Tables[0].Rows[0]["InProc"].ToString();


				DownloadDataSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DataSize"]));
				DownloadDocumentSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DocSize"]));

#if !DEBUG
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
#endif

				MySqlCommand command = new MySqlCommand();
				command.Connection = _connection;
				command.Transaction = transaction;

				command.Parameters.Add("?RegionMask", regionMask);
				command.Parameters.Add("?ToDate", toDate);
				command.Parameters.Add("?FromDate", fromDate);
				command.Parameters.Add("?UserName", Session["UserName"]);
				command.CommandText =
@"
SELECT  AlowChangePassword,
        AlowManage,
        (alowCreateRetail   = 1 OR AlowCreateVendor = 1) as AlowRegister,
        AlowClone,  
        (ShowRetail   = 1 OR ShowVendor = 1) as ShowInfo
FROM    accessright.regionaladmins  
WHERE	UserName = ?userName
";
				MySqlDataReader Reader = command.ExecuteReader();

				Reader.Read();
				RegisterHL.Visible = Convert.ToBoolean(Reader[2]);
				Session["Register"] = Reader[2];
				CloneHL.Visible = Convert.ToBoolean(Reader[3]);
				Session["Clone"] = Reader[3];
				Session["ChPass"] = Reader[0];
				ClInfHL.Visible = Convert.ToBoolean(Reader[4]);
				Session["ClInf"] = Reader[4];
				ViewAdministrators.Visible = Session["Administrator"] != null ? ((Administrator)Session["Administrator"]).AllowManageAdminAccounts : false;
				Reader.Close();

				try
				{
					if (Convert.ToInt32(ADHL.Text.Substring(0, 1)) > 0)
						ADHL.Enabled = true;
				}
				catch (Exception){}
				try
				{
					if (Convert.ToInt32(CUHL.Text.Substring(0, 1)) > 0)
						CUHL.Enabled = true;
				}
				catch (Exception){}
				try
				{
					if (Convert.ToInt32(ErrUpHL.Text.Substring(0, 1)) > 0)
						ErrUpHL.Enabled = true;
				}
				catch (Exception){}
				try
				{
					if (Convert.ToInt32(ReqHL.Text) > 0)
						ReqHL.Enabled = true;
				}
				catch (Exception){}
				try
				{
					if (Convert.ToInt32(ConfHL.Text.Substring(0, 1)) > 0)
						ConfHL.Enabled = true;
				}
				catch (Exception){}
				transaction.Commit();
			}
			finally
			{
				_connection.Close();
			}
		}
	}
}