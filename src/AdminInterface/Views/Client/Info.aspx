<%@ Page Language="c#" AutoEventWireup="true" Inherits="AdminInterface.Views.Client.Info" CodePage="1251"
	CodeBehind="Info.aspx.cs" Theme="Main" MasterPageFile="~/ForAspxInMonorail.Master" %>
<%@ Import namespace="AdminInterface.Helpers"%>
<%@ Import namespace="Castle.MonoRail.Framework"%>
<%@ Import namespace="Castle.MonoRail.Views.Brail"%>
<%@ Import namespace="System.IO"%>

	
<asp:Content runat="server" ContentPlaceHolderID="ForAspxInMonorail">	
	<style>
		.InfoRow
		{
			height: 20px;
		}
	</style>
		<h3>
			<asp:Label ID="ShortNameLB" runat="server" />
		</h3>
		<div class="BorderedBlock" style="padding: 20px 10px 20px 10px; float:left; width: 95%;">
			<div class="TwoColumn">
<% 
	TextWriter textWriter = new StringWriter();
	Controller.InPlaceRenderView(textWriter, "ClientView");
	Response.Write(textWriter.ToString());
%>	
				<form id="form1" runat="server" style="padding:0; margin:0;">
				<table id="Table2" cellpadding=0 cellspacing=0 border=1 rules=all 
						style="width: 90%; border: solid 1px #dadada; border-collapse:collapse;">
					<col style="width:50%" />
					<col style="width:50%" />
					<tr>
						<td class="Title" style="height: 30px;" colspan="2">
							<strong>Общая информация</strong>
						</td>
					</tr>
					<tr class="InfoRow">
						<td colspan=2>
							<asp:Label ID="Registred" runat="server"/>
						</td>
					</tr
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