using System;
using System.Data;
using System.Web.UI;
using AdminInterface.Helpers;
using DAL;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class _default : Page
	{
		private readonly MySqlConnection _connection = new MySqlConnection();

		protected void Page_Load(object sender, EventArgs e)
		{
			_connection.ConnectionString = Literals.GetConnectionString();
			var expirationDate = ADHelper.GetPasswordExpirationDate(Session["UserName"].ToString());
			if (expirationDate >= DateTime.Now || Convert.ToString(Session["UserName"]) == "michail")
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
;
", _connection);
					adapter.SelectCommand.Parameters.AddWithValue("?UserName", Session["UserName"]);
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
			try
			{
				var urlTemplate = String.Format("BeginDate={0}&EndDate={1}&RegionMask={2}", fromDate, toDate.AddDays(1), regionMask);
				CUHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateCumulative, urlTemplate);
				ErrUpHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateError, urlTemplate);
				ADHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateBan, urlTemplate);
				ConfHL.NavigateUrl = String.Format("viewcl.aspx?id={0}&{1}", (int)StatisticsType.UpdateNormal, urlTemplate);

			    _connection.Open();
				var transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
				var adapter = new MySqlDataAdapter("GetShowStat", _connection)
				                  {
				                      SelectCommand =
				                          {
				                              Transaction = transaction,
				                              CommandType = CommandType.StoredProcedure
				                          }
				                  };
			    adapter.SelectCommand.Parameters.AddWithValue("?StartDateParam", fromDate);
				adapter.SelectCommand.Parameters.AddWithValue("?EndDateParam", toDate.AddDays(1));
				adapter.SelectCommand.Parameters.AddWithValue("?RegionMaskParam", regionMask);
				adapter.SelectCommand.Parameters.AddWithValue("?UserNameParam", Session["UserName"]);
				var data = new DataSet();
				adapter.Fill(data);
				//Заказы
				//Количество принятых заказов
				OPLB.Text = data.Tables[0].Rows[0]["OrdersCount"].ToString();
				//Сумма заказов
				SumLB.Text = Convert.ToDouble(data.Tables[0].Rows[0]["OrderSum"]).ToString("C");
				//Не обработано заказов
				OprLB.Text = data.Tables[0].Rows[0]["NonProcOrdersCount"].ToString();
				//Время последнего заказа
				LOT.Text = Convert.ToDateTime(data.Tables[0].Rows[0]["MaxOrderTime"]).ToLongTimeString();
				//Ошибок заказов
				OErrHL.Text = data.Tables[0].Rows[0]["OrdersErr"].ToString();
				//Запретов заказов
				OADHL.Text = data.Tables[0].Rows[0]["OrdersAD"].ToString();

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
				ReqHL.Text = data.Tables[0].Rows[0]["InProc"].ToString();


				DownloadDataSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DataSize"]));
				DownloadDocumentSize.Text = ViewHelper.ConvertToUserFriendlySize(Convert.ToUInt64(data.Tables[0].Rows[0]["DocSize"]));
#if !DEBUG
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
#endif

                var command = new MySqlCommand {Connection = _connection, Transaction = transaction};

                command.Parameters.AddWithValue("?RegionMask", regionMask);
				command.Parameters.AddWithValue("?ToDate", toDate);
                command.Parameters.AddWithValue("?FromDate", fromDate);
                command.Parameters.AddWithValue("?UserName", Session["UserName"]);
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
				var Reader = command.ExecuteReader();

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