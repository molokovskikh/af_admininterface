<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.manageret" 
	CodePage="1251" CodeBehind="manageret.aspx.cs" 
	MaintainScrollPositionOnPostback="true" Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
	<style type="text/css">
		.tdCheckBox {vertical-align: middle; text-align: left;}
	</style>
	
	<script>
		jQuery(document).ready(function() {
			jQuery("#ctl00_MainContentPlaceHolder_IncludeGrid input[type=text], #ctl00_MainContentPlaceHolder_ShowClientsGrid input[type=text]").keypress(function(e) {
				if ((e.which && e.which == 13) || (e.keyCode && e.keyCode == 13)) {
					jQuery(this).parents('td').children('input[type=submit]').click()
					return false;
				} else {
					return true;
				}
			});
		});
	</script>
<form id="form1" runat="server" defaultfocus="ParametersSave" defaultbutton="ParametersSave">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="center" style="height: 754px">
					<div align="center">
						<strong>Конфигурация клиента</strong>
						<asp:Label ID="Name" runat="server" Font-Bold="True" />
					</div>
					<div align="center">
						<table id="Table2" style="width: 95%;" cellspacing="0" cellpadding="0" align="center" border="1">
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<strong>Операции</strong>
								</td>
							</tr>
							<tr>
								<td style="text-align:left" colspan=3>
									<asp:Button runat="Server" ID="Button2" Text="Отправить уведомления о регистрации поставщикам" OnClick="NotifySuppliers_Click" ValidationGroup="0" />
								</td>
							</tr>
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<strong>Общая настройка</strong>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:DropDownList ID="VisileStateList" runat="server">
										<asp:ListItem Text="Стандартный" Value="0"></asp:ListItem>
										<asp:ListItem Text="Скрытый" Value="2"></asp:ListItem>
									</asp:DropDownList>
									Тип клиента в интерфейсе поставщика
								</td>
							</tr>
							<tr id="NoiseRow" runat="server">
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="NoisedCosts" Text="Зашумлять цены"  AutoPostBack="true" oncheckedchanged="NoisedCosts_CheckedChanged" />
									<br />
									<label ID="NotNoisedSupplierLabel" runat="server">Не зашумлять прайс-листы поставщика</label>
									<asp:DropDownList ID="NotNoisedSupplier" runat="server" DataValueField="FirmCode" DataTextField="ShortName" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="ServiceClientCB" Text="Сотрудник АК Инфорум" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="RegisterCB" runat="server" Text="Реестр"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="RejectsCB" runat="server" Text="Забраковка" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:TextBox ID="MultiUserLevelTB" runat="server" BorderStyle="None" Width="33px"
										BackColor="LightGray" />-одновременных сеансов
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="AdvertisingLevelCB" runat="server" Text="Реклама" BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="WayBillCB" runat="server" Text="Накладные" BorderStyle="None" Enabled="False" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<asp:CheckBox ID="EnableUpdateCB" runat="server" Text="Автоматическое обновление версий"
										BorderStyle="None" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="AllowSubmitOrdersCB" Text="Разрешить пользователю самостоятельно активировать/дезактивировать механизм подтверждения заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="SubmitOrdersCB" Text="Активировать механизм подтверждения заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="OrdersVisualizationModeCB" Text="Показывать заказы подробно" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="CalculateLeaderCB" Text="Pассчитывать лидеров при получении заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="AllowDelayOfPaymentCB" Text="Активировать механизм отсрочки платежей" />
								</td>
							</tr>
							<tr>
								<td style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
									Таблица подчинений:
								</td>							
							</tr>
							<tr>
								<td>
									<asp:GridView ID="IncludeGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="IncludeGrid_RowCommand"
										OnRowDeleting="IncludeGrid_RowDeleting" OnRowDataBound="IncludeGrid_RowDataBound">
										<Columns>
											<asp:TemplateField>
												<HeaderTemplate>
													<asp:Button ID="AddButton" runat="server" Text="Добавить" CommandName="Add" />
												</HeaderTemplate>
												<ItemTemplate>
													<asp:Button ID="DeleteButton" runat="server" Text="Удалить" CommandName="Delete" />
												</ItemTemplate>
												<ItemStyle Width="10%" />
											</asp:TemplateField>
											<asp:TemplateField HeaderText="Родитель">
												<ItemTemplate>
													<asp:TextBox ID="SearchText" runat="server"></asp:TextBox>
													<asp:Button ID="SearchButton" runat="server" CommandName="Search" Text="Найти" ValidationGroup="0" />
													<asp:DropDownList ID="ParentList" runat="server" DataTextField="ShortName" DataValueField="FirmCode"
														Width="200px">
													</asp:DropDownList>
													<asp:CustomValidator ID="ParentValidator" runat="server" ControlToValidate="ParentList"
														ErrorMessage="Необходимо выбрать родителя." OnServerValidate="ParentValidator_ServerValidate" ClientValidationFunction="ValidateParent" ValidateEmptyText="True" ValidationGroup="1">*</asp:CustomValidator>
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField HeaderText="Тип подчинения">
												<ItemTemplate>
													<asp:DropDownList ID="IncludeTypeList" runat="server">
														<asp:ListItem Value="0">Базовый</asp:ListItem>
														<asp:ListItem Value="3">Базовый+</asp:ListItem>
														<asp:ListItem Value="1">Сеть</asp:ListItem>
														<asp:ListItem Value="2">Скрытый</asp:ListItem>
													</asp:DropDownList>
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
										<EmptyDataTemplate>
											<div style="text-align:left">
												<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="Подчинить клиента." />
											</div>
										</EmptyDataTemplate>
									</asp:GridView>
								</td>
							</tr>
							<tr>
								<td class="Title">
									Показываемые клиенты:
								</td>
							</tr>
							<tr>
								<td>
									<asp:GridView ID="ShowClientsGrid" runat="server" AutoGenerateColumns="False" OnRowCommand="ShowClientsGrid_RowCommand" OnRowDataBound="ShowClientsGrid_RowDataBound" OnRowDeleting="ShowClientsGrid_RowDeleting">
										<EmptyDataTemplate>
											<div style="text-align:left">
												<asp:Button ID="AddButton" runat="server" CommandName="Add" Text="Добавить клиента" />
											</div>
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
													<asp:CustomValidator ID="ShowCleintsValidator" 
																		 runat="server" 
																		 ControlToValidate="ShowClientsList" 
																		 ErrorMessage="Необходимо выбрать клиента." 
																		 OnServerValidate="ParentValidator_ServerValidate" 
																		 ValidateEmptyText="True" 
																		 ValidationGroup="1">*</asp:CustomValidator>
												</ItemTemplate>
											</asp:TemplateField>
											<asp:TemplateField HeaderText="Тип показываемого клиента">
												<ItemTemplate>
													<asp:DropDownList ID="ShowType" runat="server" SelectedValue='<%# Eval("ShowType") %>'>
														<asp:ListItem Value="0">Базовый</asp:ListItem>
														<asp:ListItem Value="1">Заказы</asp:ListItem>
														<asp:ListItem Value="2">Все</asp:ListItem>
													</asp:DropDownList>
												</ItemTemplate>
											</asp:TemplateField>
										</Columns>
									</asp:GridView>
								</td>
							</tr>
							<tr>
								<td  style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
									Права пользователя программы АналитФармация:
								</td>
							</tr>
							<tr>
								<td style="text-align:left;">
									<table cellpadding=0 cellspacing=0 style="border-collapse:collapse; width: 100%">
									<tr>
										<tr style="text-align: center; background-color: #f0f8ff; font-weight: bold;">
											<td style="border: solid 1px black">
												Выгрузка в Excel
											</td>
											<td style="border: solid 1px black">
												Печать
											</td>
										</tr>
										<td>
											<asp:CheckBoxList ID="ExportRulesList" runat="server" DataTextField="DisplayName" DataValueField="Id" BorderStyle="None">
											</asp:CheckBoxList>
										</td>
										<td style="vertical-align:top;">
											<asp:CheckBoxList ID="PrintRulesList" runat="server" DataTextField="DisplayName" DataValueField="Id" BorderStyle="None">
											</asp:CheckBoxList>
										</td>
									</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
									<p style="text-align: center;">
										<strong>Региональная настройка</strong></p>
								</td>
							</tr>
							<tr>
								<td style="height: 8px" valign="middle" align="left" colspan="3">
									<label>Домашний регион:</label>
									<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
										AutoPostBack="True" DataTextField="Region" DataValueField="RegionCode">
									</asp:DropDownList>
									<asp:CheckBox ID="AllRegCB" runat="server" OnCheckedChanged="AllRegCB_CheckedChanged"
										Text="Показать все регионы" AutoPostBack="True"></asp:CheckBox></td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 79px">
									<table id="Table3" cellspacing="0" cellpadding="0" border="0">
										<tr>
											<td width="200">
												<strong>Доступные регионы:</strong>
											</td>
											<td width="200">
												<strong>Регионы заказа:</strong>
											</td>
										</tr>
										<tr>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="WRList" runat="server" BorderStyle="None"
													DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="OrderList" runat="server" BorderStyle="None" 
													DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="right" colspan="3" style="height: 17px">
									<asp:Button ID="ParametersSave" runat="server" OnClick="ParametersSave_Click" Text="Применить" ValidationGroup="1"></asp:Button>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 15px">
									<asp:Label ID="ResultL" runat="server" Font-Bold="True" ForeColor="Green" Font-Italic="True"></asp:Label></td>
							</tr>
						</table>
					</div>
				</td>
			</tr>
		</table>
	</form>
</asp:Content>