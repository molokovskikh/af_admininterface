<%@ Page Language="c#" AutoEventWireup="true" 
	CodeBehind="ManageCosts.aspx.cs" Inherits="AddUser.ManageCosts" 
	Theme="Main" MasterPageFile="~/Main.Master" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="MainContentPlaceHolder">	
	<form id="form1" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="98%" border="0">
			<tr>
				<td style="height: 19px" align="center">
					<font face="Verdana" size="2"><strong>Настройка цен для прайс - листа</strong> </font>
					<asp:Label ID="PriceNameLB" runat="server" Font-Bold="True" Font-Names="Verdana"
						Font-Size="9pt"></asp:Label></td>
			</tr>
			<tr>
				<td align="center" style="height: 163px">
					<asp:DataGrid ID="CostsDG" runat="server" Font-Names="Verdana" Font-Size="8pt" BorderColor="#DADADA"
						DataMember="Costs"
						HorizontalAlign="Center" AutoGenerateColumns="False"
						ondeletecommand="CostsDG_DeleteCommand">
						<FooterStyle HorizontalAlign="Center"></FooterStyle>
						<AlternatingItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#EEF8FF">
						</AlternatingItemStyle>
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#F6F6F6"></ItemStyle>
						<HeaderStyle Font-Bold="True" HorizontalAlign="Center" BackColor="#EBEBEB"></HeaderStyle>
						<Columns>
							<asp:TemplateColumn>
								<ItemTemplate>
									<asp:HiddenField ID="CostCode" runat="server" Value='<%# Eval("CostCode") %>' />
									<asp:Label runat="server" CssClass="ValidationErrorMessage" Visible='<%# ShowWarning(Convert.ToUInt32(Eval("CostCode"))) %>'>
										Внимание! Ценовая колонка настроена.<br>
									</asp:Label>
									<asp:Button ID="DeletButton" CommandArgument='<%# Bind("CostCode") %>' runat="server"
												CausesValidation="False" CommandName="Delete" Text='<%# DeleteLabel(Convert.ToUInt32(Eval("CostCode"))) %>'
												Visible='<%# CanDelete(Eval("RegionBaseCode")) %>'/>
								</ItemTemplate>
								<ItemStyle HorizontalAlign="Left" />
							</asp:TemplateColumn>
							<asp:TemplateColumn SortExpression="CostName" HeaderText="Наименование">
								<HeaderStyle Width="160px"></HeaderStyle>
								<ItemTemplate>
									<asp:TextBox ID="CostName" Font-Size="8pt" Font-Names="Verdana" runat="server" Width="150px"
										Text='<%# DataBinder.Eval(Container, "DataItem.CostName") %>'>
									</asp:TextBox>
									<asp:RequiredFieldValidator ID="rfv1" ControlToValidate="CostName" runat="server"
										Text="*" ErrorMessage="Необходимо указать наименование цены в полях, указанных знаком *."
										Visible="true"></asp:RequiredFieldValidator>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:BoundColumn HeaderText="Дата ценовой колонки" DataField="PriceDate" />
							<asp:TemplateColumn HeaderText="Включить">
								<HeaderStyle Width="100px"></HeaderStyle>
								<ItemTemplate>
									<asp:CheckBox ID="Ena" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled")) %>'>
									</asp:CheckBox>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn HeaderText="Публиковать">
								<HeaderStyle Width="100px"></HeaderStyle>
								<ItemTemplate>
									<asp:CheckBox ID="Pub" runat="server" Checked='<%# Convert.ToBoolean(Eval("AgencyEnabled")) %>'>
									</asp:CheckBox>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:BoundColumn DataField="CostID" SortExpression="CostID" HeaderText="Идентификатор">
								<HeaderStyle Width="150px"></HeaderStyle>
							</asp:BoundColumn>
							<asp:BoundColumn Visible="False" DataField="CostCode" HeaderText="CostCode"></asp:BoundColumn>
						</Columns>
					</asp:DataGrid>
					<asp:LinkButton ID="CreateCost" runat="server" OnClick="CreateCost_Click" Font-Names="Verdana"
						Font-Size="8pt">Новая ценовая колонка</asp:LinkButton></td>
			</tr>
			<tr>
				<td height="10">
				</td>
			</tr>
			<tr align="center">
				<td>
					<asp:GridView ID="PriceRegionSettings" runat="server" Font-Names="Verdana" Font-Size="8pt" BorderColor="#DADADA" AutoGenerateColumns="False" DataMember="PriceRegionSettings" HorizontalAlign="Center" onrowdatabound="PriceRegionSettings_RowDataBound">
						<FooterStyle HorizontalAlign="Center"></FooterStyle>
						<AlternatingRowStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#EEF8FF" />
						<RowStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#F6F6F6" />
						<HeaderStyle Font-Bold="True" HorizontalAlign="Center" BackColor="#EBEBEB" />

						<Columns>
							<asp:BoundField HeaderText="Регион" DataField="Region" />
							<asp:TemplateField HeaderText="Вкл.">
								<ItemTemplate>
									<asp:CheckBox ID="EnableCheck" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled")) %>' />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Наценка">
								<ItemTemplate>
									<asp:TextBox ID="UpCostText" runat="server" Text='<%# Eval("UpCost") %>' />
									<asp:RegularExpressionValidator ID="UpCostValidator" runat="server" ControlToValidate="UpCostText"
										ErrorMessage="*" ValidationExpression="^(-)?\d+(\,\d+)?$" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Сумма минимально заказа">
								<ItemTemplate>
									<asp:TextBox ID="MinReqText" runat="server" Text='<%# Eval("MinReq") %>' />
									<asp:RegularExpressionValidator ID="MinReqValidator" runat="server" ErrorMessage="*" ControlToValidate="MinReqText" ValidationExpression="^\d+(\,\d+)?$" />
								</ItemTemplate>
							</asp:TemplateField>
							<asp:TemplateField HeaderText="Базовая цена" SortExpression="enabled">
									<ItemTemplate>
										<asp:DropDownList ID="RegionalBaseCost" runat="server"  DataTextField="Name" DataValueField="Id"/>
									</ItemTemplate>
							</asp:TemplateField>
						</Columns>
					
					</asp:GridView>
				</td>
			</tr>
			<tr>
				<td align="center">
					<asp:Label ID="UpdateLB" runat="server" Font-Bold="True" Font-Names="Verdana" Font-Size="8pt"
						Font-Italic="True" ForeColor="Green"></asp:Label></td>
			</tr>
			<tr>
				<td height="10">
				</td>
			</tr>
			<tr>
				<td align="center">
					<asp:Button ID="PostB" runat="server" OnClick="PostB_Click" Font-Names="Verdana"
						Font-Size="8pt" Text="Применить"></asp:Button></td>
			</tr>
			<tr>
				<td align="center" style="height: 14px">
					<asp:Label ID="ErrLB" runat="server" Font-Names="Verdana" Font-Size="9pt" ForeColor="#C00000"></asp:Label></td>
			</tr>
		</table>
	</form>
</asp:Content>
