<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.statcont" CodePage="1251"
	Theme="Main" CodeFile="statcont.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Статистика обращений</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
</head>
<body>
	<form id="Form1" method="post" runat="server">
		<div align="center">
			<table id="Table2" cellspacing="0" cellpadding="0" width="320" align="center" border="0">
				<tr>
					<td colspan="2">
						<p align="center">
							<strong>Выберите период</strong>
						</p>
					</td>
				</tr>
				<tr>
					<td>
						<strong>C:</strong>
					</td>
					<td colspan="1">
						<strong>По:</strong>
					</td>
				</tr>
				<tr>
					<td>
						<p align="right">
							<asp:Calendar ID="CalendarFrom" runat="server" OnSelectionChanged="CalendarFrom_SelectionChanged"
								TitleFormat="Month" ShowGridLines="True" BackColor="#F6F6F6">
								<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
							</asp:Calendar>
						</p>
					</td>
					<td>
						<p align="right">
							<asp:Calendar ID="CalendarTo" runat="server" OnSelectionChanged="CalendarTo_SelectionChanged"
								TitleFormat="Month" ShowGridLines="True" BackColor="#F6F6F6" FirstDayOfWeek="Monday">
								<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
							</asp:Calendar>
						</p>
					</td>
				</tr>
				<tr>
					<td colspan="2">
						<p align="right">
							<asp:Button ID="Button1" runat="server" Text="Показать" BorderStyle="None" />
						</p>
					</td>
				</tr>
			</table>
		</div>
		<p align="center">
			<asp:GridView ID="StatisticGrid" runat="server" AllowSorting="True" AutoGenerateColumns="False"
				OnRowCreated="StatisticGrid_RowCreated" OnSorting="StatisticGrid_Sorting">
				<Columns>
					<asp:BoundField HeaderText="Дата" DataField="WriteTime" SortExpression="WriteTime" />
					<asp:BoundField HeaderText="Оператор" DataField="UserName" SortExpression="UserName" />
					<asp:HyperLinkField DataTextField="ShortName" HeaderText="Клиент" SortExpression="ShortName"
						DataNavigateUrlFields="FirmCode, OSUserID" DataNavigateUrlFormatString="info.aspx?cc={0}&ouar={1}" />
					<asp:BoundField HeaderText="Регион" DataField="Region" SortExpression="Region" />
					<asp:BoundField HeaderText="Сообщение" DataField="Message" SortExpression="Message" />
				</Columns>
			</asp:GridView>
		</p>
	</form>
</body>
</html>
