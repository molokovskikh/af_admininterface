'mports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser

Partial Class statcont
    Inherits System.Web.UI.Page
    Dim ob As Int16
    Dim Picstr As String
    Dim HL As New HyperLink()
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

    Private Sub ShowStatS()

        Dim Pic As System.Web.UI.WebControls.Image
        Dim Комманда As New MySqlCommand()
        Dim Reader As MySqlDataReader
        Dim row As New TableRow()
        Dim cell As New TableCell()
        Dim i, MaxID, OMaxID As Int16
        Dim Order As String
        Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;")



        HL = New HyperLink()

        Pic = New System.Web.UI.WebControls.Image()
        Pic.ImageUrl = Picstr

        If ob = 0 Then
            Table5.Rows(0).Cells(0).BackColor = Drawing.ColorTranslator.FromHtml("#D8F1FF")
            Table5.Rows(0).Cells(0).Controls.Add(Pic)
            Order = Order & "writetime"
        End If
        If ob = 1 Then
            Table5.Rows(0).Cells(1).BackColor = Drawing.ColorTranslator.FromHtml("#D8F1FF")
            Table5.Rows(0).Cells(1).Controls.Add(Pic)
            Order = Order & "clientsinfo.UserName"
        End If
        If ob = 2 Then
            Table5.Rows(0).Cells(2).BackColor = Drawing.ColorTranslator.FromHtml("#D8F1FF")
            Table5.Rows(0).Cells(2).Controls.Add(Pic)
            Order = Order & "ShortName"
        End If
        If ob = 3 Then
            Table5.Rows(0).Cells(3).BackColor = Drawing.ColorTranslator.FromHtml("#D8F1FF")
            Table5.Rows(0).Cells(3).Controls.Add(Pic)
            Order = Order & "Region"
        End If
        If Picstr = "arrow-down-blue-reversed.gif" Then Order = Order & " desc"

        соединение.Open()
        With Комманда
            .CommandText = " SELECT WriteTime, clientsinfo.UserName, ShortName, Region, Message, FirmCode, osuseraccessright.rowid, clientsinfo.rowid" & _
            " FROM logs.clientsinfo, usersettings.clientsdata, accessright.showright, osuseraccessright, farm.regions" & _
            " where clientsinfo.clientcode=firmcode" & _
            " and osuseraccessright.clientcode=clientsinfo.clientcode" & _
            " and showright.RegionMask & clientsdata.RegionCode>0" & _
            " and writetime between ?FromDate and ?ToDate" & _
              " and regions.regioncode=clientsdata.RegionCode" & _
            " and showright.username='" & Session("UserName") & "' order by " & Order
            .Connection = соединение
            .Parameters.Add(New MySqlParameter("FromDate", MySqlDbType.Datetime))
            .Parameters("FromDate").Value = CalendarFrom.SelectedDate

            .Parameters.Add(New MySqlParameter("ToDate", MySqlDbType.Datetime))
            .Parameters("ToDate").Value = CalendarTo.SelectedDate.AddDays(1)

            '.Parameters.Add(New MySqlParameter("@Login", MySqlDbType.String))
            '.Parameters("@Login").Value = "%" & LoginTB.Text & "%"

            Reader = .ExecuteReader
        End With
        If Request.Cookies("Inforoom.Admins.ShowStatsC") Is Nothing Then
            Response.Cookies("Inforoom.Admins.ShowStatsC").Value = 0
            Response.Cookies("Inforoom.Admins.ShowStatsC").Expires = Now().AddYears(2)
        End If
        OMaxID = Request.Cookies("Inforoom.Admins.ShowStatsC").Value
        MaxID = 0
        While Reader.Read
            If Reader.Item(7).ToString > MaxID Then MaxID = Reader.Item(7).ToString
            row = New TableRow()
            If OMaxID < Reader.Item(7) Then row.BackColor = Color.White
            For i = 0 To Table5.Rows(0).Cells.Count - 1
                cell = New TableCell()
                If ob = i Then cell.BackColor = Drawing.Color.AliceBlue
                If i = 2 Then
                    HL = New HyperLink()
                    HL.Text = Reader.Item(i)
                    HL.NavigateUrl = "info.aspx?cc=" & Reader.Item(5).ToString & "&ouar=" & Reader.Item(6).ToString
                    cell.Controls.Add(HL)
                    HL.Dispose()
                ElseIf i = 0 Then
                    Dim PriceDate As Date = Reader.Item(i)
                    Dim Min, Month, Day As String
                    Min = PriceDate.Minute
                    If Len(Min) < 2 Then Min = 0 & Min
                    Month = PriceDate.Month
                    If Len(Month) < 2 Then Month = 0 & Month
                    Day = PriceDate.Day
                    If Len(Day) < 2 Then Day = 0 & Day
                    cell.Text = Day & "." & Month & "." & Right(PriceDate.Year, 2) & " " & PriceDate.Hour & ":" & Min
                    cell.Font.Size = WebControls.FontUnit.Point(8)
                Else
                    cell.Text = Reader.Item(i).ToString
                End If

                row.Cells.Add(cell)
            Next
            Table5.Rows.Add(row)
        End While
        Reader.Close()
        Session("MaxID") = MaxID
        соединение.Close()

    End Sub

    Private Sub CalendarTo_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CalendarTo.SelectionChanged
        Session("SelectedToDate") = CalendarTo.SelectedDate
        ShowStatS()
    End Sub

    Private Sub CalendarFrom_SelectionChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles CalendarFrom.SelectionChanged
        Session("SelectedFromDate") = CalendarFrom.SelectedDate
        ShowStatS()
    End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")

            If Not IsPostBack Then

                If Not Session("SelectedFromDate") Is Nothing Then
                    CalendarFrom.SelectedDate = Session("SelectedFromDate")
                Else
                    CalendarFrom.SelectedDate = Now().AddDays(-14)
                End If

                If Not Session("SelectedToDate") Is Nothing Then
                    CalendarTo.SelectedDate = Session("SelectedToDate")
                Else
                    CalendarTo.SelectedDate = Now()
                End If

                If Len(Request("ob")) < 1 Then
                    If Not Request.Cookies("Inforoom.Stat.OrderBy") Is Nothing Then
                        ob = CInt(Request.Cookies("Inforoom.Stat.OrderBy").Value)
                    Else
                        ob = 0
                    End If
                Else
                    If CInt(Session("ob")) = CInt(Request("ob")) Then
                        If Session("Picstr") = "arrow-down-blue.gif" Then Picstr = "arrow-down-blue-reversed.gif"
                    End If
                    Session("Picstr") = Picstr
                    ob = CInt(Request("ob"))
                    Response.Cookies("Inforoom.Stat.OrderBy").Value = ob
                    Response.Cookies("Inforoom.Stat.OrderBy").Expires = Now().AddYears(2)
                End If

                If Len(Session("Picstr")) < 1 Then
                    Picstr = "arrow-down-blue.gif"
                    Session("Picstr") = Picstr
                End If
                Session("ob") = ob
                ShowStatS()
            End If
            HL.NavigateUrl = "statcont.aspx?Ob=0"
            HL.Text = "Дата"
            'HL.ForeColor = Color.Black
            Table5.Rows(0).Cells(0).Controls.Add(HL)

            HL = New HyperLink
            HL.Text = "Оператор"
            'HL.ForeColor = Color.Black
            HL.NavigateUrl = "statcont.aspx?Ob=1"
            Table5.Rows(0).Cells(1).Controls.Add(HL)

            HL = New HyperLink
            HL.Text = "Клиент"
            'HL.ForeColor = Color.Black
            HL.NavigateUrl = "statcont.aspx?Ob=2"
            Table5.Rows(0).Cells(2).Controls.Add(HL)

            HL = New HyperLink
            HL.Text = "Регион"
            'HL.ForeColor = Color.Black
            HL.NavigateUrl = "statcont.aspx?Ob=3"
            Table5.Rows(0).Cells(3).Controls.Add(HL)

            Picstr = Session("Picstr")
            ob = Session("ob")
        End Sub
    End Class

End Namespace
