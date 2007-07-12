<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.CopySynonym" CodePage="1251"
	CodeBehind="CopySynonym.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<head xmlns="http://www.w3.org/1999/xhtml">
	<title>Система присвоения максимальных значений синонимов</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
		<p align="center">
			<font face="Verdana" size="2"><strong>Создание предварительного набора данных для клиента</strong></font></p>
		<table id="Table1" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="300"
			align="center" bgcolor="#dadada" border="1">
			<tr>
				<td bgcolor="#f0f8ff" colspan="3">
					<p align="center">
						<font face="Verdana" size="2">Выберите регион:</font>
						<asp:DropDownList ID="RegionDD" runat="server" Font-Size="8pt" Font-Names="Verdana"
							DataValueField="RegionCode" DataTextField="Region" DataSource="<%# DS %>">
						</asp:DropDownList></p>
				</td>
			</tr>
			<tr>
				<td style="width: 16px" height="35">
					<p align="right">
						<font face="Verdana" size="2">От:</font></p>
				</td>
				<td height="35">
					<p align="left">
						<asp:TextBox ID="FromTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:TextBox><asp:DropDownList
							ID="FromDD" runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode"
							DataTextField="Name" DataSource="<%# From %>" Visible="False">
						</asp:DropDownList></p>
				</td>
				<td height="35">
					<asp:Label ID="FromL" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:Label></td>
			</tr>
			<tr>
				<td style="width: 16px" height="35">
					<p align="right">
						<font face="Verdana" size="2">Для:</font></p>
				</td>
				<td height="35">
					<asp:TextBox ID="ToTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:TextBox><asp:DropDownList
						ID="ToDD" runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode"
						DataTextField="Name" DataSource="<%# ToT %>" Visible="False">
					</asp:DropDownList></td>
				<td height="35">
					<asp:Label ID="ToL" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:Label></td>
			</tr>
			<tr>
				<td bgcolor="#dadada" colspan="3">
					<p align="center">
						<asp:Button ID="FindBT" runat="server" OnClick="FindBT_Click" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
							Text="Найти"></asp:Button><asp:Button ID="SetBT" runat="server" OnClick="SetBT_Click" Font-Size="8pt" Font-Names="Verdana"
								Visible="False" BorderStyle="None" Text="Присвоить" Enabled="False"></asp:Button></p>
				</td>
			</tr>
		</table>
	</form>
	<asp:Label ID="LabelErr" runat="server" Font-Size="9pt" Font-Names="Verdana" ForeColor="Red"
		Font-Bold="True"></asp:Label>
</body>
