Imports System.Web.Mail
'Imports ByteFX.Data.MySqlClient
Imports MySql.Data.MySqlClient


Namespace AddUser

Public Class Func
        Public Sub Mail(ByVal From As String, ByVal Subject As String, ByVal BodyFormat As MailFormat, ByVal Body As String, ByVal MessageTo As String, ByVal MessageBCC As String, ByVal Encoding As System.Text.Encoding)

            Try
                Dim message As New MailMessage
                message.From = From
                message.Subject = Subject
                message.BodyFormat = BodyFormat
                message.Body = Body
                message.Bcc = MessageBCC
                'message.Cc = MessageCC
                message.To = MessageTo
                message.BodyEncoding = Encoding
                SmtpMail.SmtpServer = "box.analit.net"
                SmtpMail.Send(message)

            Catch
            End Try
        End Sub

    Public Function GeneratePassword() As String
        Randomize()
        Dim i, r As Int16
        Dim PassStr As String
        Try
            For i = 0 To 7 'должно быть 7
                r = CInt(Int((62 * Rnd()) + 1))
                If CInt(r) >= 0 And CInt(r) < 26 Then
                    If (65 + CInt(r)) = 73 Then r = CInt(r) + 1
                    PassStr = PassStr + Chr(65 + CInt(r))
                End If
                If CInt(r) >= 26 And CInt(r) < 52 Then
                    If (71 + CInt(r)) = 108 Then r = CInt(r) + 1
                    PassStr = PassStr + Chr(71 + CInt(r))
                End If
                If CInt(r) >= 52 Then PassStr = PassStr + Chr(48 + CInt(Rnd() * 9))
            Next
            If Len(PassStr) < 8 Then
                r = 97 + CInt(Now.Second / 3)
                If (CInt(r) = 108) Then
                    r = CInt(r) + 1
                End If
                PassStr = PassStr + Chr(CInt(r))
            End If
            GeneratePassword = PassStr
        Catch
            GeneratePassword = "[Ошибка формирования пароля]"
        End Try
    End Function

    Public Function SelectTODS(ByVal SQLQuery As String, ByVal Table As String, ByVal DS As DataSet, Optional ByVal MySQLCommand As MySqlCommand = Nothing, Optional ByVal CommandAdd As String = "") As Boolean
        Dim myMySqlCommand As New MySqlCommand
        Dim myMySqlConnection As New MySqlConnection("Data Source=testsql.analit.net;Database=usersettings;User ID=system;Password=123;Connect Timeout=300;")
        Dim Комманда As New MySqlCommand
        Dim myMySqlDataAdapter As New MySqlDataAdapter
        Try
            myMySqlConnection.Open()
        Catch
            Exit Function
        End Try
        Try
            If Not MySQLCommand Is Nothing Then myMySqlCommand = MySQLCommand
            With myMySqlCommand
                .CommandText = SQLQuery & CommandAdd
                .Connection = myMySqlConnection
            End With
            myMySqlDataAdapter.SelectCommand = myMySqlCommand
            myMySqlDataAdapter.Fill(DS, Table)
            SelectTODS = True
        Catch err As Exception
            SelectTODS = False
        Finally
            Комманда.Dispose()
            myMySqlDataAdapter.Dispose()
            myMySqlConnection.Close()
            myMySqlConnection.Dispose()
        End Try
    End Function

    Public Function GetDecimal(ByVal InputStr As String) As Decimal
        Dim str() As String
        Dim Delimiter, ResStr As String
        Dim i, q, w As Int16
        If InputStr.Length < 1 Then
            ResStr = 0
        Else

            For i = 1 To InputStr.Length
                If Asc(Mid(InputStr, i, 1)) > 57 Then Err.Raise(1, "Не верно указанна скидка(" & InputStr & ").", "Для указания десятичных долей используйте знаки ""."" или "","", к примеру 5.54")
            Next

            For i = 1 To InputStr.Length
                If Asc(Mid(InputStr, i, 1)) = 44 Or Asc(Mid(InputStr, i, 1)) = 46 Then Delimiter = Mid(InputStr, i, 1)
            Next

            str = Split(InputStr, Delimiter, 2, )
            If str(0).Length < 1 Then str(0) = 0

            For i = 0 To str.Length - 1
                If i = 0 Then
                    If Asc(Mid(str(i), 1)) = 45 Then
                        w = 2
                    Else
                        w = 1
                    End If
                End If
                For q = w To str(i).Length
                    If (Asc(Mid(str(i), q, 1)) > 57 Or Asc(Mid(str(i), q, 1)) < 48) And Asc(Mid(str(i), q, 1)) <> 45 Then Err.Raise(1, "Не верно указанна скидка(" & InputStr & ").", "Для указания десятичных долей используйте знаки ""."" или "",""")
                Next
            Next

            ResStr = str(0)
            If str.Length > 1 Then
                For q = 1 To str(1).Length
                    If Asc(Mid(str(1), q, 1)) > 57 Or Asc(Mid(str(1), q, 1)) < 48 Then Err.Raise(1, "Не верно указанна скидка(" & InputStr & ").", "Для указания десятичных долей используйте знаки ""."" или "",""")
                Next
                If str(1).Length < 1 Then str(1) = 0
                ResStr = ResStr & "," & str(1)
            End If
        End If
        GetDecimal = CDec(ResStr)
    End Function

    Public Function GetFirmType(ByVal FrimTypeCode As Integer) As String
        Dim FirmTypeStr As String
        If FrimTypeCode = 0 Then
            FirmTypeStr = "Поставщик"
        Else
            FirmTypeStr = "Аптека"
        End If
        GetFirmType = FirmTypeStr
    End Function

        Public Function CalcDays(ByVal CurContDate As Date) As Boolean
            If Now().Subtract(CurContDate).TotalDays > 3 Then Return False
            Return True
        End Function

End Class

End Namespace
