<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.CopySynonym" codePage="1251" CodeFile="CopySynonym.aspx.vb" %>
<HEAD>
	<title>Система присвоения максимальных значений синонимов</title>
	<META http-equiv="Content-Type" content="text/html; charset=windows-1251">
</HEAD>
<BODY vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
	<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	<form id="Form1" method="post" runat="server">
		<P align="center"><FONT face="Verdana" size="2"><STRONG>Создание предварительного набора 
					данных для клиента</STRONG></FONT></P>
		<TABLE id="Table1" borderColor="#dadada" cellSpacing="0" cellPadding="0" width="300" align="center"
			bgColor="#dadada" border="1">
			<TR>
				<TD bgColor="#f0f8ff" colSpan="3">
					<P align="center"><FONT face="Verdana" size="2">Выберите регион:</FONT>
						<asp:dropdownlist id=RegionDD runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="RegionCode" DataTextField="Region" DataSource="<%# DS %>">
						</asp:dropdownlist></P>
				</TD>
			</TR>
			<TR>
				<TD style="WIDTH: 16px" height="35">
					<P align="right"><FONT face="Verdana" size="2">От:</FONT></P>
				</TD>
				<TD height="35">
					<P align="left"><asp:textbox id="FromTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:textbox><asp:dropdownlist id=FromDD runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode" DataTextField="Name" DataSource="<%# From %>" Visible="False">
						</asp:dropdownlist></P>
				</TD>
				<TD height="35"><asp:label id="FromL" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:label></TD>
			</TR>
			<TR>
				<TD style="WIDTH: 16px" height="35">
					<P align="right"><FONT face="Verdana" size="2">Для:</FONT></P>
				</TD>
				<TD height="35"><asp:textbox id="ToTB" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:textbox><asp:dropdownlist id=ToDD runat="server" Font-Size="8pt" Font-Names="Verdana" DataValueField="ClientCode" DataTextField="Name" DataSource="<%# ToT %>" Visible="False">
					</asp:dropdownlist></TD>
				<TD height="35"><asp:label id="ToL" runat="server" Font-Size="8pt" Font-Names="Verdana"></asp:label></TD>
			</TR>
			<TR>
				<TD bgColor="#dadada" colSpan="3">
					<P align="center"><asp:button id="FindBT" runat="server" Font-Size="8pt" Font-Names="Verdana" BorderStyle="None"
							Text="Найти"></asp:button><asp:button id="SetBT" runat="server" Font-Size="8pt" Font-Names="Verdana" Visible="False" BorderStyle="None"
							Text="Присвоить" Enabled="False"></asp:button></P>
				</TD>
			</TR>
		</TABLE>
	</form>
	<asp:label id="LabelErr" runat="server" Font-Size="9pt" Font-Names="Verdana" ForeColor="Red"
		Font-Bold="True"></asp:label>
</BODY>
