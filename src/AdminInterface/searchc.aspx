<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.searchc" codePage="1251" CodeFile="searchc.aspx.vb" %>
<HTML>
	<HEAD>
		<title>���������� � ��������</title>
		<meta content="False" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" border="0">
				<TR>
					<TD colSpan="2" height="20">
						<P align="center"><STRONG><FONT face="Verdana" size="2">���������� ������ �������:</FONT></STRONG></P>
					</TD>
				</TR>
				<TR>
					<TD vAlign="baseline" align="center" colSpan="2">
						<TABLE id="Table2" cellSpacing="0" cellPadding="0" width="350" bgColor="#f6f6f6" border="0">
							<TR>
								<TD colSpan="3">
									<P align="center"><FONT face="Verdana" size="2">��������� ����� �������:</FONT></P>
								</TD>
							</TR>
							<TR>
								<TD>
									<P align="center"><asp:textbox id="FindTB" runat="server" BorderStyle="None" Font-Names="Verdana" Font-Size="8pt"></asp:textbox></P>
								</TD>
								<TD align="center"><asp:radiobuttonlist id="FindRB" runat="server" BorderStyle="None" Font-Names="Verdana" Font-Size="8pt"
										Width="81px">
										<asp:ListItem Value="0" Selected="True">���</asp:ListItem>
										<asp:ListItem Value="1">ID</asp:ListItem>
										<asp:ListItem Value="3">Billing ID</asp:ListItem>
										<asp:ListItem Value="2">�����</asp:ListItem>
									</asp:radiobuttonlist></TD>
								<TD>
									<P align="center"><asp:button id="GoFind" runat="server" BorderStyle="None" Font-Names="Verdana" Font-Size="8pt"
											Text="�����"></asp:button></P>
								</TD>
							</TR>
							<TR>
								<TD colSpan="3"><asp:checkbox id="ADCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="���������� ������ Active Directory" Checked="True"></asp:checkbox></TD>
							</TR>
						</TABLE>
					</TD>
				</TR>
				<TR>
					<TD vAlign="baseline" align="center" colSpan="2" height="10">
						<P align="left">&nbsp;</P>
					</TD>
				</TR>
				<TR>
					<TD vAlign="baseline" align="center" colSpan="2">
						<P><asp:table id="Table3" runat="server" BorderStyle="Solid" Font-Names="Verdana" Font-Size="8pt"
								BorderColor="#DADADA" CellSpacing="0" CellPadding="0" GridLines="Both" Visible="False"
								BackColor="#EBEBEB">
								<asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue" Font-Size="8pt"
									Font-Names="Verdana" Font-Bold="True">
									<asp:TableCell Text="�������&lt;br&gt;���"></asp:TableCell>
									<asp:TableCell Width="70px" Text="���"></asp:TableCell>
									<asp:TableCell Text="������������"></asp:TableCell>
									<asp:TableCell Text="������"></asp:TableCell>
									<asp:TableCell Text="�������&lt;br&gt;(��������������)&lt;br&gt;����������"></asp:TableCell>
									<asp:TableCell Text="����������&lt;br&gt;(����������������)&lt;br&gt;����������"></asp:TableCell>
									<asp:TableCell Text="EXE"></asp:TableCell>
									<asp:TableCell Text="MDB"></asp:TableCell>
									<asp:TableCell Text="Login"></asp:TableCell>
									<asp:TableCell Text="�������"></asp:TableCell>
									<asp:TableCell Text="���"></asp:TableCell>
								</asp:TableRow>
							</asp:table></P>
						<P><asp:label id="Label1" runat="server" Font-Names="Verdana" Font-Size="9pt" Visible="False"
								Font-Bold="True">������ �� ������</asp:label></P>
						<P align="center"><asp:table id="Table4" runat="server" Font-Names="Verdana" Font-Size="8pt" Width="253px" Visible="False">
								<asp:TableRow>
									<asp:TableCell BackColor="#FF6600" Width="30px"></asp:TableCell>
									<asp:TableCell Text=" - ������ ��������"></asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell BackColor="Aqua"></asp:TableCell>
									<asp:TableCell Text=" - ������� ������ ���������"></asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell BackColor="Violet"></asp:TableCell>
									<asp:TableCell Text=" - ������� ������ ��������������"></asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell BackColor="Gray"></asp:TableCell>
									<asp:TableCell Text=" - ��������� ����� 2 ����� �����"></asp:TableCell>
								</asp:TableRow>
								<asp:TableRow>
									<asp:TableCell BackColor="Red"></asp:TableCell>
									<asp:TableCell Text=" - ������ Active Directory"></asp:TableCell>
								</asp:TableRow>
							</asp:table></P>
						<asp:label id="TimeFLB" runat="server" Font-Names="Verdana" Font-Size="8pt">����� ������:</asp:label><asp:label id="TimeSLB" runat="server" Font-Names="Verdana" Font-Size="8pt"></asp:label></TD>
				</TR>
				<TR>
					<TD style="HEIGHT: 16px" height="16">
						<P align="center"><FONT face="Arial, Helvetica, sans-serif"><FONT color="#000000" size="1">� 
									�� "</FONT> <A href="http://www.analit.net/"><FONT size="1">�������</FONT></A><FONT color="#000000" size="1">" 
									2004</FONT></FONT></P>
					</TD>
				</TR>
			</TABLE>
			&nbsp;</form>
	</body>
</HTML>
