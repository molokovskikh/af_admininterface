<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.orders" codePage="1251" CodeFile="orders.aspx.vb" %>
<HTML>
	<HEAD>
		<title>Статистика заказов</title>
		<meta content="False" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" border="0">
				<TR>
					<TD>
						<DIV align="center">
							<TABLE id="Table2" cellSpacing="0" cellPadding="0" width="320" align="center" border="0">
								<TR>
									<TD colSpan="2">
										<P align="center"><FONT face="Arial" size="2"><STRONG>Укажите период</STRONG></FONT></P>
									</TD>
								</TR>
								<TR>
									<TD><FONT face="Arial" size="2"><STRONG>C:</STRONG></FONT></TD>
									<TD colSpan="1"><FONT face="Arial" size="2"><STRONG>По:</STRONG></FONT></TD>
								</TR>
								<TR>
									<TD>
										<P align="right"><asp:calendar id="CalendarFrom" runat="server" TitleFormat="Month" ShowGridLines="True" Font-Size="10pt"
												Font-Names="Arial" BackColor="#F6F6F6">
												<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
											</asp:calendar></P>
									</TD>
									<TD>
										<P align="right"><asp:calendar id="CalendarTo" runat="server" TitleFormat="Month" FirstDayOfWeek="Monday" ShowGridLines="True"
												Font-Size="10pt" Font-Names="Arial" BackColor="#F6F6F6">
												<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
											</asp:calendar></P>
									</TD>
								</TR>
								<TR>
									<TD colSpan="2">
										<P align="right"><asp:button id="Button1" runat="server" Text="Показать" Font-Names="Arial" Font-Size="10pt"
												BorderStyle="None"></asp:button></P>
									</TD>
								</TR>
							</TABLE>
						</DIV>
					</TD>
				</TR>
				<TR>
					<TD>
						<DIV align="center">
							<asp:Table id="Table3" runat="server" BorderStyle="Solid" Font-Size="8pt" Font-Names="Arial"
								CellPadding="0" CellSpacing="0" GridLines="Both" BackColor="#F6F6F6">
								<asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue" Font-Size="10pt"
									Font-Bold="True">
									<asp:TableCell Width="70px" Text="№"></asp:TableCell>
									<asp:TableCell Width="110px" Text="Дата&lt;br&gt;заказа"></asp:TableCell>
									<asp:TableCell Width="110px" Text="Дата&lt;br&gt;прайса"></asp:TableCell>
									<asp:TableCell Width="120px" Text="Потребитель"></asp:TableCell>
									<asp:TableCell Width="120px" Text="Поставщик"></asp:TableCell>
									<asp:TableCell Width="100px" Text="Прайс"></asp:TableCell>
									<asp:TableCell Width="70px" Text="Позиций"></asp:TableCell>
									<asp:TableCell Width="90px" Text="SMTP ID"></asp:TableCell>
								</asp:TableRow>
							</asp:Table></DIV>
					</TD>
				</TR>
				<TR>
					<TD>
						<P align="center"><FONT face="Arial, Helvetica, sans-serif"><FONT color="#000000" size="1">© 
									АК "</FONT> <A href="http://www.analit.net/"><FONT size="1">Инфорум</FONT></A><FONT color="#000000" size="1">" 
									2004</FONT></FONT></P>
					</TD>
				</TR>
			</TABLE>
		</form>
	</body>
</HTML>
