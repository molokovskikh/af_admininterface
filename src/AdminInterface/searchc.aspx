<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.searchc" CodePage="1251"
	CodeBehind="searchc.aspx.cs" Theme="Main" MasterPageFile="~/Main.Master" %>
<%@ Import Namespace="AdminInterface.Models.Security"%>
<%@ Import Namespace="AdminInterface.Security"%>



<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server" defaultbutton="GoFind">
		<h3>
			Статистика работы клиента:
		</h3>
		<table style="margin-left:auto; margin-right: auto">
			<tr><td>
			<table style="background-color: #e7f6e0; padding: 10px; text-align:center; width: 450px;" id="Table2 onload="return SetSearchTitle();">
				<tr>
					<td colspan="3">
						Выполните поиск клиента:
					</td>
				</tr>
				<tr>
				
					<td style="width: 181px;">
						<asp:TextBox ID="FindTB" runat="server" />
						<asp:CustomValidator ID="SearchTextValidator" runat="server" ControlToValidate="FindTB"
							ErrorMessage="*" ClientValidationFunction="ValidateSearch" OnServerValidate="SearchTextValidator_ServerValidate" ValidateEmptyText="True" />
					</td>
					<td style="text-align: left;">
						<asp:RadioButtonList ID="FindRB" runat="server" BorderStyle="None" Width="120px">
							<asp:ListItem Value="Automate" Selected="True">Автоматический</asp:ListItem>
							<asp:ListItem Value="ShortName">Имя</asp:ListItem>
							<asp:ListItem Value="Code">ID</asp:ListItem>
							<asp:ListItem Value="BillingCode">Billing ID</asp:ListItem>
							<asp:ListItem Value="Login">Имя пользователя</asp:ListItem>
							<asp:ListItem Value="JuridicalName">Юридическое наименование</asp:ListItem>
						</asp:RadioButtonList>
					</td>
					<td rowspan="2">
						<asp:Button ID="GoFind" runat="server" Text="Найти" OnClick="GoFind_Click" CausesValidation="true" />
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						Тип клиента:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientType" runat="server">
							<asp:ListItem>Все</asp:ListItem>
							<asp:ListItem>Аптеки</asp:ListItem>
							<asp:ListItem>Поставщики</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						Состояние:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientState" runat="server">
							<asp:ListItem>Все</asp:ListItem>
							<asp:ListItem>Включен</asp:ListItem>
							<asp:ListItem>Отключен</asp:ListItem>
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td style="text-align:right">
						Регион:
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientRegion" runat="server" DataTextField="Region" DataValueField="RegionCode">
						</asp:DropDownList>
					</td>
				</tr>
				<tr>
					<td colspan="3">
						<asp:CheckBox ID="ADCB" runat="server" Text="Показывать статус Active Directory"
							Checked="True" />
					</td>
				</tr>
			</table>
			</td></tr>
		</table>
		<div style="margin-top: 20px;">
			<asp:GridView ID="ClientsGridView" CssClass="HighLightCurrentRow" runat="server" AutoGenerateColumns="False" DataSource='<%# ClientsDataView %>'
				OnRowDataBound="ClientsGridView_RowDataBound" AllowSorting="True" OnRowCreated="ClientsGridView_RowCreated"
				OnSorting="ClientsGridView_Sorting">
				<Columns>
					<asp:TemplateField HeaderText="Биллинг код" SortExpression="billingcode">
						<ItemTemplate>
							<asp:HyperLink ID="HyperLink1" 
											runat="server" 
											Enabled='<%# SecurityContext.Administrator.HavePermisions(PermissionType.Billing) %>' 
											Text='<%# Bind("BillingCode") %>' 
											NavigateUrl='<%# String.Format("Billing/edit.rails?ClientCode={0}", Eval("bfc")) %>' />
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="firmcode" HeaderText="Код" SortExpression="firmcode"></asp:BoundField>
					<asp:TemplateField HeaderText="Наименование" SortExpression="ShortName">
						<ItemTemplate>
							<asp:HyperLink ID="HyperLink1" runat="server" 
								Text='<%# Bind("ShortName") %>' 
								NavigateUrl='<%# String.Format("Client/{0}", Eval("bfc")) %>'></asp:HyperLink>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="region" HeaderText="Регион" SortExpression="region"></asp:BoundField>
					<asp:TemplateField HeaderText="Подтвержденное обновление" SortExpression="FirstUpdate">
						<ItemTemplate>
							<asp:Label ID="Label1" runat="server" Text='<%# Eval("FirstUpdate").ToString() %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Неподтвержденное обновление" SortExpression="SecondUpdate">
						<ItemTemplate>
							<asp:Label ID="Label2" runat="server" Text='<%# Eval("SecondUpdate").ToString() %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="EXE" HeaderText="Версия" SortExpression="EXE"></asp:BoundField>
					<asp:BoundField DataField="UserName" HeaderText="Имя пользователя" SortExpression="UserName" />
					<asp:TemplateField HeaderText="Сегмент" SortExpression="FirmSegment">
						<ItemTemplate>
							<asp:Label ID="Label3" runat="server" Text='<%# Eval("FirmSegment").ToString() == "0" ? "Опт" : "Справка" %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:TemplateField HeaderText="Тип" SortExpression="FirmType">
						<ItemTemplate>
							<asp:Label ID="Label4" runat="server" Text='<%# Eval("FirmType").ToString() == "1" ? "Аптека" : "Поставщик" %>'></asp:Label>
						</ItemTemplate>
					</asp:TemplateField>
					<asp:BoundField DataField="IncludeType" HeaderText="Тип подчинения" NullDisplayText="-"
						SortExpression="IncludeType" />
				</Columns>
				<EmptyDataRowStyle Font-Bold="True" HorizontalAlign="Center" />
				<EmptyDataTemplate>
					Клиент не найден
				</EmptyDataTemplate>
			</asp:GridView>
			<table style="text-align: center; margin-top: 20px; margin-left: auto; margin-right: auto;">
				<tr>
				<td>
				<table runat=server id="Table4" visible=false>
					<tr>
						<td style="background-color:#FF6600; width:30px"></td>
						<td> - Клиент отключен</td>
					</tr>
					<tr>
						<td class="DisabledLogin"></td>
						<td> - Пользователь отключен</td>
					</tr>
					<tr>
						<td class="BlockedLogin"></td>
						<td> - Пользователь заблокирован</td>
					</tr>
					<tr>
						<td class="LoginNotExists"></td>
						<td> - Пользователь удален</td>
					</tr>
					<tr>
						<td class="not-base-client"></td>
						<td> - Скрытый или недоступный клиент</td>
					</tr>
					<tr>
						<td style="background-color:Gray"></td>
						<td> - Обновление более 2 суток назад</td>
					</tr>
				</table>
				<div style="margin-top: 20px;">
					<asp:Label ID="SearchTimeLabel" Visible="false" runat="server" />
				</div>
				</td>
				</tr>
			</table>
		</div>
	</form>
</asp:Content>