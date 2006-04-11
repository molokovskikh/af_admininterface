Imports MySql.data.MySqlClient

Partial Class ManageCosts
    Inherits System.Web.UI.Page

#Region " Web Form Designer Generated Code "

    'This call is required by the Web Form Designer.
    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS = New System.Data.DataSet
        Me.Costs = New System.Data.DataTable
        Me.DataColumn1 = New System.Data.DataColumn
        Me.DataColumn2 = New System.Data.DataColumn
        Me.DataColumn3 = New System.Data.DataColumn
        Me.DataColumn4 = New System.Data.DataColumn
        Me.MyDA = New MySql.Data.MySqlClient.MySqlDataAdapter
        Me.SelCommand = New MySql.Data.MySqlClient.MySqlCommand
        Me.MyCn = New MySql.Data.MySqlClient.MySqlConnection
        Me.UpdCommand = New MySql.Data.MySqlClient.MySqlCommand
        CType(Me.DS, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Costs, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS
        '
        Me.DS.DataSetName = "NewDataSet"
        Me.DS.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS.Tables.AddRange(New System.Data.DataTable() {Me.Costs})
        '
        'Costs
        '
        Me.Costs.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn1, Me.DataColumn2, Me.DataColumn3, Me.DataColumn4})
        Me.Costs.TableName = "Costs"
        '
        'DataColumn1
        '
        Me.DataColumn1.ColumnName = "CostCode"
        Me.DataColumn1.DataType = GetType(System.Int32)
        '
        'DataColumn2
        '
        Me.DataColumn2.ColumnName = "CostName"
        '
        'DataColumn3
        '
        Me.DataColumn3.ColumnName = "BaseCost"
        Me.DataColumn3.DataType = GetType(System.Boolean)
        '
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "CostID"
        '
        'MyDA
        '
        Me.MyDA.DeleteCommand = Nothing
        Me.MyDA.InsertCommand = Nothing
        Me.MyDA.SelectCommand = Me.SelCommand
        Me.MyDA.UpdateCommand = Me.UpdCommand
        '
        'SelCommand
        '
        Me.SelCommand.CommandText = Nothing
        Me.SelCommand.CommandTimeout = 0
        Me.SelCommand.CommandType = System.Data.CommandType.Text
        Me.SelCommand.Connection = Me.MyCn
        Me.SelCommand.Transaction = Nothing
        Me.SelCommand.UpdatedRowSource = System.Data.UpdateRowSource.Both
        '
        'MyCn
        '
        Me.MyCn.ConnectionString = "Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;char" & _
        "set=utf8"
        '
        'UpdCommand
        '
        Me.UpdCommand.CommandText = Nothing
        Me.UpdCommand.CommandTimeout = 0
        Me.UpdCommand.CommandType = System.Data.CommandType.Text
        Me.UpdCommand.Connection = Me.MyCn
        Me.UpdCommand.Transaction = Nothing
        Me.UpdCommand.UpdatedRowSource = System.Data.UpdateRowSource.Both
        CType(Me.DS, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Costs, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub
    Protected WithEvents DS As System.Data.DataSet
    Protected WithEvents MyDA As MySql.Data.MySqlClient.MySqlDataAdapter
    Protected WithEvents MyCn As MySql.Data.MySqlClient.MySqlConnection
    Protected WithEvents SelCommand As MySql.Data.MySqlClient.MySqlCommand
    Protected WithEvents UpdCommand As MySql.Data.MySqlClient.MySqlCommand
    Protected WithEvents Costs As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents BaseCostRB As System.Web.UI.WebControls.RadioButton
    Protected WithEvents BCRB As System.Web.UI.WebControls.RadioButton


    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region
    Dim MyTrans As MySqlTransaction
    Dim PriceCode As Int32
    Dim MyReader As MySqlDataReader
    Dim Itm As DataGridItem



    'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load


    'End Sub

    Private Sub PostB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PostB.Click
        Dim StrHost, StrUser As String
        Dim i As Integer

        UpdateLB.Text = ""

        StrHost = HttpContext.Current.Request.UserHostAddress
        StrUser = HttpContext.Current.User.Identity.Name
        If Left(StrUser, 7) = "ANALIT\" Then
            StrUser = Mid(StrUser, 8)
        End If


        Try

            MyCn.Open()
            MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted)
        Catch err As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & err.Message & "]"
            Exit Sub
        End Try
        Try
            MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted)


            For Each Itm In CostsDG.Items
                For i = 0 To DS.Tables(0).Rows.Count - 1
                    If DS.Tables(0).Rows(i).Item("CostCode") = Itm.Cells(4).Text Then

                        If DS.Tables(0).Rows(i).Item("CostName") <> CType(Itm.Cells(0).FindControl("CostName"), TextBox).Text Then
                            DS.Tables(0).Rows(i).Item("CostName") = CType(Itm.Cells(0).FindControl("CostName"), TextBox).Text
                        End If

                        If DS.Tables(0).Rows(i).Item("CostCode") = Request.Form("uid").ToString Then
                            DS.Tables(0).Rows(i).Item("BaseCost") = True
                        Else
                            DS.Tables(0).Rows(i).Item("BaseCost") = False
                        End If



                    End If
                Next
            Next

            With UpdCommand
                .Parameters.Add(New MySqlParameter("OperatorName", MySqlDbType.String))
                .Parameters.Add(New MySqlParameter("OperatorHost", MySqlDbType.String))

                .Parameters.Add(New MySqlParameter("CostCode", MySqlDbType.Int32))
                .Parameters("CostCode").Direction = ParameterDirection.Input
                .Parameters("CostCode").SourceColumn = "CostCode"
                .Parameters("CostCode").SourceVersion = DataRowVersion.Current

                .Parameters.Add(New MySqlParameter("CostName", MySqlDbType.VarChar))
                .Parameters("CostName").Direction = ParameterDirection.Input
                .Parameters("CostName").SourceColumn = "CostName"
                .Parameters("CostName").SourceVersion = DataRowVersion.Current

                .Parameters.Add(New MySqlParameter("BaseCost", MySqlDbType.Bit))
                .Parameters("BaseCost").Direction = ParameterDirection.Input
                .Parameters("BaseCost").SourceColumn = "BaseCost"
                .Parameters("BaseCost").SourceVersion = DataRowVersion.Current

                .Parameters("OperatorHost").Value = StrHost
                .Parameters("OperatorName").Value = StrUser

                .CommandText = "Insert into logs.pricescostsupdate(LogTime, OperatorName, OperatorHost, CostCode, CostName, BaseCost)" & _
                                         " select now(), ?OperatorName, ?OperatorHost, ?CostCode, nullif(?CostName, CostName), nullif(?BaseCost, BaseCost)" & _
                                         " from (pricescosts) where costcode=?CostCode;"

                .CommandText &= " update pricescosts set BaseCost=?BaseCost, CostName=?CostName where CostCode=?CostCode;"
            End With

            MyDA.Update(DS, "Costs")

            MyTrans.Commit()
            CostsDG.DataBind()
            UpdateLB.Text = "Сохранено."
        Catch ex As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & ex.Message & "]"

            MyTrans.Rollback()
        Finally
            If MyCn.State = ConnectionState.Open Then MyCn.Close()
        End Try

    End Sub

    Public Function IsChecked(ByVal Checked As Boolean) As String
        If Checked Then
            Return "checked"
        Else
            Return ""
        End If
    End Function

    Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

        If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")

        PriceCode = Request("pc")

        PostDataToGrid()
       
    End Sub

    Protected Sub CreateCost_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles CreateCost.Click
        Dim Func As New AddUser.Func
        Dim FirmCode As Int32
        Dim ShortName, PriceName As String
        Try

            MyCn.Open()
            MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted)
        Catch err As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & err.Message & "]"
            Exit Sub
        End Try
        UpdCommand.Connection = MyCn
        UpdCommand.Transaction = MyTrans


        Try
            UpdCommand.CommandText = "select pd.FirmCode, pd.PriceName, cd.ShortName from pricesdata pd, clientsdata cd" & _
            " where cd.firmcode=pd.firmcode and pd.pricecode=" & PriceCode
            MyReader = UpdCommand.ExecuteReader
            MyReader.Read()
            FirmCode = MyReader("FirmCode")
            ShortName = MyReader("ShortName")
            PriceName = MyReader("PriceName")
            MyReader.Close()

            'Добавляем прайс в PricesData, FormRules, Sources
            UpdCommand.CommandText = "INSERT INTO pricesdata(Firmcode, PriceCode) values(" & FirmCode & ", null); " & _
    " set @NewPriceCode:=Last_Insert_ID(); insert into farm.formrules(firmcode) values(@NewPriceCode); " & _
    " insert into farm.sources(FirmCode) values(@NewPriceCode);"

            'Добавляем ценовую колонку
            UpdCommand.CommandText &= "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " & _
                                    " Select @NewPriceCode, @NewPriceCode, 0, " & PriceCode & ";" & _
                                    " Insert into farm.costformrules(PC_CostCode) Select @NewPriceCode;"

            UpdCommand.ExecuteNonQuery()

            MyTrans.Commit()
            Func.SelectTODS("select regionaladmins.username, regions.regioncode, regions.region, regionaladmins.alowcreateretail, regionaladmins.alowcreatevendor, regionaladmins.alowchangesegment, regionaladmins.defaultsegment, AlowCreateInvisible, regionaladmins.email  from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" & Session("UserName") & "' order by region", "admin", DS)

            Func.Mail("register@analit.net", """" & ShortName & """ - регистрация ценовой колонки", Mail.MailFormat.Text, _
                            "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                            & "Прайс-лист: " & PriceName & Chr(10) & Chr(13) _
                            , "RegisterList@subscribe.analit.net", DS.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)


            PostDataToGrid()
        Catch ex As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & ex.Message & "]"

            MyTrans.Rollback()
        Finally
            If MyCn.State = ConnectionState.Open Then MyCn.Close()
        End Try

    End Sub

    Private Sub PostDataToGrid()
        Try

            If MyCn.State = ConnectionState.Closed Then MyCn.Open()
            MyTrans = MyCn.BeginTransaction(IsolationLevel.ReadCommitted)
        Catch err As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & err.Message & "]"
            Exit Sub
        End Try


        Try
            MyTrans = MyCn.BeginTransaction

            SelCommand.CommandText = "select PriceName from (pricesdata) where PriceCode=" & PriceCode
            MyReader = SelCommand.ExecuteReader
            MyReader.Read()
            PriceNameLB.Text = MyReader.Item(0)
            MyReader.Close()

            SelCommand.CommandText = " SELECT CostCode, concat(ifnull(ExtrMask, ''), ' - ', if(FieldName='BaseCost', concat(TxtBegin, ' - ', TxtEnd), if(left(FieldName,1)='F'," & _
" concat('№', right(Fieldname, length(FieldName)-1)), Fieldname))) CostID, CostName, BaseCost, pc.Enabled, pc.AgencyEnabled" & _
" FROM (farm.costformrules cf, pricescosts pc, pricesdata pd)" & _
" left join farm.sources s on s.firmcode=pc.pricecode" & _
" where cf.pc_costcode=pc.costcode" & _
" and pd.pricecode=showpricecode" & _
" and ShowPriceCode=" & PriceCode

            MyDA.Fill(DS, "Costs")
            If Not IsPostBack Then
                CostsDG.DataBind()
            End If


            MyTrans.Commit()
        Catch ex As Exception
            ErrLB.Text = "Извините, доступ временно закрыт.Пожалуйста повторите попытку через несколько минут.[" & ex.Message & "]"

            MyTrans.Rollback()
        Finally
            If MyCn.State = ConnectionState.Open Then MyCn.Close()
        End Try
    End Sub
End Class
