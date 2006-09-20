using System;
using System.Data;
using System.Runtime.InteropServices;
using System.Web.UI;
using ActiveDs;
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
			try
			{
                _connection.Open();
                MySqlTransaction transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
                IADsUser ADUser = Marshal.BindToMoniker(@"WinNT://adc.analit.net/" + Convert.ToString(Session["UserName"])) as IADsUser;
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
                command.Transaction = transaction;

                command.CommandText =
@"
SELECT  AlowChangePassword ,
        AlowManage ,
        (alowCreateRetail   = 1 
        OR AlowCreateVendor = 1) as AlowRegister ,
        AlowClone ,  
        (ShowRet   = 1 
        OR ShowOpt = 1) as ShowInfo 
FROM    (accessright.showright as a ,accessright.regionaladmins as b)  
WHERE   a.username     = b.username 
        AND a.username = ?userName;
";
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
				ViewAdministrators.Visible = Session["Administrator"] != null ? ((Administrator)Session["Administrator"]).AllowManageAdminAccounts : false;
				Reader.Close();

                command.CommandText =
@"
SELECT  sum(UncommittedUpdateTime >= CURDATE() 
        AND UpdateTime           <> UncommittedUpdateTime) as request ,  
        max(UpdateTime)                                    as MaxUpdateTime 
FROM    (usersettings.retclientsset ,usersettings.clientsdata ,accessright.showright)  
WHERE   clientsdata.firmcode                  = retclientsset.clientcode  
        AND RegionCode & showright.regionmask > 0  
        AND username                          = ?userName;
";
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
                command.CommandText =
@"
SELECT  concat(count(DISTINCT oh.rowid) ,'(' ,count(DISTINCT oh.clientcode) ,')') as OrdersCount ,
        sum(cost*quantity)                                                       as Summ ,
        count(DISTINCT if(processed = 0 ,orderid ,null))                         as NProc ,
        Max(WriteTime)                                                           as MaxTime  
FROM    (orders.ordershead oh ,orders.orderslist ,usersettings.clientsdata cd ,accessright.showright ,usersettings.retclientsset rcs)  
WHERE   oh.rowid                              = orderid 
        AND cd.firmcode                       = oh.clientcode 
        AND rcs.clientcode                    = oh.clientcode  
        AND firmsegment                       = 0 
        AND serviceclient                     = 0 
        AND showright.regionmask & maskregion > 0  
        AND showright.username                = ?userName 
        AND WriteTime                        >= CURDATE();
";
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
                command.CommandText =
@"
SELECT  round(sum(UpdateType      = 3)/sum(UpdateType < 3)*100 ,2) ReGet ,  
        concat(Sum(if(EXEVersion > 0 ,UpdateType = 5 ,null)) ,'(' ,count(DISTINCT if(EXEVersion > 0 
        AND UpdateType           = 5 ,p.ClientCode ,null)) ,')') AD ,  
        concat(Sum(if(EXEVersion > 0 ,UpdateType = 6 ,null)) ,'(' ,count(DISTINCT if(EXEVersion > 0 
        AND UpdateType           = 6 ,p.clientcode ,null)) ,')') Err ,  
        concat(Sum(if(EXEVersion = 0 ,UpdateType = 6 ,null)) ,'(' ,count(DISTINCT if(EXEVersion = 0 
        AND UpdateType           = 6 ,p.clientcode ,null)) ,')') OErr ,  
        concat(Sum(if(EXEVersion = 0 ,UpdateType = 5 ,null)) ,'(' ,count(DISTINCT if(EXEVersion = 0 
        AND UpdateType           = 5 ,p.clientcode ,null)) ,')') OAD ,  
        concat(sum(UpdateType    = 2) ,'(' ,count(DISTINCT if(UpdateType = 2 ,p.clientcode ,null)) ,')') CU ,  
        concat(sum(UpdateType    = 1) ,'(' ,count(DISTINCT if(UpdateType = 1 ,p.clientcode ,null)) ,')') NU 
FROM    (logs.prgdataex p ,usersettings.clientsdata ,accessright.showright) 
WHERE   p.LogTime                             > curDate() 
        AND firmcode                          = clientcode 
        AND showright.regionmask & maskregion > 0 
        AND showright.username                = ?userName;
";
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
				
				command.CommandText =
@"
DROP temporary table if exists tmp; 
create temporary table tmp (PriceCode int(32)); 
        INSERT 
        INTO    tmp  
        SELECT  DISTINCT formlogs.pricecode 
        FROM    (accessright.showright ,logs.formlogs)  
        LEFT JOIN usersettings.pricesdata using (PriceCode) 
        LEFT JOIN usersettings.clientsdata cd using (FirmCode)  
        WHERE   resultid                                                          > 3 
                AND logtime                                                       > curdate()  
                AND if(cd.firmcode is not null ,showright.regionmask & maskregion > 0 ,1) 
                AND showright.username                                            = ?userName ;  
        SELECT  @ProblemPr                                                       := count(DISTINCT tmp.pricecode)-count(DISTINCT if(resultid < 4 ,tmp.pricecode ,null)) ProblemPr  
        FROM    (tmp ,logs.formlogs) 
        WHERE   tmp.pricecode = formlogs.pricecode 
                AND logtime   > curdate();  
        SELECT @ProblemPr ProblemPr;
";
				Reader = command.ExecuteReader();
				Reader.Read();
				FormErrLB.Text = Reader[0].ToString();
				Reader.Close();
				
				command.CommandText =
@"
DROP temporary table if exists tmp; 
create temporary table tmp (PriceCode int(32)); 
        INSERT 
        INTO    tmp  
        SELECT  DISTINCT downlogs.pricecode 
        FROM    (accessright.showright ,logs.downlogs)  
        LEFT JOIN usersettings.pricesdata using (PriceCode) 
        LEFT JOIN usersettings.clientsdata cd using (FirmCode)  
        WHERE   downlogs.pricecode                                                > 0 
                AND length(addition)                                              > 0 
                AND logtime                                                       > curdate()  
                AND if(cd.firmcode is not null ,showright.regionmask & maskregion > 0 ,1) 
                AND showright.username                                            = ?userName ;  
        SELECT  count(DISTINCT tmp.pricecode)-count(DISTINCT if(length(addition)  = 0 ,tmp.pricecode ,null)) ProblemPr  
        FROM    (tmp ,logs.downlogs) 
        WHERE    tmp.pricecode = downlogs.pricecode  
                AND logtime    > curdate();
";
				Reader = command.ExecuteReader();
				Reader.Read();
				DownErrLB.Text = Reader[0].ToString();
				Reader.Close();

                command.CommandText =
@"
SELECT  max(if(DateLastForm  > curdate() ,DateLastForm ,'-')) Form ,
        max(if(DateCurPrice > curdate() ,DateCurPrice ,'-')) Down ,  
        sum(DatecurPrice    > DateLastForm 
        AND DatecurPrice    > curdate())-@ProblemPr Wait  
FROM    (usersettings.clientsdata cd ,accessright.showright ,farm.formrules fr ,usersettings.pricesdata pd)  
WHERE   pd.pricecode                          = fr.firmcode 
        AND pd.firmcode                       = cd.firmcode  
        AND showright.regionmask & maskregion > 0 
        AND showright.username                = ?userName;
";
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

                command.CommandText =
@"
SELECT  concat(count(if(LENGTH(addition) < 1 
        AND dl.pricecode                > 0 ,dl.pricecode ,null)) ,'(' ,count( DISTINCT if(LENGTH(addition) < 1 
        AND dl.pricecode                > 0 ,dl.pricecode ,null)) ,')') OKCount ,  
        count(if(LENGTH(addition)       > 0 
        AND dl.pricecode                = 0 ,dl.pricecode ,null)) ErrCount  
FROM    (accessright.showright ,logs.downlogs dl) 
LEFT JOIN usersettings.pricesdata using (PriceCode)  
LEFT JOIN usersettings.clientsdata cd using (FirmCode) 
WHERE   logtime                                                           > curdate()  
        AND if(cd.firmcode is not null ,showright.regionmask & maskregion > 0 ,1) 
        AND showright.username                                            = ?userName;
";
				Reader = command.ExecuteReader();
				Reader.Read();
				PriceDOKLB.Text = Reader["OKCount"].ToString();
				PriceDERRLB.Text = Reader["ErrCount"].ToString();
				Reader.Close();

                command.CommandText =
@"
SELECT  concat(count(if(dl.resultid < 4 ,dl.pricecode ,null)) ,'(' ,count( DISTINCT if(dl.resultid < 4 ,dl.pricecode ,null)) ,')') OKCount ,  
        count(if(LENGTH(addition)  > 0 
        OR dl.pricecode            = 0 ,dl.pricecode ,null)) ErrCount  
FROM    (accessright.showright ,logs.formlogs dl) 
LEFT JOIN usersettings.pricesdata using (PriceCode)  
LEFT JOIN usersettings.clientsdata cd using (FirmCode) 
WHERE   logtime                                                           > curdate()  
        AND if(cd.firmcode is not null ,showright.regionmask & maskregion > 0 ,1) 
        AND showright.username                                            = ?userName;
";
				Reader = command.ExecuteReader();
				Reader.Read();
				PriceFOKLB.Text = Reader["OKCount"].ToString();
				Reader.Close();
                transaction.Commit();
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