'Imports MySql.Data.MySqlClient
Imports MySql.Data.MySqlClient
Imports ActiveDs
'Imports AddUser.Func
Imports System
'Imports System.Web.Mail
Imports System.IO
Imports System.Security.Permissions
'Imports System.Windows.Forms.Design
'Imports System.Windows.Forms


Namespace AddUser



Partial Class WebForm1
    Inherits System.Web.UI.Page

#Region "Компоненты webform1"
    Protected WithEvents Regions As System.Data.DataTable
    Protected WithEvents DataColumn1 As System.Data.DataColumn
    Protected WithEvents DataColumn2 As System.Data.DataColumn
    Protected WithEvents DS1 As System.Data.DataSet
    Protected WithEvents FreeCodes As System.Data.DataTable
    Protected WithEvents DataColumn3 As System.Data.DataColumn
    Protected WithEvents DataColumn4 As System.Data.DataColumn
    Protected WithEvents WorkReg As System.Data.DataTable
    Protected WithEvents DataColumn5 As System.Data.DataColumn
    Protected WithEvents DataColumn6 As System.Data.DataColumn
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
    Protected WithEvents OldCodeTB As System.Web.UI.WebControls.TextBox
    Protected WithEvents RequiredFieldValidator7 As System.Web.UI.WebControls.RequiredFieldValidator
    Protected WithEvents admin As System.Data.DataTable
    Protected WithEvents DataTable1 As System.Data.DataTable
    Protected WithEvents DataColumn22 As System.Data.DataColumn
    Protected WithEvents DataColumn23 As System.Data.DataColumn
    Protected WithEvents Incudes As System.Data.DataTable
    Protected WithEvents DataColumn24 As System.Data.DataColumn
    Protected WithEvents DataColumn25 As System.Data.DataColumn
    Protected WithEvents DataColumn26 As System.Data.DataColumn
