<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.orders" CodePage="1251"
	Theme="Main" CodeBehind="orders.aspx.cs" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Статистика заказов</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
</head>
<body>
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td>
					<div align="center">
						<table id="Table2" cellspacing="0" cellpadding="0" width="320" align="center" border="0">
							<tr>
								<td colspan="2">
									<p align="center">
										<font face="Arial" size="2"><strong>Укажите период</strong></font></p>
								</td>
							</tr>
							<tr>
								<td>
									<font face="Arial" size="2"><strong>C:</strong></font></td>
								<td colspan="1">
									<font face="Arial" size="2"><strong>По:</strong></font></td>
							</tr>
							<tr>
								<td>
									<p align="right">
										<asp:Calendar ID="CalendarFrom" runat="server" TitleFormat="Month" ShowGridLines="True"
											Font-Size="10pt" Font-Names="Arial" BackColor="#F6F6F6">
											<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
										</asp:Calendar>
									</p>
								</td>
								<td>
									<p align="right">
										<asp:Calendar ID="CalendarTo" runat="server" TitleFormat="Month" FirstDayOfWeek="Monday"
											ShowGridLines="True" Font-Size="10pt" Font-Names="Arial" BackColor="#F6F6F6">
											<SelectedDayStyle ForeColor="Black" BackColor="#0093E1"></SelectedDayStyle>
										</asp:Calendar>
									</p>
								</td>
							</tr>
							<tr>
								<td colspan="2">
									<p align="right">
										<asp:Button ID="Button1" runat="server" OnClick="Button1_Click" Text="Показать" Font-Names="Arial"
											Font-Size="10pt" BorderStyle="None"></asp:Button></p>
								</td>
							</tr>
						</table>
					</div>
				</td>
			</tr>
			<tr>
				<td>
					<div align="center">
						<asp:GridView ID="OrdersGrid" runat="server" AutoGenerateColumns="False" AllowSorting="True" OnRowCreated="OrdersGrid_RowCreated" OnSorting="OrdersGrid_Sorting">
							<Columns>
								<asp:BoundField HeaderText="№" DataField="RowID" SortExpression="RowID" />
								<asp:BoundField HeaderText="№ в AnalitF" DataField="ClientOrderId" SortExpression="ClientOrderId" />
								<asp:BoundField HeaderText="Дата заказа" DataField="WriteTime" SortExpression="WriteTime"/>
								<asp:BoundField HeaderText="Дата прайса" DataField="PriceDate" SortExpression="PriceDate"/>
								<asp:BoundField HeaderText="Аптека" DataField="Customer" SortExpression="Customer"/>
								<asp:BoundField HeaderText="Поставщик" DataField="Supplier" SortExpression="Supplier" />
								<asp:BoundField HeaderText="Прайс" DataField="PriceName" SortExpression="PriceName"/>
								<asp:BoundField HeaderText="Позиций" DataField="RowCount" SortExpression="RowCount"/>
								<asp:TemplateField HeaderText="SmtpID">
									<ItemTemplate>
										<asp:Label ID="Label1" runat="server" Text='<%# GetResult((DataRowView) GetDataItem()) %>'></asp:Label>
									</ItemTemplate>
								</asp:TemplateField>
								<asp:TemplateField HeaderText="Дата подтверждения" SortExpression="SubmitDate">
									<ItemTemplate>
										<asp:Label ID="Label1" runat="server" Text='<%# GetSubmiteDate((DataRowView) GetDataItem()) %>'></asp:Label>
									</ItemTemplate>
								</asp:TemplateField>
							</Columns>
							<EmptyDataTemplate>
								За указанный период заказов не найдено.
							</EmptyDataTemplate>
						</asp:GridView>
					</div>
				</td>
			</tr>
		</table>
		<div class="CopyRight">
			© АК <a href="http://www.analit.net/">"Инфорум"</a> 2004
		</div>
	</form>
</body>
</html>
