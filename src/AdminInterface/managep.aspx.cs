'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser


Partial Class managep
    Inherits System.Web.UI.Page
    Protected WithEvents DS1 As System.Data.DataSet
    Protected WithEvents Regions As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Protected WithEvents WorkReg As System.Data.DataTable
    Protected WithEvents DataColumn5 As System.Data.DataColumn
    Protected WithEvents DataColumn7 As System.Data.DataColumn
    Protected WithEvents DataColumn8 As System.Data.DataColumn
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Dim myMySqlConnection As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;Pooling=no")
    Dim myMySqlCommand As New MySqlCommand()
    Dim myMySqlDataReader As MySqlDataReader
    Dim myMySqlDataAdapter As New MySqlDataAdapter()
    Dim myTrans As MySqlTransaction
    Dim ClientCode, HomeRegionCode, WorkMask, ShowMask, OrderMask As Int64
    Dim i As Int16
    Dim InsertCommand As String
    Protected WithEvents PD As System.Data.DataTable
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents DataColumn6 As System.Data.DataColumn
    Protected WithEvents PriceCode As System.Data.DataColumn
    Protected WithEvents PriceName As System.Data.DataColumn
    Protected WithEvents AgencyEnabled As System.Data.DataColumn
    Protected WithEvents Enabled As System.Data.DataColumn
    Protected WithEvents PriceType As System.Data.DataColumn
    Protected WithEvents ShowInWeb As System.Data.DataColumn
    Protected WithEvents AlowInt As System.Data.DataColumn
    Dim Func As New Func()
#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS1 = New System.Data.DataSet()
        Me.Regions = New System.Data.DataTable()
        Me.DataColumn1 = New System.Data.DataColumn()
        Me.DataColumn2 = New System.Data.DataColumn()
        Me.WorkReg = New System.Data.DataTable()
        Me.DataColumn5 = New System.Data.DataColumn()
        Me.DataColumn7 = New System.Data.DataColumn()
        Me.DataColumn8 = New System.Data.DataColumn()
        Me.DataColumn3 = New System.Data.DataColumn()
        Me.PD = New System.Data.DataTable()
        Me.DataColumn4 = New System.Data.DataColumn()
        Me.DataColumn6 = New System.Data.DataColumn()
        Me.PriceCode = New System.Data.DataColumn()
        Me.PriceName = New System.Data.DataColumn()
        Me.AgencyEnabled = New System.Data.DataColumn()
        Me.Enabled = New System.Data.DataColumn()
        Me.PriceType = New System.Data.DataColumn()
        Me.ShowInWeb = New System.Data.DataColumn()
        Me.AlowInt = New System.Data.DataColumn()
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.PD, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS1
        '
        Me.DS1.DataSetName = "NewDataSet"
        Me.DS1.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS1.Tables.AddRange(New System.Data.DataTable() {Me.Regions, Me.WorkReg, Me.PD})
        '
        'Regions
        '
        Me.Regions.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn1, Me.DataColumn2})
        Me.Regions.TableName = "Regions"
        '
        'DataColumn1
        '
        Me.DataColumn1.ColumnName = "Region"
        '
        'DataColumn2
        '
        Me.DataColumn2.ColumnName = "RegionCode"
        '
        'WorkReg
        '
        Me.WorkReg.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn5, Me.DataColumn7, Me.DataColumn8, Me.DataColumn3})
        Me.WorkReg.TableName = "WorkReg"
        '
        'DataColumn5
        '
        Me.DataColumn5.ColumnName = "RegionCode"
        '
        'DataColumn7
        '
        Me.DataColumn7.ColumnName = "ShowMask"
        Me.DataColumn7.DataType = GetType(System.Boolean)
        '
        'DataColumn8
        '
        Me.DataColumn8.ColumnName = "RegMask"
        Me.DataColumn8.DataType = GetType(System.Boolean)
        '
        'DataColumn3
        '
        Me.DataColumn3.ColumnName = "Region"
        '
        'PD
        '
        Me.PD.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn4, Me.DataColumn6, Me.PriceCode, Me.PriceName, Me.AgencyEnabled, Me.Enabled, Me.PriceType, Me.ShowInWeb, Me.AlowInt})
        Me.PD.TableName = "PD"
        '
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "FirmCode"
        Me.DataColumn4.DataType = GetType(System.UInt32)
        '
        'DataColumn6
        '
        Me.DataColumn6.ColumnName = "ShortName"
        '
        'PriceCode
        '
        Me.PriceCode.ColumnName = "PriceCode"
        Me.PriceCode.DataType = GetType(System.UInt32)
        '
        'PriceName
        '
        Me.PriceName.ColumnName = "PriceName"
        '
        'AgencyEnabled
        '
        Me.AgencyEnabled.ColumnName = "AgencyEnabled"
        Me.AgencyEnabled.DataType = GetType(System.Boolean)
        '
        'Enabled
        '
        Me.Enabled.ColumnName = "Enabled"
        Me.Enabled.DataType = GetType(System.Boolean)
        '
        'PriceType
        '
        Me.PriceType.ColumnName = "PriceType"
        Me.PriceType.DataType = GetType(System.Int16)
        '
        'ShowInWeb
        '
        Me.ShowInWeb.ColumnName = "ShowInWeb"
        Me.ShowInWeb.DataType = GetType(System.Boolean)
        '
        'AlowInt
        '
        Me.AlowInt.ColumnName = "AlowInt"
        Me.AlowInt.DataType = GetType(System.Boolean)
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.PD, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

        Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
            'CODEGEN: This method call is required by the Web Form Designer
            'Do not modify it using the code editor.
            InitializeComponent()
        End Sub