#End Region
#Region "Web Form Designer Generated Code "
    'инициализация компонент
    'This call is required by the Web Form Designer.

    <System.Diagnostics.DebuggerStepThrough()> Private Sub InitializeComponent()
        Me.DS1 = New System.Data.DataSet
        Me.Regions = New System.Data.DataTable
        Me.DataColumn1 = New System.Data.DataColumn
        Me.DataColumn2 = New System.Data.DataColumn
        Me.FreeCodes = New System.Data.DataTable
        Me.DataColumn3 = New System.Data.DataColumn
        Me.DataColumn4 = New System.Data.DataColumn
        Me.WorkReg = New System.Data.DataTable
        Me.DataColumn5 = New System.Data.DataColumn
        Me.DataColumn6 = New System.Data.DataColumn
        Me.DataColumn7 = New System.Data.DataColumn
        Me.DataColumn8 = New System.Data.DataColumn
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
        Me.DataTable1 = New System.Data.DataTable
        Me.DataColumn22 = New System.Data.DataColumn
        Me.DataColumn23 = New System.Data.DataColumn
        Me.Incudes = New System.Data.DataTable
        Me.DataColumn24 = New System.Data.DataColumn
        Me.DataColumn25 = New System.Data.DataColumn
        Me.DataColumn26 = New System.Data.DataColumn
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.FreeCodes, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Clientsdata, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.admin, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.DataTable1, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.Incudes, System.ComponentModel.ISupportInitialize).BeginInit()
        '
        'DS1
        '
        Me.DS1.DataSetName = "NewDataSet"
        Me.DS1.Locale = New System.Globalization.CultureInfo("ru-RU")
        Me.DS1.Tables.AddRange(New System.Data.DataTable() {Me.Regions, Me.FreeCodes, Me.WorkReg, Me.Clientsdata, Me.admin, Me.DataTable1, Me.Incudes})
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
        'FreeCodes
        '
        Me.FreeCodes.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn3, Me.DataColumn4})
        Me.FreeCodes.TableName = "FreeCodes"
        '
        'DataColumn3
        '
        Me.DataColumn3.ColumnName = "FirmCode"
        Me.DataColumn3.DataType = GetType(System.Int32)
        '
        'DataColumn4
        '
        Me.DataColumn4.ColumnName = "ShortName"
        '
        'WorkReg
        '
        Me.WorkReg.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn5, Me.DataColumn6, Me.DataColumn7, Me.DataColumn8})
        Me.WorkReg.TableName = "WorkReg"
        '
        'DataColumn5
        '
        Me.DataColumn5.ColumnName = "RegionCode"
        Me.DataColumn5.DataType = GetType(System.Int32)
        '
        'DataColumn6
        '
        Me.DataColumn6.ColumnName = "Region"
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
        'DataTable1
        '
        Me.DataTable1.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn22, Me.DataColumn23})
        Me.DataTable1.TableName = "Payers"
        '
        'DataColumn22
        '
        Me.DataColumn22.ColumnName = "PayerID"
        Me.DataColumn22.DataType = GetType(System.Int32)
        '
        'DataColumn23
        '
        Me.DataColumn23.ColumnName = "PayerName"
        '
        'Incudes
        '
        Me.Incudes.Columns.AddRange(New System.Data.DataColumn() {Me.DataColumn24, Me.DataColumn25, Me.DataColumn26})
        Me.Incudes.TableName = "Includes"
        '
        'DataColumn24
        '
        Me.DataColumn24.ColumnName = "FirmCode"
        Me.DataColumn24.DataType = GetType(System.UInt32)
        '
        'DataColumn25
        '
        Me.DataColumn25.ColumnName = "ShortName"
        '
        'DataColumn26
        '
        Me.DataColumn26.ColumnName = "RegionCode"
        Me.DataColumn26.DataType = GetType(System.UInt64)
        CType(Me.DS1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Regions, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.FreeCodes, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.WorkReg, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Clientsdata, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.admin, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.DataTable1, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.Incudes, System.ComponentModel.ISupportInitialize).EndInit()

    End Sub

    Private Sub Page_Init(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Init
        'CODEGEN: This method call is required by the Web Form Designer
        'Do not modify it using the code editor.
        InitializeComponent()
    End Sub

#End Region

#Region "MySQL and AD"
    'Dim myMySqlConnection As New MySqlConnection("Data Source=prg2;Database=mainclients;User ID=root;Password=;Connect Timeout=300;Pooling=no;COMMAND LOGGING=false")
    Dim myMySqlConnection As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;Pooling=no")
    Dim myMySqlCommand As New MySqlCommand()
    Dim myMySqlDataReader As MySqlDataReader
    Dim myMySqlDataAdapter As New MySqlDataAdapter()
    'для проверки аккаунта в сети, используется при проверке учетного имени
    Dim ADUser As ActiveDs.IADsUser
    Dim ADComp As ActiveDs.IADsComputer
    Dim Domain As IADs
    Dim m_iCode As Int64
    Dim func As New Email()
    Dim SelectToDS As New func
    Dim ms_Command As New MySqlCommand
    Dim mytrans As MySqlTransaction

#End Region


    Dim Funct As New func
        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load, Me.Load

        'End Sub
    '******** InsertOldData *************
    'заполнить форму из базы, базируясь на FirmCode

    '******** SelectTODS *************
    'выполнить запрос, данные запроса будут внесены в DataSource DS1, в таблицу с имененм Table

    '******** SetWorkRegions *************
    'при выборе региона клиента обновляет 'Регионы работы' и 'Показываемые регионы'
    'выделяя те регионы, которые установлены как регионы по умолчанию в таблице regions
        Public Sub SetWorkRegions(ByVal RegCode As String)
            'SelectTODS("select a.RegionCode, a.Region,  a.defaultshowregionmask & " & RegCode & ">0 as ShowMask," & _
            '    "a.defaultregionmask & " & RegCode & ">0 as RegMask from mainclients.regions as a where a.regioncode>0 And a.defaultshowregionmask & " & RegCode & ">0 order by region", "WorkReg")
            SelectToDS.SelectTODS("select a.RegionCode, a.Region,  (b.defaultshowregionmask & " & RegCode.ToString & ")>0 as ShowMask," & _
                "a.regioncode=" & RegCode.ToString & " as RegMask from (farm.regions as a, farm.regions as b) where b.regioncode=" & RegCode.ToString & " and a.regioncode & b.defaultshowregionmask>0 order by region", "WorkReg", DS1)
            'If Not IsPostBack Then
            WRList.DataBind()
            WRList2.DataBind()
            OrderList.DataBind()
            ShowList.DataBind()
            'End If
            'SelectTODS("select a.RegionCode, a.Region,  a.defaultshowregionmask & " & RegCode & ">0 as ShowMask," & _
            '    "a.defaultregionmask & " & RegCode & ">0 as RegMask from mainclients.regions as a where a.regioncode>0 And a.defaultshowregionmask & " & RegCode & ">0 order by region", "WorkReg")
            'If Not IsPostBack Then
            'End If
            Dim i As Int32

            For i = 0 To WRList.Items.Count - 1
                WRList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("regmask")
                OrderList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("regmask")
                WRList2.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("regmask")
                ShowList.Items(i).Selected = DS1.Tables("Workreg").Rows(i).Item("showmask")
            Next
        End Sub
    '******** RegionDD_SelectedIndexChanged *************
    'если выбран другой регион то...    
    Private Sub RegionDD_SelectedIndexChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles RegionDD.SelectedIndexChanged
        SetWorkRegions(RegionDD.SelectedItem.Value)
    End Sub
    '******** FreeCodeDD_SelectedIndexChanged *************
    'если просмотр нулевых записей включен, то при выборе одного из клиентов
    'в форму заносится информация о нем из базы

    '******** OldCodeCB_CheckedChanged *************
    'если просмотр нулевых записей включен, занести информацию о выбраном клиенте в базу

    '******** Button1_Click *************
    'регистрация нового клиента
    'обновляются три таблицы clientsdata, retclientsset, osuseraccessright
    'если все таблицы заполнены успешно, то завершить транзакцию и показать карточку клиента, 
    'данные передаются используя переменные strClients, strLogin и strPassword
    'to do надо добавить обновление ActiveDirectories

    '******** LoginTB_TextChanged *************
    'проверка учетного имени, если уже существует то генерируется сообщение об ошибке,
    'если нет, то происходит автоматическая генерация пароля (все буквы и цифры за исключением I и l)
    'при автоматической проверке логина при изменении содержания поля Учетная запись иногда происходит медленно 
    'из-за этого пользователь может продолжить заполнение полей не заметив что Учетная запись еще не проверена
    'в результате все что введено в это время будет удалено
    'Чтобы избежать этого, проверка логина осуществляется только после нажатия кнопки зарегестрировать
    'Private Sub LoginTB_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles LoginTB.TextChanged
    Private Function CheckLogin()
            Dim rc, rc1 As Single
            'Dim pwd, PassStr As String

        Label2.Text = ""
        'Label1.ForeColor = Color.Black
        myMySqlConnection.Open()
            myMySqlDataReader = New MySqlCommand("select Max(osusername='" & LoginTB.Text & "') as Present from (osuseraccessright)", myMySqlConnection).ExecuteReader
        myMySqlDataReader.Read()
        If myMySqlDataReader.Read() Then
            rc = CInt(myMySqlDataReader.Item(0))
        Else
            rc = 0
        End If
        myMySqlDataReader.Close()
        myMySqlConnection.Close()
        Try
            'проверим имя в AD
            ADUser = GetObject("WinNT://adc.analit.net/" & LoginTB.Text)
            rc1 = 1
        Catch
            rc1 = 0
        End Try
        ADUser = Nothing

        If rc > 0 Or rc1 > 0 Then
            'Label1.ForeColor = Color.Red
            Label2.Text = "Учетное имя '" & LoginTB.Text & "' существует в системе."
            LoginTB.Text = ""
            CheckLogin = -1
            Exit Function
        End If

        'Dim Func As New Func()
        PassTB.Text = Funct.GeneratePassword
        CheckLogin = 0
    End Function





    Private Sub Register_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Register.Click


        'проверка учетной записи
        If CInt(CheckLogin()) = -1 And Not IncludeCB.Checked Then
            Label3.Text = "Ошибка в Учетном имени!"
            Exit Sub
        End If


        myMySqlConnection.Open()
        mytrans = myMySqlConnection.BeginTransaction

        'calculate region
        Dim MaskRegion, ShowRegionMask, WorkMask, OrderMask, i As Int64

        For i = 0 To ShowList.Items.Count - 1
            If ShowList.Items(i).Selected Then ShowRegionMask += ShowList.Items(i).Value
            If WRList.Items(i).Selected Then MaskRegion += WRList.Items(i).Value
            If WRList2.Items(i).Selected Then WorkMask += WRList2.Items(i).Value
            If OrderList.Items(i).Selected Then OrderMask += OrderList.Items(i).Value
        Next

        'fillin parametrs
        ms_Command.Transaction = mytrans
        With ms_Command
            'clientsdata
            .Parameters.Add(New MySqlParameter("MaskRegion", MySqlDbType.Int64))
            .Parameters("MaskRegion").Value = MaskRegion

            .Parameters.Add(New MySqlParameter("OrderMask", MySqlDbType.Int64))
            .Parameters("OrderMask").Value = OrderMask

            .Parameters.Add(New MySqlParameter("ShowRegionMask", MySqlDbType.Int64))
            .Parameters("ShowRegionMask").Value = ShowRegionMask

            .Parameters.Add(New MySqlParameter("WorkMask", MySqlDbType.Int64))
            .Parameters("WorkMask").Value = WorkMask

            .Parameters.Add(New MySqlParameter("fullname", MySqlDbType.String))
            .Parameters("fullname").Value = FullNameTB.Text

            .Parameters.Add(New MySqlParameter("shortname", MySqlDbType.String))
            .Parameters("shortname").Value = ShortNameTB.Text

            .Parameters.Add(New MySqlParameter("BeforeNamePrefix", MySqlDbType.String))
            .Parameters("BeforeNamePrefix").Value = ""
            If TypeDD.SelectedItem.Value = 1 Then .Parameters("BeforeNamePrefix").Value = "Аптека"

            .Parameters.Add(New MySqlParameter("phone", MySqlDbType.String))
            .Parameters("phone").Value = PhoneTB.Text

            .Parameters.Add(New MySqlParameter("fax", MySqlDbType.String))
            .Parameters("fax").Value = FaxTB.Text

            .Parameters.Add(New MySqlParameter("url", MySqlDbType.String))
            .Parameters("url").Value = URLTB.Text

            .Parameters.Add(New MySqlParameter("firmsegment", MySqlDbType.Int24))
            .Parameters("firmsegment").Value = SegmentDD.SelectedItem.Value

            .Parameters.Add(New MySqlParameter("RegionCode", MySqlDbType.Int24))
            .Parameters("RegionCode").Value = RegionDD.SelectedItem.Value

            .Parameters.Add(New MySqlParameter("adress", MySqlDbType.String))
            .Parameters("adress").Value = AddressTB.Text

            .Parameters.Add(New MySqlParameter("bussinfo", MySqlDbType.String))
            .Parameters("bussinfo").Value = BusInfoTB.Text

            .Parameters.Add(New MySqlParameter("bussstop", MySqlDbType.String))
            .Parameters("bussstop").Value = BussStopTB.Text

            .Parameters.Add(New MySqlParameter("firmtype", MySqlDbType.Int24))
            .Parameters("firmtype").Value = TypeDD.SelectedItem.Value

            .Parameters.Add(New MySqlParameter("registrant", MySqlDbType.String))
            .Parameters("registrant").Value = Session("UserName")

            .Parameters.Add(New MySqlParameter("mail", MySqlDbType.String))
            .Parameters("mail").Value = EmailTB.Text

            .Parameters.Add(New MySqlParameter("InvisibleOnFirm", MySqlDbType.Byte))
            .Parameters("InvisibleOnFirm").Value = 0

            .Parameters.Add(New MySqlParameter("OrderManagerName", MySqlDbType.String))
            .Parameters("OrderManagerName").Value = TBOrderManagerName.Text

            .Parameters.Add(New MySqlParameter("OrderManagerPhone", MySqlDbType.String))
            .Parameters("OrderManagerPhone").Value = TBOrderManagerPhone.Text

            .Parameters.Add(New MySqlParameter("OrderManagerMail", MySqlDbType.String))
            .Parameters("OrderManagerMail").Value = TBOrderManagerMail.Text

            .Parameters.Add(New MySqlParameter("ClientManagerName", MySqlDbType.String))
            .Parameters("ClientManagerName").Value = TBClientManagerName.Text

            .Parameters.Add(New MySqlParameter("ClientManagerPhone", MySqlDbType.String))
            .Parameters("ClientManagerPhone").Value = TBClientManagerPhone.Text

            .Parameters.Add(New MySqlParameter("ClientManagerMail", MySqlDbType.String))
            .Parameters("ClientManagerMail").Value = TBClientManagerMail.Text

            .Parameters.Add(New MySqlParameter("AccountantName", MySqlDbType.String))
            .Parameters("AccountantName").Value = TBAccountantName.Text

            .Parameters.Add(New MySqlParameter("AccountantPhone", MySqlDbType.String))
            .Parameters("AccountantPhone").Value = TBAccountantPhone.Text

            .Parameters.Add(New MySqlParameter("AccountantMail", MySqlDbType.String))
            .Parameters("AccountantMail").Value = TBAccountantMail.Text

            'osuseraccessright
            .Parameters.Add(New MySqlParameter("ClientCode", MySqlDbType.Int24))
            '.Parameters("@ClientCode").Value = LoginTB.Text
            .Parameters.Add(New MySqlParameter("AllowGetData", MySqlDbType.Int24))
            .Parameters("AllowGetData").Value = TypeDD.SelectedItem.Value
            .Parameters.Add(New MySqlParameter("OSUserName", MySqlDbType.String))
            .Parameters("OSUserName").Value = LoginTB.Text
            .Parameters.Add(New MySqlParameter("OSUserPass", MySqlDbType.String))
            .Parameters("OSUserPass").Value = PassTB.Text

            'retclientsset

            .Connection = myMySqlConnection
        End With

        If InvCB.Checked Then ms_Command.Parameters("invisibleonfirm").Value = 1

        'внесем даные в базу
        Try
            Label3.Text = ""

            'Создаем (или присваиваем) код биллинга, записываем в сессию 
            If IncludeCB.Checked Then
                'Это не совсем так
                myMySqlDataReader = New MySqlCommand("select billingcode from clientsdata where firmcode=" & IncludeSDD.SelectedValue, myMySqlConnection).ExecuteReader
                If myMySqlDataReader.Read() Then
                    Session("DogN") = CInt(myMySqlDataReader.Item(0).ToString)
                End If
                If Not myMySqlDataReader.IsClosed Then myMySqlDataReader.Close()
            Else
                If Not PayerPresentCB.Checked Then
                    Session("DogN") = CreateClientOnBilling()
                Else
                    Session("DogN") = PayerDDL.SelectedItem.Value
                End If
            End If

            'Создаем Клиента в ClientsData, записываем в сессию для рег. карты
            ms_Command.Parameters("ClientCode").Value = CreateClientOnClientsData()
            Session("Code") = ms_Command.Parameters("ClientCode").Value

            If IncludeCB.Checked Then
                'Создаем записи в IncludeRegulation и ShowRegulation
                CreateClientOnShowInclude(IncludeSDD.SelectedValue)
            Else
                'Создаем Клиента в OSUserAccessRight
                CreateClientOnOSUserAccessRight()
            End If


            'Записываем ЛОГ
            LogRegister()

            If TypeDD.SelectedItem.Value = 1 Then
                'Если тип клиента=1(Аптека), то вставляем записи в RetClientsSet, Intersection, Inscribe(если клиент не невидимый)
                CreateClientOnRCS_and_I(InvCB.Checked)
            Else
                'Если тип клиента=0(Поставщик), то вставляем записи в PricesData, FormRules, Sources, RegionalData, PricesRegionalData, Intersection
                CreatePriceRecords()
            End If

            'Создаем пользователя в AD
            ' Try
            If Not IncludeCB.Checked Then
                Domain = GetObject("LDAP://OU=Пользователи,OU=Клиенты,DC=adc,DC=analit,DC=net")
                ADUser = Domain.Create("user", "cn=" & ms_Command.Parameters("OSUserName").Value)
                ADUser.Put("samAccountName", ms_Command.Parameters("OSUserName").Value)
                'ADUser.AccountExpirationDate = Now().AddDays(14)
                ADUser.SetInfo()
                ADUser = Nothing
                ADUser = GetObject("WinNT://adc.analit.net/" & ms_Command.Parameters("OSUserName").Value)
                ADUser.SetPassword(ms_Command.Parameters("OSUserPass").Value)
                ADUser.SetInfo()
                Dim fl As Int32 = 66049
                ADUser.Put("userFlags", fl)
                ADUser.Description = ms_Command.Parameters("ClientCode").Value
                ADUser.AccountDisabled = False
                    ADUser.LoginWorkstations = "ISRV"
                Dim grp As ActiveDs.IADsGroup
                grp = GetObject("WinNT://adc.analit.net/Базовая группа клиентов - получателей данных")
                grp.Add("WinNT://adc.analit.net/" & ms_Command.Parameters("OSUserName").Value)
                ADUser.SetInfo()
                ADUser = Nothing
            End If
            'Пользователь создан успешно.
            'Для испытаний!!!
            ' mytrans.Rollback()
            mytrans.Commit()

            Session("strStatus") = "Yes"

            'Уведомляем поставщиков
#If Not DEBUG Then

            Try

                If Not (InvCB.Checked Or TypeDD.SelectedItem.Value = 0) Then
                    If Funct.SelectTODS("SELECT ClientManagerName, ClientManagerMail" & _
    " FROM clientsdata" & _
    " where MaskRegion & " & RegionDD.SelectedItem.Value & ">0" & _
    " and firmsegment=0" & _
    " and LENGTH(ClientManagerMail)>0" & _
    " and firmtype=0" & _
    " and firmstatus=1" & _
    " and firmcode in (select pd.FirmCode from" & _
    " pricesdata as pd, pricesregionaldata as prd where regioncode=" & RegionDD.SelectedItem.Value & " and pd.enabled=1 and prd.enabled=1)" & _
    " group by ClientManagerMail", "FirmEmail", DS1) Then
                        Dim Row As DataRow
                        For Each Row In DS1.Tables("FirmEmail").Rows
                                Funct.Mail("Аналитическая Компания Инфорум <pharm@analit.net>", "Новый клиент в системе ""АналитФАРМАЦИЯ""", Mail.MailFormat.Text, "Добрый день." & Chr(10) & Chr(13) & Chr(10) & Chr(13) & _
                                "В информационной системе ""АналитФАРМАЦИЯ"", участником которой является Ваша организация, зарегистрирован новый клиент: " & ShortNameTB.Text & " в регионе(городе) " & RegionDD.SelectedItem.Text & "." & Chr(10) & Chr(13) & "Пожалуйста произведите настройки для данного клиента (Раздел ""Для зарегистрированных пользователей"" на сайте www.analit.net)." & _
                                Chr(10) & Chr(13) & Chr(10) & Chr(13) & "С уважением," & Chr(10) & Chr(13) & "Аналитическая компания ""Инфорум"", г. Воронеж" & _
                                Chr(10) & Chr(13) & "4732-206000", """" & Row.Item(0) & """<" & Row.Item(1) & ">", Nothing, System.Text.Encoding.GetEncoding(1251))
                        Next
                        Funct.Mail("register@analit.net", """Debug: " & FullNameTB.Text & """ - Уведомления поставщиков", Mail.MailFormat.Text, _
                                                                   "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                                                                   & "Регион: " & RegionDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                                                   & "Login: " & LoginTB.Text & Chr(10) & Chr(13) _
                                                                   & "Код: " & Session("Code") & Chr(10) & Chr(13) & Chr(10) & Chr(13) _
                                                                   & "Сегмент: " & SegmentDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                                                   & "Тип: " & TypeDD.SelectedItem.Text _
                                                                    & "О Регистрации уведомлено поставщиков: " & DS1.Tables("FirmEmail").Rows.Count - 1 _
                                                                   , "RegisterList@subscribe.analit.net", DS1.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)

                    End If
                Else
                    Funct.Mail("register@analit.net", """" & FullNameTB.Text & """ - ошибка уведомления поставщиков", Mail.MailFormat.Text, _
                                           "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                                           & "Регион: " & RegionDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                           & "Login: " & LoginTB.Text & Chr(10) & Chr(13) _
                                           & "Код: " & Session("Code") & Chr(10) & Chr(13) & Chr(10) & Chr(13) _
                                           & "Сегмент: " & SegmentDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                           & "Тип: " & TypeDD.SelectedItem.Text _
                                            & "Ошибка:  Ничего не получилось выбрать из базы" _
                                           , "RegisterList@subscribe.analit.net", DS1.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)

                End If

            Catch err As Exception
                Funct.Mail("register@analit.net", """" & FullNameTB.Text & """ - ошибка уведомления поставщиков", Mail.MailFormat.Text, _
                          "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                          & "Регион: " & RegionDD.SelectedItem.Text & Chr(10) & Chr(13) _
                          & "Login: " & LoginTB.Text & Chr(10) & Chr(13) _
                          & "Код: " & Session("Code") & Chr(10) & Chr(13) & Chr(10) & Chr(13) _
                          & "Сегмент: " & SegmentDD.SelectedItem.Text & Chr(10) & Chr(13) _
                          & "Тип: " & TypeDD.SelectedItem.Text _
                           & "Ошибка: " & err.Source & ": " & err.Message _
                          , "RegisterList@subscribe.analit.net", DS1.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)

            End Try
            Try
                'Письмо в рассылку о регистрации

                Funct.Mail("register@analit.net", """" & FullNameTB.Text & """ - успешная регистрация", Mail.MailFormat.Text, _
                "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                & "Регион: " & RegionDD.SelectedItem.Text & Chr(10) & Chr(13) _
                & "Login: " & LoginTB.Text & Chr(10) & Chr(13) _
                & "Код: " & Session("Code") & Chr(10) & Chr(13) & Chr(10) & Chr(13) _
                & "Сегмент: " & SegmentDD.SelectedItem.Text & Chr(10) & Chr(13) _
                & "Тип: " & TypeDD.SelectedItem.Text _
                , "RegisterList@subscribe.analit.net", DS1.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)

                'Подписка клиента на тематические рассылки

                Funct.Mail("""" & FullNameTB.Text & """ <" & EmailTB.Text & ">", "Sub", Mail.MailFormat.Text, "", "FirmEmailList-on@subscribe.analit.net", Nothing, System.Text.Encoding.UTF8)

                If Not TBClientManagerMail.Text = "" Then
                    Funct.Mail("""" & TBClientManagerName.Text & """ <" & TBClientManagerMail.Text & ">", "Sub", Mail.MailFormat.Text, "", "ClientManagerList-on@subscribe.analit.net", Nothing, System.Text.Encoding.UTF8)
                End If

                If Not TBOrderManagerMail.Text = "" Then
                    Funct.Mail("""" & TBOrderManagerName.Text & """ <" & TBOrderManagerMail.Text & ">", "Sub", Mail.MailFormat.Text, "", "OrderManagerList-on@subscribe.analit.net", Nothing, System.Text.Encoding.UTF8)
                End If

                If Not TBAccountantMail.Text = "" Then
                    Funct.Mail("""" & TBAccountantName.Text & """ <" & TBAccountantMail.Text & ">", "Sub", Mail.MailFormat.Text, "", "AccountantList-on@subscribe.analit.net", Nothing, System.Text.Encoding.UTF8)
                End If
            Catch err As Exception
                Funct.Mail("register@analit.net", """" & FullNameTB.Text & """ - ошибка подписки поставщиков", Mail.MailFormat.Text, _
                                         "Оператор: " & Session("UserName") & Chr(10) & Chr(13) _
                                         & "Регион: " & RegionDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                         & "Login: " & LoginTB.Text & Chr(10) & Chr(13) _
                                         & "Код: " & Session("Code") & Chr(10) & Chr(13) & Chr(10) & Chr(13) _
                                         & "Сегмент: " & SegmentDD.SelectedItem.Text & Chr(10) & Chr(13) _
                                         & "Тип: " & TypeDD.SelectedItem.Text _
                                          & "Ошибка: " & err.Source & ": " & err.Message _
                                         , "RegisterList@subscribe.analit.net", DS1.Tables("admin").Rows(0).Item("email"), System.Text.Encoding.UTF8)


            End Try

#End If



            Session("Name") = FullNameTB.Text
            Session("ShortName") = ShortNameTB.Text
            Session("Login") = LoginTB.Text
            Session("Password") = PassTB.Text
            Session("Tariff") = TypeDD.SelectedItem.Text


            Session("Register") = True
            If IncludeCB.Checked Then
                Page.Controls.Clear()
                Dim LB As New Label
                LB.Text = "Регистрация завершена успешно."
                LB.Font.Name = "Verdana"
                Page.Controls.Add(LB)
            Else
                Response.Redirect("report.aspx")
            End If
            Catch excL As Exception
                If Not TypeOf (excL) Is System.Threading.ThreadAbortException Then
                    If Not myMySqlDataReader.IsClosed Then myMySqlDataReader.Close()
                    Label3.Text = "Ошибка при регистрации клиента: " & excL.Message
                    mytrans.Rollback()
                End If
        Finally
            If Not myMySqlDataReader.IsClosed Then myMySqlDataReader.Close()
            If myMySqlConnection.State = ConnectionState.Open Then myMySqlConnection.Close()
            ms_Command.Dispose()
            myMySqlConnection.Dispose()
        End Try

    End Sub



    Private Sub PayerPresentCB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles PayerPresentCB.CheckedChanged
        If PayerPresentCB.Checked Then
            'PayerDDL.Visible = True
            PayerPresentCB.Text = "Плательщик существует:"
            PayerFTB.Visible = True
            FindPayerB.Visible = True
        Else
            PayerPresentCB.Text = "Плательщик существует"
            PayerDDL.Visible = False
            PayerFTB.Visible = False
            FindPayerB.Visible = False
            PayerCountLB.Visible = False
        End If
    End Sub

    Private Sub FindPayerB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles FindPayerB.Click
        SelectToDS.SelectTODS(" SELECT distinct PayerID, concat(PayerID, '. ', p.ShortName) PayerName" & _
                                   " FROM clientsdata as cd, accessright.showright, billing.payers p " & _
                                   " where  p.payerid=cd.billingcode and cd.regioncode  & showright.regionmask > 0  " & _
                                   " and showright.UserName='" & Session("UserName") & "' and FirmType=if(ShowRet+ShowOpt=2, FirmType, if(ShowRet=1, 1, 0))  " & _
                                   " and if(UseRegistrant=1, Registrant='" & Session("UserName") & "', 1=1) " & _
                                     " and firmstatus=1 and billingstatus=1 " & _
                                    " and p.ShortName like '%" & PayerFTB.Text & "%' " & _
                                   " order by p.shortname", "Payers", DS1)
        PayerDDL.DataBind()
        PayerCountLB.Text = "[" & PayerDDL.Items.Count & "]"
        PayerCountLB.Visible = True
        If PayerDDL.Items.Count > 0 Then
            PayerDDL.Visible = True
            PayerFTB.Visible = False
            FindPayerB.Visible = False
        End If

    End Sub

    Private Sub IncludeCB_CheckedChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles IncludeCB.CheckedChanged
        PayerPresentCB.Text = "Плательщик существует"
        PayerPresentCB.Visible = True
        PayerPresentCB.Checked = False

        If IncludeCB.Checked Then
            IncludeCB.Text = "Подчинен клиенту:"
            PayerPresentCB.Visible = False
            PayerFTB.Visible = False
            FindPayerB.Visible = False

            LoginTB.Enabled = False
            Requiredfieldvalidator4.Enabled = False
            RegionDD.Enabled = False
            TypeDD.Enabled = False
            SegmentDD.Enabled = False
            InvCB.Enabled = False
            ShowList.Enabled = False
            WRList.Enabled = False
            WRList2.Enabled = False

            PayerDDL.Visible = False
            PayerCountLB.Visible = False
            IncludeSTB.Visible = True
            IncludeSB.Visible = True
        Else
            LoginTB.Enabled = True
            Requiredfieldvalidator4.Enabled = True
            RegionDD.Enabled = True
            TypeDD.Enabled = True
            SegmentDD.Enabled = True
            InvCB.Enabled = True
            ShowList.Enabled = True
            WRList.Enabled = True
            WRList2.Enabled = True

            IncludeCB.Text = "Подчиненный клиент"
            IncludeSTB.Visible = False
            IncludeSB.Visible = False
            IncludeSDD.Visible = False
            IncludeCountLB.Visible = False
        End If
    End Sub


    Private Sub IncludeSB_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles IncludeSB.Click
            SelectToDS.SelectTODS(" SELECT distinct cd.FirmCode, concat(cd.FirmCode, '. ', cd.ShortName) ShortName, cd.RegionCode" & _
                                    " FROM  (accessright.showright, clientsdata as cd)" & _
                                    " left join includeregulation  ir on ir.includeclientcode=cd.firmcode" & _
                                    " where cd.regioncode  & showright.regionmask > 0" & _
                                    " and showright.UserName='" & Session("UserName") & "'" & _
                                    " and FirmType=if(ShowRet+ShowOpt=2, FirmType, if(ShowRet=1, 1, 0)) " & _
                                    " and if(UseRegistrant=1, Registrant='" & Session("UserName") & "', 1=1)" & _
                                    " and cd.ShortName like '%" & IncludeSTB.Text & "%' " & _
                                    " and FirmStatus=1" & _
                                    " and billingstatus=1" & _
                                    " And FirmType=1" & _
                                    " and ir.primaryclientcode is null" & _
                                    " order by cd.shortname", "Includes", DS1)
        IncludeSDD.DataBind()
        IncludeCountLB.Text = "[" & IncludeSDD.Items.Count & "]"
        IncludeCountLB.Visible = True

        'Заполняем значения регионов
        RegionDD.SelectedValue = DS1.Tables("Includes").Rows(0).Item("RegionCode").ToString
        SetWorkRegions(RegionDD.SelectedItem.Value)

        If IncludeSDD.Items.Count > 0 Then
            IncludeSDD.Visible = True
            IncludeSTB.Visible = False
            IncludeSB.Visible = False
        End If
    End Sub

    Private Function CreateClientOnBilling() As Integer
        ms_Command.CommandText = "insert into billing.payers(OldTariff, OldPayDate, Comment, PayerID, ShortName, BeforeNamePrefix) values(0, now(), 'Дата регистрации: " & Now() & "', null, ?ShortName, ?BeforeNamePrefix); "
        ms_Command.CommandText &= "SELECT LAST_INSERT_ID()"
        CreateClientOnBilling = ms_Command.ExecuteScalar()
    End Function
    Private Function CreateClientOnClientsData() As Integer
            ms_Command.CommandText = "INSERT INTO usersettings.clientsdata (regionmask, MaskRegion, ShowRegionMask, FullName, ShortName, Phone, Fax, URL, FirmSegment, RegionCode, Adress, BussInfo, BussStop, FirmType, Mail, OrderManagerName, OrderManagerPhone, OrderManagerMail, ClientManagerName, ClientManagerPhone, ClientManagerMail, AccountantName, AccountantPhone, AccountantMail, FirmStatus, registrant, BillingCode, BillingStatus) "
        If Not IncludeCB.Checked Then
            ms_Command.CommandText &= " Values(0, ?maskregion, ?ShowRegionMask, ?FullName, ?ShortName, ?Phone, ?Fax, ?URL, ?FirmSegment, ?RegionCode, ?Adress, ?BussInfo, ?BussStop, ?FirmType, ?Mail, ?OrderManagerName, ?OrderManagerPhone, ?OrderManagerMail, ?ClientManagerName, ?ClientManagerPhone, ?ClientManagerMail, ?AccountantName, ?AccountantPhone, ?AccountantMail, 1, ?registrant, " & Session("DogN") & ", 1); "
        Else
            ms_Command.CommandText &= " select 0, maskregion, ShowRegionMask, ?FullName, ?ShortName, ?Phone, ?Fax, ?URL, FirmSegment, RegionCode, ?Adress, ?BussInfo, ?BussStop, FirmType, ?Mail, ?OrderManagerName, ?OrderManagerPhone, ?OrderManagerMail, ?ClientManagerName, ?ClientManagerPhone, ?ClientManagerMail, ?AccountantName, ?AccountantPhone, ?AccountantMail, 1, ?registrant, BillingCode, BillingStatus" & _
                                        " from usersettings.clientsdata where firmcode=" & IncludeSDD.SelectedValue & "; "
        End If
        ms_Command.CommandText &= "SELECT LAST_INSERT_ID()"
        CreateClientOnClientsData = ms_Command.ExecuteScalar()
    End Function
        Private Sub CreateClientOnOSUserAccessRight()
            ms_Command.CommandText = "INSERT INTO usersettings.osuseraccessright (ClientCode, AllowGetData, OSUserName) Values(?ClientCode, ?AllowGetData, ?OSUserName)"
            ms_Command.ExecuteNonQuery()
        End Sub
        Private Sub LogRegister()
            ms_Command.CommandText = " insert into logs.register select null, now(), ?registrant, '" & HttpContext.Current.Request.UserHostAddress & "', ?ClientCode, ?FullName, ?FirmSegment, ?FirmType"
            ms_Command.ExecuteNonQuery()
        End Sub

        Private Sub CreateClientOnRCS_and_I(ByVal Invisible As Boolean)
            'Добавляем в RetClientsSet
            ms_Command.CommandText = "INSERT INTO usersettings.retclientsset (ClientCode, InvisibleOnFirm, WorkRegionMask, OrderRegionMask) Values(?ClientCode, ?InvisibleOnFirm, ?WorkMask, ?OrderMask); "

            'Добавляем в Intersection(новый механизм, с ценами)
            ms_Command.CommandText &= " INSERT " & _
" INTO intersection" & _
"     (" & _
"         ClientCode, " & _
"         regioncode, " & _
"         pricecode, " & _
"         InvisibleonFirm, " & _
"         costcode" & _
"     ) " & _
" SELECT DISTINCT " & _
"     clientsdata2.firmcode, " & _
"     regions.regioncode, " & _
"     pc.showpricecode, " & _
"     a.invisibleonfirm, " & _
"     (" & _
"     SELECT " & _
"         costcode " & _
"     FROM pricescosts pcc " & _
"     WHERE basecost " & _
"         AND showpricecode=pc.showpricecode" & _
"     ) " & _
" FROM (clientsdata, " & _
"     farm.regions, " & _
"     pricescosts pc, " & _
"     pricesdata) " & _
" LEFT JOIN " & _
"     clientsdata AS clientsdata2 " & _
"     ON clientsdata2.firmcode=?ClientCode " & _
" LEFT JOIN " & _
"     intersection " & _
"     ON intersection.pricecode=pc.showpricecode " & _
"     AND intersection.regioncode=regions.regioncode " & _
"     AND intersection.clientcode=clientsdata2.firmcode " & _
" LEFT JOIN " & _
"     retclientsset AS a " & _
"     ON a.clientcode=clientsdata2.firmcode " & _
" WHERE intersection.pricecode IS NULL " & _
"     AND clientsdata.firmstatus=1 " & _
"     AND clientsdata.firmsegment=clientsdata2.firmsegment " & _
"     AND clientsdata.firmtype=0 " & _
"     AND pricesdata.firmcode=clientsdata.firmcode " & _
"     AND pricesdata.pricecode=pc.showpricecode " & _
"     AND " & _
"     ( " & _
"         clientsdata.maskregion & regions.regioncode " & _
"     ) " & _
"     >0 " & _
"     AND " & _
"     ( " & _
"         clientsdata2.maskregion & regions.regioncode " & _
"     ) " & _
"     >0;"

            If Not Invisible Then ms_Command.CommandText &= " insert into inscribe(ClientCode) values(?ClientCode); "

            ms_Command.ExecuteNonQuery()
        End Sub

        Private Sub CreatePriceRecords()
            'Добавляем прайс в PricesData, FormRules, Sources
            ms_Command.CommandText = "INSERT INTO pricesdata(Firmcode, PriceCode) values(?ClientCode, null); " & _
    " set @NewPriceCode:=Last_Insert_ID(); insert into farm.formrules(firmcode) values(@NewPriceCode); " & _
    " insert into farm.sources(FirmCode) values(@NewPriceCode);"

            'Добавляем ценовую колонку
            ms_Command.CommandText &= "Insert into PricesCosts(CostCode, PriceCode, BaseCost, ShowPriceCode) " & _
                                    " Select @NewPriceCode, @NewPriceCode, 1, @NewPriceCode;" & _
                                    " Insert into farm.costformrules(PC_CostCode) Select @NewPriceCode;"

            'Добавляем фирму в RegionalData
            ms_Command.CommandText &= " insert into regionaldata(regioncode, firmcode)" & _
           " SELECT distinct regions.regioncode, clientsdata.firmcode" & _
          " FROM (clientsdata, farm.regions, pricesdata)" & _
          " left join regionaldata on regionaldata.firmcode=clientsdata.firmcode and regionaldata.regioncode= regions.regioncode" & _
          " where pricesdata.firmcode=clientsdata.firmcode" & _
          " and clientsdata.firmcode=?ClientCode" & _
          " and (clientsdata.maskregion & regions.regioncode)>0" & _
          " and regionaldata.firmcode is null;"

            'Добавляем прайсы в PricesRegionalData
            ms_Command.CommandText &= " insert into pricesregionaldata(regioncode, pricecode)" & _
         " SELECT distinct regions.regioncode, pricesdata.pricecode" & _
         " FROM (clientsdata, farm.regions, pricesdata, clientsdata as a)" & _
         " left join pricesregionaldata on pricesregionaldata.pricecode=pricesdata.pricecode and pricesregionaldata.regioncode= regions.regioncode" & _
         " where pricesdata.firmcode=clientsdata.firmcode" & _
         " and clientsdata.firmcode=?ClientCode" & _
         " and (clientsdata.maskregion & regions.regioncode)>0" & _
         " and pricesregionaldata.pricecode is null;"

            'Добавляем пересечения прайсов с клиентами в Intersection
            ms_Command.CommandText &= " insert into intersection(clientcode, regioncode, pricecode, invisibleonclient, InvisibleonFirm, CostCode) " & _
            " SELECT distinct clientsdata2.firmcode, regions.regioncode, pricesdata.pricecode," & _
            " 0 as invisibleonclient, a.invisibleonfirm, pricesdata.pricecode" & _
            " FROM (clientsdata, farm.regions, pricesdata, pricesregionaldata, clientsdata as clientsdata2, retclientsset as a)" & _
            " LEFT JOIN intersection ON intersection.pricecode=pricesdata.pricecode and  intersection.regioncode=regions.regioncode  and  intersection.clientcode=clientsdata2.firmcode" & _
            " WHERE intersection.pricecode IS NULL and" & _
            " clientsdata.firmstatus=1 and clientsdata.firmsegment=clientsdata2.firmsegment" & _
            " and clientsdata.firmcode=?ClientCode" & _
            " and clientsdata2.firmtype=1" & _
            " and a.clientcode=clientsdata2.firmcode" & _
            " and pricesdata.firmcode=clientsdata.firmcode" & _
            " and pricesregionaldata.pricecode=pricesdata.pricecode" & _
            " and pricesregionaldata.regioncode=regions.regioncode" & _
            " and (clientsdata.maskregion & regions.regioncode)>0" & _
            " and (clientsdata2.maskregion & regions.regioncode)>0;"

            ms_Command.ExecuteNonQuery()

        End Sub

        Private Sub CreateClientOnShowInclude(ByVal PrimaryClientCode As Integer)
            ms_Command.CommandText = "INSERT INTO showregulation" & _
                                    "(PrimaryClientCode, ShowClientCode, Addition)" & _
                                    " VALUES (" & PrimaryClientCode & ", ?ClientCode, ?ShortName);" & _
                                    "INSERT INTO includeregulation" & _
                                    "(ID, PrimaryClientCode, IncludeClientCode, Addition)" & _
                                    "VALUES(NULL," & PrimaryClientCode & ", ?ClientCode, ?ShortName)"
            ms_Command.ExecuteNonQuery()
        End Sub

    Private Sub IncludeSDD_SelectedIndexChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles IncludeSDD.SelectedIndexChanged
        myMySqlConnection.Open()
        myMySqlDataReader = New MySqlCommand("select RegionCode from clientsdata where firmcode=" & IncludeSDD.SelectedValue, myMySqlConnection).ExecuteReader
        If myMySqlDataReader.Read() Then
            RegionDD.SelectedValue = CInt(myMySqlDataReader.Item(0).ToString)
        End If
        If Not myMySqlDataReader.IsClosed Then myMySqlDataReader.Close()
        myMySqlConnection.Close()
        SetWorkRegions(RegionDD.SelectedItem.Value)
    End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
#If DEBUG Then
        If Not IsPostBack Then
            FullNameTB.Text = "Тестер"
            ShortNameTB.Text = "Тестер"
            PhoneTB.Text = "4732-206000"
            FaxTB.Text = "4732-727622"
            URLTB.Text = "http://www.analit.net/"
            LoginTB.Text = "Tester" & Now().Second
            AddressTB.Text = "г. Воронеж, Ленинский пр. 160 - 415"
            EmailTB.Text = "tech@analit.net"
        End If
#End If
            Dim i As Int32

            'SelectTODS("select regioncode, region from mainclients.regions where regioncode>0 order by region", "regions")
            ' SelectTODS("select regioncode, region from farm.regions where regioncode>0 order by region", "regions")
            'считать клиентов не имеющих запись в osuseraccessright
            'SelectTODS("select Concat(FirmCode, ' [', shortname, ' - ', if(firmtype=0, 'пост.', 'потр.'), ']') as Shortname, FirmCode from clientsdata " & _
            '"left join osuseraccessright as a on a.clientcode=firmcode where clientcode is null", "freecodes")
            'SelectTODS("select Concat(FirmCode, ' [', shortname, ' - ', if(firmtype=0, 'пост.', 'потр.'), ']') as Shortname, FirmCode from usersettings.clientsdata " & _
            '"left join usersettings.osuseraccessright as a on a.clientcode=firmcode where clientcode is null", "freecodes")
            'считать данные о текущем кленте
            'SelectTODS("select regionaladmins.username, regionaladmins.regioncode, regions.region, regionaladmins.alowcreateretail, regionaladmins.alowcreatevendor, regionaladmins.alowchangesegment, regionaladmins.defaultsegment from mainclients.regionaladmins, mainclients.regions where mainclients.regionaladmins.regioncode=mainclients.regions.regioncode and username='" & strUser & "'", "admin")
            SelectToDS.SelectTODS("select regionaladmins.username, regions.regioncode, regions.region, regionaladmins.alowcreateretail, regionaladmins.alowcreatevendor, regionaladmins.alowchangesegment, regionaladmins.defaultsegment, AlowCreateInvisible, regionaladmins.email  from accessright.regionaladmins, farm.regions where accessright.regionaladmins.regionmask & farm.regions.regioncode >0 and username='" & Session("UserName") & "' order by region", "admin", DS1)

            If DS1.Tables("admin").Rows.Count < 1 Then
                Session("strError") = "Пользователь " & Session("UserName") & " не найден!"
                'System.Web.Security.FormsAuthentication.SignOut()
                Response.Redirect("error.aspx")
            End If
            'если user controls загружаются первый раз то...
            'присоеденить данные из таблиц к user controls
            If Not IsPostBack Then
                If CInt(DS1.Tables("admin").Rows(0).Item("AlowCreateInvisible").ToString) = 1 Then InvCB.Visible = True
                RegionDD.DataBind()
                'базируясь на данных о пользователе выбрать регионы по умолчанию
                For i = 0 To RegionDD.Items.Count - 1
                    If RegionDD.Items(i).Text = DS1.Tables("admin").Rows(0).Item(2) Then
                        RegionDD.SelectedIndex = i
                        Exit For
                    End If
                Next
                'RegionDD.Enabled = False
                'Dim uiTemp As UInt64 = DS1.Tables("admin").Rows(0).Item(1)
                'str = uiTemp.ToString
                Dim iInt As String
                'iInt = CInt(str)
                iInt = DS1.Tables("admin").Rows(0).Item(1).ToString
                SetWorkRegions(iInt)
                'тип
                'Dim sbTemp As SByte
                'sbTemp = 
                'str = sbTemp.ToString
                'iInt = CInt(str)


                If DS1.Tables("admin").Rows(0).Item(3).ToString = 1 Then
                    TypeDD.Items.Add("Аптека")
                    TypeDD.Items(0).Value = 1
                End If
                'sbTemp = DS1.Tables("admin").Rows(0).Item(4)
                'str = sbTemp.ToString
                'iInt = CInt(str)
                If DS1.Tables("admin").Rows(0).Item(4).ToString = 1 Then
                    TypeDD.Items.Add("Поставщик")
                    TypeDD.Items(TypeDD.Items.Count - 1).Value = 0
                End If
                If TypeDD.Items.Count = 1 Then TypeDD.Enabled = False
                'сегмент
                'sbTemp = DS1.Tables("admin").Rows(0).Item(5)
                'str = sbTemp.ToString
                'iInt = CInt(str)
                If DS1.Tables("admin").Rows(0).Item(5).ToString = 1 Then
                    SegmentDD.Items.Add("Опт")
                    SegmentDD.Items(0).Value = 0

                    SegmentDD.Items.Add("Розница")
                    SegmentDD.Items(1).Value = 1

                    ' sbTemp = 
                    'str = sbTemp.ToString
                    'iInt = CInt(str)
                    SegmentDD.SelectedIndex = DS1.Tables("admin").Rows(0).Item(6).ToString
                Else
                    'sbTemp = 
                    'str = sbTemp.ToString
                    'iInt = CInt(str)
                    If DS1.Tables("admin").Rows(0).Item(6).ToString = 0 Then
                        SegmentDD.Items.Add("Опт")
                        SegmentDD.Items(0).Value = 0
                    Else
                        SegmentDD.Items.Add("Розница")
                        SegmentDD.Items(0).Value = 1
                    End If
                    SegmentDD.Enabled = False
                End If
            End If
            Session("strStatus") = "Yes"
        End Sub
    End Class

End Namespace
