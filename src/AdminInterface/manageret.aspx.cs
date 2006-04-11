'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser

Partial Class manageret
    Inherits System.Web.UI.Page
    Protected WithEvents DS1 As System.Data.DataSet
    Protected WithEvents Regions As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Protected WithEvents WorkReg As System.Data.DataTable
    Protected WithEvents DataColumn5 As System.Data.DataColumn
    Protected WithEvents DataColumn7 As System.Data.DataColumn
    Protected WithEvents DataColumn8 As System.Data.DataColumn
    Protected WithEvents Clientsdata As System.Data.DataTable
    Protected WithEvents DataColumn9 As System.Data.DataColumn
    Protected WithEvents DataColumn10 As System.Data.DataColumn
    Protected WithEvents DataColumn11 As System.Data.DataColumn
    Protected WithEvents DataColumn12 As System.Data.DataColumn
    Protected WithEvents DataColumn13 As System.Data.DataColumn
    Protected WithEvents DataColumn14 As System.Data.DataColumn
    Protected WithEvents DataColumn15 As System.Data.DataColumn
    Protected WithEvents DataColumn16 As System.Data.DataColumn
    Protected WithEvents DataColumn17 As System.Data.DataColumn
    Protected WithEvents DataColumn18 As System.Data.DataColumn
    Protected WithEvents DataColumn19 As System.Data.DataColumn
    Protected WithEvents DataColumn20 As System.Data.DataColumn
    Protected WithEvents DataColumn21 As System.Data.DataColumn
    Protected WithEvents admin As System.Data.DataTable
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents RetClientsSet As System.Data.DataTable

    Dim myMySqlConnection As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;Pooling=no")
    Dim myMySqlCommand As New MySqlCommand()
    Dim myMySqlDataReader As MySqlDataReader
    Dim myMySqlDataAdapter As New MySqlDataAdapter()
    Dim myTrans As MySqlTransaction
    Dim ClientCode, HomeRegionCode, WorkMask, ShowMask, OrderMask As Int64
    Dim i As Int16
    Dim InsertCommand As String
    Dim Func As New Func

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS1 = New System.Data.DataSet
        Me.Regions = New System.Data.DataTable
        Me.DataColumn1 = New System.Data.DataColumn
        Me.DataColumn2 = New System.Data.DataColumn
        Me.WorkReg = New System.Data.DataTable
        Me.DataColumn5 = New System.Data.DataColumn
        Me.DataColumn7 = New System.Data.DataColumn
        Me.DataColumn8 = New System.Data.DataColumn
        Me.DataColumn3 = New System.Data.DataColumn
        Me.DataColumn4 = New System.Data.DataColumn
        Me.Clientsdata = New System.Data.DataTable
        Me.DataColumn9 = New System.Data.DataColumn
        Me.DataColumn10 = New System.Data.DataColumn
        Me.DataColumn11 = New System.Data.DataColumn
        Me.DataColumn12 = New System.Data.DataColumn
        Me.DataColumn13 = New System.Data.DataColumn
        Me.DataColumn14 = New System.Data.DataColumn
        Me.DataColumn15 = New System.Data.DataColumn
        Me.DataColumn16 = New System.Data.DataColumn
        Me.DataColumn17 = New System.Data.DataColumn
        Me.DataColumn18 = New System.Data.DataColumn
        Me.DataColumn19 = New System.Data.DataColumn
        Me.DataColumn20 = New System.Data.DataColumn
        Me.DataColumn21 = New System.Data.DataColumn
        Me.admin = New System.Data.DataTable
        Me.RetClientsSet = New System.Data.DataTable
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Clientsdata, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.admin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.RetClientsSet, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS1
        '
        Me.DS1.DataSetName = "NewDataSet"
        Me.DS1.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS1.Tables.AddRange(New System.Data.DataTable() {Me.Regions, Me.WorkReg, Me.Clientsdata, Me.admin, Me.RetClientsSet})
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
        Me.DataColumn2.DataType = GetType(System.Int64)
        '
        'WorkReg
        '
        Me.WorkReg.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn5, Me.DataColumn7, Me.DataColumn8, Me.DataColumn3, Me.DataColumn4})
        Me.WorkReg.TableName = "WorkReg"
        '
        'DataColumn5
        '
        Me.DataColumn5.ColumnName = "RegionCode"
        Me.DataColumn5.DataType = GetType(System.Int32)
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
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "OrderMask"
        Me.DataColumn4.DataType = GetType(System.Boolean)
        '
        'Clientsdata
        '
        Me.Clientsdata.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn9, Me.DataColumn10, Me.DataColumn11, Me.DataColumn12, Me.DataColumn13, Me.DataColumn14, Me.DataColumn15, Me.DataColumn16, Me.DataColumn17, Me.DataColumn18, Me.DataColumn19, Me.DataColumn20, Me.DataColumn21})
        Me.Clientsdata.TableName = "Clientsdata"
        '
        'DataColumn9
        '
        Me.DataColumn9.ColumnName = "adress"
        '
        'DataColumn10
        '
        Me.DataColumn10.ColumnName = "bussinfo"
        '
        'DataColumn11
        '
        Me.DataColumn11.ColumnName = "bussstop"
        '
        'DataColumn12
        '
        Me.DataColumn12.ColumnName = "fax"
        '
        'DataColumn13
        '
        Me.DataColumn13.ColumnName = "firmsegment"
        Me.DataColumn13.DataType = GetType(System.Int16)
        '
        'DataColumn14
        '
        Me.DataColumn14.ColumnName = "firmtype"
        Me.DataColumn14.DataType = GetType(System.Int16)
        '
        'DataColumn15
        '
        Me.DataColumn15.ColumnName = "oldcode"
        Me.DataColumn15.DataType = GetType(System.Int16)
        '
        'DataColumn16
        '
        Me.DataColumn16.ColumnName = "phone"
        '
        'DataColumn17
        '
        Me.DataColumn17.ColumnName = "regioncode"
        Me.DataColumn17.DataType = GetType(System.Int64)
        '
        'DataColumn18
        '
        Me.DataColumn18.ColumnName = "shortname"
        '
        'DataColumn19
        '
        Me.DataColumn19.ColumnName = "url"
        '
        'DataColumn20
        '
        Me.DataColumn20.ColumnName = "fullname"
        '
        'DataColumn21
        '
        Me.DataColumn21.ColumnName = "mail"
        '
        'admin
        '
        Me.admin.TableName = "admin"
        '
        'RetClientsSet
        '
        Me.RetClientsSet.TableName = "RetClientsSet"
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Clientsdata, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.admin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.RetClientsSet, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load



        Private Sub ParametersSave_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ParametersSave.Click
            If AlowCumulativeCB.Checked And Len(CUWTB.Text) < 5 And AlowCumulativeCB.Enabled Then
                ResultL.Text = "Изменения не сохранены.<br>Укажите причину кумулятивного обновления."
                ResultL.ForeColor = Color.Red
                Exit Sub
            End If

            If ResetCopyIDCB.Checked And CopyIDWTB.Text.Length < 5 And ResetCopyIDCB.Enabled Then
                ResultL.Text = "Изменения не сохранены.<br>Укажите причину сброса идентификатора."
                ResultL.ForeColor = Color.Red
                Exit Sub
            End If

            myMySqlConnection.Open()
            myTrans = myMySqlConnection.BeginTransaction

            With myMySqlCommand
                .Transaction = myTrans
                .Parameters.Add(New MySqlParameter("InvisibleOnFirm", MySqlDbType.Int16))
                .Parameters("InvisibleOnFirm").Value = InvisibleCB.Checked

                .Parameters.Add(New MySqlParameter("AlowRegister", MySqlDbType.Int16))
                .Parameters("AlowRegister").Value = RegisterCB.Checked

                .Parameters.Add(New MySqlParameter("AlowRejection", MySqlDbType.Int16))
                .Parameters("AlowRejection").Value = RejectsCB.Checked

                .Parameters.Add(New MySqlParameter("AlowDocuments", MySqlDbType.Int16))
                .Parameters("AlowDocuments").Value = DocumentsCB.Checked

                .Parameters.Add(New MySqlParameter("MultiUserLevel", MySqlDbType.Int16))
                .Parameters("MultiUserLevel").Value = MultiUserLevelTB.Text

                .Parameters.Add(New MySqlParameter("AdvertisingLevel", MySqlDbType.Int16))
                .Parameters("AdvertisingLevel").Value = AdvertisingLevelCB.Checked

                .Parameters.Add(New MySqlParameter("AlowWayBill", MySqlDbType.Int16))
                .Parameters("AlowWayBill").Value = WayBillCB.Checked

                .Parameters.Add(New MySqlParameter("AlowChangeSegment", MySqlDbType.Int16))
                .Parameters("AlowChangeSegment").Value = ChangeSegmentCB.Checked

                .Parameters.Add(New MySqlParameter("EnableUpdate", MySqlDbType.Int16))
                .Parameters("EnableUpdate").Value = EnableUpdateCB.Checked

                .Parameters.Add(New MySqlParameter("CheckCopyID", MySqlDbType.Int16))
                .Parameters("CheckCopyID").Value = CheckCopyIDCB.Checked

                .Parameters.Add(New MySqlParameter("AlowCumulativeUpdate", MySqlDbType.Int16))
                .Parameters("AlowCumulativeUpdate").Value = AlowCumulativeCB.Checked

                .Parameters.Add(New MySqlParameter("ResetIDCause", MySqlDbType.String))
                .Parameters("ResetIDCause").Value = CopyIDWTB.Text

                .Parameters.Add(New MySqlParameter("CumulativeUpdateCause", MySqlDbType.String))
                .Parameters("CumulativeUpdateCause").Value = CUWTB.Text

            End With
            HomeRegionCode = RegionDD.SelectedItem.Value

            For i = 0 To ShowList.Items.Count - 1
                If ShowList.Items(i).Selected Then ShowMask = ShowMask + ShowList.Items(i).Value
                If WRList.Items(i).Selected Then WorkMask = WorkMask + WRList.Items(i).Value
                If OrderList.Items(i).Selected Then OrderMask = OrderMask + OrderList.Items(i).Value
            Next
            Try

                myMySqlCommand.CommandText = "select RegionCode=" & HomeRegionCode & " and ShowRegionMask=" & ShowMask & _
                " and MaskRegion=" & WorkMask & " from clientsdata where firmcode=" & ClientCode
                If myMySqlCommand.ExecuteScalar = 0 Then
                    InsertCommand = InsertCommand & "insert into logs.clientsdataupdates select null, now(), '" & Session("UserName") & "', '" & HttpContext.Current.Request.UserHostAddress & _
                    "', " & ClientCode & ", null, null, null, if(RegionCode=" & HomeRegionCode & ", null, " & HomeRegionCode & "), if(MaskRegion=" & WorkMask & ", null, " & WorkMask & _
                    "), if(ShowRegionMask=" & ShowMask & ", null, " & ShowMask & "), null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null from clientsdata where firmcode=" & ClientCode & "; "
                End If

                myMySqlCommand.CommandText = "select MaskRegion=" & WorkMask & " from clientsdata where firmcode=" & ClientCode
                If myMySqlCommand.ExecuteScalar = 0 Then
                    InsertCommand = InsertCommand & " insert into intersection(ClientCode, regioncode, pricecode,  invisibleonclient, InvisibleonFirm, CostCode)" & _
                        "  SELECT distinct clientsdata2.firmcode, regions.regioncode, pricesdata.pricecode," & _
                        "  pricesdata.PriceType=2 as invisibleonclient, a.invisibleonfirm, (SELECT costcode FROM pricescosts pcc WHERE basecost" & _
                        " AND showpricecode=pc.showpricecode)" & _
                        "  FROM (clientsdata, farm.regions, pricesdata, pricesregionaldata, pricescosts pc)" & _
                        "  left join clientsdata as clientsdata2 on clientsdata2.firmcode=" & ClientCode & _
                        "  LEFT JOIN intersection ON intersection.pricecode=pricesdata.pricecode and  intersection.regioncode=regions.regioncode  and  intersection.clientcode=clientsdata2.firmcode" & _
                        "  left join retclientsset as a on a.clientcode=clientsdata2.firmcode" & _
                        "  WHERE intersection.pricecode IS NULL and " & _
                        " clientsdata.firmstatus=1 " & _
                        " and clientsdata.firmsegment=clientsdata2.firmsegment" & _
                        "  and clientsdata.firmtype=0" & _
                        "  and pricesdata.firmcode=clientsdata.firmcode" & _
                        "  and pricesregionaldata.pricecode=pricesdata.pricecode" & _
                        "  and pricesregionaldata.regioncode=regions.regioncode" & _
                        "  and pricesdata.pricetype<>1" & _
                        "     AND pricesdata.pricecode=pc.showpricecode " & _
                        "  and (clientsdata.maskregion & regions.regioncode)>0" & _
                        "  and (" & WorkMask & " & regions.regioncode)>0;"
                End If

                myMySqlCommand.CommandText = "select OrderRegionMask=" & OrderMask & _
                " and InvisibleOnFirm=?InvisibleOnFirm and AlowRegister=?AlowRegister and AlowRejection=?AlowRejection and AlowDocuments=?AlowDocuments and MultiUserLevel=?MultiUserLevel and (WorkRegionMask=if(WorkRegionMask & " & WorkMask & ">0, WorkRegionMask, " & HomeRegionCode & "))" & _
                "and AdvertisingLevel=?AdvertisingLevel and AlowWayBill=?AlowWayBill and AlowChangeSegment=?AlowChangeSegment and EnableUpdate=?EnableUpdate and CheckCopyID=?CheckCopyID " & _
                " from retclientsset where clientcode=" & ClientCode

                If myMySqlCommand.ExecuteScalar = 0 Or ((AlowCumulativeCB.Enabled And AlowCumulativeCB.Checked) _
                Or (ResetCopyIDCB.Enabled And ResetCopyIDCB.Checked)) Then
                    InsertCommand = InsertCommand & "insert into logs.retclientssetupdate select null, now(), '" & Session("UserName") & "', '" & HttpContext.Current.Request.UserHostAddress & _
                    "', " & ClientCode & ", if(InvisibleOnFirm=?InvisibleOnFirm, null, ?InvisibleOnFirm), null, null, null, null, if(AlowRegister=?AlowRegister, null, ?AlowRegister), if(AlowRejection=?AlowRejection, null, ?AlowRejection), " & _
                    "if(AlowDocuments=?AlowDocuments, null, ?AlowDocuments), if(MultiUserLevel=?MultiUserLevel, null, ?MultiUserLevel), if(AdvertisingLevel=?AdvertisingLevel, null, ?AdvertisingLevel), " & _
                    "if(AlowWayBill=?AlowWayBill, null, ?AlowWayBill), if(AlowChangeSegment=?AlowChangeSegment, null, ?AlowChangeSegment), null, if(WorkRegionMask=if(WorkRegionMask & " & WorkMask & ">0, WorkRegionMask, " & HomeRegionCode & "), null, " & HomeRegionCode & "), " & _
                    "if(OrderRegionMask=" & OrderMask & ", null, " & OrderMask & "), null, null, if(EnableUpdate=?EnableUpdate, null, ?EnableUpdate), if(CheckCopyID=?CheckCopyID, null, ?CheckCopyID), "

                    If AlowCumulativeCB.Enabled And AlowCumulativeCB.Checked Then
                        InsertCommand &= "?CumulativeUpdateCause, "
                    Else
                        InsertCommand &= "null, "
                    End If


                    If ResetCopyIDCB.Enabled And ResetCopyIDCB.Checked Then

                        InsertCommand &= "?ResetIDCause "

                    Else
                        InsertCommand &= "null "
                    End If

                    InsertCommand &= ",null, null, null from retclientsset where clientcode=" & ClientCode & "; "
                End If

                If InvisibleCB.Enabled Then
                    myMySqlCommand.CommandText = "select InvisibleOnFirm=?InvisibleOnFirm from retclientsset where clientcode=" & ClientCode
                    If myMySqlCommand.ExecuteScalar = 0 Then
                        InsertCommand = InsertCommand & " update retclientsset, intersection, pricesdata set retclientsset.invisibleonfirm=?InvisibleOnFirm,  intersection.invisibleonfirm=?InvisibleOnFirm"
                        If InvisibleCB.Checked Then InsertCommand = InsertCommand & ", DisabledByFirm=if(PriceType=2, 1, 0),  InvisibleOnClient=if(PriceType=2, 1, 0)"
                        InsertCommand = InsertCommand & " where intersection.clientcode=retclientsset.clientcode and intersection.pricecode=pricesdata.pricecode and intersection.clientcode=" & ClientCode & "; "
                    End If
                End If

                InsertCommand &= "update retclientsset, clientsdata set OrderRegionMask=" & OrderMask & _
                ", MaskRegion=" & WorkMask & ", ShowRegionMask=" & ShowMask & ", RegionCode=" & HomeRegionCode & ", WorkRegionMask=if(WorkRegionMask & " & WorkMask & ">0, WorkRegionMask, " & HomeRegionCode & "), " & _
                " AlowRegister=?AlowRegister, AlowRejection=?AlowRejection, AlowDocuments=?AlowDocuments, MultiUserLevel=?MultiUserLevel, " & _
                "AdvertisingLevel=?AdvertisingLevel, AlowWayBill=?AlowWayBill, AlowChangeSegment=?AlowChangeSegment, EnableUpdate=?EnableUpdate, CheckCopyID=?CheckCopyID, AlowCumulativeUpdate=?AlowCumulativeUpdate"

                If ResetCopyIDCB.Enabled And ResetCopyIDCB.Checked Then InsertCommand &= ", UniqueCopyID=''"

                InsertCommand &= " where clientcode=firmcode and firmcode=" & ClientCode
                myMySqlCommand.CommandText = InsertCommand
                myMySqlCommand.ExecuteNonQuery()
                myTrans.Commit()
            Catch err As Exception
                myTrans.Rollback()
                ResultL.ForeColor = Color.Red
                ResultL.Text = err.Message
                Exit Sub
            Finally
                myMySqlConnection.Close()
            End Try

            ResultL.ForeColor = Color.Green
            ResultL.Text = "Сохранено."
        End Sub

        Private Sub SendMessage_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SendMessage.Click
            myMySqlConnection.Open()
            myTrans = myMySqlConnection.BeginTransaction
            myMySqlCommand.Transaction = myTrans
            Try
                myMySqlCommand.CommandText = "insert into logs.retclientssetupdate (RowID, LogTime, OperatorName, OperatorHost, ClientCode, ShowMessageCount, Message) select null, now(), '" & Session("UserName") & "', '" & HttpContext.Current.Request.UserHostAddress & _
                "', " & ClientCode & ", " & SendMessageCountDD.SelectedItem.Value & ", ?Message; " & _
                "update retclientsset set ShowMessageCount=" & SendMessageCountDD.SelectedItem.Value & _
                ", Message=?Message where clientcode=" & ClientCode
                myMySqlCommand.Parameters.Add("Message", MySqlDbType.String)
                myMySqlCommand.Parameters(0).Value = MessageTB.Text

                myMySqlCommand.ExecuteNonQuery()
                myTrans.Commit()
                MessageTB.Text = ""
                StatusL.Visible = True
            Catch err As Exception
                myTrans.Rollback()
                StatusL.Text = err.Message
                StatusL.ForeColor = Color.Red
                StatusL.Visible = True
                Exit Sub
            Finally
                myMySqlConnection.Close()
            End Try


        End Sub


        Private Sub RegionDD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RegionDD.SelectedIndexChanged
            Dim OldRegion As Boolean = False
            If RegionDD.SelectedItem.Value = HomeRegionCode Then OldRegion = True
            SetWorkRegions(RegionDD.SelectedItem.Value, OldRegion, False)
        End Sub

        Private Sub SetWorkRegions(ByVal RegCode As Int64, ByVal OldRegion As Boolean, ByVal AllRegions As Boolean)
            Dim SQLTXT As String
            SQLTXT = " select a.RegionCode, a.Region, ShowRegionMask & a.regioncode>0 as ShowMask," & _
                    " MaskRegion & a.regioncode>0 as RegMask, OrderRegionMask & a.regioncode>0 as OrderMask" & _
                    " from farm.regions as a, farm.regions as b, clientsdata, retclientsset, accessright.regionaladmins" & _
                    " where"
            If Not AllRegions Then SQLTXT &= " b.regioncode=" & RegCode & " and"

            SQLTXT &= " clientsdata.firmcode=" & ClientCode & _
                    " and a.regioncode & b.defaultshowregionmask>0 " & _
                    " and clientcode=firmcode" & _
                    " and regionaladmins.username='" & Session("UserName") & "'" & _
                    " and a.regioncode & regionaladmins.RegionMask > 0" & _
                    " group by regioncode" & _
                    " order by region"
            Func.SelectTODS(SQLTXT, "WorkReg", DS1)

            WRList.DataBind()
            OrderList.DataBind()
            ShowList.DataBind()
            For i = 0 To WRList.Items.Count - 1
                If OldRegion Then
                    WRList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("RegMask")
                    OrderList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("OrderMask")
                    ShowList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("ShowMask")
                Else
                    ShowList.Items(i).Selected = True
                    If WRList.Items(i).Value = RegCode Then WRList.Items(i).Selected = True
                    OrderList.Items(i).Selected = WRList.Items(i).Selected
                End If
            Next
        End Sub


        Private Sub AllRegCB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AllRegCB.CheckedChanged
            If AllRegCB.Checked Then
                SetWorkRegions(RegionDD.SelectedItem.Value, True, True)
            Else
                SetWorkRegions(RegionDD.SelectedItem.Value, True, False)
            End If
        End Sub


        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            StatusL.Visible = False
            ClientCode = CInt(Request("cc"))
            myMySqlCommand.Connection = myMySqlConnection

            myMySqlConnection.Open()
            myTrans = myMySqlConnection.BeginTransaction
            myMySqlCommand.Transaction = myTrans

            myMySqlCommand.CommandText = " SELECT RegionCode, MaskRegion, ShowRegionMask" & _
    " FROM clientsdata as cd, accessright.regionaladmins" & _
    " where  cd.regioncode  & regionaladmins.regionmask > 0 and UserName='" & Session("UserName") & "'" & _
    " and FirmType=if(AlowCreateRetail+AlowCreateVendor=2, FirmType, if(AlowCreateRetail=1, 1, 0))" & _
    " and FirmSegment=if(regionaladmins.AlowChangeSegment=1, FirmSegment, DefaultSegment)" & Chr(10) & Chr(13) & _
    " and if(UseRegistrant=1, Registrant='" & Session("UserName") & "', 1=1)" & _
    " and AlowManage=1 and cd.firmcode=" & ClientCode
            HomeRegionCode = CInt(myMySqlCommand.ExecuteScalar.ToString)
            If HomeRegionCode < 1 Then Exit Sub
            If Not IsPostBack Then
                Func.SelectTODS("select regions.regioncode, regions.region  from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" & Session("UserName") & "' order by region", "admin", DS1)
                RegionDD.DataBind()
                For i = 0 To RegionDD.Items.Count - 1
                    If RegionDD.Items(i).Value = HomeRegionCode Then
                        RegionDD.SelectedIndex = i
                        Exit For
                    End If
                Next

                SetWorkRegions(HomeRegionCode, True, False)
                myMySqlCommand.CommandText = " select InvisibleOnFirm, AlowRegister, AlowRejection, AlowDocuments, MultiUserLevel," & _
                           " AdvertisingLevel,  AlowWayBill, retclientsset.AlowChangeSegment, EnableUpdate, CheckCopyID," & _
                           " AlowCumulativeUpdate, AlowCreateInvisible, length(UniqueCopyID)=0 from retclientsset, accessright.regionaladmins where clientcode=" & ClientCode & " and username='" & Session("UserName") & "'"
                myMySqlDataReader = myMySqlCommand.ExecuteReader
                myMySqlDataReader.Read()

                InvisibleCB.Checked = myMySqlDataReader(0).ToString
                InvisibleCB.Enabled = myMySqlDataReader(11).ToString

                RegisterCB.Checked = myMySqlDataReader(1).ToString

                RejectsCB.Checked = myMySqlDataReader(2).ToString

                DocumentsCB.Checked = myMySqlDataReader(3).ToString

                MultiUserLevelTB.Text = myMySqlDataReader(4).ToString

                AdvertisingLevelCB.Checked = myMySqlDataReader(5).ToString

                WayBillCB.Checked = myMySqlDataReader(6).ToString

                ChangeSegmentCB.Checked = myMySqlDataReader(7).ToString

                EnableUpdateCB.Checked = myMySqlDataReader(8).ToString

                CheckCopyIDCB.Checked = myMySqlDataReader(9).ToString

                AlowCumulativeCB.Checked = myMySqlDataReader(10).ToString

                ResetCopyIDCB.Checked = myMySqlDataReader(12).ToString

                If Not AlowCumulativeCB.Checked Then
                    AlowCumulativeCB.Enabled = True
                    CUWTB.Enabled = True
                    CUSetL.Visible = False
                End If

                If Not ResetCopyIDCB.Checked Then
                    ResetCopyIDCB.Enabled = True
                    CopyIDWTB.Enabled = True
                    IDSetL.Visible = False
                End If

                myMySqlDataReader.Close()
            End If
            myTrans.Commit()
            myMySqlConnection.Close()
        End Sub

    End Class

End Namespace
