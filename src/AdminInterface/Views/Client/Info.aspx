<%@ Page Language="c#" AutoEventWireup="true" Inherits="AdminInterface.Views.Client.Info" CodePage="1251"
	CodeBehind="Info.aspx.cs" Theme="Main" %>
<%@ Import namespace="Castle.MonoRail.Framework"%>
<%@ Import namespace="Castle.MonoRail.Views.Brail"%>
<%@ Import namespace="System.IO"%>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
	<title>Информация о клиентах</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	
	<style>
	.InfoRow
	{
		height: 20px;
	}
	</style>
	<link rel="stylesheet" type="text/css" href="~/Css/Contacts.css" />
</head>
<body>
	<form id="Form1" method="post" runat="server">
		<h3>
			<asp:Label ID="ShortNameLB" runat="server" />
		</h3>
		<div class="BorderedBlock" style="padding-top: 20px; padding-bottom: 20px;">
			<div class="TwoColumn" align="left">
				<table align="center" border="1" bordercolor="#dadada" cellpadding="0" cellspacing="0"
					id="Table2" width="90%">
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="ConfigHL" runat="server">Настройка</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="BillingLink" runat="server">Биллинг</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="ChPassHL" Target="_blank" runat="server">Статистика изменения пароля</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="UpdateListHL" Target="_blank" runat="server">Статистика обновлений</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="DocumentLog" Target="_blank" runat="server">Статистика документов</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="OrderHistoryHL" Target="_blank" runat="server">История заказов</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="ChPass" runat="server">Изменение пароля</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" height="20">
							<asp:HyperLink ID="UserInterfaceHL" runat="server">Интерфейс пользователя</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2" style="background-color: #f0f8ff; text-align: center; vertical-align: middle; font-weight:bold;">
							Показываемые клиенты:
						</td>
					</tr>
					<tr>
						<td colspan="2">
							<asp:GridView ID="ShowClientsGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="ShowClientsGrid_RowCommand" OnRowDataBound="ShowClientsGrid_RowDataBound" OnRowDeleting="ShowClientsGrid_RowDeleting">
								<EmptyDataTemplate>
									<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="Добавить клиента" />
								</EmptyDataTemplate>
								<Columns>
									<asp:TemplateField>
										<HeaderTemplate>
											<asp:Button ID="AddButton" CommandName="Add" runat="server" Text="Добавить" />
										</HeaderTemplate>
										<ItemTemplate>
											<asp:Button ID="DeleteButton" CommandName="Delete" runat="server" Text="Удалить" />
										</ItemTemplate>
										<ItemStyle Width="10%" />
									</asp:TemplateField>
									<asp:TemplateField HeaderText="Клиент">
										<ItemTemplate>
											<asp:TextBox ID="SearchText" runat="server" />
											<asp:Button ID="SearchButton" CommandName="Search" runat="server" Text="Найти" ValidationGroup="3" />
											<asp:DropDownList ID="ShowClientsList" runat="server" DataTextField="ShortName" DataValueField="FirmCode">
											</asp:DropDownList>
											<asp:CustomValidator ID="ShowCleintsValidator" runat="server" ControlToValidate="ShowClientsList"
												ErrorMessage="Необходимо выбрать клиента." ValidateEmptyText="True" ValidationGroup="1" OnServerValidate="ShowCleintsValidator_ServerValidate" Width="13px">*</asp:CustomValidator>
										</ItemTemplate>
									</asp:TemplateField>
								</Columns>
							</asp:GridView>
						</td>
					</tr>
					<tr id="UnlockRow" runat="server">
						<td colspan="2">
							<asp:Button ID="UnlockButton" runat="server" Text="Разблокировать" OnClick="UnlockButton_Click" ValidationGroup="2" />
							<asp:Label ID="UnlockedLabel" runat="server" Text="Разблокированно" ForeColor="Green" />
						</td>
					</tr>
					<tr>
						<td style="background-color: #f0f8ff; text-align: center; vertical-align: middle;
							height: 30px;" colspan="2">
							<strong>Общая информация</strong>
						</td>
					</tr>
					<tr class="InfoRow">
						<td align="right">
							Полное наименование:
						</td>
						<td>
							<asp:TextBox ID="FullNameText" runat="server" Width="90%" />
						</td>
					</tr>
					<tr class="InfoRow">
						<td style="text-align: right;">
							Краткое наименование:
						</td>
						<td>
							<asp:TextBox ID="ShortNameText" runat="server" Width="90%" />
						</td>
					</tr>
					<tr class="InfoRow">
						<td align="right">
							Адрес:
						</td>
						<td>
							<asp:TextBox ID="AddressText" runat="server" Width="90%" />
						</td>
					</tr>
					<tr class="InfoRow">
						<td align="right">
							Факс:
						</td>
						<td align="left">
							<asp:TextBox ID="FaxText" runat="server" Width="90%" />
						</td>
					</tr>
					<tr class="InfoRow">
						<td align="right">
							URL:
						</td>
						<td align="left">
							<asp:TextBox ID="UrlText" runat="server" Width="90%" />
							<asp:RegularExpressionValidator ID="UrlValidator" runat="server" ErrorMessage="*"
								ControlToValidate="UrlText" ValidationExpression="http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?"
								ValidationGroup="1"></asp:RegularExpressionValidator>
						</td>
					</tr>
					<tr>
						<td colspan="2">
<% 
	TextWriter textWriter = new StringWriter();
	Controller.InPlaceRenderView(textWriter, "ContactViewer");
	Response.Write(textWriter.ToString());		
%>

						</td>
					</tr>
					<tr style="text-align: center;">
						<td colspan="2">
							<asp:Label ID="IsInfoSavedLabel" runat="server" Visible="False" Text="Сохранено"
								ForeColor="Green"></asp:Label>
						</td>
					</tr>
					<tr style="text-align: center;">
						<td colspan="2">
							<asp:Button ID="SaveButton" runat="server" Text="Применить" OnClick="SaveButton_Click"
								ValidationGroup="1" />
						</td>
					</tr>
				</table>
			</div>
			<div class="TwoColumn" style="text-align: center;">
				Новое обращение(сообщение):
				<br />
				<asp:Label ID="IsMessafeSavedLabel" runat="server" Visible="False" Text="Сохранено"
					ForeColor="Green"></asp:Label>
				<asp:TextBox ID="ProblemTB" runat="server" Height="150px" TextMode="MultiLine" Width="90%" />
				<br />
				<asp:Button ID="Button1" runat="server" OnClick="Button1_Click"
					Text="Принять" ValidationGroup="2" />
				<p>
					<asp:GridView ID="LogsGrid" runat="server" AutoGenerateColumns="False" DataMember="Logs">
						<Columns>
							<asp:BoundField DataField="Date" HeaderText="Дата" />
							<asp:BoundField DataField="UserName" HeaderText="Оператор" />
							<asp:BoundField DataField="Message" HeaderText="Событие" />
						</Columns>
						<EmptyDataTemplate>
							Сообщений нет.
						</EmptyDataTemplate>
					</asp:GridView>
				</p>
			</div>
		</div>
		<div class="CopyRight">
			© АК<a href="http://www.analit.net/">"Инфорум"</a>2005
		</div>
	</form>
</body>
</html>
