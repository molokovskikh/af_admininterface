<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.CopySynonym"
	CodeBehind="CopySynonym.aspx.cs" MasterPageFile="~/Main.Master" Theme="Main" %>


<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
<form id="form1" runat="server">
		<p align="center">
			<font face="Verdana" size="2"><strong>Создание предварительного набора данных для клиента</strong></font></p>
		<table id="Table1" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="300"
			align="center" bgcolor="#dadada" border="1">
			<tr>
				<td bgcolor="#f0f8ff" colspan="3">
					<p align="center">
						<font face="Verdana" size="2">Выберите регион:</font>
						<asp:DropDownList ID="RegionDD" runat="server" Font-Size="8pt" Font-Names="Verdana"
							DataValueField="RegionCode" DataTextField="Region">
						</asp:DropDownList>
					</p>
				</td>
			</tr>
			<tr>
				<td style="width: 16px" height="35">
					<p align="right">
						<font face="Verdana" size="2">От:</font></p>
				</td>
				<td height="35">
					<p align="left">
						<asp:TextBox ID="FromTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:TextBox>
						<asp:DropDownList
							ID="FromDD" runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode"
							DataTextField="Name" Visible="False">
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
					<asp:TextBox ID="ToTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:TextBox>
					<asp:DropDownList
						ID="ToDD" runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode"
						DataTextField="Name" Visible="False">
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
	<asp:Label ID="LabelErr" runat="server" Font-Size="9pt" Font-Names="Verdana" ForeColor="Red" Font-Bold="True"></asp:Label>
</form>
</asp:Content>