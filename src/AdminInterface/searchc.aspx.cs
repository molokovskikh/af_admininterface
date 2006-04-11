'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient
Imports ActiveDs


Namespace AddUser


Partial Class searchc
    Inherits System.Web.UI.Page
    Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;")
    Dim Комманда As New MySqlCommand()
    Dim Reader As MySqlDataReader
    Dim row As New TableRow()
    Dim cell As New TableCell()
    Dim i As Int16
    Dim Order As String
    Dim HL As New HyperLink
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

        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'End Sub



    'Private Sub GoFind_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles GoFind.Click
    '    Finder()
    ' End Sub



    Private Sub Finder()
        Dim ADUser As ActiveDs.IADsUser

        соединение.Open()
        With Комманда
            .Connection = соединение


            If FindRB.SelectedItem.Value = 0 Then
                'If FindTB.Text.Length < 3 Then Err.Raise(1, "Длина строки поиска", "Минимальная длина строки поиска при поиске по наименованию - 3 сивола.")
                .Parameters.Add(New MySqlParameter("Name", MySqlDbType.VarChar))
                .Parameters("Name").Value = "%" & FindTB.Text & "%"
            End If

            If FindRB.SelectedItem.Value = 1 Then
                .Parameters.Add(New MySqlParameter("ClientCode", MySqlDbType.Int32))
                .Parameters("ClientCode").Value = FindTB.Text
            End If


            If FindRB.SelectedItem.Value = 2 Then
                .Parameters.Add(New MySqlParameter("Login", MySqlDbType.VarChar))
                .Parameters("Login").Value = "%" & FindTB.Text & "%"
            End If

            If FindRB.SelectedItem.Value = 3 Then
                .Parameters.Add(New MySqlParameter("BillingCode", MySqlDbType.Int32))
                .Parameters("BillingCode").Value = FindTB.Text
            End If


                .CommandText = " SELECT cd. billingcode, cd.firmcode, ShortName,  region, concat(' ', max(datecurprice)) FirstUpdate, concat(' ', max(dateprevprice)) SecondUpdate,  null EXE, null MDB, " & _
                            " if(ouar2.rowid is null, ouar.OSUSERNAME,  ouar2.OSUSERNAME) as UserName," & _
                            " FirmSegment, FirmType, (Firmstatus=0 or Billingstatus=0) Firmstatus," & _
                            " if(ouar2.rowid is null, ouar.rowid,  ouar2.rowid) as ouarid, cd.firmcode as bfc" & _
                            " FROM (clientsdata as cd, farm.regions, accessright.showright, pricesdata, farm.formrules)" & _
                            " left join showregulation on ShowClientCode=cd.firmcode" & _
                            " left join osuseraccessright as ouar2 on ouar2.clientcode=cd.firmcode" & _
                            " left join osuseraccessright as ouar on ouar.clientcode=if(primaryclientcode is null, cd.firmcode, primaryclientcode)" & _
                            " where formrules.firmcode=pricesdata.pricecode" & _
                            " and pricesdata.firmcode=cd.firmcode" & _
                            " and regions.regioncode=cd.regioncode" & _
                            " and cd.regioncode  & showright.regionmask > 0" & _
                            " and showright.UserName='" & Session("UserName") & "'" & _
                            " and if(ShowOpt=1, FirmType=0, 0)" & _
                            " and if(UseRegistrant=1, Registrant=showright.UserName, 1)"
            If FindRB.SelectedItem.Value = 0 Then
                .CommandText &= " and (cd.shortname like ?Name or cd.fullname like ?Name)"
            End If

            If FindRB.SelectedItem.Value = 1 Then
                .CommandText &= " and cd.firmcode=?ClientCode"
            End If

            If FindRB.SelectedItem.Value = 2 Then
                .CommandText &= " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)"
            End If

            If FindRB.SelectedItem.Value = 3 Then
                .CommandText &= " and cd.billingcode=?BillingCode"
            End If
                .CommandText &= " group by cd.firmcode" & _
                            " union" & _
                            " SELECT cd. billingcode, if(includeregulation.PrimaryClientCode is null, cd.firmcode, concat(cd.firmcode, '[', includeregulation.PrimaryClientCode, ']')), " & _
                            " if(includeregulation.PrimaryClientCode is null, cd.ShortName, concat(cd.ShortName, '[', incd.shortname, ']'))," & _
                            " region, UpdateTime FirstUpdate,   UncommittedUpdateTime  SecondUpdate, " & _
                            " EXEVersion as EXE, MDBVersion MDB, " & _
                            " if(ouar2.rowid is null, ouar.OSUSERNAME,  ouar2.OSUSERNAME) as UserName," & _
                            " cd.FirmSegment, cd.FirmType, (cd.Firmstatus=0 or cd.Billingstatus=0) Firmstatus," & _
                            " if(ouar2.rowid is null, ouar.rowid,  ouar2.rowid) as ouarid, cd.firmcode as bfc" & _
                            " FROM (clientsdata as cd, farm.regions, accessright.showright, retclientsset as rts)" & _
                            " left join showregulation on ShowClientCode=cd.firmcode" & _
                            " left join includeregulation on includeclientcode=cd.firmcode" & _
                            " left join clientsdata incd on incd.firmcode=includeregulation.PrimaryClientCode" & _
                            " left join osuseraccessright as ouar2 on ouar2.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" & _
                            " left join osuseraccessright as ouar on ouar.clientcode=ifnull(showregulation.primaryclientcode, cd.firmcode)" & _
                            " left join  logs.prgdataex on prgdataex.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" & _
                            " and prgdataex.rowid=(select max(rowid) from logs.prgdataex where clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode) and updatetype in(1,2))" & _
                            " where" & _
                            " rts.clientcode=ifnull(includeregulation.PrimaryClientCode, cd.firmcode)" & _
                            " and regions.regioncode=cd.regioncode" & _
                            " and cd.regioncode  & showright.regionmask > 0" & _
                            " and showright.UserName='" & Session("UserName") & "'" & _
                            " and if(ShowRet=1, cd.FirmType=1, 0)" & _
                            " and if(UseRegistrant=1, cd.Registrant=showright.UserName, 1)"
            If FindRB.SelectedItem.Value = 0 Then
                .CommandText &= " and (cd.shortname like ?Name or cd.fullname like ?Name)"
            End If

            If FindRB.SelectedItem.Value = 1 Then
                .CommandText &= " and cd.firmcode=?ClientCode"
            End If

            If FindRB.SelectedItem.Value = 2 Then
                .CommandText &= " and (ouar.osusername like ?Login or ouar2.osusername like ?Login)"
            End If
            If FindRB.SelectedItem.Value = 3 Then
                .CommandText &= " and cd.billingcode=?BillingCode"
            End If
            .CommandText &= " group by cd.firmcode" & _
                        " order by 3, 4"


            Dim str As String
            str = .CommandText

        End With
        Dim tSpan As TimeSpan
        Dim StTime As DateTime = Now()
        Reader = Комманда.ExecuteReader


        While Reader.Read
            row = New TableRow
            If Reader.Item("FirmStatus").ToString = 1 Then row.BackColor = Color.FromArgb(255, 102, 0)
            For i = 0 To Table3.Rows(0).Cells.Count - 1

                Try
                    cell = New TableCell
                    If i = 8 And Len(Reader.Item(i).ToString) > 0 Then
                        If ADCB.Checked Then
                            Try
                                ADUser = GetObject("WinNT://adc.analit.net/" & Reader.Item(i).ToString)
                                If ADUser.IsAccountLocked Then cell.BackColor = Color.Violet
                                If ADUser.AccountDisabled Then cell.BackColor = Color.Aqua
                            Catch
                                cell.BackColor = Color.Red
                            End Try
                        End If
                        cell.Text = Reader.Item(i).ToString
                    ElseIf i = 2 Then
                        HL = New HyperLink
                        HL.Text = Reader.Item(i).ToString
                        HL.NavigateUrl = "info.aspx?cc=" & Reader.Item("bfc").ToString & "&ouar=" & Reader.Item("ouarid")
                        cell.Controls.Add(HL)
                    ElseIf i = 9 Then
                        If CInt(Reader.Item(i).ToString) = 0 Then
                            cell.Text = "Опт"
                        Else
                            cell.Text = "Справка"
                        End If
                    ElseIf i = 10 Then
                        If CInt(Reader.Item(i).ToString) = 1 Then
                            cell.Text = "Аптека"
                        Else
                            cell.Text = "Поставщик"
                        End If
                    ElseIf i = 4 Or i = 5 Then
                        Dim PriceDate As Date = Reader.Item(i)

                        If i = 4 Then
                            If Now().Subtract(PriceDate).TotalDays > 2 And Reader.Item("FirmStatus").ToString = 0 Then
                                cell.BackColor = Color.Gray
                            End If
                        End If

                        Dim Min, Month, Day As String
                        Min = PriceDate.Minute
                        If Len(Min) < 2 Then Min = 0 & Min
                        Month = PriceDate.Month
                        If Len(Month) < 2 Then Month = 0 & Month
                        Day = PriceDate.Day
                        If Len(Day) < 2 Then Day = 0 & Day
                        cell.Text = Day & "." & Month & "." & Right(PriceDate.Year, 2) & " " & PriceDate.Hour & ":" & Min
                        cell.HorizontalAlign = HorizontalAlign.Center
                    Else
                        cell.Text = Reader.Item(i).ToString
                    End If
                Catch err As Exception
                    cell = New TableCell
                    cell.Text = Reader.Item(i).ToString
                End Try
                row.Cells.Add(cell)
            Next
            Table3.Rows.Add(row)
            TimeSLB.Text = Now().Subtract(StTime).ToString
        End While
        If Table3.Rows.Count > 1 Then
            Table3.Visible = True
            Table4.Visible = True
            Label1.Visible = False
        Else
            Label1.Visible = True
            Table3.Visible = False
            Table4.Visible = False
        End If
        Reader.Close()
        соединение.Close()
    End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            Dim ShowStatChecked As Boolean = False

            соединение.Open()
            Комманда.CommandText = "select max(UserName='" & Session("UserName") & "') from accessright.showright"
            Комманда.Connection = соединение
            If Комманда.ExecuteScalar = 0 Then
                Session("strError") = "Пользователь " & Session("UserName") & " не найден!"
                соединение.Close()
                'System.Web.Security.FormsAuthentication.SignOut()
                Response.Redirect("error.aspx")
            End If
            соединение.Close()

            If IsPostBack Then Finder()
        End Sub
    End Class

End Namespace
