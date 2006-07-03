using System;
using System.Runtime.InteropServices;
using System.Web.UI;
using ActiveDs;
using MySql.Data.MySqlClient;

namespace AddUser
{
	partial class _default : Page
	{
		private MySqlConnection _connection = new MySqlConnection();
//		private MySqlDataReader Reader;
		private IADsUser ADUser;

		protected void Page_Load(object sender, EventArgs e)
		{
			_connection.ConnectionString = Literals.GetConnectionString();
			_connection.Open();
			try
			{
				ADUser = Marshal.BindToMoniker(@"WinNT://adc.analit.net/" + Convert.ToString(Session["UserName"])) as IADsUser;
				if ((ADUser.PasswordExpirationDate >= DateTime.Now) || (Convert.ToString(Session["UserName"]) == "michail"))
				{
					PassLB.Text = "Срок действия Вашего пароля истекает " + ADUser.PasswordExpirationDate.ToShortDateString() + " в " +
					              ADUser.PasswordExpirationDate.ToShortTimeString() + ". <br>Пожалуйста не забывайте изменять пароль.";
				}
				else
				{
					FTPHL.Visible = false;
					RegisterHL.Visible = false;
					CloneHL.Visible = false;
					ChPassHL.Visible = false;
					ClInfHL.Visible = false;
					ClManageHL.Visible = false;
					ShowStatHL.Visible = false;
					BillingHL.Visible = false;
					PassLB.Text = "Срок действия Вашего пароля истек " + ADUser.PasswordExpirationDate.ToShortDateString() + " в " +
					              ADUser.PasswordExpirationDate.ToShortTimeString() +
					              ". <br>Доступ к системе будет открыт после изменения пароля.";
					return;
				}
				Session["AccessGrant"] = 1;
				
				MySqlCommand command = new MySqlCommand();
				command.Connection = _connection;
				command.CommandText = " SELECT AlowChangePassword, AlowManage, (alowCreateRetail=1 or AlowCreateVendor=1) as AlowRegister, AlowClone," +
				                      " (ShowRet=1 or ShowOpt=1) as ShowInfo FROM (accessright.showright as a, accessright.regionaladmins as b)" +
				                      " where a.username=b.username and a.username=?userName";
				command.Parameters.Add("?userName", Session["UserName"]);
				MySqlDataReader Reader = command.ExecuteReader();
				
				Reader.Read();
				RegisterHL.Visible = Convert.ToBoolean(Reader[2]);
				Session["Register"] = Reader[2];
				CloneHL.Visible = Convert.ToBoolean(Reader[3]);
				Session["Clone"] = Reader[3];
				ChPassHL.Visible = Convert.ToBoolean(Reader[0]);
				Session["ChPass"] = Reader[0];
				ClInfHL.Visible = Convert.ToBoolean(Reader[4]);
				Session["ClInf"] = Reader[4];
				ClManageHL.Visible = Convert.ToBoolean(Reader[1]);
				Session["ClManage"] = Reader[1];
				Reader.Close();

				command.CommandText = " select sum(UncommittedUpdateTime>=CURDATE() and UpdateTime<>UncommittedUpdateTime) as request," +
									  " max(UpdateTime) as MaxUpdateTime from (usersettings.retclientsset, usersettings.clientsdata, accessright.showright)" +
									  " where clientsdata.firmcode=retclientsset.clientcode" + " and RegionCode & showright.regionmask>0" +
				                      " and username=?userName";
				Reader = command.ExecuteReader();
				
				Reader.Read();
				try
				{
					ReqHL.Text = Reader[0].ToString();
					MaxUpdateTime.Text = Convert.ToDateTime(Reader[1]).ToLongTimeString();
				}
				catch
				{
				}
				Reader.Close();
				command.CommandText = " SELECT concat(count(distinct oh.rowid), '(', count(distinct oh.clientcode), ')') as OrdersCount, sum(cost*quantity) as Summ, count(distinct if(processed=0, orderid, null)) as NProc, Max(WriteTime) as MaxTime" +
						" FROM (orders.ordershead oh, orders.orderslist, usersettings.clientsdata cd, accessright.showright, usersettings.retclientsset rcs)" +
						" where oh.rowid=orderid and cd.firmcode=oh.clientcode and rcs.clientcode=oh.clientcode" +
						" and firmsegment=0 and serviceclient=0 and showright.regionmask & maskregion>0" +
						" and showright.username=?userName and WriteTime>=CURDATE()";
				Reader = command.ExecuteReader();
				Reader.Read();
				try
				{
					OPLB.Text = Reader[0].ToString();
					OprLB.Text = Reader[2].ToString();
					SumLB.Text = Convert.ToDouble(Reader[1]).ToString("C");
					LOT.Text = Convert.ToDateTime(Reader[3]).ToLongTimeString();
				}
				catch
				{
				}
				Reader.Close();
				command.CommandText = " SELECT round(sum(UpdateType=3)/sum(UpdateType<3)*100, 2) ReGet, " +
						" concat(Sum(if(EXEVersion>0, UpdateType=5, null)), '(', count(distinct if(EXEVersion>0 and UpdateType=5, p.ClientCode, null)), ')') AD, " +
						" concat(Sum(if(EXEVersion>0, UpdateType=6, null)), '(', count(distinct if(EXEVersion>0 and UpdateType=6, p.clientcode, null)), ')') Err, " +
						" concat(Sum(if(EXEVersion=0, UpdateType=6, null)), '(', count(distinct if(EXEVersion=0 and UpdateType=6, p.clientcode, null)), ')') OErr, " +
						" concat(Sum(if(EXEVersion=0, UpdateType=5, null)), '(', count(distinct if(EXEVersion=0 and UpdateType=5, p.clientcode, null)), ')') OAD, " +
						" concat(sum(UpdateType=2), '(', count(distinct if(UpdateType=2, p.clientcode, null)), ')') CU, " +
						" concat(sum(UpdateType=1), '(', count(distinct if(UpdateType=1, p.clientcode, null)), ')') NU " +
						" FROM (logs.prgdataex p, usersettings.clientsdata, accessright.showright) " + " WHERE p.LogTime>curDate() " +
						" and firmcode=clientcode  and showright.regionmask & maskregion>0  and showright.username=?userName";
				Reader = command.ExecuteReader();
				Reader.Read();
				try
				{
					ReGetHL.Text = Reader["Reget"].ToString();
					ADHL.Text = Reader["AD"].ToString();
					ErrUpHL.Text = Reader["Err"].ToString();
					CUHL.Text = Reader["CU"].ToString();
					OADHL.Text = Reader["OAD"].ToString();
					OErrHL.Text = Reader["OErr"].ToString();
					ConfHL.Text = Reader["NU"].ToString();
				}
				catch (Exception err)
				{
					PassLB.Text = err.Message;
					PassLB.Visible = true;
				}
				try
				{
					if (Convert.ToInt32(ADHL.Text.Substring(0, 1)) > 0)
					{
						ADHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				try
				{
					if (Convert.ToInt32(CUHL.Text.Substring(0, 1)) > 0)
					{
						CUHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				try
				{
					if (Convert.ToInt32(ErrUpHL.Text.Substring(0, 1)) > 0)
					{
						ErrUpHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				try
				{
					if (Convert.ToInt32(ReqHL.Text) > 0)
					{
						ReqHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				try
				{
					if (Convert.ToInt32(ReGetHL.Text) > 0)
					{
						ReGetHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				try
				{
					if (Convert.ToInt32(ConfHL.Text.Substring(0, 1)) > 0)
					{
						ConfHL.Enabled = true;
					}
				}
				catch (Exception)
				{
				}
				Reader.Close();
				
				command.CommandText = " drop temporary table if exists tmp; create temporary table tmp (PriceCode int(32)); insert into tmp" +
						" SELECT distinct formlogs.pricecode FROM (accessright.showright, logs.formlogs)" +
						" left join usersettings.pricesdata using (PriceCode) left join usersettings.clientsdata cd using (FirmCode)" +
						" where resultid>3 and logtime>curdate()" +
						" and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1) and showright.username=?userName ;" +
						" select @ProblemPr:=count(distinct tmp.pricecode)-count(distinct if(resultid<4, tmp.pricecode, null)) ProblemPr" +
						" from (tmp, logs.formlogs) where tmp.pricecode=formlogs.pricecode and logtime>curdate();" +
						" select @ProblemPr ProblemPr";
				Reader = command.ExecuteReader();
				Reader.Read();
				FormErrLB.Text = Reader[0].ToString();
				Reader.Close();
				
				command.CommandText = " drop temporary table if exists tmp; create temporary table tmp (PriceCode int(32)); insert into tmp" +
						" SELECT distinct downlogs.pricecode FROM (accessright.showright, logs.downlogs)" +
						" left join usersettings.pricesdata using (PriceCode) left join usersettings.clientsdata cd using (FirmCode)" +
						" where downlogs.pricecode>0 and length(addition)>0 and logtime>curdate()" +
						" and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1) and showright.username= ?userName ;" +
						" select count(distinct tmp.pricecode)-count(distinct if(length(addition)=0, tmp.pricecode, null)) ProblemPr" +
						" from (tmp, logs.downlogs) where" + " tmp.pricecode=downlogs.pricecode" + " and logtime>curdate()";
				Reader = command.ExecuteReader();
				Reader.Read();
				DownErrLB.Text = Reader[0].ToString();
				Reader.Close();
				
				command.CommandText = " SELECT max(if(DateLastForm>curdate(), DateLastForm, '-')) Form, max(if(DateCurPrice>curdate(), DateCurPrice, '-')) Down, " +
						" sum(DatecurPrice>DateLastForm and DatecurPrice>curdate())-@ProblemPr Wait" +
						" FROM (usersettings.clientsdata cd, accessright.showright, farm.formrules fr, usersettings.pricesdata pd)" +
						" WHERE pd.pricecode=fr.firmcode and pd.firmcode=cd.firmcode" +
						" and showright.regionmask & maskregion>0 and showright.username= ?userName";
				Reader = command.ExecuteReader();
				Reader.Read();
				try
				{
					FormPLB.Text = Convert.ToDateTime(Reader["Form"]).ToLongTimeString();
				}
				catch
				{
				}
				try
				{
					DownPLB.Text = Convert.ToDateTime(Reader["Down"]).ToLongTimeString();
				}
				catch
				{
				}
				try
				{
					WaitPLB.Text = Reader["Wait"].ToString();
				}
				catch
				{
				}
				Reader.Close();
				
				command.CommandText = " select concat(count(if(LENGTH(addition)<1 and dl.pricecode>0, dl.pricecode, null)), '(', count( distinct if(LENGTH(addition)<1 and dl.pricecode>0, dl.pricecode, null)), ')') OKCount," +
						" count(if(LENGTH(addition)>0 and dl.pricecode=0, dl.pricecode, null)) ErrCount" +
						" from (accessright.showright, logs.downlogs dl) left join usersettings.pricesdata using (PriceCode)" +
						" left join usersettings.clientsdata cd using (FirmCode) where logtime>curdate()" +
						" and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1) and showright.username= ?userName";
				Reader = command.ExecuteReader();
				Reader.Read();
				PriceDOKLB.Text = Reader["OKCount"].ToString();
				PriceDERRLB.Text = Reader["ErrCount"].ToString();
				Reader.Close();

				command.CommandText = " select concat(count(if(dl.resultid<4, dl.pricecode, null)), '(', count( distinct if(dl.resultid<4, dl.pricecode, null)), ')') OKCount," +
						" count(if(LENGTH(addition)>0 or dl.pricecode=0, dl.pricecode, null)) ErrCount" +
						" from (accessright.showright, logs.formlogs dl) left join usersettings.pricesdata using (PriceCode)" +
						" left join usersettings.clientsdata cd using (FirmCode) where logtime>curdate()" +
						" and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1) and showright.username= ?userName";
				Reader = command.ExecuteReader();
				Reader.Read();
				PriceFOKLB.Text = Reader["OKCount"].ToString();
				Reader.Close();
			}
			catch (Exception err)
			{
				PassLB.Text = "Что-то не получилось... " + err.Message;
			}
			finally
			{
				_connection.Close();
			}
		}
	}
}