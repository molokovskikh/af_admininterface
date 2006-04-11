'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient
Imports ActiveDs


Namespace AddUser


Partial Class _default
    Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region
    Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=accessright;User ID=system;Password=123;Connect Timeout=300;Pooling=no")
    Dim Reader As MySqlDataReader
    Dim func As New Func()
    Dim ShowStat As Boolean
    Dim ADUser As ActiveDs.IADsUser

        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


        'End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
#If 1 = 1 Then
            соединение.Open()
            Try
                ADUser = GetObject("WinNT://adc.analit.net/" & Session("UserName"))
                If ADUser.PasswordExpirationDate >= Now() Or Session("UserName") = "michail" Then
                    PassLB.Text = "—рок действи€ ¬ашего парол€ истекает " & FormatDateTime(ADUser.PasswordExpirationDate, DateFormat.ShortDate) & " в " & FormatDateTime(ADUser.PasswordExpirationDate, DateFormat.ShortTime) & _
                    ". <br>ѕожалуйста не забывайте измен€ть пароль."

                Else

                    FTPHL.Visible = False
                    RegisterHL.Visible = False
                    CloneHL.Visible = False
                    ChPassHL.Visible = False
                    ClInfHL.Visible = False
                    ClManageHL.Visible = False
                    ShowStatHL.Visible = False
                    BillingHL.Visible = False
                    PassLB.Text = "—рок действи€ ¬ашего парол€ истек " & FormatDateTime(ADUser.PasswordExpirationDate, DateFormat.ShortDate) & " в " & FormatDateTime(ADUser.PasswordExpirationDate, DateFormat.ShortTime) & _
         ". <br>ƒоступ к системе будет открыт после изменени€ парол€."
                    Exit Sub
                End If

                Session("AccessGrant") = 1

                Reader = New MySqlCommand(" SELECT AlowChangePassword, AlowManage, (alowCreateRetail=1 or AlowCreateVendor=1) as AlowRegister, AlowClone," & _
         " (ShowRet=1 or ShowOpt=1) as ShowInfo" & _
         " FROM (showright as a, regionaladmins as b)" & _
         " where  a.username=b.username and a.username='" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                RegisterHL.Visible = Reader.Item(2).ToString
                Session("Register") = CBool(Reader.Item(2).ToString)

                CloneHL.Visible = Reader.Item(3).ToString
                Session("Clone") = CBool(Reader.Item(3).ToString)

                ChPassHL.Visible = Reader.Item(0).ToString
                Session("ChPass") = CBool(Reader.Item(0).ToString)

                ClInfHL.Visible = Reader.Item(4).ToString
                Session("ClInf") = CBool(Reader.Item(4).ToString)

                ClManageHL.Visible = Reader.Item(1).ToString
                Session("ClManage") = CBool(Reader.Item(1).ToString)

                Reader.Close()
                Reader = New MySqlCommand(" select sum(UncommittedUpdateTime>=CURDATE() and UpdateTime<>UncommittedUpdateTime) as request," & _
        " max(UpdateTime) as MaxUpdateTime" & _
        " from (usersettings.retclientsset, usersettings.clientsdata, accessright.showright)" & _
        " where clientsdata.firmcode=retclientsset.clientcode" & _
        " and RegionCode & showright.regionmask>0" & _
        " and username='" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                Try
                    ReqHL.Text = Reader.Item(0).ToString
                    'ConfHL.Text = Reader.Item(1).ToString
                    MaxUpdateTime.Text = FormatDateTime(Reader.Item(1), DateFormat.LongTime)
                Catch
                End Try
                Reader.Close()
                Reader = New MySqlCommand(" SELECT concat(count(distinct oh.rowid), '(', count(distinct oh.clientcode), ')') as OrdersCount, sum(cost*quantity) as Summ, count(distinct if(processed=0, orderid, null)) as NProc, Max(WriteTime) as MaxTime" & _
        " FROM (orders.ordershead oh, orders.orderslist, usersettings.clientsdata cd, accessright.showright, usersettings.retclientsset rcs)" & _
        " where oh.rowid=orderid" & _
        " and cd.firmcode=oh.clientcode" & _
        " and rcs.clientcode=oh.clientcode" & _
        " and firmsegment=0" & _
        " and serviceclient=0" & _
        " and showright.regionmask & maskregion>0" & _
        " and showright.username='" & Session("UserName") & "'" & _
        " and WriteTime>=CURDATE()", соединение).ExecuteReader
                Reader.Read()
                Try
                    OPLB.Text = Reader.Item(0).ToString
                    OprLB.Text = Reader.Item(2).ToString
                    SumLB.Text = FormatCurrency(Reader.Item(1))
                    LOT.Text = FormatDateTime(Reader.Item(3), DateFormat.LongTime)
                Catch
                End Try

                Reader.Close()
                Reader = New MySqlCommand(" SELECT round(sum(UpdateType=3)/sum(UpdateType<3)*100, 2) ReGet, " & _
        " concat(Sum(if(EXEVersion>0, UpdateType=5, null)), '(', count(distinct if(EXEVersion>0 and UpdateType=5,  p.ClientCode, null)), ')')  AD, " & _
        " concat(Sum(if(EXEVersion>0, UpdateType=6, null)), '(', count(distinct if(EXEVersion>0 and UpdateType=6, p.clientcode, null)), ')') Err,  " & _
        " concat(Sum(if(EXEVersion=0, UpdateType=6, null)), '(', count(distinct if(EXEVersion=0 and UpdateType=6, p.clientcode, null)), ')') OErr,  " & _
        " concat(Sum(if(EXEVersion=0, UpdateType=5, null)), '(', count(distinct if(EXEVersion=0 and  UpdateType=5, p.clientcode, null)), ')')   OAD,  " & _
        " concat(sum(UpdateType=2), '(', count(distinct if(UpdateType=2, p.clientcode, null)), ')') CU, " & _
        " concat(sum(UpdateType=1), '(', count(distinct if(UpdateType=1, p.clientcode, null)), ')') NU " & _
        " FROM (logs.prgdataex p,  usersettings.clientsdata, accessright.showright) " & _
        " WHERE p.LogTime>curDate() " & _
        " and firmcode=clientcode " & _
        " and showright.regionmask & maskregion>0 " & _
        " and showright.username= '" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                Try
                    ReGetHL.Text = Reader.Item("Reget").ToString
                    ADHL.Text = Reader.Item("AD").ToString
                    ErrUpHL.Text = Reader.Item("Err").ToString
                    CUHL.Text = Reader.Item("CU").ToString
                    OADHL.Text = Reader.Item("OAD").ToString
                    OErrHL.Text = Reader.Item("OErr").ToString
                    ConfHL.Text = Reader.Item("NU").ToString
                    'SumLB.Text = FormatCurrency(Reader.Item(1))
                    'LOT.Text = FormatDateTime(Reader.Item(3), DateFormat.LongTime)
                Catch err As Exception
                    PassLB.Text = err.Message
                    PassLB.Visible = True
                End Try


                Try
                    If CInt(Left(ADHL.Text, 1)) > 0 Then ADHL.Enabled = True
                Catch ex As Exception
                End Try

                Try
                    If CInt(Left(CUHL.Text, 1)) > 0 Then CUHL.Enabled = True
                Catch ex As Exception
                End Try


                Try
                    If CInt(Left(ErrUpHL.Text, 1)) > 0 Then ErrUpHL.Enabled = True
                Catch ex As Exception
                End Try


                Try
                    If CInt(ReqHL.Text) > 0 Then ReqHL.Enabled = True
                Catch ex As Exception
                End Try


                Try
                    If CInt(ReGetHL.Text) > 0 Then ReGetHL.Enabled = True
                Catch ex As Exception
                End Try


                Try
                    If CInt(Left(ConfHL.Text, 1)) > 0 Then ConfHL.Enabled = True
                Catch ex As Exception
                End Try



                Reader.Close()
                Reader = New MySqlCommand(" drop  temporary  table if exists tmp;" & _
        " create temporary table tmp (PriceCode int(32));" & _
        " insert into tmp" & _
        " SELECT distinct formlogs.pricecode" & _
        " FROM (accessright.showright, logs.formlogs)" & _
        "  left join usersettings.pricesdata using (PriceCode)" & _
        " left join usersettings.clientsdata cd using (FirmCode)" & _
        " where resultid>3" & _
        " and logtime>curdate()" & _
        "  and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1)" & _
        " and showright.username= '" & Session("UserName") & "';" & _
        " select @ProblemPr:=count(distinct tmp.pricecode)-count(distinct if(resultid<4, tmp.pricecode, null)) ProblemPr" & _
        " from (tmp, logs.formlogs) where" & _
        " tmp.pricecode=formlogs.pricecode" & _
        " and  logtime>curdate();" & _
        " select @ProblemPr ProblemPr", соединение).ExecuteReader
                Reader.Read()
                FormErrLB.Text = Reader.Item(0)
                Reader.Close()


                Reader = New MySqlCommand(" drop  temporary  table if exists tmp;" & _
        " create temporary table tmp (PriceCode int(32));" & _
        " insert into tmp" & _
        " SELECT distinct downlogs.pricecode" & _
        " FROM (accessright.showright, logs.downlogs)" & _
        "  left join usersettings.pricesdata using (PriceCode)" & _
        " left join usersettings.clientsdata cd using (FirmCode)" & _
        " where downlogs.pricecode>0 and length(addition)>0" & _
        " and logtime>curdate()" & _
        "  and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1)" & _
        " and showright.username= '" & Session("UserName") & "';" & _
        " select count(distinct tmp.pricecode)-count(distinct if(length(addition)=0, tmp.pricecode, null)) ProblemPr" & _
        " from (tmp, logs.downlogs) where" & _
        " tmp.pricecode=downlogs.pricecode" & _
        " and  logtime>curdate()", соединение).ExecuteReader
                Reader.Read()
                DownErrLB.Text = Reader.Item(0)
                Reader.Close()



                Reader = New MySqlCommand(" SELECT max(if(DateLastForm>curdate(), DateLastForm, '-')) Form, max(if(DateCurPrice>curdate(), DateCurPrice, '-')) Down, " & _
        " sum(DatecurPrice>DateLastForm and DatecurPrice>curdate())-@ProblemPr Wait" & _
        " FROM (usersettings.clientsdata cd, accessright.showright, farm.formrules fr, usersettings.pricesdata pd)" & _
        " WHERE" & _
        " pd.pricecode=fr.firmcode" & _
        " and pd.firmcode=cd.firmcode" & _
        " and showright.regionmask & maskregion>0" & _
        " and showright.username= '" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                Try
                    FormPLB.Text = FormatDateTime(Reader.Item("Form"), DateFormat.LongTime)
                Catch
                End Try
                Try
                    DownPLB.Text = FormatDateTime(Reader.Item("Down"), DateFormat.LongTime)
                Catch
                End Try
                Try
                    WaitPLB.Text = Reader.Item("Wait").ToString
                Catch
                End Try
                Reader.Close()

                Reader = New MySqlCommand(" select concat(count(if(LENGTH(addition)<1 and dl.pricecode>0, dl.pricecode, null)), '(', count( distinct if(LENGTH(addition)<1 and dl.pricecode>0, dl.pricecode, null)), ')') OKCount," & _
                " count(if(LENGTH(addition)>0 and dl.pricecode=0, dl.pricecode, null)) ErrCount" & _
                " from (accessright.showright, logs.downlogs dl)" & _
                " left join usersettings.pricesdata using (PriceCode)" & _
                " left join usersettings.clientsdata cd using (FirmCode)" & _
                " where logtime>curdate()" & _
                " and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1)" & _
                " and showright.username= '" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                PriceDOKLB.Text = Reader.Item("OKCount").ToString
                PriceDERRLB.Text = Reader.Item("ErrCount").ToString
                'DownErrLB.Text = Reader.Item("PrCount").ToString
                Reader.Close()



                Reader = New MySqlCommand(" select concat(count(if(dl.resultid<4, dl.pricecode, null)), '(', count( distinct if(dl.resultid<4, dl.pricecode, null)), ')') OKCount," & _
        " count(if(LENGTH(addition)>0 or dl.pricecode=0, dl.pricecode, null)) ErrCount" & _
        " from (accessright.showright, logs.formlogs dl)" & _
        " left join usersettings.pricesdata using (PriceCode)" & _
        " left join usersettings.clientsdata cd using (FirmCode)" & _
        " where logtime>curdate()" & _
        " and if(cd.firmcode is not null, showright.regionmask & maskregion>0, 1)" & _
        " and showright.username= '" & Session("UserName") & "'", соединение).ExecuteReader
                Reader.Read()
                PriceFOKLB.Text = Reader.Item("OKCount").ToString
                'PriceDERRLB.Text = Reader.Item("ErrCount").ToString
                Reader.Close()
            Catch err As Exception
                PassLB.Text = "„то-то не получилось... " & err.Message
            Finally
                соединение.Close()
            End Try
#Else
            Session("AccessGrant") = 1

#End If
        End Sub
    End Class

End Namespace
