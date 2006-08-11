<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser._default" CodePage="1251"
	CodeFile="default.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Интерфейс управления клиентами</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="left" width="200">
					<p>
						<asp:HyperLink ID="RegisterHL" runat="server" NavigateUrl="register.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Регистрация клиентов</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="CloneHL" runat="server" NavigateUrl="CopySynonym.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Клонирование</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ChPassHL" runat="server" NavigateUrl="clchpass.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Изменение паролей</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ClInfHL" runat="server" NavigateUrl="searchc.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Информация о клиентах</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ClManageHL" runat="server" NavigateUrl="manage.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Управление клиентами</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ShowStatHL" runat="server" NavigateUrl="statcont.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Статистика обращений</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="BillingHL" runat="server" NavigateUrl="debetors.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Билинг</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="FTPHL" runat="server" NavigateUrl="docs.aspx">
								<font size="2" face="Verdana, Arial, Helvetica, sans-serif">Общие документы</font></asp:HyperLink></p>
					<p>
						<asp:HyperLink ID="ViewAdministrators" Font-Names="Verdana, Arial, Helvetica, sans-serif" Font-Size="small"
							runat="server" NavigateUrl="ViewAdministrators.aspx">Региональные администраторы</asp:HyperLink>
					</p>
				</td>
				<td align="center">
					<table id="Table2" bordercolor="#dadada" cellspacing="1" cellpadding="0" width="95%"
						border="0">
						<tr>
							<td bgcolor="#eef8ff" colspan="4" height="30">
								<p align="center">
									<strong><font face="Verdana" size="2">Статистика текущего дня:</font></strong></p>
							</td>
						</tr>
						<tr>
							<td align="center" bgcolor="#dadada" colspan="4" height="20">
								<font face="Verdana" size="2"><strong>Обновления:</strong></font></td>
						</tr>
						<tr>
							<td style="height: 18px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">Приготовлено обычных:&nbsp;</font>
							</td>
							<td style="width: 76px; height: 18px" bgcolor="#d8f1ff" height="18">
								<asp:HyperLink ID="ConfHL" runat="server" NavigateUrl="viewcl.aspx?id=2" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
							<td style="height: 18px" align="right" bgcolor="#d8f1ff" height="18">
								<font face="Verdana" size="2">В процессе:&nbsp;</font></td>
							<td style="height: 18px" bgcolor="#d8f1ff" height="18">
								<asp:HyperLink ID="ReqHL" runat="server" NavigateUrl="viewcl.aspx?id=4" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">Приготовлено КО:</font>&nbsp;</td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:HyperLink ID="CUHL" runat="server" NavigateUrl="viewcl.aspx?id=1" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
							<td align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">% докачки:&nbsp;</font></td>
							<td bgcolor="#d8f1ff" height="20">
								<asp:HyperLink ID="ReGetHL" runat="server" NavigateUrl="viewcl.aspx?id=5" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
						</tr>
						<tr>
							<td style="height: 19px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">Ошибок:&nbsp;</font>
							</td>
							<td style="width: 76px; text-align:center;" bgcolor="#d8f1ff" height="19">
								<asp:HyperLink ID="ErrUpHL" runat="server" NavigateUrl="viewcl.aspx?id=3" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
							<td style="height: 19px" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">Запретов:&nbsp;</font></td>
							<td style="height: 19px" bgcolor="#d8f1ff" height="19">
								<asp:HyperLink ID="ADHL" runat="server" NavigateUrl="viewcl.aspx?id=0" Enabled="False"
									Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">Последнее обновление:</font>&nbsp;</td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="MaxUpdateTime" runat="server" Enabled="False" Font-Size="8pt" Font-Names="Verdana"
									Font-Bold="True">-</asp:Label></td>
							<td bgcolor="#d8f1ff" height="20">
							</td>
							<td bgcolor="#d8f1ff" height="20">
							</td>
						</tr>
						<tr>
							<td align="center" bgcolor="#dadada" colspan="4" height="20">
								<font face="Verdana" size="2"><strong>Заказы:</strong></font></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px">
								<font face="Verdana" size="2">Принято:&nbsp;</font>
							</td>
							<td style="width: 76px" bgcolor="#ebebeb" height="20">
								<asp:Label ID="OPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" height="20">
								<font face="Verdana" size="2">Запретов:&nbsp;</font></td>
							<td bgcolor="#ebebeb" height="20">
								<asp:HyperLink ID="OADHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px">
								<font face="Verdana" size="2">Очередь:</font>&nbsp;</td>
							<td style="width: 76px" bgcolor="#ebebeb" height="20">
								<asp:Label ID="OprLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" height="20">
								<font face="Verdana" size="2">Ошибок:&nbsp;</font></td>
							<td bgcolor="#ebebeb" height="20">
								<asp:HyperLink ID="OErrHL" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True"></asp:HyperLink></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#ebebeb" style="width: 229px">
								<font face="Verdana" size="2">Последний заказ:</font>&nbsp;</td>
							<td style="width: 76px" bgcolor="#ebebeb" height="20">
								<asp:Label ID="LOT" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#ebebeb" height="20">
								<font face="Verdana" size="2">Сумма:&nbsp;</font></td>
							<td bgcolor="#ebebeb" height="20">
								<asp:Label ID="SumLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td bgcolor="#dadada" colspan="4" height="20">
								<p align="center">
									<font face="Verdana" size="2"><strong>Прайсы:</strong></font></p>
							</td>
						</tr>
						<tr>
							<td style="height: 20px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">Получено:&nbsp;</font></td>
							<td style="width: 76px; height: 20px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceDOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td style="height: 20px" align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">Не опознано:&nbsp;</font></td>
							<td style="height: 20px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceDERRLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">Последнее получение:</font>&nbsp;</td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="DownPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">Не получено:&nbsp;</font></td>
							<td bgcolor="#d8f1ff" height="20">
								<asp:Label ID="DownErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td align="right" bgcolor="#d8f1ff" style="width: 229px">
								<font face="Verdana" size="2">Формализовано:&nbsp;</font></td>
							<td style="width: 76px" bgcolor="#d8f1ff" height="20">
								<asp:Label ID="PriceFOKLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td align="right" bgcolor="#d8f1ff" height="20">
								<font face="Verdana" size="2">Не&nbsp; формализовано:</font></td>
							<td bgcolor="#d8f1ff" height="20">
								<asp:Label ID="FormErrLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td style="height: 19px; width: 229px;" align="right" bgcolor="#d8f1ff">
								<font face="Verdana" size="2">Последняя формализация:</font>&nbsp;
							</td>
							<td style="width: 76px; height: 19px" bgcolor="#d8f1ff" height="19">
								<asp:Label ID="FormPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
							<td style="height: 19px" align="right" bgcolor="#d8f1ff" height="19">
								<font face="Verdana" size="2"><font face="Verdana" size="2">Очередь формализации:&nbsp;</font></font></td>
							<td style="height: 19px" bgcolor="#d8f1ff" height="19">
								<asp:Label ID="WaitPLB" runat="server" Font-Size="8pt" Font-Names="Verdana" Font-Bold="True">-</asp:Label></td>
						</tr>
						<tr>
							<td style="height: 19px" align="center" bgcolor="#eef8ff" colspan="4">
								<font face="Verdana" size="2">В скобках указанно&nbsp;колличество уникальных клиентов.</font></td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
		<p>
			<asp:Label ID="PassLB" runat="server" Font-Size="9pt" Font-Names="Verdana"></asp:Label></p>
	</form>
</body>
</html>
