<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser._default" codePage="1251" CodeFile="default.aspx.vb" %>
<HTML>
	<HEAD>
		<title>Интерфейс управления клиентами</title>
		<meta content="True" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" border="0">
				<TR>
					<TD vAlign="top" align="left" width="200">
						<P><asp:hyperlink id="RegisterHL" runat="server" NavigateUrl="register.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Регистрация клиентов</font></asp:hyperlink></P>
						<P><asp:hyperlink id="CloneHL" runat="server" NavigateUrl="CopySynonym.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Клонирование</font></asp:hyperlink></P>
						<P><asp:hyperlink id="ChPassHL" runat="server" NavigateUrl="clchpass.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Изменение паролей</font></asp:hyperlink></P>
						<P><asp:hyperlink id="ClInfHL" runat="server" NavigateUrl="searchc.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Информация о клиентах</font></asp:hyperlink></P>
						<P><asp:hyperlink id="ClManageHL" runat="server" NavigateUrl="manage.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Управление клиентами</font></asp:hyperlink></P>
						<P><asp:hyperlink id="ShowStatHL" runat="server" NavigateUrl="statcont.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Статистика обращений</font></asp:hyperlink></P>
						<P><asp:hyperlink id="BillingHL" runat="server" NavigateUrl="debetors.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Билинг</font></asp:hyperlink></P>
						<P><asp:hyperlink id="FTPHL" runat="server" NavigateUrl="docs.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Общие документы</font></asp:hyperlink></P>
					</TD>
					<TD align="center">
						<TABLE id="Table2" borderColor="#dadada" cellSpacing="1" cellPadding="0" width="95%" border="0">
							<TR>
								<TD bgColor="#eef8ff" colSpan="4" height="30">
									<P align="center"><STRONG><FONT face="Verdana" size="2">Статистика текущего дня:</FONT></STRONG></P>
								</TD>
							</TR>
							<TR>
								<TD align="center" bgColor="#dadada" colSpan="4" height="20"><FONT face="Verdana" size="2"><STRONG>Обновления:</STRONG></FONT></TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 18px" align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Приготовлено 
										обычных:&nbsp;</FONT>
								</TD>
								<TD style="WIDTH: 76px; HEIGHT: 18px" bgColor="#d8f1ff" height="18"><asp:hyperlink id="ConfHL" runat="server" NavigateUrl="viewcl.aspx?id=2" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True">-</asp:hyperlink></TD>
								<TD style="HEIGHT: 18px" align="right" bgColor="#d8f1ff" height="18"><FONT face="Verdana" size="2">В 
										процессе:&nbsp;</FONT></TD>
								<TD style="HEIGHT: 18px" bgColor="#d8f1ff" height="18"><asp:hyperlink id="ReqHL" runat="server" NavigateUrl="viewcl.aspx?id=4" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True">-</asp:hyperlink></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Приготовлено КО:</FONT>&nbsp;</TD>
								<TD style="WIDTH: 76px" bgColor="#d8f1ff" height="20"><asp:hyperlink id="CUHL" runat="server" NavigateUrl="viewcl.aspx?id=1" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True"></asp:hyperlink></TD>
								<TD align="right" bgColor="#d8f1ff" height="20"><FONT face="Verdana" size="2">% 
										докачки:&nbsp;</FONT></TD>
								<TD bgColor="#d8f1ff" height="20"><asp:hyperlink id="ReGetHL" runat="server" NavigateUrl="viewcl.aspx?id=5" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True">-</asp:hyperlink></TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 19px" align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Ошибок:&nbsp;</FONT>
								</TD>
								<TD style="WIDTH: 76px; HEIGHT: 19px" align="left" bgColor="#d8f1ff" height="19"><asp:hyperlink id="ErrUpHL" runat="server" NavigateUrl="viewcl.aspx?id=3" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True">-</asp:hyperlink></TD>
								<TD style="HEIGHT: 19px" align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Запретов:&nbsp;</FONT></TD>
								<TD style="HEIGHT: 19px" align="left" bgColor="#d8f1ff" height="19"><asp:hyperlink id="ADHL" runat="server" NavigateUrl="viewcl.aspx?id=0" Enabled="False" Font-Size="8pt"
										Font-Names="Verdana" Font-Bold="True">-</asp:hyperlink></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Последнее обновление:</FONT>&nbsp;</TD>
								<TD style="WIDTH: 76px" bgColor="#d8f1ff" height="20"><asp:label id="MaxUpdateTime" runat="server" Enabled="False" Font-Size="8pt" Font-Names="Verdana"
										Font-Bold="True">-</asp:label></TD>
								<TD bgColor="#d8f1ff" height="20"></TD>
								<TD bgColor="#d8f1ff" height="20"></TD>
							</TR>
							<TR>
								<TD align="center" bgColor="#dadada" colSpan="4" height="20"><FONT face="Verdana" size="2"><STRONG>Заказы:</STRONG></FONT></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#ebebeb"><FONT face="Verdana" size="2">Принято:&nbsp;</FONT>
								</TD>
								<TD style="WIDTH: 76px" bgColor="#ebebeb" height="20"><asp:label id="OPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD align="right" bgColor="#ebebeb" height="20"><FONT face="Verdana" size="2">Запретов:&nbsp;</FONT></TD>
								<TD bgColor="#ebebeb" height="20"><asp:hyperlink id="OADHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:hyperlink></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#ebebeb"><FONT face="Verdana" size="2">Очередь:</FONT>&nbsp;</TD>
								<TD style="WIDTH: 76px" bgColor="#ebebeb" height="20"><asp:label id="OprLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD align="right" bgColor="#ebebeb" height="20"><FONT face="Verdana" size="2">Ошибок:&nbsp;</FONT></TD>
								<TD bgColor="#ebebeb" height="20"><asp:hyperlink id="OErrHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:hyperlink></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#ebebeb"><FONT face="Verdana" size="2">Последний заказ:</FONT>&nbsp;</TD>
								<TD style="WIDTH: 76px" bgColor="#ebebeb" height="20"><asp:label id="LOT" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD align="right" bgColor="#ebebeb" height="20"><FONT face="Verdana" size="2">Сумма:&nbsp;</FONT></TD>
								<TD bgColor="#ebebeb" height="20"><asp:label id="SumLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
							</TR>
							<TR>
								<TD bgColor="#dadada" colSpan="4" height="20">
									<P align="center"><FONT face="Verdana" size="2"><STRONG>Прайсы:</STRONG></FONT></P>
								</TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 20px" align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Получено:&nbsp;</FONT></TD>
								<TD style="WIDTH: 76px; HEIGHT: 20px" bgColor="#d8f1ff" height="20"><asp:label id="PriceDOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD style="HEIGHT: 20px" align="right" bgColor="#d8f1ff" height="20"><FONT face="Verdana" size="2">Не 
										опознано:&nbsp;</FONT></TD>
								<TD style="HEIGHT: 20px" bgColor="#d8f1ff" height="20"><asp:label id="PriceDERRLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Последнее получение:</FONT>&nbsp;</TD>
								<TD style="WIDTH: 76px" bgColor="#d8f1ff" height="20"><asp:label id="DownPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD align="right" bgColor="#d8f1ff" height="20"><FONT face="Verdana" size="2">Не 
										получено:&nbsp;</FONT></TD>
								<TD bgColor="#d8f1ff" height="20"><asp:label id="DownErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
							</TR>
							<TR>
								<TD align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Формализовано:&nbsp;</FONT></TD>
								<TD style="WIDTH: 76px" bgColor="#d8f1ff" height="20"><asp:label id="PriceFOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD align="right" bgColor="#d8f1ff" height="20"><FONT face="Verdana" size="2">Не&nbsp; 
										формализовано:</FONT></TD>
								<TD bgColor="#d8f1ff" height="20"><asp:label id="FormErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 19px" align="right" bgColor="#d8f1ff"><FONT face="Verdana" size="2">Последняя 
										формализация:</FONT>&nbsp;
								</TD>
								<TD style="WIDTH: 76px; HEIGHT: 19px" bgColor="#d8f1ff" height="19"><asp:label id="FormPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
								<TD style="HEIGHT: 19px" align="right" bgColor="#d8f1ff" height="19"><FONT face="Verdana" size="2"><FONT face="Verdana" size="2">Очередь 
											формализации:&nbsp;</FONT></FONT></TD>
								<TD style="HEIGHT: 19px" bgColor="#d8f1ff" height="19"><asp:label id="WaitPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:label></TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 19px" align="center" bgColor="#eef8ff" colSpan="4"><FONT face="Verdana" size="2">В 
										скобках указанно&nbsp;колличество уникальных клиентов.</FONT></TD>
							</TR>
						</TABLE>
					</TD>
				</TR>
			</TABLE>
			<P><asp:label id="PassLB" runat="server" Font-Size="9pt" Font-Names="Verdana"></asp:label></P>
		</form>
	</body>
</HTML>
