'mports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser


Partial Class orders
    Inherits System.Web.UI.Page
    Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;")
    Dim Комманда As New MySqlCommand()
    Dim Reader As MySqlDataReader
    Dim ClientCode As Int64
    Dim row As New TableRow()
    Dim cell As New TableCell()
    Dim i As Int16

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

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        соединение.Open()
        ClientCode = Request("cc")

        With Комманда
            .Connection = соединение
            'ToDo Этот запрос устарел - нго необходимо переписать
            .CommandText = " SELECT ordershead.rowid, WriteTime, PriceDate, client.shortname, firm.shortname, PriceName, RowCount, Processed" & _
            " FROM orders.ordershead, usersettings.pricesdata, usersettings.clientsdata as firm, usersettings.clientsdata as client, usersettings.clientsdata as sel where " & _
            " clientcode=client.firmcode" & _
            " and client.firmcode=if(sel.firmtype=1, sel.firmcode, client.firmcode)" & _
            " and firm.firmcode=if(sel.firmtype=0, sel.firmcode, firm.firmcode)" & _
            " and writetime between ?FromDate and ?ToDate" & _
            " and pricesdata.pricecode=ordershead.pricecode" & _
            " and firm.firmcode=pricesdata.firmcode" & _
            " and sel.firmcode=" & ClientCode & " order by writetime desc"

            .Parameters.Add(New MySqlParameter("FromDate", MySqlDbType.Datetime))
            .Parameters("FromDate").Value = CalendarFrom.SelectedDate

            .Parameters.Add(New MySqlParameter("ToDate", MySqlDbType.Datetime))
            .Parameters("ToDate").Value = CalendarTo.SelectedDate
        End With
        Reader = Комманда.ExecuteReader
        While Reader.Read
            row = New TableRow
            If CInt(Reader(6).ToString) = 0 Then row.BackColor = Color.Red
            For i = 0 To Table3.Rows(0).Cells.Count - 1
                cell = New TableCell
                If i = 1 Or i = 2 Then
                    cell.Text = FormatDateTime(Reader.Item(i))
                    cell.HorizontalAlign = HorizontalAlign.Center
                Else
                    cell.Text = Reader.Item(i).ToString
                End If
                row.Cells.Add(cell)
            Next
            Table3.Rows.Add(row)
        End While
        Reader.Close()
        соединение.Close()
    End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            If Not Page.IsPostBack Then
                CalendarFrom.SelectedDate = Now().AddDays(-7)
                CalendarTo.SelectedDate = Now().AddDays(1)
            End If
        End Sub
    End Class

End Namespace
