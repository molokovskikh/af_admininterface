<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.managep" CodeFile="managep.aspx.vb" %>
<HTML>
	<HEAD>
		<title>Конфигурация пользователя</title>
		<META http-equiv="Content-Type" content="text/html; charset=windows-1251">
		<meta content="False" name="vs_showGrid">
		<meta content="http://schemas.microsoft.com/intellisense/ie5" name="vs_targetSchema">
	</HEAD>
	<body vLink="#ab51cc" aLink="#0093e1" link="#0093e1" bgColor="#ffffff">
		<form id="Form1" method="post" runat="server">
			<TABLE id="Table1" cellSpacing="0" cellPadding="0" width="100%" border="0">
				<TR>
					<TD vAlign="top" align="center">
						<DIV align="center"><FONT face="Verdana"><FONT size="2"><STRONG>Конфигурация клиента</STRONG>
								</FONT></FONT>
							<asp:label id="NameLB" runat="server" Font-Size="9pt" Font-Names="Verdana" Font-Bold="True"></asp:label></DIV>
					</TD>
				</TR>
				<TR>
					<TD vAlign="top" align="center">
						<TABLE id="Table4" borderColor="#dadada" cellSpacing="0" cellPadding="0" width="95%" align="center"
							border="1">
							<TBODY>
								<TR>
									<TD vAlign="middle" align="left" bgColor="#f0f8ff" colSpan="3">
										<P align="center"><FONT face="Verdana" size="2"><STRONG>Прайс-листы:</STRONG></FONT></P>
									</TD>
								</TR>
								<TR>
									<TD style="HEIGHT: 8px" vAlign="middle" align="center" colSpan="3"><br>
										<asp:repeater id=R runat="server" DataSource="<%# PD %>">
											<ItemTemplate>
												<tr bgcolor="#EEF8FF" style="FONT-SIZE: 8pt">
													<td>
														<%#DataBinder.Eval(Container, "DataItem.PriceCode") %>
													</td>
													<td>
															<asp:HyperLink ID="HL" runat="server" NavigateUrl='<%#"managecosts.aspx?pc=" & DataBinder.Eval(Container, "DataItem.PriceCode")%>'><%#DataBinder.Eval(Container, "DataItem.PriceName") %></asp:HyperLink>
													
													</td>
													<td>
														<%#DataBinder.Eval(Container, "DataItem.DateCurPrice")%>
													</td>
													<td>
														<%#DataBinder.Eval(Container, "DataItem.DateLastForm") %>
													</td>
													<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AgencyEnabled") %>' >
														</asp:checkbox>
													</td>
													<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.Enabled") %>' >
														</asp:checkbox>
													</td>
																										
													<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AlowInt") %>' >
														</asp:checkbox>
													</td>
													<td align="center" bgcolor="#ebebeb">
														<asp:checkbox runat="server" Checked="False"></asp:checkbox>
													</td>
												</tr>
											</ItemTemplate>
											<AlternatingItemTemplate>
												<tr bgcolor="#F6F6F6" style="FONT-SIZE: 8pt">
													<td >
														<%#DataBinder.Eval(Container, "DataItem.PriceCode") %>
																											</td>
													<td>
														<asp:HyperLink ID="HL" runat="server" NavigateUrl='<%#"managecosts.aspx?pc=" & DataBinder.Eval(Container, "DataItem.PriceCode")%>'><%#DataBinder.Eval(Container, "DataItem.PriceName") %></asp:HyperLink>
													</td>
													<td >
														<%#DataBinder.Eval(Container, "DataItem.DateCurPrice")%>
													</td>
													<td>
														<%#DataBinder.Eval(Container, "DataItem.DateLastForm") %>
													</td>
													<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AgencyEnabled") %>' ID="Checkbox1" >
														</asp:checkbox>
													</td>
													<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.Enabled") %>' ID="Checkbox2" >
														</asp:checkbox>
													</td>
															<td align="center">
														<asp:checkbox runat="server" Checked='<%#DataBinder.Eval(Container, "DataItem.AlowInt") %>' ID="Checkbox4" >
														</asp:checkbox>
													</td>
													<td align="center" bgcolor="#ebebeb">
														<asp:checkbox runat="server" Checked="False" ID="Checkbox5"></asp:checkbox>
													</td>
												</tr>
											</AlternatingItemTemplate>
											<HeaderTemplate>
												<table cellspacing="0" cellpadding="0" rules="all" bordercolor="#DADADA" border="1" style="FONT-SIZE: 8pt; BORDER-LEFT-COLOR: #dadada; BORDER-BOTTOM-COLOR: #dadada; BORDER-TOP-STYLE: solid; BORDER-TOP-COLOR: #dadada; FONT-FAMILY: Verdana; BORDER-RIGHT-STYLE: solid; BORDER-LEFT-STYLE: solid; BORDER-COLLAPSE: collapse; BORDER-RIGHT-COLOR: #dadada; BORDER-BOTTOM-STYLE: solid">
													<tr bgcolor="#ebebeb">
														<th width="70">
															Код
														</th>
														<th width="140">
															Наименование
														</th>
														<th width="140">
															Получен
														</th>
														<th width="140">
															Формализован
														</th>
														<th width="70">
															Вкл.
														</th>
														<th width="70">
															В работе
														</th>
														
														<th width="70">
															Интегр.
														</th>
														<th width="70">
															Удал.
														</th>
													</tr>
											</HeaderTemplate>
											<FooterTemplate>
												<tr bgcolor="#ebebeb" style="FONT-WEIGHT: bold">
													<td colspan="5" align="right">
														&nbsp;
													</td>
													<td colspan="3" align="center">
														<asp:button id="Button2" runat="server" Font-Names="Verdana" BorderStyle="None" Text="Новый"
															Font-Size="8pt"></asp:button>
													</td>
												</tr>
						</TABLE>
						</FooterTemplate> </asp:repeater><br>
                                        </TD>
				</TR>
				<TR>
					<TD vAlign="middle" align="right" colSpan="3"><asp:button id="Button1" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Применить"
							BorderStyle="None"></asp:button></TD>
				</TR>
			</TABLE>
			</TD></TR>
			<TR>
				<TD vAlign="top" align="center">
					<DIV align="center">&nbsp;</DIV>
					<DIV align="center">
						<TABLE id="Table2" borderColor="#dadada" cellSpacing="0" cellPadding="0" width="95%" align="center"
							border="1">
							<TR>
								<TD vAlign="middle" align="left" bgColor="#f0f8ff" colSpan="3">
									<P align="center"><FONT face="Verdana" size="2"><STRONG>Региональная настройка</STRONG></FONT></P>
								</TD>
							</TR>
							<TR>
								<TD style="HEIGHT: 8px" vAlign="middle" align="left" colSpan="3"><FONT face="Verdana" size="2">Домашний 
										регион:</FONT>
									<asp:dropdownlist id=RegionDD runat="server" Font-Size="8pt" Font-Names="Verdana" DataSource="<%# Regions %>" AutoPostBack="True" DataTextField="Region" DataValueField="RegionCode">
									</asp:dropdownlist></TD>
							</TR>
							<TR>
								<TD vAlign="middle" align="center" colSpan="3">
									<TABLE id="Table3" cellSpacing="0" cellPadding="0" border="0">
										<TR>
											<TD width="210"><FONT face="Arial" size="2"><STRONG><FONT face="Verdana">Показываемые регионы:</FONT></STRONG><FONT face="Times New Roman" size="3">&nbsp;</FONT></FONT></TD>
											<TD width="210"><FONT face="Verdana" size="2"><STRONG>Доступные регионы:</STRONG></FONT></TD>
										</TR>
										<TR>
											<TD vAlign="top" align="left" width="210"><asp:checkboxlist id=ShowList runat="server" Font-Size="8pt" Font-Names="Verdana" DataSource="<%# WorkReg %>" BorderStyle="None" DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0"></asp:checkboxlist></TD>
											<TD vAlign="top" align="left" width="210"><asp:checkboxlist id=WRList runat="server" Font-Size="8pt" Font-Names="Verdana" DataSource="<%# WorkReg %>" BorderStyle="None" DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0"></asp:checkboxlist></TD>
										</TR>
									</TABLE>
								</TD>
							</TR>
							<TR>
								<TD vAlign="middle" align="right" colSpan="3"><asp:button id="ParametersSave" runat="server" Font-Size="10pt" Font-Names="Arial" Text="Применить"
										BorderStyle="None"></asp:button></TD>
							</TR>
							<TR>
								<TD vAlign="middle" align="center" colSpan="3"><asp:label id="ResultL" runat="server" Font-Size="10pt" Font-Names="Arial" Font-Bold="True"
										ForeColor="Green" Font-Italic="True"></asp:label></TD>
							</TR>
						</TABLE>
					</DIV>
					<DIV align="center"><FONT face="Arial"><STRONG></STRONG></FONT></DIV>
					<P align="center"><FONT face="Arial, Helvetica, sans-serif"><FONT color="#000000" size="1">© 
								АК "</FONT> <A href="http://www.analit.net/"><FONT color="#800080" size="1">Инфорум</FONT></A><FONT color="#000000" size="1">" 
								2004</FONT></FONT></P>
				</TD>
			</TR>
			</TABLE></form>
	</body>
</HTML>
