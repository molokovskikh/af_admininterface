<%@ Page Language="VB" AutoEventWireup="false" CodeFile="ManageCosts.aspx.vb" Inherits="ManageCosts" %>

<HTML>
	<HEAD>
		<title>Система управления "Оптовик" - Аналитическая Компания "Инфорум"</title>
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="98%" border="0">
				<TR>
					<TD style="HEIGHT: 19px" align="center"><FONT face="Verdana" size="2"><STRONG>Настройка цен 
								для прайс - листа</STRONG> </FONT>
						<asp:label id="PriceNameLB" runat="server" Font-Bold="True" Font-Names="Verdana" Font-Size="9pt"></asp:label></TD>
				</TR>
				<TR>
					<TD align="center"><asp:datagrid id=CostsDG runat="server" Font-Names="Verdana" Font-Size="8pt" BorderColor="#DADADA" DataSource="<%# Costs %>" HorizontalAlign="Center" AutoGenerateColumns="False">
							<FooterStyle HorizontalAlign="Center"></FooterStyle>
							<AlternatingItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#EEF8FF"></AlternatingItemStyle>
							<ItemStyle HorizontalAlign="Center" VerticalAlign="Middle" BackColor="#F6F6F6"></ItemStyle>
							<HeaderStyle Font-Bold="True" HorizontalAlign="Center" BackColor="#EBEBEB"></HeaderStyle>
							<Columns>
								<asp:TemplateColumn SortExpression="CostName" HeaderText="Наименование">
									<HeaderStyle Width="160px"></HeaderStyle>
									<ItemTemplate>
										<asp:TextBox id="CostName" Font-Size="8pt" Font-Names="Verdana" runat="server" Width="150px" text='<%#DataBinder.eval(Container, "DataItem.CostName") %>' >
 >
												</asp:TextBox>
										<asp:RequiredFieldValidator id="rfv1" ControlToValidate="CostName" runat="server" text="*" ErrorMessage="Необходимо указать наименование цены в полях, указанных знаком *."
											Visible="true"></asp:RequiredFieldValidator>
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn HeaderText="Включить">
									<HeaderStyle Width="100px"></HeaderStyle>
									<ItemTemplate>
										<asp:CheckBox id="Ena" runat="server" Checked='<%# DataBinder.Eval(Container, "DataItem.Enabled")%>'></asp:CheckBox>
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:TemplateColumn HeaderText="Публиковать">
									<HeaderStyle Width="100px"></HeaderStyle>
									<ItemTemplate>
										<asp:CheckBox id="Pub" runat="server" Checked='<%# DataBinder.Eval(Container, "DataItem.AgencyEnabled")%>'></asp:CheckBox>
									</ItemTemplate>
								</asp:TemplateColumn>
								
								<asp:TemplateColumn SortExpression="BaseCost" HeaderText="Базовая&lt;br&gt;цена">
									<HeaderStyle Width="80px"></HeaderStyle>
									<ItemTemplate>
										<input value='<%# DataBinder.Eval(Container, "DataItem.CostCode")%>' type="radio" name="uid" <%# ischecked(DataBinder.Eval(Container, "DataItem.BaseCost")) %> >
									</ItemTemplate>
								</asp:TemplateColumn>
								<asp:BoundColumn DataField="CostID" SortExpression="CostID" HeaderText="Идентификатор">
									<HeaderStyle Width="150px"></HeaderStyle>
								</asp:BoundColumn>
								<asp:BoundColumn Visible="False" DataField="CostCode" HeaderText="CostCode"></asp:BoundColumn>
							</Columns>
						</asp:datagrid>
                        <asp:LinkButton ID="CreateCost" runat="server" Font-Names="Verdana" Font-Size="8pt">Новая ценовая колонка</asp:LinkButton></TD>
				</TR>
				<TR>
					<TD height="10"></TD>
				</TR>
				<TR>
					<TD align="center"><asp:label id="UpdateLB" runat="server" Font-Bold="True" Font-Names="Verdana" Font-Size="8pt"
							Font-Italic="True" ForeColor="Green"></asp:label></TD>
				</TR>
				<TR>
					<TD height="10"></TD>
				</TR>
				<TR>
					<TD align="center"><asp:button id="PostB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Применить"></asp:button></TD>
				</TR>
				<TR>
					<TD align="center" style="HEIGHT: 14px"><asp:label id="ErrLB" runat="server" Font-Names="Verdana" Font-Size="9pt" ForeColor="#C00000"></asp:label></TD>
				</TR>
				<TR>
					<TD align="center"><FONT face="Arial, Helvetica, sans-serif"><FONT face="veranda" color="#000000" size="1">© 
								АК "</FONT> <A href="http://www.analit.net/"><FONT face="veranda" size="1">Инфорум</FONT></A><FONT face="veranda" color="#000000" size="1">" 
								2005</FONT></FONT></TD>
				</TR>
			</TABLE>
		</form>
	</body>
</HTML>
