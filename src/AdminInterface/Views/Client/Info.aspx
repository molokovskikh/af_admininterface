<%@ Page Language="c#" AutoEventWireup="true" Inherits="AdminInterface.Views.Client.Info" CodePage="1251"
	CodeBehind="Info.aspx.cs" Theme="Main" MasterPageFile="~/Main.Master" %>
<%@ Import namespace="AdminInterface.Helpers"%>
<%@ Import namespace="Castle.MonoRail.Framework"%>
<%@ Import namespace="Castle.MonoRail.Views.Brail"%>
<%@ Import namespace="System.IO"%>

	
<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
<form id="form1" runat="server">
	<style>
		.InfoRow
		{
			height: 20px;
		}
	</style>
		<h3>
			<asp:Label ID="ShortNameLB" runat="server" />
		</h3>
		<div class="BorderedBlock" style="padding-top: 20px; padding-bottom: 20px;">
			<div class="TwoColumn" align="left">
				<table align="center" border="1" bordercolor="#dadada" cellpadding="0" cellspacing="0"
					id="Table2" width="90%">
					<col style="width:50%" />
					<col style="width:50%" />
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
							<asp:HyperLink ID="UserInterfaceHL" runat="server">Интерфейс пользователя</asp:HyperLink>
						</td>
					</tr>
					<tr>
						<td colspan="2">
<% 
	TextWriter textWriter = new StringWriter();
	Controller.InPlaceRenderView(textWriter, "ClientLoginsView");
	Response.Write(textWriter.ToString());		
%>							
						</td>
					</tr>
					<tr>
						<td colspan="2" class="Title">
							Операции:
						</td>
					</tr>
					<tr id="UnlockRow" runat="server">
						<td colspan="2">
							<asp:Button ID="UnlockButton" runat="server" Text="Разблокировать" OnClick="UnlockButton_Click" ValidationGroup="4" />
							<asp:Label ID="UnlockedLabel" runat="server" Text="Разблокированно" ForeColor="Green" />
						</td>
					</tr>
					<tr id="DeletePrepareDataRow" runat="server">
						<td colspan="2">
							<asp:Button ID="DeletePrepareDataButton" runat="server" ValidationGroup="4" OnClick="DeletePrepareDataButton_Click" Text="Удалить подготовленные данные" />
							<asp:Label ID="DeleteLabel" runat="server" />
						</td>
					</tr>
					<tr id="ResetUINRow" runat="server">
						<td valign="middle" align="left" colspan="2" style="height: 22px">
							<asp:Button ID="ResetCopyIDCB" runat="server" Text="Сбросить УИН" Enabled="False" ValidationGroup="3" OnClick="ResetUniqueCopyID" />
							<asp:Label ID="IsUniqueCopyIDSet" runat="server" ForeColor="Green">Идентификатор не присвоен</asp:Label>
							<label id="ResearReasonLable" runat="server">Причина:</label>
							<asp:TextBox ID="ResetIDCause" runat="server" BorderStyle="None" BackColor="LightGray" Enabled="False" />
							<asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" 
								ControlToValidate="ResetIDCause" Text="Укажите причину сброса идентификатора." 
								ValidationGroup="3" Display="Dynamic"  />
						</td>
					</tr>
					<tr>
						<td class="Title" style="height: 30px;" colspan="2">
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
								ValidationGroup="1" Display="Dynamic"></asp:RegularExpressionValidator>
						</td>
					</tr>
					<tr>
						<td colspan="2">
<% 
	textWriter = new StringWriter();
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
							<asp:TemplateField HeaderText="Событие">
								<ItemTemplate>
									<%# ViewHelper.FormatMessage(Eval("Message").ToString())%>
								</ItemTemplate>
							</asp:TemplateField>
						</Columns>
						<EmptyDataTemplate>
							Сообщений нет.
						</EmptyDataTemplate>
					</asp:GridView>
				</p>
			</div>
		</div>
	</form>
</asp:Content>