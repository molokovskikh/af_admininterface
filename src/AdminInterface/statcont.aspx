<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.statcont" codePage="1251" CodeFile="statcont.aspx.vb" %>
<HTML>
	<HEAD>
		<title>Статистика обращений</title>
		<meta content="False" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<DIV align="center">
				<TABLE id="Table2" cellSpacing="0" cellPadding="0" width="320" align="center" border="0">
					<TR>
						<TD colSpan="2">
							<P align="center"><FONT face="Arial" size="2"><STRONG>Выберите&nbsp;период</STRONG></FONT></P>
						</TD>
					</TR>
					<TR>
						<TD><FONT face="Arial" size="2"><STRONG>C:</STRONG></FONT></TD>
						<TD colSpan="1"><FONT face="Arial" size="2"><STRONG>По:</STRONG></FONT></TD>
					</TR>
					<TR>
						<TD>
							<P align="right">
								<asp:calendar id="CalendarFrom" runat="server" Font-Names="Arial" Font-Size="10pt" TitleFormat="Month"
									ShowGridLines="True" BackColor="#F6F6F6">
									<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
								</asp:calendar></P>
						</TD>
						<TD>
							<P align="right">
								<asp:calendar id="CalendarTo" runat="server" Font-Names="Arial" Font-Size="10pt" TitleFormat="Month"
									ShowGridLines="True" BackColor="#F6F6F6" FirstDayOfWeek="Monday">
									<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
								</asp:calendar></P>
						</TD>
					</TR>
					<TR>
						<TD colSpan="2">
							<P align="right">
								<asp:button id="Button1" runat="server" Font-Names="Arial" Font-Size="10pt" Text="Показать"
									BorderStyle="None"></asp:button></P>
						</TD>
					</TR>
				</TABLE>
			</DIV>
	
			<P align="center"><asp:table id="Table5" runat="server" Font-Names="Arial" Font-Size="8pt" BackColor="#EBEBEB"
					GridLines="Both" CellPadding="0" CellSpacing="0" BorderColor="#DADADA" BorderStyle="Solid">
					<asp:TableRow VerticalAlign="Middle" HorizontalAlign="Center" BackColor="AliceBlue" Font-Size="10pt"
						Font-Names="Arial" Font-Bold="True">
						<asp:TableCell Width="90px" Text="Дата"></asp:TableCell>
						<asp:TableCell Width="90px" Text="Оператор"></asp:TableCell>
						<asp:TableCell Width="120px" Text="Клиент"></asp:TableCell>
						<asp:TableCell Width="100px" Text="Регион"></asp:TableCell>
						<asp:TableCell Text="Сообщение"></asp:TableCell>
					</asp:TableRow>
				</asp:table></P>
		</form>
	</body>
</HTML>
