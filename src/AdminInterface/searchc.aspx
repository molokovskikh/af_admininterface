<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.searchc" CodePage="1251"
	CodeBehind="searchc.aspx.cs" Theme="Main" MasterPageFile="~/Main.Master" %>
<%@ Import Namespace="AdminInterface.Models.Security"%>
<%@ Import Namespace="AdminInterface.Security"%>



<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">
	<form id="form1" runat="server" defaultbutton="GoFind">
		<h3>
			Статистика работы клиента:
		</h3>
		<div style="text-align: center;">
			<table style="background-color: #e7f6e0;" id="Table2" cellspacing="0" cellpadding="0"
				width="450" border="0" onload="return SetSearchTitle();">
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
							<asp:ListItem Value="Login">Логин</asp:ListItem>
							<asp:ListItem Value="JuridicalName">Юридическое наименование</asp:ListItem>
						</asp:RadioButtonList>
					</td>
					<td rowspan="2">
						<asp:Button ID="GoFind" runat="server" Text="Найти" OnClick="GoFind_Click" CausesValidation="true" />
					</td>
				</tr>
				<tr>
					<td>
					</td>
					<td colspan="2" style="text-align:left;">
						<asp:DropDownList ID="ClientType" runat="server">
							<asp:ListItem>Все</asp:ListItem>
							<asp:ListItem>Аптеки</asp:ListItem>
							<asp:ListItem>Поставщики</asp:ListItem>
						</asp:DropDownList>
						<br />
						<asp:DropDownList ID="ClientState" runat="server">
							<asp:ListItem>Все</asp:ListItem>
							<asp:ListItem>Включен</asp:ListItem>
							<asp:ListItem>Отключен</asp:ListItem>
						</asp:DropDownList>
						<br />
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
		</div>
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
								NavigateUrl='<%# String.Format("Client/info.rails?cc={0}", Eval("bfc")) %>'></asp:HyperLink>
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
					<asp:BoundField DataField="EXE" HeaderText="EXE" SortExpression="EXE"></asp:BoundField>
					<asp:BoundField DataField="MDB" HeaderText="MDB" SortExpression="MDB" />
					<asp:BoundField DataField="UserName" HeaderText="Login" SortExpression="UserName" />
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
			<div style="text-align: center; margin-top: 20px;">
				<asp:Table ID="Table4" runat="server" Visible="False">
					<asp:TableRow runat="server">
						<asp:TableCell BackColor="#FF6600" Width="30px" runat="server"></asp:TableCell>
						<asp:TableCell Text=" - Клиент отключен" runat="server"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow runat="server">
						<asp:TableCell CssClass="DisabledLogin" runat="server"></asp:TableCell>
						<asp:TableCell Text=" - Учетная запись отключена" runat="server"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow runat="server">
						<asp:TableCell CssClass="BlockedLogin" runat="server"></asp:TableCell>
						<asp:TableCell Text=" - Учетная запись заблокированна" runat="server"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow ID="TableRow1" runat="server">
						<asp:TableCell ID="TableCell1" CssClass="LoginNotExists" runat="server"></asp:TableCell>
						<asp:TableCell ID="TableCell2" Text=" - Учетной записи не существует" runat="server"></asp:TableCell>
					</asp:TableRow>
					<asp:TableRow runat="server">
						<asp:TableCell BackColor="Gray" runat="server"></asp:TableCell>
						<asp:TableCell Text=" - Обновение более 2 суток назад" runat="server"></asp:TableCell>
					</asp:TableRow>
				</asp:Table>
				<div style="margin-top: 20px;">
					<asp:Label ID="SearchTimeLabel" Visible="false" runat="server" />
				</div>
			</div>
		</div>
	</form>
</asp:Content>