<%@ Page Language="c#" AutoEventWireup="true" Inherits="AddUser.manageret" CodePage="1251"
	CodeFile="manageret.aspx.cs" MaintainScrollPositionOnPostback="true" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head>
	<title>Конфигурация пользователя</title>
	<meta http-equiv="Content-Type" content="text/html; charset=windows-1251" />
	<style type="text/css">
		.tdCheckBox {vertical-align: middle; text-align: left; font-size: 8pt; font-family: Verdana;}
	</style>
</head>
<body vlink="#ab51cc" alink="#0093e1" link="#0093e1" bgcolor="#ffffff">
	<form id="Form1" method="post" runat="server">
		<table id="Table1" cellspacing="0" cellpadding="0" width="100%" border="0">
			<tr>
				<td valign="top" align="center" style="height: 754px">
					<div align="center">
						<font face="Verdana"><font size="2"><strong>Конфигурация клиента</strong> </font></font>
						<asp:Label ID="Name" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="10pt"></asp:Label></div>
					<div align="center">
						<table id="Table2" bordercolor="#dadada" cellspacing="0" cellpadding="0" width="95%"
							align="center" border="1">
							<tr align="center" bgcolor="mintcream">
								<td bgcolor="aliceblue" height="20">
									<font face="Verdana" size="2"><strong>Общая настройка</strong></font></td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox ID="InvisibleCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="&#34;Невидимый&#34; клиент" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="RegisterCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Реестр"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="RejectsCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Забраковка"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
									<font face="Arial" size="2">
										<div>
											<asp:TextBox ID="MultiUserLevelTB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												BorderStyle="None" Width="33px" BackColor="LightGray"></asp:TextBox><font face="Verdana">&nbsp;-
													одновременных сеансов</font></div>
									</font>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="AdvertisingLevelCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Реклама" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="WayBillCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Накладные" BorderStyle="None" Enabled="False"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="ChangeSegmentCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Изменение сегмента" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3">
											<asp:CheckBox ID="EnableUpdateCB" runat="server" Font-Names="Verdana" Font-Size="8pt"
												Text="Автоматическое обновление версий" BorderStyle="None"></asp:CheckBox>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" colspan="3" style="height: 22px">
									<asp:CheckBox ID="ResetCopyIDCB" runat="server" Font-Names="Verdana" Text="Сбросить уникальный идентификатор, причина:"
										Font-Size="8pt" Enabled="False"></asp:CheckBox>
									<asp:TextBox ID="CopyIDWTB" runat="server" Font-Names="Verdana" BorderStyle="None"
										Font-Size="8pt" BackColor="LightGray" Enabled="False"></asp:TextBox>
									<asp:Label ID="IDSetL" runat="server" Font-Size="8pt" Font-Names="Verdana" ForeColor="Green">Идентификатор не присвоен</asp:Label></td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3" style="height: 22px">
									<asp:CheckBox runat="server" ID="CalculateLeaderCB" Text="Pассчитывать лидеров при получении заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="AllowSubmitOrdersCB" Text="Разрешить управление подтверждением заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="SubmitOrdersCB" Text="Разрешить подтверждение заказов" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="ServiceClientCB" Text="Служебный клиент" />
								</td>
							</tr>
							<tr>
								<td class="tdCheckBox" colspan="3">
									<asp:CheckBox runat="server" ID="OrdersVisualizationModeCB" Text="Показывать заказы подробно" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="left" bgcolor="#f0f8ff" colspan="3">
									<p style="font-size: small; font-family: Verdana; text-align: center;">
										<strong>Региональная настройка</strong></p>
								</td>
							</tr>
							<tr>
								<td style="height: 8px" valign="middle" align="left" colspan="3">
									<font face="Arial" size="2">Домашний регион:</font>
									<asp:DropDownList ID="RegionDD" runat="server" OnSelectedIndexChanged="RegionDD_SelectedIndexChanged"
										Font-Names="Verdana" Font-Size="8pt" AutoPostBack="True" DataSource="<%# admin %>"
										DataTextField="Region" DataValueField="RegionCode">
									</asp:DropDownList>
									<asp:CheckBox ID="AllRegCB" runat="server" OnCheckedChanged="AllRegCB_CheckedChanged"
										Font-Size="8pt" Font-Names="Verdana" Text="Показать все регионы" AutoPostBack="True">
									</asp:CheckBox></td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 79px">
									<table id="Table3" cellspacing="0" cellpadding="0" border="0">
										<tr>
											<td width="200">
												<font face="Verdana" size="2"><strong>Доступные регионы:</strong></font></td>
											<td width="200">
												<font face="Verdana" size="2"><strong>Регионы заказа:</strong></font></td>
										</tr>
										<tr>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="WRList" runat="server" Font-Names="Verdana" Font-Size="8pt"
													BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode"
													CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
											<td valign="top" align="left" width="200">
												<asp:CheckBoxList ID="OrderList" runat="server" Font-Names="Verdana" Font-Size="8pt"
													BorderStyle="None" DataSource="<%# WorkReg %>" DataTextField="Region" DataValueField="RegionCode"
													CellSpacing="0" CellPadding="0">
												</asp:CheckBoxList></td>
										</tr>
									</table>
								</td>
							</tr>
							<tr>
								<td valign="middle" align="right" colspan="3" style="height: 17px">
									<asp:Button ID="ParametersSave" runat="server" OnClick="ParametersSave_Click" Font-Names="Verdana"
										Font-Size="8pt" Text="Применить" BorderStyle="None"></asp:Button></td>
							</tr>
							<tr>
								<td colspan="3" style="height: 22px">
									<asp:Button runat="Server" ID="GeneratePasswords" Text="Сгенерировать пароли" BorderStyle="Groove" Font-Names="Verdana" Font-Size="8pt" OnClick="GeneratePasswords_Click" />
								</td>
							</tr>
							<tr>
								<td valign="middle" align="center" colspan="3" style="height: 15px">
									<asp:Label ID="ResultL" runat="server" Font-Names="Verdana" Font-Bold="True" Font-Size="8pt"
										ForeColor="Green" Font-Italic="True"></asp:Label></td>
							</tr>
						</table>
					</div>
					<div align="center">
						<font face="Arial"><strong>
							<p align="center">
								<table id="Table4" cellspacing="0" cellpadding="0" border="0">
									<tbody>
										<tr>
											<td colspan="2">
												<p align="center">
													<strong><font face="Verdana" size="2">Сообщение клиенту:</font></strong></p>
											</td>
										</tr>
										<tr>
											<td valign="middle" align="center" colspan="2" style="height: 228px">
												<table id="Table5" cellspacing="0" cellpadding="0" width="300" border="0">
													<tbody>
														<tr>
															<td>
																<asp:TextBox ID="MessageTB" runat="server" BorderStyle="None" Width="400px" Height="150px"
																	TextMode="MultiLine" Font-Names="Verdana" Font-Size="8pt" BackColor="LightGray"></asp:TextBox></td>
														</tr>
														<tr>
															<td>
																<p align="left">
																	<font face="Verdana" size="2">Показать</font>
																	<asp:DropDownList ID="SendMessageCountDD" runat="server">
																		<asp:ListItem Value="1" Selected="True">1</asp:ListItem>
																		<asp:ListItem Value="2">2</asp:ListItem>
																		<asp:ListItem Value="5">5</asp:ListItem>
																		<asp:ListItem Value="10">10</asp:ListItem>
																	</asp:DropDownList>&nbsp;<font face="Verdana" size="2">раз.</font></p>
															</td>
														</tr>
														<tr>
															<td>
																<p align="right">
																	<asp:Button ID="SendMessage" runat="server" OnClick="SendMessage_Click" Font-Names="Arial"
																		Font-Size="10pt" Text="Отправить" BorderStyle="None"></asp:Button></p>
															</td>
														</tr>
													</tbody>
												</table>
												<p align="center">
													<asp:Label ID="StatusL" runat="server" Font-Bold="True" ForeColor="Green" Font-Names="Verdana"
														Font-Size="8pt" Font-Italic="True"> Cообщение отправленно.</asp:Label><br /> 
														<asp:Label ID="MessageLeftL" runat="Server" ForeColor="Green" Font-Names="Verdana"
														Font-Size="8pt" Font-Italic="True" Text="Остались не показанные сообщения" /></p>
											</td>
										</tr>
									</tbody>
								</table>
							</p>
						</strong></font>
					</div>
					<p align="center">
						<font face="Verdana"><font size="1"><font color="#000000">© АК "</font> </font></font>
						<a href="http://www.analit.net/"><font color="#800080" size="1" face="Verdana">Инфорум</font></a><font
							color="#000000" size="1" face="Verdana">" 2005</font></p>
				</td>
			</tr>
		</table>
		&nbsp;
	</form>
</body>
</html>
