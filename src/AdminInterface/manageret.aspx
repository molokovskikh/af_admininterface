<%@ Page Language="vb" AutoEventWireup="false" Inherits="AddUser.manageret" codePage="1251" CodeFile="manageret.aspx.vb" %>
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
							<asp:label id="Name" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="10pt"></asp:label></DIV>
						<DIV align="center">
							<TABLE id="Table2" borderColor="#dadada" cellSpacing="0" cellPadding="0" width="95%" align="center"
								border="1">
								<TR align="center" bgColor="mintcream">
									<TD bgColor="aliceblue" height="20"><FONT face="Verdana" size="2"><STRONG>Общая настройка</STRONG></FONT></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="InvisibleCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text='"Невидимый" клиент'
													BorderStyle="None"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="RegisterCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Реестр"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="RejectsCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Забраковка"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="DocumentsCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Документы"
													Enabled="False"></asp:checkbox></DIV>
										</FONT>
									</TD>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:textbox id="MultiUserLevelTB" runat="server" Font-Names="Verdana" Font-Size="8pt" BorderStyle="None"
													Width="33px" BackColor="LightGray"></asp:textbox><FONT face="Verdana">&nbsp;- 
													одновременных сеансов</FONT></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="AdvertisingLevelCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Реклама"
													BorderStyle="None"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="WayBillCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Накладные"
													BorderStyle="None" Enabled="False"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="ChangeSegmentCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Изменение сегмента"
													BorderStyle="None"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">
											<DIV><asp:checkbox id="EnableUpdateCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Автоматическое обновление версий"
													BorderStyle="None"></asp:checkbox></DIV>
										</FONT>
									</TD>
								</TR>
								<TR>
									<TD style="HEIGHT: 12px" vAlign="middle" align="left" colSpan="3"><asp:checkbox id="CheckCopyIDCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Проверять уникальный идентификатор"
											BorderStyle="None" Enabled="False"></asp:checkbox></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3" style="HEIGHT: 22px">
										<asp:CheckBox id="ResetCopyIDCB" runat="server" Font-Names="Verdana" Text="Сбросить уникальный идентификатор, причина:"
											Font-Size="8pt" Enabled="False"></asp:CheckBox>
										<asp:textbox id="CopyIDWTB" runat="server" Font-Names="Verdana" BorderStyle="None" Font-Size="8pt"
											BackColor="LightGray" Enabled="False"></asp:textbox>
										<asp:Label id="IDSetL" runat="server" Font-Size="8pt" Font-Names="Verdana" ForeColor="Green">Идентификатор не присвоен</asp:Label></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" colSpan="3" style="HEIGHT: 23px"><asp:checkbox id="AlowCumulativeCB" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Разрешить 1 кумулятивное обновление, причина обновления:"
											BorderStyle="None" Enabled="False" Checked="True"></asp:checkbox><FONT face="Arial" size="2"><asp:textbox id="CUWTB" runat="server" BorderStyle="None" Enabled="False" Font-Names="Verdana"
												Font-Size="8pt" BackColor="LightGray"></asp:textbox>
											<asp:Label id="CUSetL" runat="server" Font-Size="8pt" Font-Names="Verdana" ForeColor="Green">Кумулятивное обновление разрешено</asp:Label></FONT></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="left" bgColor="#f0f8ff" colSpan="3">
										<P align="center"><FONT face="Verdana" size="2"><STRONG>Региональная настройка</STRONG></FONT></P>
									</TD>
								</TR>
								<TR>
									<TD style="HEIGHT: 8px" vAlign="middle" align="left" colSpan="3"><FONT face="Arial" size="2">Домашний 
											регион:</FONT>
										<asp:dropdownlist id=RegionDD runat="server" Font-Names="Verdana" Font-Size="8pt" AutoPostBack="True" DataSource="<%# admin %>" DataTextField="Region" DataValueField="RegionCode">
										</asp:dropdownlist>
										<asp:CheckBox id="AllRegCB" runat="server" Font-Size="8pt" Font-Names="Verdana" Text="Показать все регионы"
											AutoPostBack="True"></asp:CheckBox></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="center" colSpan="3">
										<TABLE id="Table3" cellSpacing="0" cellPadding="0" border="0">
											<TR>
												<TD width="200"><FONT face="Arial" size="2"><STRONG><FONT face="Verdana">Показываемые регионы:</FONT></STRONG><FONT face="Times New Roman" size="3">&nbsp;</FONT></FONT></TD>
												<TD width="200"><FONT face="Verdana" size="2"><STRONG>Доступные регионы:</STRONG></FONT></TD>
												<TD width="200"><FONT face="Verdana" size="2"><STRONG>Регионы заказа:</STRONG></FONT></TD>
											</TR>
											<TR>
												<TD vAlign="top" align="left" width="200"><asp:checkboxlist id=ShowList runat="server" Font-Names="Verdana" Font-Size="8pt" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0"></asp:checkboxlist></TD>
												<TD vAlign="top" align="left" width="200"><asp:checkboxlist id=WRList runat="server" Font-Names="Verdana" Font-Size="8pt" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0"></asp:checkboxlist></TD>
												<TD vAlign="top" align="left" width="200"><asp:checkboxlist id=OrderList runat="server" Font-Names="Verdana" Font-Size="8pt" BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode" CellSpacing="0" CellPadding="0"></asp:checkboxlist></TD>
											</TR>
										</TABLE>
									</TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="right" colSpan="3" style="HEIGHT: 17px"><asp:button id="ParametersSave" runat="server" Font-Names="Verdana" Font-Size="8pt" Text="Применить"
											BorderStyle="None"></asp:button></TD>
								</TR>
								<TR>
									<TD vAlign="middle" align="center" colSpan="3"><asp:label id="ResultL" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="8pt"
											ForeColor="Green" Font-Italic="True"></asp:label></TD>
								</TR>
							</TABLE>
						</DIV>
						<DIV align="center"><FONT face="Arial"><STRONG>
									<P align="center">
										<TABLE id="Table4" cellSpacing="0" cellPadding="0" border="0">
											<TBODY>
												<TR>
													<TD colSpan="2">
														<P align="center"><STRONG><FONT face="Verdana" size="2">Сообщение клиенту:</FONT></STRONG></P>
													</TD>
												</TR>
												<TR>
													<TD vAlign="middle" align="center" colSpan="2">
														<TABLE id="Table5" cellSpacing="0" cellPadding="0" width="300" border="0">
															<TBODY>
																<TR>
																	<TD><asp:textbox id="MessageTB" runat="server" BorderStyle="None" Width="400px" Height="150px" TextMode="MultiLine"
																			Font-Names="Verdana" Font-Size="8pt" BackColor="LightGray"></asp:textbox></TD>
																</TR>
																<TR>
																	<TD>
																		<P align="left"><FONT face="Verdana" size="2">Показать</FONT>
																			<asp:dropdownlist id="SendMessageCountDD" runat="server">
																				<asp:ListItem Value="1" Selected="True">1</asp:ListItem>
																				<asp:ListItem Value="2">2</asp:ListItem>
																				<asp:ListItem Value="5">5</asp:ListItem>
																				<asp:ListItem Value="10">10</asp:ListItem>
																			</asp:dropdownlist>&nbsp;<FONT face="Verdana" size="2">раз.</FONT></P>
																	</TD>
																</TR>
																<TR>
																	<TD>
																		<P align="right"><asp:button id="SendMessage" runat="server" Font-Names="Arial" Font-Size="10pt" Text="Отправить"
																				BorderStyle="None"></asp:button></P>
																	</TD>
																</TR>
															</TBODY>
														</TABLE>
														<P align="center"><asp:label id="StatusL" runat="server" Font-Bold="True" ForeColor="Green" Font-Names="Verdana"
																Font-Size="8pt" Font-Italic="True"> Cообщение отправленно.</asp:label></P>
													</TD>
												</TR>
											</TBODY>
										</TABLE>
									</P>
								</STRONG></FONT>
						</DIV>
						<P align="center"><FONT face="Verdana"><FONT size="1"><FONT color="#000000">© АК "</FONT> </FONT>
							</FONT><A href="http://www.analit.net/"><FONT color="#800080" size="1" face="Verdana">Инфорум</FONT></A><FONT color="#000000" size="1" face="Verdana">" 
								2005</FONT></P>
					</TD>
				</TR>
			</TABLE>
			&nbsp;
		</form>
	</body>
</HTML>