#End Region

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            ClientCode = CInt(Request("cc"))
            'ToDo При релизе коментировать!
            ' ClientCode = 1179
            'Session("UserName") = "michail"
            '     myMySqlCommand.Connection = myMySqlConnection

            '   myMySqlConnection.Open()
            '  myTrans = myMySqlConnection.BeginTransaction
            '  myMySqlCommand.Transaction = myTrans

            ' myMySqlCommand.CommandText = " SELECT RegionCode" & _
            '" FROM clientsdata as cd, accessright.regionaladmins" & _
            '" where  cd.regioncode  & regionaladmins.regionmask > 0 and UserName='" & Session("UserName") & "'" & _
            '" and FirmType=if(AlowCreateRetail+AlowCreateVendor=2, FirmType, if(AlowCreateRetail=1, 1, 0))" & _
            '" and FirmSegment=if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)" & Chr(10) & Chr(13) & _
            '" and if(UseRegistrant=1, Registrant='" & Session("UserName") & "', 1=1)" & _
            '" and AlowManage=1 and cd.firmcode=" & ClientCode
            'HomeRegionCode = CInt(myMySqlCommand.ExecuteScalar.ToString)
            'If HomeRegionCode < 1 Then Exit Sub
            If Not IsPostBack Then
                PostDataFromDB()
                'Func.SelectTODS("select regions.regioncode, regions.region  from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" & Session("UserName") & "' order by region", "regions", DS1)
                'RegionDD.DataBind()
                'For i = 0 To RegionDD.Items.Count - 1
                'If RegionDD.Items(i).Value = HomeRegionCode Then
                'RegionDD.SelectedIndex = i
                'Exit For
                'End If
                'Next
                'SetWorkRegions(HomeRegionCode, True)
            End If
            'myTrans.Commit()
            'myMySqlConnection.Close()
        End Sub
        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load
        
        ' End Sub

        Private Sub PostDataFromDB()

            If Func.SelectTODS(" set @UserName:='" & Session("UserName") & _
              "'; set @FirmCode:=" & ClientCode & _
              "; SELECT cd.firmcode, ShortName, pricesdata.PriceCode, PriceName, pricesdata.AgencyEnabled, pricesdata.Enabled,  AlowInt," & _
               " DateCurPrice, DateLastForm" & _
              " FROM (clientsdata as cd, farm.regions, accessright.regionaladmins, pricesdata, farm.formrules fr," & _
               " pricescosts pc)" & _
              " where regions.regioncode=cd.regioncode" & _
              " and pricesdata.firmcode=cd.firmcode " & _
               " and pricesdata.pricecode=fr.firmcode " & _
              " and pc.showpricecode=pricesdata.pricecode " & _
              " and cd.regioncode  & regionaladmins.regionmask > 0 " & _
              " and regionaladmins.UserName=@UserName" & _
              " and if(UseRegistrant=1, Registrant=@UserName, 1=1)" & _
              " and AlowManage=1" & _
              " and AlowCreateVendor=1" & _
            " and cd.firmcode=@FirmCode group by 3", "PD", DS1) Then
                R.DataBind()
                'For i = 0 To R.Items.Count - 1
                'CType(R.Items(i).Controls(5), DropDownList).SelectedIndex = DS1.Tables("PD").Rows(i).Item("PriceType")
                ' Next
            End If
            NameLB.Text = DS1.Tables("PD").Rows(0).Item("ShortName")
        End Sub

        Public Sub SetWorkRegions(ByVal RegCode As Int64, ByVal OldRegion As Boolean)

            Func.SelectTODS(" select a.RegionCode, a.Region, ShowRegionMask & a.regioncode>0 as ShowMask," & _
            " MaskRegion & a.regioncode>0 as RegMask, OrderRegionMask & a.regioncode>0 as OrderMask" & _
            " from (farm.regions as a, farm.regions as b, clientsdata, retclientsset)" & _
            " where b.regioncode=" & RegCode & _
            " and clientsdata.firmcode=" & ClientCode & _
            " and a.regioncode & b.defaultshowregionmask>0 " & _
            " and clientcode=firmcode" & _
            " order by region", "WorkReg", DS1)

            WRList.DataBind()
            ShowList.DataBind()
            For i = 0 To WRList.Items.Count - 1
                If OldRegion Then
                    WRList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("RegMask")
                    ShowList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("ShowMask")
                Else
                    ShowList.Items(i).Selected = True
                    If WRList.Items(i).Value = RegCode Then WRList.Items(i).Selected = True
                End If
            Next
        End Sub

        Private Sub RegionDD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs)
            Dim OldRegion As Boolean = False
            If RegionDD.SelectedItem.Value = HomeRegionCode Then OldRegion = True
            SetWorkRegions(RegionDD.SelectedItem.Value, OldRegion)
        End Sub

        Private Sub R_ItemCommand(ByVal source As Object, ByVal e As System.Web.UI.WebControls.RepeaterCommandEventArgs) Handles R.ItemCommand
            Dim Запрос As String
            Try
                myMySqlConnection.Open()
            Catch err As Exception
                Exit Try
            End Try
            Try
                myMySqlCommand.Connection = myMySqlConnection
                myMySqlCommand.CommandText = " set @UserName:='" & Session("UserName") & _
                  "'; set @FirmCode:=" & ClientCode & _
        "; SELECT  max(cd.regioncode  & regionaladmins.regionmask > 0" & _
        " and regionaladmins.UserName=@UserName " & _
        " and if(UseRegistrant=1, Registrant=@UserName, 1=1) " & _
        " and AlowManage=1 " & _
        " and AlowCreateVendor=1 " & _
        " and cd.firmcode=@FirmCode) FROM (clientsdata as cd, accessright.regionaladmins) "
                If myMySqlCommand.ExecuteScalar = 1 Then
                    myTrans = myMySqlConnection.BeginTransaction(IsolationLevel.ReadCommitted)
                    Запрос = "insert into pricesdata(PriceCode, FirmCode) values(Null, " & ClientCode & "); "
                    Запрос &= "select @PriceCode:=Last_insert_id(); "
                    Запрос &= "insert into farm.formrules(Firmcode) values (@PriceCode); "
                    Запрос &= "insert into farm.sources(FirmCode) values (@PriceCode); "

                    Запрос &= "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " & _
                                                      " Select @PriceCode, @PriceCode, 1, @PriceCode;" & _
                                                      " Insert into farm.costformrules(PC_CostCode) Select @PriceCode; "

                    Запрос &= "insert into pricesregionaldata(regioncode, pricecode, enabled) " & _
                        "SELECT regions.regioncode, pricesdata.pricecode, if(pricesdata.pricetype<>1, 1, 0) " & _
                        "FROM (clientsdata, farm.regions, pricesdata) " & _
                        "left join pricesregionaldata on pricesregionaldata.pricecode=pricesdata.pricecode and pricesregionaldata.regioncode=regions.regioncode " & _
                        "where pricesdata.firmcode=clientsdata.firmcode " & _
                        "and clientsdata.firmstatus=1 " & _
                        "and clientsdata.firmtype=0 " & _
                        "and pricesdata.pricecode=@PriceCode " & _
                        "and (clientsdata.maskregion & regions.regioncode)>0 " & _
                        "and pricesregionaldata.pricecode is null; "

                    Запрос &= "insert into intersection(regioncode, clientcode, pricecode, invisibleonfirm,  invisibleonclient, CostCode) " & _
                        "select regions.regioncode, clientsdata.firmcode, pricesdata.pricecode, retclientsset.invisibleonfirm, " & _
                        " 1 as invisibleonclient,  pricesdata.PriceCode " & _
                        "from (clientsdata, farm.regions, pricesdata, pricesregionaldata, retclientsset) " & _
                        "left join intersection on intersection.clientcode=clientsdata.firmcode and intersection.pricecode=pricesdata.pricecode and intersection.regioncode=regions.regioncode " & _
                        "left join clientsdata as b on b.firmcode=pricesdata.firmcode " & _
                        "where clientsdata.firmstatus=1 and clientsdata.firmsegment=b.firmsegment " & _
                        "and clientsdata.firmtype=1 " & _
                        "and (clientsdata.maskregion & regions.regioncode)>0 " & _
                        "and (b.maskregion & regions.regioncode)>0 " & _
                        "and retclientsset.clientcode=clientsdata.firmcode " & _
                        "and pricesdata.PriceCode=@PriceCode " & _
                        "and intersection.pricecode is null  " & _
                        "and pricesdata.pricetype<>1 " & _
                        "and pricesregionaldata.regioncode=regions.regioncode " & _
                        "and pricesregionaldata.pricecode=pricesdata.pricecode; "
                    myMySqlCommand.Transaction = myTrans
                    myMySqlCommand.CommandText = Запрос
                    myMySqlCommand.ExecuteNonQuery()
                End If
                myTrans.Commit()
                PostDataFromDB()
            Catch err As Exception
                myTrans.Rollback()
            Finally
                myMySqlConnection.Close()
            End Try
        End Sub

       
    End Class

End Namespace
