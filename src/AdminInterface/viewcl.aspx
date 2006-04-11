<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.viewcl" CodeFile="viewcl.aspx.vb" %>
<HTML>
	<HEAD>
		<title>Интерфейс управления клиентами</title>
		<meta content="True" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
		<meta http-equiv="Content-Type" content="text/html; charset=windows-1251">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="95%" align="center" border="0">
				<TR>
					<TD align="center"><asp:label id="HeaderLB" runat="server" Font-Names="Verdana" Font-Size="10pt" Font-Bold="True"></asp:label></TD>
				</TR>
				<TR>
					<TD align="center"><FONT face="Verdana" size="2">Клиентов, отвечающих условиям выборки:</FONT>
						<asp:label id="CountLB" runat="server" Font-Names="Verdana" Font-Size="8pt"></asp:label></TD>
				</TR>
				<TR>
					<TD align="center"><asp:datagrid id=CLList runat="server" Font-Names="Verdana" Font-Size="8pt" CellPadding="0" AutoGenerateColumns="False" BorderColor="#DADADA" PageSize="20" DataSource="<%# DataTable1 %>">
							<FooterStyle Font-Names="Verdana"></FooterStyle>
							<AlternatingItemStyle BackColor="#F6F6F6"></AlternatingItemStyle>
							<ItemStyle HorizontalAlign="Center" BackColor="#EEF8FF"></ItemStyle>
							<HeaderStyle Font-Size="8pt" Font-Names="Verdana" Font-Bold="True" HorizontalAlign="Center" VerticalAlign="Middle"
								BackColor="#EBEBEB"></HeaderStyle>
							<Columns>
								<asp:TemplateColumn HeaderText="Время">
									<ItemTemplate>
										&nbsp;
										<asp:Label runat="server" Text='<%# FormatDateTime(DataBinder.Eval(Container, "DataItem.LogTime"), DateFormat.LongTime) %>'>
										</asp:Label>&nbsp;
									</ItemTemplate>
									<EditItemTemplate>
										<asp:TextBox runat="server" Text='<%# DataBinder.Eval(Container, "DataItem.LogTime") %>'>
										</asp:TextBox>
									</EditItemTemplate>
								</asp:TemplateColumn>
								<asp:HyperLinkColumn DataNavigateUrlField="FirmCode" DataNavigateUrlFormatString="info.aspx?cc={0}" DataTextField="ShortName"
									HeaderText="Клиент"></asp:HyperLinkColumn>
								<asp:BoundColumn DataField="Region" SortExpression="Region" HeaderText="Регион"></asp:BoundColumn>
								<asp:BoundColumn DataField="Addition" HeaderText="Описание"></asp:BoundColumn>
							</Columns>
							<PagerStyle VerticalAlign="Middle" Font-Size="9pt" Font-Names="Verdana" Font-Bold="True" HorizontalAlign="Center"
								Position="TopAndBottom" Mode="NumericPages"></PagerStyle>
						</asp:datagrid></TD>
				</TR>
			</TABLE>
		</form>
	</body>
</HTML>
