<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.Error" CodeBehind="Error.aspx.cs" Theme="Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>������ � ���������������� ����������</title>
    <meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
    <script type="text/javascript" language="javascript" src="./JavaScript/Main.js" ></script>
</head>
<body bgcolor="#ffffff">
    <form id="Form1" method="post" runat="server">
		<div style="width:100%; text-align:center;">
			<h3 style="color: red">
			������ � ���������������� ����������
			</h3>
			<p id="Message" runat="server">
			��������� �� ������ ���� ����������� �������������. ��������� ���� ��������.
			</p>
			<asp:Button ID="BackButton" runat="server" Text="��������� �� ��������" OnClick="BackButton_Click" />
        </div>
    </form>
</body>
</html>