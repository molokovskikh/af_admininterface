<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser._default" CodePage="1251"
	CodeBehind="default.aspx.cs" Theme="Main" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Интерфейс управления клиентами</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<style type="text/css">
		td
		{
			font-family: Verdana;
			font-size: 10pt;
		}
		.StatisticData
		{
			background-color:#d8f1ff; 
			text-align:right; 
			font-size: 9pt; 
			font-family:Verdana;
		}
		
		a
		{
			font-family: Verdana, Arial, Helvetica, sans-serif;
			font-size: small;
		}
		
		.statistic-top-header
		{
			background-color: #eef8ff;
			padding: 10px 0 10px 0;
		}
		
		.statistic-header
		{
			background-color:#dadada;
			font-weight:bold;
			text-align:center;
		}
		
		.statistic-label
		{
			background-color:#d8f1ff;
			text-align:right;
		}
		
		.statistic-value
		{
			background-color:#d8f1ff;
			font-weight:bold;
			text-align: center;
		}
	</style>
</head>
<body>
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="left" width="200" style="height: 427px">
					<p>
						<asp:HyperLink ID="RegisterHL" runat="server" NavigateUrl="register.aspx">
							Регистрация клиентов
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="CloneHL" runat="server" NavigateUrl="CopySynonym.aspx">
							Клонирование
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="ClInfHL" runat="server" NavigateUrl="searchc.aspx">Информация о клиентах</font></asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="ShowStatHL" runat="server" NavigateUrl="statcont.aspx">
							Статистика обращений
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="BillingHL" runat="server" NavigateUrl="~/Billing/Search.rails">
							Биллинг
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="FTPHL" runat="server" NavigateUrl="docs.aspx">
							Общие документы
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="ViewAdministrators" runat="server" NavigateUrl="ViewAdministrators.aspx">
							Региональные администраторы
						</asp:HyperLink>
					</p>
					<p>
						<asp:HyperLink ID="MonitorUpdates" runat="server" NavigateUrl="./Logs/ClientRegistrationLog.rails">
							Мониторинг обновления клиентов
						</asp:HyperLink>
					</p>
					<p>
						<a href="./SmapRejector/Show.rails">Отклоненные сообщения</a>
					</p>
					<p>
						<asp:HyperLink ID="Telephony" runat=server NavigateUrl="./Telephony/Show.rails">Правила обратного звонка</asp:HyperLink>
					</p>
				</td>
				<td align="center" style="height: 427px">
					<fieldset style="width:20%;float:right;background-color:#d8f1ff;border:0;padding:3px 3px 3px 3px;">
						<legend style="font-weight:bold;">Фильтр статистики </legend>
						<div style="text-align:left;background-color:#eef8ff;padding: 3px;">
							<label for="RegionList">Регион:</label>
							<asp:DropDownList Width="100%" ID="RegionList" runat="server" />
						</div>
						<div style="text-align:left;background-color:#eef8ff; padding: 3px;">
							<label for="FromCalendar">Начиная с даты:</label>
							<asp:Calendar ID="FromCalendar" runat="server" BackColor="White" BorderColor="#999999"
								CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt"
								ForeColor="Black" Width="100%">
								<SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
								<TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
								<SelectorStyle BackColor="#CCCCCC" />
								<WeekendDayStyle BackColor="#ebebeb" />
								<OtherMonthDayStyle ForeColor="#808080" />
								<NextPrevStyle VerticalAlign="Bottom" />
								<DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
								<TitleStyle BackColor="#dadada" BorderColor="Black" Font-Bold="True" />
							</asp:Calendar>
						</div>
						<div style="text-align:left;background-color:#eef8ff;padding: 3px;">
							<label for="ToCalendar">По дату:</label>
							<asp:Calendar ID="ToCalendar" runat="server" BackColor="White" BorderColor="#999999"
								CellPadding="4" DayNameFormat="Shortest" Font-Names="Verdana" Font-Size="8pt"
								ForeColor="Black" Width="100%">
								<SelectedDayStyle BackColor="#666666" Font-Bold="True" ForeColor="White" />
								<TodayDayStyle BackColor="#CCCCCC" ForeColor="Black" />
								<SelectorStyle BackColor="#CCCCCC" />
								<WeekendDayStyle BackColor="#ebebeb" />
								<OtherMonthDayStyle ForeColor="#808080" />
								<NextPrevStyle VerticalAlign="Bottom" />
								<DayHeaderStyle BackColor="#CCCCCC" Font-Bold="True" Font-Size="7pt" />
								<TitleStyle BackColor="#dadada" BorderColor="Black" Font-Bold="True" />
							</asp:Calendar>
						</div>
						<div style="text-align:right;">
							<asp:Button ID="ShowButton" runat="server" Text="Показать" OnClick="ShowButton_Click" />
						</div>
					</fieldset>
					<table id="Table2" bordercolor="#dadada" cellspacing="1" cellpadding="0" width="69%"
						border="0">
						<tr>
							<td class="statistic-top-header" colspan=4>
								Статистика:
							</td>
						</tr>
						<tr>
							<td class="statistic-header" colspan=4>
								Обновления:
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Приготовлено обычных:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="ConfHL" runat="server" Enabled="False">-</asp:HyperLink>
							</td>
							<td class="statistic-label">
								В процессе:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="ReqHL" runat="server" NavigateUrl="./Monitoring/UpdatingClients.rails" Enabled="False">-</asp:HyperLink>
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Приготовлено КО:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="CUHL" runat="server" Enabled="False">-</asp:HyperLink>
							</td>
							<td class="statistic-label">
								Запретов:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="ADHL" runat="server" Enabled="False">-</asp:HyperLink>
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Ошибок:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="ErrUpHL" runat="server" Enabled="False">-</asp:HyperLink>
							</td>
							<td class="statistic-label">
								Загруженно данных:
							</td>
							<td class="statistic-value">
								<asp:Label runat="server" ID="DownloadDataSize" />
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Последнее обновление:
							</td>
							<td class="statistic-value">
								<asp:Label ID="MaxUpdateTime" runat="server" Enabled="False">-</asp:Label>
							</td>
							<td class="statistic-label">
								Загружено документов:
							</td>
							<td class="statistic-value">
								<asp:Label runat="server" ID="DownloadDocumentSize" />
							</td>
						</tr>
						<tr>
							<td class="statistic-header" colspan=4>
								Заказы:
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Очередь:
							</td>
							<td class="statistic-value">
								<asp:HyperLink ID="OprLB" runat=server Enabled=false NavigateUrl="Orders/" Target=_blank>-</asp:HyperLink>
							</td>
							<td class="statistic-label">
								Принято:
							</td>
							<td class="statistic-value">
								<asp:Label ID="OPLB" runat="server">-</asp:Label>
							</td>
						</tr>
						<tr>
							<td class="statistic-label">
								Последний заказ:
							</td>
							<td class="statistic-value">
								<asp:Label ID="LOT" runat="server">-</asp:Label>
							</td>
							<td class="statistic-label">
								Сумма:
							</td>
							<td class="statistic-value">
								<asp:Label ID="SumLB" runat="server">-</asp:Label>
							</td>
						</tr>
						<tr>
							<td class="statistic-header" colspan=4>
								Прайсы:
							</td>
						</tr>
						<tr>
							<td class="statistic-label" colspan=2>Очередь формализации:</td>
							<td class="statistic-value" colspan=2>
								<asp:Label ID="FormalizationQueue" runat=server Enabled=false NavigateUrl="Orders/">-</asp:Label>
							</td>
						</tr>
						<tr>
							<td class="statistic-label">Загружено:</td>
							<td class="statistic-value"><asp:Label ID="PriceDOKLB" runat="server">-</asp:Label></td>
							<td class="statistic-label">Последняя загрузка:</td>
							<td class="statistic-value"><asp:Label ID="DownPLB" runat="server">-</asp:Label></td>
						</tr>
						<tr>
							<td class="statistic-label">Формализовано:</td>
							<td class="statistic-value"><asp:Label ID="PriceFOKLB" runat="server">-</asp:Label></td>
							<td class="statistic-label">Последняя формализация:</td>
							<td class="statistic-value"><asp:Label ID="FormPLB" runat="server" >-</asp:Label></td>
						</tr>
						<tr>
							<td colspan="4" class="statistic-header">Службы:</td>
						</tr>
						<tr>
							<td class="statistic-label">Обработки заказов:</td>
							<td class="statistic-value" colspan=3><asp:Label runat="server" ID="OrderProcStatus" /></td>
						</tr>
						<tr>
							<td class="statistic-label">Обработки прайс листов (Master):</td>
							<td class="statistic-value" colspan=3><asp:Label runat="server" ID="PriceProcessorMasterStatus" /></td>
						</tr>
						<tr>
							<td class="statistic-label">Обработки прайс листов (Slave):</td>
							<td class="statistic-value" colspan=3><asp:Label runat="server" ID="PriceProcessorSlaveStatus" /></td>
						</tr>
						<tr>
							<td class="statistic-top-header" colspan=4>
								В скобках указанно количество уникальных клиентов.
							</td>
						</tr>
					</table>
				</td>
			</tr>
		</table>
		<p>
			<asp:Label ID="PassLB" runat="server" Font-Size="9pt" Font-Names="Verdana"></asp:Label>
		</p>
	</form>
</body>
</html>
