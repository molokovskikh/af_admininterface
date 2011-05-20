<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.statcont"
	Theme="Main" CodeBehind="statcont.aspx.cs" MasterPageFile="~/Main.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="Form1" method="post" runat="server" defaultbutton="Button1">
		<div align="center">
			<table id="Table2" cellspacing="0" cellpadding="0" width="320" align="center" border="0">
				<tr>
					<td colspan="2">	
						<p align="center">
							<strong>Выберите период или введите текст для поиска:</strong>
						</p>
					</td>
				</tr>
				<tr>
					<td colspan="2">
						<asp:TextBox runat="server" ID="SearchText" Width="100%" />
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
							<asp:Button ID="Button1" runat="server" Text="Показать" BorderStyle="None" OnClick="Button1_Click" />
						</p>
					</td>
				</tr>
			</table>
		</div>
		<p align="center">
			<asp:GridView ID="StatisticGrid" runat="server" AllowSorting="True" AutoGenerateColumns="False"
				OnRowCreated="StatisticGrid_RowCreated" OnSorting="StatisticGrid_Sorting" OnRowDataBound="StatisticGrid_DataBound">
				<Columns>
					<asp:BoundField HeaderText="Дата" DataField="WriteTime" SortExpression="WriteTime" />
					<asp:BoundField HeaderText="Оператор" DataField="UserName" SortExpression="UserName" />
					<asp:HyperLinkField DataTextField="ShortName" HeaderText="Клиент" SortExpression="ShortName" DataNavigateUrlFields="FirmCode" DataNavigateUrlFormatString="Client/{0}" />
					<asp:HyperLinkField DataTextField="Login" HeaderText="Пользователь" SortExpression="Login" DataNavigateUrlFields="Id" DataNavigateUrlFormatString="users/{0}/edit" />
					<asp:BoundField HeaderText="Регион" DataField="Region" SortExpression="Region" />
					<asp:BoundField HeaderText="Сообщение" DataField="Message" SortExpression="Message" />
				</Columns>
			</asp:GridView>
		</p>
	</form>
</asp:Content>
