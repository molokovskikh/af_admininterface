'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser


Partial Class report
    Inherits System.Web.UI.Page
   
    Dim соединение As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;")
    Dim Комманда As New MySqlCommand()
    Protected WithEvents Image1 As System.Web.UI.WebControls.Image
    Protected WithEvents Image2 As System.Web.UI.WebControls.Image

    Dim Reader As MySqlDataReader

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
    '******** Page_Load *************
    'при загрузке страницы пытается считать данные из формы WebForm1
    'если данные пустые, то перейти на страницу code.aspx
        'Private Sub Page_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        'End Sub

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")
            'Put user code to initialize the page here
            ' Exit Sub

            Dim str, Code, Name, Login, Password, DogN, Tariff, ShortName As String
            Dim IsRegister As Boolean
            str = Session("strStatus").ToString
            Code = Session("Code").ToString
            DogN = Session("DogN").ToString
            Name = Session("Name").ToString
            ShortName = Session("ShortName").ToString
            Login = Session("Login").ToString
            Password = Session("Password").ToString
            IsRegister = Session("Register").ToString
            Tariff = Session("Tariff").ToString
            ChPassMessLB.Visible = Not IsRegister
            'RegisterLB.Visible = IsRegister
            'ChPassLb.Visible = Not IsRegister
            RepLb.Visible = Not IsRegister
            'Dim dtTemp As DateTime
            'dtTemp = Now()
            'LBDate.Text = dtTemp.Day & "." & dtTemp.Month & "." & dtTemp.Year
            If str = "Yes" Then
                'Dim wf1 As WebForm1
                'получаем ссылку на хэндлер текущего экземпляра                         
                'wf1 = Context.Handler
                LBClient.Text = Name
                LBCCard.Text = Name
                LBShortName.Text = ShortName

                LBLogin.Text = Login
                LBLcard.Text = Login
                LBPassword.Text = Password

                LBCode.Text = Code
                LBCard.Text = Code
                DogNLB.Text = DogN
                DogNNLB.Text = DogN
                TariffLB.Text = Tariff
                TariffD.Text = Tariff
                Dim dtTemp As DateTime
                dtTemp = Now()
                'LBDate.Text = dtTemp.Day & "." & dtTemp.Month & "." & dtTemp.Year & "  " & dtTemp.Hour & ":" & dtTemp.Minute & ":" & dtTemp.Second
                LBDate.Text = dtTemp
                RegDate.Text = dtTemp
                If (LBClient.Text = "") And (LBLogin.Text = "") And (LBPassword.Text = "") Then
                    Server.Transfer("default.aspx")
                End If
            Else
                Server.Transfer("default.aspx")
            End If
        End Sub
    End Class

End Namespace
