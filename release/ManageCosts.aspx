<%@ Page Language="c#" AutoEventWireup="true" CodeBehind="ManageCosts.aspx.cs" Inherits="AddUser.ManageCosts" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Система управления "Оптовик" - Аналитическая Компания "Инфорум"</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
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
						DataSource="<%# Costs %>" HorizontalAlign="Center" AutoGenerateColumns="False">
						<FooterStyle HorizontalAlign="Center"></FooterStyle>
						<AlternatingItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#EEF8FF">
						</AlternatingItemStyle>
						<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#F6F6F6"></ItemStyle>
						<HeaderStyle Font-Bold="True" HorizontalAlign="Center" BackColor="#EBEBEB"></HeaderStyle>
						<Columns>
							<asp:TemplateColumn SortExpression="CostName" HeaderText="Наименование">
								<HeaderStyle Width="160px"></HeaderStyle>
								<ItemTemplate>
									<asp:TextBox ID="CostName" Font-Size="8pt" Font-Names="Verdana" runat="server" Width="150px"
										Text='<%#DataBinder.Eval(Container, "DataItem.CostName") %>'>
									</asp:TextBox>
									<asp:RequiredFieldValidator ID="rfv1" ControlToValidate="CostName" runat="server"
										Text="*" ErrorMessage="Необходимо указать наименование цены в полях, указанных знаком *."
										Visible="true"></asp:RequiredFieldValidator>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn HeaderText="Включить">
								<HeaderStyle Width="100px"></HeaderStyle>
								<ItemTemplate>
									<asp:CheckBox ID="Ena" runat="server" Checked='<%# Convert.ToBoolean(Eval("Enabled"))%>'>
									</asp:CheckBox>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn HeaderText="Публиковать">
								<HeaderStyle Width="100px"></HeaderStyle>
								<ItemTemplate>
									<asp:CheckBox ID="Pub" runat="server" Checked='<%# Convert.ToBoolean(Eval("AgencyEnabled"))%>'>
									</asp:CheckBox>
								</ItemTemplate>
							</asp:TemplateColumn>
							<asp:TemplateColumn SortExpression="BaseCost" HeaderText="Базовая&lt;br&gt;цена">
								<HeaderStyle Width="80px"></HeaderStyle>
								<ItemTemplate>
									<input value='<%# DataBinder.Eval(Container, "DataItem.CostCode")%>' type="radio" <%# IsChecked(Convert.ToBoolean(DataBinder.Eval(Container, "DataItem.BaseCost"))) %> name="uid" />
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
					<asp:GridView ID="PriceRegionSettings" runat="server" Font-Names="Verdana" Font-Size="8pt" BorderColor="#DADADA" AutoGenerateColumns="False" DataMember="PriceRegionSettings" HorizontalAlign="Center">
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
			<tr>
				<td align="center">
					<font face="Arial, Helvetica, sans-serif"><font face="veranda" color="#000000" size="1">
						© АК "</font> <a href="http://www.analit.net/"><font face="veranda" size="1">Инфорум</font></a><font
							face="veranda" color="#000000" size="1">" 2005</font></font></td>
			</tr>
		</table>
	</form>
</body>
</html>
