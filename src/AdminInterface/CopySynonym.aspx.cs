Imports MySql.Data.MySqlClient
'Imports ByteFX.Data.MySqlClient
'Imports System.Web.Mail
Imports System.Net.Dns


Namespace AddUser


Partial Class CopySynonym
    Inherits System.Web.UI.Page
    Protected WithEvents DS As System.Data.DataSet
    Protected WithEvents Regions As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;Pooling=no")
    Dim DA As New MySqlDataAdapter()
    Protected WithEvents From As System.Data.DataTable
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents DataColumn5 As System.Data.DataColumn
    Protected WithEvents DataColumn6 As System.Data.DataColumn
    Protected WithEvents ToT As System.Data.DataTable
    Dim UserName As String
    Protected WithEvents DataColumn7 As System.Data.DataColumn
    Dim func As New AddUser.Func()
#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS = New System.Data.DataSet()
        Me.Regions = New System.Data.DataTable()
        Me.DataColumn1 = New System.Data.DataColumn()
        Me.DataColumn2 = New System.Data.DataColumn()
        Me.DataColumn7 = New System.Data.DataColumn()
        Me.From = New System.Data.DataTable()
        Me.DataColumn3 = New System.Data.DataColumn()
        Me.DataColumn4 = New System.Data.DataColumn()
        Me.ToT = New System.Data.DataTable()
        Me.DataColumn5 = New System.Data.DataColumn()
        Me.DataColumn6 = New System.Data.DataColumn()
        CType(Me.DS, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.From, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.ToT, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS
        '
        Me.DS.DataSetName = "NewDataSet"
        Me.DS.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS.Tables.AddRange(New System.Data.DataTable() {Me.Regions, Me.From, Me.ToT})
        '
        'Regions
        '
        Me.Regions.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn1, Me.DataColumn2, Me.DataColumn7})
        Me.Regions.TableName = "Regions"
        '
        'DataColumn1
        '
        Me.DataColumn1.ColumnName = "Region"
        '
        'DataColumn2
        '
        Me.DataColumn2.ColumnName = "RegionCode"
        Me.DataColumn2.DataType = GetType(System.Int64)
        '
        'DataColumn7
        '
        Me.DataColumn7.ColumnName = "Email"
        '
        'From
        '
        Me.From.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn3, Me.DataColumn4})
        Me.From.TableName = "From"
        '
        'DataColumn3
        '
        Me.DataColumn3.ColumnName = "Name"
        '
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "ClientCode"
        Me.DataColumn4.DataType = GetType(System.Int32)
        '
        'ToT
        '
        Me.ToT.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn5, Me.DataColumn6})
        Me.ToT.TableName = "ToT"
        '
        'DataColumn5
        '
        Me.DataColumn5.ColumnName = "Name"
        '
        'DataColumn6
        '
        Me.DataColumn6.ColumnName = "ClientCode"
        Me.DataColumn6.DataType = GetType(System.Int32)
        CType(Me.DS, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.From, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.ToT, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FindBT.Click
        FindClient(FromTB.Text, "From")
        FindClient(ToTB.Text, "ToT")
        FromL.Text = DS.Tables("from").Rows.Count
        ToL.Text = DS.Tables("tot").Rows.Count

        If DS.Tables("from").Rows.Count > 0 Then

            If DS.Tables("from").Rows.Count > 50 Then
                LabelErr.Text = "Найдено более 50 записей ""От"". Уточните условие поиска."
                Exit Sub
            End If

            FromTB.Visible = False
            FromDD.Visible = True
            FromDD.DataBind()
        End If

        If DS.Tables("tot").Rows.Count > 0 Then

            If DS.Tables("tot").Rows.Count > 50 Then
                LabelErr.Text = "Найдено более 50 записей ""Для"". Уточните условие поиска."
                Exit Sub
            End If

            ToTB.Visible = False
            ToDD.Visible = True
            ToDD.DataBind()
        End If
        If DS.Tables("tot").Rows.Count > 0 And DS.Tables("from").Rows.Count > 0 Then
            SetBT.Enabled = True
            FindBT.Enabled = False
            SetBT.Visible = True
        Else
            SetBT.Visible = False
        End If

    End Sub

        Public Sub FindClient(ByVal NameStr As String, ByVal Where As String)
            With DA
                .SelectCommand = New MySqlCommand("select shortname name, firmcode as clientcode from clientsdata, accessright.regionaladmins" & _
                " where regioncode =" & RegionDD.SelectedItem.Value & _
                " and firmtype=1 and firmstatus=1" & _
                " and shortname like ?NameStr" & _
                 " and UserName='" & Session("UserName") & "'" & _
                   " and FirmSegment=if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)" & Chr(10) & Chr(13) & _
                 " and if(UseRegistrant=1, Registrant='" & Session("UserName") & "', 1=1)" & _
                " and username='" & UserName & "'", соединение)
                .SelectCommand.Parameters.Add(New MySqlParameter("NameStr", MySqlDbType.String))
                .SelectCommand.Parameters("NameStr").Value = "%" & NameStr & "%"
                .Fill(DS, Where)
            End With
        End Sub

    Private Sub Button2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SetBT.Click
        Dim ClientCode, ParentClientCode As Int32
        Dim Query As String
        ClientCode = ToDD.SelectedItem.Value
        ParentClientCode = FromDD.SelectedItem.Value
        Dim MyCommand As New MySqlCommand
        Dim MyTrans As MySqlTransaction

        Try
            'Query = " insert into pricesregionaldata(regioncode, pricecode, enabled)" & _
            '" SELECT regions.regioncode, pricesdata.pricecode, if(pricesdata.pricetype<>1, 1, 0)" & _
            '" FROM clientsdata, farm.regions, pricesdata, clientsdata as a" & _
            '" left join pricesregionaldata on" & _
            '" pricesregionaldata.pricecode=pricesdata.pricecode and" & _
            '" pricesregionaldata.regioncode= regions.regioncode" & _
            '" where pricesdata.firmcode=clientsdata.firmcode" & _
            '" and clientsdata.firmsegment=a.firmsegment" & _
            '" and a.firmcode=" & ClientCode & _
            '" and clientsdata.firmstatus=1" & _
            '" and clientsdata.firmtype=0" & _
            '" and  pricesdata.enabled=1" & _
            '" and (clientsdata.maskregion & regions.regioncode)>0" & _
            '" and pricesregionaldata.pricecode is null;"

            'Query = Query & " insert into intersection(ClientCode, regioncode, pricecode, disabledbyfirm," & _
            '" invisibleonclient, InvisibleonFirm)" & _
            '" SELECT clientsdata2.firmcode, regions.regioncode, pricesdata.pricecode," & _
            '" if(pricesdata.PriceType=2, 1, 0) as disabledbyfirm, if(a.invisibleonfirm=1" & _
            '" and pricesdata.PriceType=2, 1, 0) as invisibleonclient, a.invisibleonfirm" & _
            '" FROM clientsdata, farm.regions, pricesdata, pricesregionaldata" & _
            '" LEFT JOIN intersection ON intersection.pricecode=pricesdata.pricecode and" & _
            '" intersection.regioncode=regions.regioncode  and" & _
            '" intersection.clientcode=clientsdata2.firmcode" & _
            '" left join clientsdata as clientsdata2 on clientsdata2.firmcode=" & ClientCode & _
            '" left join retclientsset as a on a.clientcode=clientsdata2.firmcode" & _
            '" WHERE intersection.pricecode IS NULL and" & _
            '" clientsdata.firmstatus=1 and" & _
            '" clientsdata.firmsegment=clientsdata2.firmsegment" & _
            '" and clientsdata.firmtype=0" & _
            '" and pricesdata.firmcode=clientsdata.firmcode" & _
            '" and pricesregionaldata.pricecode=pricesdata.pricecode" & _
            '" and pricesregionaldata.regioncode=regions.regioncode" & _
            '" and pricesregionaldata.enabled=1" & _
            '" and pricesdata.enabled=1" & _
            '" and pricesdata.pricetype<>1" & _
            '" and (clientsdata.maskregion & regions.regioncode)>0" & _
            '" and (clientsdata2.maskregion & regions.regioncode)>0;"

            Query &= " update intersection set MaxSynonymCode=0, MaxSynonymFirmCrCode=0," & _
            " lastsent='2003-01-01' where clientcode=" & ClientCode & ";"

            Query &= " update retclientsset as a, retclientsset as b" & _
            " set b.updatetime=a.updatetime, b.AlowCumulativeUpdate=0, b.Active=0 where a.clientcode=" & ParentClientCode & " and b.clientcode=" & ClientCode & ";"

            Query &= " update intersection as a, intersection as b" & _
            " set a.MaxSynonymFirmCrCode=b.MaxSynonymFirmCrCode," & _
            " a.MaxSynonymCode=b.MaxSynonymCode, a.lastsent=b.lastsent" & _
            " where a.clientcode=" & ClientCode & " and b.clientcode=" & ParentClientCode & _
            " and a.pricecode=b.pricecode;"

            Query &= " insert into logs.clone (LogTime, UserName, FromClientCode, ToClientCode) values (now(), '" & Session("UserName") & _
            "', " & ParentClientCode & ", " & ClientCode & ")"

            соединение.Open()
            MyTrans = соединение.BeginTransaction
            With MyCommand
                .CommandText = Query
                .Transaction = MyTrans
                .Connection = соединение
            End With

            MyCommand.ExecuteNonQuery()
            MyTrans.Commit()
            func.Mail("register@analit.net", "Успешное присвоение кодов(" & ParentClientCode & " > " & ClientCode & ")", System.Web.Mail.MailFormat.Text, "От: " & FromDD.SelectedItem.Text & Chr(10) & Chr(13) & "Для: " & ToDD.SelectedItem.Text & _
              Chr(10) & Chr(13) & "Оператор: " & UserName, DS.Tables("Regions").Rows(0).Item("email"), "RegisterList@subscribe.analit.net", System.Text.Encoding.UTF8)
            LabelErr.ForeColor = Color.Green
            LabelErr.Text = "Присвоение успешно завершено.Время операции: " & Now()
            FromDD.Visible = False
            ToDD.Visible = False
            FromTB.Visible = True
            ToTB.Visible = True
            FindBT.Visible = True
            FindBT.Enabled = True
            SetBT.Visible = False
        Catch err As Exception
            LabelErr.Text = err.Message
            MyTrans.Rollback()
        Finally
            соединение.Close()
        End Try

    End Sub



        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            UserName = Session("UserName")
            With DA
                .SelectCommand = New MySqlCommand("select regions.region, regions.regioncode, Email from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" & UserName & _
                "' order by region", соединение)
                .Fill(DS, "Regions")
            End With

            If DS.Tables("Regions").Rows.Count < 1 Then

            End If

            If Not IsPostBack Then
                RegionDD.DataBind()
            End If
        End Sub
    End Class

End Namespace
