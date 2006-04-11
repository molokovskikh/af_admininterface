Imports MySql.Data.MySqlClient


Namespace AddUser

Partial Class viewcl
    Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS = New System.Data.DataSet
        Me.DataTable1 = New System.Data.DataTable
        Me.DataColumn1 = New System.Data.DataColumn
        Me.DataColumn2 = New System.Data.DataColumn
        Me.DataColumn3 = New System.Data.DataColumn
        Me.DataColumn4 = New System.Data.DataColumn
        Me.DataColumn5 = New System.Data.DataColumn
        Me.MyCmd = New MySql.Data.MySqlClient.MySqlCommand
        Me.MyCn = New MySql.Data.MySqlClient.MySqlConnection
        Me.MyDA = New MySql.Data.MySqlClient.MySqlDataAdapter
        CType(Me.DS, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS
        '
        Me.DS.DataSetName = "DS"
        Me.DS.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS.Tables.AddRange(New System.Data.DataTable() {Me.DataTable1})
        '
        'DataTable1
        '
        Me.DataTable1.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn1, Me.DataColumn2, Me.DataColumn3, Me.DataColumn4, Me.DataColumn5})
        Me.DataTable1.TableName = "Table"
        '
        'DataColumn1
        '
        Me.DataColumn1.ColumnName = "LogTime"
        Me.DataColumn1.DataType = GetType(System.DateTime)
        '
        'DataColumn2
        '
        Me.DataColumn2.ColumnName = "FirmCode"
        '
        'DataColumn3
        '
        Me.DataColumn3.ColumnName = "ShortName"
        '
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "Addition"
        '
        'DataColumn5
        '
        Me.DataColumn5.ColumnName = "Region"
        '
        'MyCmd
        '
        Me.MyCmd.CommandText = Nothing
        Me.MyCmd.CommandTimeout = 0
        Me.MyCmd.CommandType = System.Data.CommandType.Text
        Me.MyCmd.Connection = Me.MyCn
        Me.MyCmd.Transaction = Nothing
        Me.MyCmd.UpdatedRowSource = System.Data.UpdateRowSource.Both
        '
        'MyDA
        '
        Me.MyDA.DeleteCommand = Nothing
        Me.MyDA.InsertCommand = Nothing
        Me.MyDA.SelectCommand = Me.MyCmd
        Me.MyDA.UpdateCommand = Nothing
        CType(Me.DS, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataTable1, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Protected WithEvents DS As System.Data.DataSet
    Protected WithEvents DataTable1 As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents MyCmd As MySql.Data.MySqlClient.MySqlCommand
    Protected WithEvents MyDA As MySql.Data.MySqlClient.MySqlDataAdapter
    Protected WithEvents MyCn As MySql.Data.MySqlClient.MySqlConnection
    Protected WithEvents DataColumn5 As System.Data.DataColumn


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'End Sub

    Public Function FormatDate(ByVal InputDate As Date, ByVal Enable As Boolean) As String
        FormatDate = FormatDateTime(InputDate, DateFormat.LongTime)
    End Function

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            'id=0 - Запреты
            'id=1 - КО
            'id=2 - Обычные
            'id=3 - Ошибки
            'id=4 - В процессе
            'id=5 - Докачивали
            ID = Request("id")
            MyCn.ConnectionString = "Data Source=testsql.analit.net;Database=accessright;User ID=system;Password=123;Connect Timeout=300;Pooling=no"
            MyCn.Open()

            MyCmd.CommandText = " SELECT  Logtime, FirmCode, ShortName, Region, Addition" & _
            " FROM (logs.prgdataex p,  usersettings.clientsdata, accessright.showright, farm.regions r,  usersettings.retclientsset rcs)" & _
            " WHERE p.LogTime>curDate() " & _
            " and rcs.clientcode=p.clientcode " & _
            " and firmcode=p.clientcode " & _
            " and r.regioncode=clientsdata.regioncode " & _
            " and showright.regionmask & maskregion>0"

            If ID = 0 Then
                MyCmd.CommandText &= " and EXEVersion>0 and UpdateType=5"
                HeaderLB.Text = "Запреты:"
            End If

            If ID = 1 Then
                MyCmd.CommandText &= " and UpdateType=2"
                HeaderLB.Text = "Кумулятивные обновления:"
            End If

            If ID = 2 Then
                MyCmd.CommandText &= " and UpdateType=1"
                HeaderLB.Text = "Обычные обновления:"
            End If

            If ID = 3 Then
                MyCmd.CommandText &= " and EXEVersion>0 and UpdateType=6"
                HeaderLB.Text = "Ошибки подготовки данных:"
            End If

            If ID = 4 Then
                MyCmd.CommandText &= " and UncommittedUpdateTime>=CURDATE() and UpdateTime<>UncommittedUpdateTime"
                HeaderLB.Text = "В процессе получения обновления - Нет данных"
                CountLB.Text = "Нет данных"
                Exit Sub
            End If

            If ID = 5 Then
                MyCmd.CommandText &= " and UpdateType=3"
                HeaderLB.Text = "Докачки:"
            End If
            If ID > 5 Then GoTo exits

            MyCmd.CommandText &= " and showright.username='" & Session("UserName") & "'" & _
            " group by p.rowid" & _
            " order by p.logtime desc"
            CountLB.Text = MyDA.Fill(DS, "Table")
            CLList.DataBind()
exits:
            MyCn.Close()
        End Sub
    End Class

End Namespace
