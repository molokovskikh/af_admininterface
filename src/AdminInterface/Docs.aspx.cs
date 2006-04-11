Imports System.IO
Imports ICSharpCode.SharpZipLib


Namespace AddUser


Partial Class Documents
    Inherits System.Web.UI.Page

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

        Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
            If Session.Item("AccessGrant") <> 1 Then Response.Redirect("default.aspx")

            Const ResPath As String = "\\offdc\Data\Общего пользования\"

            Dim FileList As String()
            Dim FileName As String
            Dim Row As TableRow
            Dim Cel As TableCell
            Dim FileInfo As FileInfo
            Dim HL As HyperLink

            If Len(Request("doc")) < 5 Then

                FileList = Directory.GetFiles(ResPath)

                For Each FileName In FileList
                    Row = New TableRow

                    Cel = New TableCell
                    FileInfo = New FileInfo(FileName)
                    HL = New HyperLink
                    HL.Text = FileInfo.Name
                    HL.NavigateUrl = "Docs.aspx?Doc=" & FileInfo.Name
                    Cel.Controls.Add(HL)
                    Row.Cells.Add(Cel)

                    Cel = New TableCell
                    Cel.Text = FileInfo.LastWriteTime.ToShortDateString
                    Row.Cells.Add(Cel)

                    Cel = New TableCell
                    Cel.Text = Math.Round(FileInfo.Length / 1024, 1) & " k"
                    Row.Cells.Add(Cel)



                    FileListTab.Rows.Add(Row)
                Next

            Else


                FileInfo = New FileInfo(ResPath & Request("doc"))

                If FileInfo.Exists Then
                    FileName = Request("doc")

                    Dim ZipOutputStream As New IO.MemoryStream
                    Dim ZipInputStream As New Zip.ZipOutputStream(ZipOutputStream)
                    Dim ZipObject As New Zip.ZipEntry("ReqFile" & Right(FileName, 4))
                    Dim InputFileStream As New IO.FileStream(ResPath & Request("doc"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 10240)
                    Dim InputFileByteArray(InputFileStream.Length) As Byte


                    InputFileStream.Read(InputFileByteArray, 0, InputFileStream.Length)


                    ZipInputStream.SetLevel(9)
                    ZipObject.DateTime = Now()
                    ZipInputStream.PutNextEntry(ZipObject)
                    ZipInputStream.Write(InputFileByteArray, 0, InputFileStream.Length)

                    ZipInputStream.Finish()
                    InputFileStream.Close()
                    ZipInputStream.Close()

                    Response.Clear()
                    Response.ContentType = "application/octet-stream"
                    Response.AddHeader("Content-Disposition", "attachment; filename=""ReqFile.zip""")
                    'Response.AddHeader("Content-Disposition", "attachment")
                    Response.Flush()
                    Response.BinaryWrite(ZipOutputStream.ToArray)
                    ZipOutputStream.Close()
                End If


            End If
        End Sub
    End Class

End Namespace
